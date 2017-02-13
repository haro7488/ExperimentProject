using UnityEngine;
using System.Collections;
using System;

#if EM_UIAP
using UnityEngine.Purchasing;
using UnityEngine.Purchasing.Security;
#endif

namespace EasyMobile
{
    public class IAPManager : MonoBehaviour
    {
        public static IAPManager Instance { get; private set; }

        // Suppress the "Event is never used" warnings.
        #pragma warning disable 0067
        public static event Action<IAPProduct> PurchaseCompleted = delegate {};
        public static event Action<IAPProduct> PurchaseFailed = delegate {};
        // Restore events are fired on iOS or MacOSX only
        public static event Action RestoreCompleted = delegate {};
        public static event Action RestoreFailed = delegate {};
        #pragma warning restore 0067

        #if EM_UIAP
        /// <summary>
        /// The underlying UnityIAP's IStoreController used in this module.
        /// </summary>
        /// <value>The store controller.</value>
        public static IStoreController StoreController { get { return _storeController; } }

        /// <summary>
        /// The underlying UnityIAP's IExtensionProvider used in this module. Use it to access
        /// store-specific extended functionality.
        /// </summary>
        /// <value>The store extension provider.</value>
        public static IExtensionProvider StoreExtensionProvider { get { return _storeExtensionProvider; } }

        // The Unity Purchasing system
        private static IStoreController _storeController;

        // The store-specific Purchasing subsystems
        private static IExtensionProvider _storeExtensionProvider;

        // Store listener to handle purchasing events
        private static StoreListener _storeListener = new StoreListener();
        #endif

        void Awake()
        {
            if (Instance != null)
            {
                Destroy(this);
            }
            else
            {
                Instance = this;
            }
        }

        void Start()
        {
            #if EM_UIAP
            // If we haven't set up the Unity Purchasing reference
            if (_storeController == null)
            {
                // Begin to configure our connection to Purchasing
                InitializePurchasing();
            }
            #endif
        }

        public static void InitializePurchasing()
        {
            #if EM_UIAP
            if (IsInitialized())
            {
                return;
            }

            // Create a builder, first passing in a suite of Unity provided stores.
            var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());

            // Add products
            foreach (IAPProduct pd in EM_Settings.InAppPurchasing.Products)
            {                
                if (pd.StoreSpecificIds != null && pd.StoreSpecificIds.Length > 0)
                {
                    // Add store-specific id if any
                    IDs storeIDs = new IDs();

                    foreach (IAPProduct.StoreSpecificId sId in pd.StoreSpecificIds)
                    {
                        storeIDs.Add(sId.id, new string[] { GetStoreName(sId.store) });
                    }

                    // Add product with store-specific ids
                    builder.AddProduct(pd.Id, GetProductType(pd.Type), storeIDs);
                }
                else
                {
                    // Add product using store-independent id
                    builder.AddProduct(pd.Id, GetProductType(pd.Type));
                }
            }

            // Kick off the remainder of the set-up with an asynchrounous call, passing the configuration 
            // and this class' instance. Expect a response either in OnInitialized or OnInitializeFailed.
            UnityPurchasing.Initialize(_storeListener, builder);
            #else
            if (Debug.isDebugBuild)
                Debug.Log("InitializePurchase FAILED: SDK missing. Please enable Unity Purchasing service.");
            #endif
        }

        /// <summary>
        /// Determines whether UnityIAP is initialized. All further actions like purchasing
        /// or restoring can only be done if UnityIAP is initialized.
        /// </summary>
        /// <returns><c>true</c> if initialized; otherwise, <c>false</c>.</returns>
        public static bool IsInitialized()
        {
            #if EM_UIAP
            // Only say we are initialized if both the Purchasing references are set.
            return _storeController != null && _storeExtensionProvider != null;
            #else
            return false;
            #endif
        }

        /// <summary>
        /// Purchase the specified product.
        /// </summary>
        /// <param name="product">Product.</param>
        public static void Purchase(IAPProduct product)
        {
            if (product != null && product.Id != null)
            {
                PurchaseWithId(product.Id);
            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("Purchase FAILED: Either the product or its id is invalid.");
            }
        }

        /// <summary>
        /// Purchases the product with specified name.
        /// </summary>
        /// <param name="productName">Product name.</param>
        public static void Purchase(string productName)
        {
            IAPProduct pd = GetIAPProductByName(productName);

            if (pd != null && pd.Id != null)
            {
                PurchaseWithId(pd.Id);
            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("PurchaseWithName FAILED: Not found product with name: " + productName + " or its id is invalid.");
            }
        }

        /// <summary>
        /// Purchase the product with specified productId.
        /// </summary>
        /// <param name="productId">Product identifier.</param>
        public static void PurchaseWithId(string productId)
        {
            #if EM_UIAP
            if (IsInitialized())
            {
                Product product = _storeController.products.WithID(productId);

                if (product != null && product.availableToPurchase)
                {
                    if (Debug.isDebugBuild)
                        Debug.Log("Purchasing product asychronously: " + product.definition.id);

                    // Buy the product, expect a response either through ProcessPurchase or OnPurchaseFailed asynchronously.
                    _storeController.InitiatePurchase(product);
                }
                else
                {
                    if (Debug.isDebugBuild)
                        Debug.Log("BuyProductID FAILED: either product not found or not available for purchase.");
                }
            }
            else
            {
                // Purchasing has not succeeded initializing yet.
                if (Debug.isDebugBuild)
                    Debug.Log("BuyProductID FAILED: In-App Purchasing is not initialized.");
            }
            #else
            if (Debug.isDebugBuild)
                Debug.Log("PurchaseWithId: FAIL. IAP module is not enabled.");
            #endif
        }

        /// <summary>
        /// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google.
        /// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
        /// This method only has effect on iOS and MacOSX apps.
        /// </summary>
        public static void RestorePurchases()
        {
            #if EM_UIAP
            if (!IsInitialized())
            {
                if (Debug.isDebugBuild)
                    Debug.Log("RestorePurchases FAILED: In-App Purchasing is not initialized.");
                return;
            }

            if (Application.platform == RuntimePlatform.IPhonePlayer ||
                Application.platform == RuntimePlatform.OSXPlayer)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("RestorePurchases started ...");

                // Fetch the Apple store-specific subsystem.
                var apple = _storeExtensionProvider.GetExtension<IAppleExtensions>();

                // Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
                // the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
                apple.RestoreTransactions((result) =>
                    {
                        // The first phase of restoration. If no more responses are received on ProcessPurchase then 
                        // no purchases are available to be restored.
                        if (Debug.isDebugBuild)
                            Debug.Log("RestorePurchases result: " + result);

                        if (result)
                        {
                            // Fire restore complete event.
                            RestoreCompleted();
                        }
                        else
                        {
                            // Fire event failed event.
                            RestoreFailed();
                        }
                    });
            }
            else
            {
                // We are not running on an Apple device. No work is necessary to restore purchases.
                if (Debug.isDebugBuild)
                    Debug.Log("RestorePurchases FAILED: Not supported on this platform: " + Application.platform.ToString());
            }
            #else
            if (Debug.isDebugBuild)
                Debug.Log("RestorePurchases: FAIL. IAP module is not enabled.");
            #endif
        }

        #if EM_UIAP
        /// <summary>
        /// Gets the product localized data provided by the store.
        /// </summary>
        /// <returns>The product localized data.</returns>
        /// <param name="productId">Product name.</param>
        public static ProductMetadata GetProductLocalizedData(string productName)
        {            
            if (!IsInitialized())
            {
                if (Debug.isDebugBuild)
                    Debug.Log("GetProductLocalizedData FAILED: In-App Purchasing is not initialized.");
                return null;
            }

            Product pd = _storeController.products.WithID(GetIAPProductByName(productName).Id);

            if (pd != null)
            {
                return pd.metadata;
            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("GetProductLocalizedData FAILED: Not found product with name: " + productName);
                return null;
            }           
        }
        #endif

        /// <summary>
        /// Determines whether the product with the specified name is owned
        /// by checking if its receipt exists (and valid) or not.
        /// Note that consumable's receipts are not persisted between app restarts,
        /// therefore this method only returns true for consumable products in the session they're purchased.
        /// </summary>
        /// <returns><c>true</c> if this instance is product owned the specified productId; otherwise, <c>false</c>.</returns>
        /// <param name="productId">Product name.</param>
        public static bool IsProductOwned(string productName)
        {
            #if EM_UIAP
            if (!IsInitialized())
            {
                if (Debug.isDebugBuild)
                    Debug.Log("IsProductOwned FAILED: In-App Purchasing is not initialized..");
                return false;
            }

            Product pd = _storeController.products.WithID(GetIAPProductByName(productName).Id);

            if (pd != null)
            {
                if (pd.receipt != null)
                {
                    bool isValid = true; // presume validity if not validate receipt.
                    bool canValidateReceipt = false;    // disable receipt validation by default

                    if (Application.platform == RuntimePlatform.Android)
                    {
                        // On Android, receipt validation is only available for Google Play store
                        canValidateReceipt = EM_Settings.InAppPurchasing.IsValidateGooglePlayReceipt;
                        canValidateReceipt &= (GetAndroidStore(EM_Settings.InAppPurchasing.TargetAndroidStore) == AndroidStore.GooglePlay);
                    }
                    else if (Application.platform == RuntimePlatform.IPhonePlayer ||
                             Application.platform == RuntimePlatform.OSXPlayer ||
                             Application.platform == RuntimePlatform.tvOS)
                    {
                        // Receipt validation is also available for Apple app stores
                        canValidateReceipt = EM_Settings.InAppPurchasing.IsValidateAppleReceipt;
                    }

                    if (canValidateReceipt)
                    {
                        isValid = ValidateReceipt(pd.receipt);
                    }
                    
                    return isValid;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("IsProductOwned FAILED: Not found product with name: " + productName);

                return false;
            }
            #else
            if (Debug.isDebugBuild)
                Debug.Log("IsProductOwned FAIL: SDK missing. Please enable Unity In-App Purchasing service.");

            return false;
            #endif
        }

        /// <summary>
        /// Gets the IAP product with the specified name.
        /// </summary>
        /// <returns>The IAP product.</returns>
        /// <param name="productName">Product name.</param>
        public static IAPProduct GetIAPProductByName(string productName)
        {
            foreach (IAPProduct pd in EM_Settings.InAppPurchasing.Products)
            {
                if (pd.Name.Equals(productName))
                    return pd;
            }

            return null;
        }

        /// <summary>
        /// Gets the IAP product by identifier.
        /// </summary>
        /// <returns>The IAP product by identifier.</returns>
        /// <param name="pId">P identifier.</param>
        public static IAPProduct GetIAPProductById(string productId)
        {
            foreach (IAPProduct pd in EM_Settings.InAppPurchasing.Products)
            {
                if (pd.Id.Equals(productId))
                    return pd;
            }

            return null;
        }

        #region Helpers

        #if EM_UIAP
        public static string GetStoreName(IAPStore store)
        {
            switch (store)
            {
                case IAPStore.AmazonApps:
                    return AmazonApps.Name;
                case IAPStore.AppleAppStore:
                    return AppleAppStore.Name;
                case IAPStore.GooglePlay:
                    return GooglePlay.Name;
                case IAPStore.MacAppStore:
                    return MacAppStore.Name;
                case IAPStore.SamsungApps:
                    return SamsungApps.Name;
                case IAPStore.WindowsStore:
                    return WindowsStore.Name;
                default:
                    return string.Empty;
            }
        }

        public static ProductType GetProductType(IAPProductType pType)
        {
            switch (pType)
            {
                case IAPProductType.Consumable:
                    return ProductType.Consumable;
                case IAPProductType.NonConsumable:
                    return ProductType.NonConsumable;
                case IAPProductType.Subscription:
                    return ProductType.Subscription;
                default:
                    return ProductType.Consumable;
            }
        }

        public static AndroidStore GetAndroidStore(IAPAndroidStore store)
        {
            switch (store)
            {
                case IAPAndroidStore.AmazonAppStore:
                    return AndroidStore.AmazonAppStore;
                case IAPAndroidStore.GooglePlay:
                    return AndroidStore.GooglePlay;
                case IAPAndroidStore.SamsungApps:
                    return AndroidStore.SamsungApps;
                case IAPAndroidStore.NotSpecified:
                    return AndroidStore.NotSpecified;
                default:
                    return AndroidStore.NotSpecified;
            }
        }
        #endif

        #endregion

        #region IStoreListener implementation

        #if EM_UIAP
        private class StoreListener : IStoreListener
        {
            public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
            {
                // Purchasing has succeeded initializing. Collect our Purchasing references.
                if (Debug.isDebugBuild)
                    Debug.Log("In-App Purchasing OnInitialized: PASS");

                // Overall Purchasing system, configured with products for this application.
                _storeController = controller;

                // Store specific subsystem, for accessing device-specific store features.
                _storeExtensionProvider = extensions;
            }

            public void OnInitializeFailed(InitializationFailureReason error)
            {
                // Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
                if (Debug.isDebugBuild)
                    Debug.Log("In-App Purchasing OnInitializeFailed. InitializationFailureReason:" + error);
            }

            public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
            {
                // A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
                // this reason with the user to guide their troubleshooting actions.
                if (Debug.isDebugBuild)
                    Debug.Log(string.Format("Purchase product FAILED: Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));

                // Fire purchase failure event
                IAPProduct pd = GetIAPProductById(product.definition.id);
                PurchaseFailed(pd);
            }

            public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("Processing purchase of product: " + args.purchasedProduct.transactionID);
                
                bool validPurchase = true;  // presume validity if not validate receipt
                bool canValidateReceipt = false;    // disable receipt validation by default

                if (Application.platform == RuntimePlatform.Android)
                {
                    // On Android, receipt validation is only available for Google Play store
                    canValidateReceipt = EM_Settings.InAppPurchasing.IsValidateGooglePlayReceipt;
                    canValidateReceipt &= (GetAndroidStore(EM_Settings.InAppPurchasing.TargetAndroidStore) == AndroidStore.GooglePlay);
                }
                else if (Application.platform == RuntimePlatform.IPhonePlayer ||
                         Application.platform == RuntimePlatform.OSXPlayer ||
                         Application.platform == RuntimePlatform.tvOS)
                {
                    // Receipt validation is also available for Apple app stores
                    canValidateReceipt = EM_Settings.InAppPurchasing.IsValidateAppleReceipt;
                }

                if (canValidateReceipt)
                {
                    validPurchase = ValidateReceipt(args.purchasedProduct.receipt, true);
                }
        
                IAPProduct pd = GetIAPProductById(args.purchasedProduct.definition.id);

                if (validPurchase)
                {
                    if (Debug.isDebugBuild)
                        Debug.Log("Product purchase completed.");
                    
                    // Fire purchase success event
                    PurchaseCompleted(pd);
                }
                else
                {
                    // Fire purchase failure event
                    if (Debug.isDebugBuild)
                        Debug.Log("Purchase FAILED: Invalid receipt.");
                    
                    PurchaseFailed(pd);
                }

                return PurchaseProcessingResult.Complete;
            }
        }

        /// <summary>
        /// Validates the receipt. Works with receipts from Apple stores and Google Play store only.
        /// Always returns true for other stores.
        /// </summary>
        /// <returns><c>true</c>, if receipt was validated, <c>false</c> otherwise.</returns>
        /// <param name="receipt">Receipt.</param>
        /// <param name="logReceiptContent">If set to <c>true</c> log receipt content.</param>
        private static bool ValidateReceipt(string receipt, bool logReceiptContent = false)
        {
            bool isValidReceipt = true; // presume validity for platforms with no receipt validation.

            // Unity IAP's receipt validation is only available for Apple app stores and Google Play store.   
            


#if UNITY_ANDROID || UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS
        
            byte[] googlePlayTangleData = null;
            byte[] appleTangleData = null;

            // Here we populate the secret keys for each platform.
            // Note that the code is disabled in the editor for it to not stop the EM editor code (due to ClassNotFound error)
            // from recreating the dummy AppleTangle and GoogleTangle classes if they were inadvertently removed.
            


#if UNITY_ANDROID && !UNITY_EDITOR
            googlePlayTangleData = GooglePlayTangle.Data();
            #endif
        
            


#if (UNITY_IOS || UNITY_STANDALONE_OSX || UNITY_TVOS) && !UNITY_EDITOR
            appleTangleData = AppleTangle.Data();
            #endif
        
            // Prepare the validator with the secrets we prepared in the Editor obfuscation window.
            var validator = new CrossPlatformValidator(googlePlayTangleData, appleTangleData, Application.bundleIdentifier);

            try
            {
                // On Google Play, result has a single product ID.
                // On Apple stores, receipts contain multiple products.
                var result = validator.Validate(receipt);

                // For informational purposes, we list the receipt(s)
                if (Debug.isDebugBuild && logReceiptContent)
                {
                    Debug.Log("Receipt contents:");
                    foreach (IPurchaseReceipt productReceipt in result)
                    {
                        if (productReceipt != null)
                        {
                            Debug.Log(productReceipt.productID);
                            Debug.Log(productReceipt.purchaseDate);
                            Debug.Log(productReceipt.transactionID);
                        }
                    }
                }
            }
            catch (IAPSecurityException)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("Receipt Validation: Invalid receipt.");

                isValidReceipt = false;
            }
            #endif
        
            return isValidReceipt;
        }

        #endif

        #endregion
    }
}