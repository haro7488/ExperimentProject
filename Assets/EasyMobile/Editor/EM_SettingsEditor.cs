using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using SgLib.Editor;

namespace EasyMobile.Editor
{
    [CustomEditor(typeof(EM_Settings))]
    public partial class EM_SettingsEditor : UnityEditor.Editor
    {
        #region Modules

        public enum Module
        {
            Advertising = 0,
            InAppPurchase = 1,
            GameService = 2,
            Notification = 3
        }

        static Module activeModule = Module.Advertising;

        #endregion

        #region Target properties

        // Module toggles
        SerializedProperty isAdModuleEnable;
        SerializedProperty isIAPModuleEnable;
        SerializedProperty isGameServiceModuleEnable;
        SerializedProperty isNotificationModuleEnable;

        // Active module (currently selected on the toolbar)
        SerializedProperty activeModuleIndex;

        public class EMProperty
        {
            public SerializedProperty property;
            public GUIContent content;

            public EMProperty(SerializedProperty p, GUIContent c)
            {
                property = p;
                content = c;
            }
        }

        // Ad module properties
        private static class AdProperties
        {
            public static SerializedProperty mainProperty;
            public static EMProperty iosAdMobConfig = new EMProperty(null, new GUIContent("[iOS] AdMob Ids"));
            public static EMProperty androidAdMobConfig = new EMProperty(null, new GUIContent("[Android] AdMob Ids"));
            public static EMProperty heyzapPublisherId = new EMProperty(null, new GUIContent("Heyzap Publisher Id"));
            public static EMProperty heyzapShowTestSuite = new EMProperty(null, new GUIContent("Show Heyzap Test Suite"));
            public static EMProperty autoLoadDefaultAds = new EMProperty(null, new GUIContent("Auto-Load Default Ads", "Automatically load ads from default ad networks"));
            public static EMProperty adCheckingInterval = new EMProperty(null, new GUIContent("Ad Checking Interval", "Time (seconds) between 2 checks (to see if ads were loaded)"));
            public static EMProperty adLoadingInterval = new EMProperty(null, new GUIContent("Ad Loading Interval", "Minimum time (seconds) between two ad-loading requests (to restrict the number of requests sent to ad networks)"));
            public static EMProperty iosDefaultAdNetworks = new EMProperty(null, new GUIContent("[iOS] Default Ad Networks"));
            public static EMProperty androidDefaultAdNetworks = new EMProperty(null, new GUIContent("[Android] Default Ad Networks"));
        }

        // In App Purchase module properties
        private static class IAPProperties
        {
            public static SerializedProperty mainProperty;
            public static EMProperty targetAndroidStore = new EMProperty(null, new GUIContent("Target Android Store", "Target Android store"));
            public static EMProperty validateAppleReceipt = new EMProperty(null, new GUIContent("Validate Apple Receipt", "Validate receipts from Apple App stores"));
            public static EMProperty validateGooglePlayReceipt = new EMProperty(null, new GUIContent("Validate Google Play Receipt", "Validate receipts from Google Play store"));
            public static EMProperty products = new EMProperty(null, new GUIContent("Products"));
        }
            
        // Game Service module properties
        private static class GameServiceProperties
        {
            public static SerializedProperty mainProperty;
            public static EMProperty gpgsDebugLog = new EMProperty(null, new GUIContent("GPGS Debug Log", "Show debug log from Google Play Games plugin"));
            public static EMProperty autoInit = new EMProperty(null, new GUIContent("Auto Init", "Should the service automatically initialize on start"));
            public static EMProperty autoInitDelay = new EMProperty(null, new GUIContent("Auto Init Delay", "Delay time (seconds) after Start() that the service is automatically initialized"));
            public static EMProperty androidMaxLoginRequest = 
                new EMProperty(null, 
                    new GUIContent("[Android] Max Login Requests", 
                        "[Auto-init and ManagedInit only] The total number of times the login popup can be displayed if the user has not logged in. " +
                        "When this value is reached, the init process will stop thus not showing the login popup anymore (avoid annoying the user). " +
                        "Put -1 to ignore this limit."));
            public static EMProperty leaderboards = new EMProperty(null, new GUIContent("Leaderboards"));
            public static EMProperty achievements = new EMProperty(null, new GUIContent("Achievements"));
            public static EMProperty androidXmlResources = new EMProperty(null, new GUIContent("Android XML Resources", "The XML resources exported from Google Play Console"));
        }

        // Push Notification module properties
        private static class NotificationProperties
        {
            public static SerializedProperty mainProperty;
            public static EMProperty oneSignalAppId = new EMProperty(null, new GUIContent("OneSignal App Id", "The app Id obtained from OneSignal dashboard"));
            public static EMProperty autoInit = new EMProperty(null, new GUIContent("Auto Init", "Should the service automatically initialize on start"));
            public static EMProperty autoInitDelay = new EMProperty(null, new GUIContent("Auto Init Delay", "Delay time (seconds) after Start() that the service is automatically initialized"));
        }

        #endregion

        #region Variables

        // List of Android leaderboard and achievement ids constructed from the generated GPGSIds class.
        private Dictionary<string, string> gpgsIdDict;

        #if EM_UIAP
        // Booleans indicating whether AppleTangle and GooglePlayTangle are not dummy classes.
        bool isAppleTangleValid;
        bool isGooglePlayTangleValid;
        #endif

        #endregion

        #region GUI

        void OnEnable()
        {
            if (serializedObject == null)
                Debug.Log("SerializedObject is NULL");
            
            // Module-control properties.
            isAdModuleEnable = serializedObject.FindProperty("_isAdModuleEnable");
            isIAPModuleEnable = serializedObject.FindProperty("_isIAPModuleEnable");
            isGameServiceModuleEnable = serializedObject.FindProperty("_isGameServiceModuleEnable");
            isNotificationModuleEnable = serializedObject.FindProperty("_isNotificationModuleEnable");

            activeModuleIndex = serializedObject.FindProperty("_activeModuleIndex");

            if (System.Enum.IsDefined(typeof(Module), activeModuleIndex.intValue))
            {
                activeModule = (Module)activeModuleIndex.intValue;
            }

            // Ad module properties.
            AdProperties.mainProperty = serializedObject.FindProperty("_advertisingSettings");
            AdProperties.iosAdMobConfig.property = AdProperties.mainProperty.FindPropertyRelative("_iosAdMobConfig");
            AdProperties.androidAdMobConfig.property = AdProperties.mainProperty.FindPropertyRelative("_androidAdMobConfig");
            AdProperties.heyzapPublisherId.property = AdProperties.mainProperty.FindPropertyRelative("_heyzapPublisherId");
            AdProperties.heyzapShowTestSuite.property = AdProperties.mainProperty.FindPropertyRelative("_heyzapShowTestSuite");
            AdProperties.autoLoadDefaultAds.property = AdProperties.mainProperty.FindPropertyRelative("_autoLoadDefaultAds");
            AdProperties.adCheckingInterval.property = AdProperties.mainProperty.FindPropertyRelative("_adCheckingInterval");
            AdProperties.adLoadingInterval.property = AdProperties.mainProperty.FindPropertyRelative("_adLoadingInterval");
            AdProperties.iosDefaultAdNetworks.property = AdProperties.mainProperty.FindPropertyRelative("_iosDefaultAdNetworks");
            AdProperties.androidDefaultAdNetworks.property = AdProperties.mainProperty.FindPropertyRelative("_androidDefaultAdNetwork");


            // In App Purchase module properties.
            IAPProperties.mainProperty = serializedObject.FindProperty("_inAppPurchaseSettings");
            IAPProperties.targetAndroidStore.property = IAPProperties.mainProperty.FindPropertyRelative("_targetAndroidStore");
            IAPProperties.validateAppleReceipt.property = IAPProperties.mainProperty.FindPropertyRelative("_validateAppleReceipt");
            IAPProperties.validateGooglePlayReceipt.property = IAPProperties.mainProperty.FindPropertyRelative("_validateGooglePlayReceipt");
            IAPProperties.products.property = IAPProperties.mainProperty.FindPropertyRelative("_products");

            // Game Service module properties.
            GameServiceProperties.mainProperty = serializedObject.FindProperty("_gameServiceSettings");
            GameServiceProperties.gpgsDebugLog.property = GameServiceProperties.mainProperty.FindPropertyRelative("_gpgsDebugLog");
            GameServiceProperties.autoInit.property = GameServiceProperties.mainProperty.FindPropertyRelative("_autoInit");
            GameServiceProperties.autoInitDelay.property = GameServiceProperties.mainProperty.FindPropertyRelative("_autoInitDelay");
            GameServiceProperties.androidMaxLoginRequest.property = GameServiceProperties.mainProperty.FindPropertyRelative("_androidMaxLoginRequests");
            GameServiceProperties.leaderboards.property = GameServiceProperties.mainProperty.FindPropertyRelative("_leaderboards");
            GameServiceProperties.achievements.property = GameServiceProperties.mainProperty.FindPropertyRelative("_achievements");
            GameServiceProperties.androidXmlResources.property = GameServiceProperties.mainProperty.FindPropertyRelative("_androidXmlResources");

            // Notification module properties.
            NotificationProperties.mainProperty = serializedObject.FindProperty("_notificationSettings");
            NotificationProperties.oneSignalAppId.property = NotificationProperties.mainProperty.FindPropertyRelative("_oneSignalAppId");
            NotificationProperties.autoInit.property = NotificationProperties.mainProperty.FindPropertyRelative("_autoInit");
            NotificationProperties.autoInitDelay.property = NotificationProperties.mainProperty.FindPropertyRelative("_autoInitDelay");

            // Get the list of GPGS leaderboard and achievement ids.
            gpgsIdDict = EM_EditorUtil.GetGPGSIds();

            #if EM_UIAP
            // Determine if AppleTangle and GooglePlayTangle classes are valid ones (generated by UnityIAP's receipt validation obfuscator).
            isAppleTangleValid = EM_EditorUtil.IsValidAppleTangleClass();
            isGooglePlayTangleValid = EM_EditorUtil.IsValidGooglePlayTangleClass();
            #endif
        }

        public override void OnInspectorGUI()
        {
            // Update the serializedProperty - always do this in the beginning of OnInspectorGUI.
            serializedObject.Update();

            // Draw the module-select toolbar.
            EditorGUILayout.BeginHorizontal();
            EM_EditorGUI.ToolbarButton(new GUIContent(null, EM_GUIStyleManager.AdIcon, "Advertising"), Module.Advertising, ref activeModule, EditorGUIUtility.isProSkin ? EM_GUIStyleManager.ModuleToolbarButtonLeft : EM_GUIStyleManager.ModuleToolbarButton);
            EM_EditorGUI.ToolbarButton(new GUIContent(null, EM_GUIStyleManager.IAPIcon, "In-App Purchase"), Module.InAppPurchase, ref activeModule, EditorGUIUtility.isProSkin ? EM_GUIStyleManager.ModuleToolbarButtonMid : EM_GUIStyleManager.ModuleToolbarButton);
            EM_EditorGUI.ToolbarButton(new GUIContent(null, EM_GUIStyleManager.GameServiceIcon, "Game Service"), Module.GameService, ref activeModule, EditorGUIUtility.isProSkin ? EM_GUIStyleManager.ModuleToolbarButtonMid : EM_GUIStyleManager.ModuleToolbarButton);
            EM_EditorGUI.ToolbarButton(new GUIContent(null, EM_GUIStyleManager.NotificationIcon, "Notification"), Module.Notification, ref activeModule, EditorGUIUtility.isProSkin ? EM_GUIStyleManager.ModuleToolbarButtonRight : EM_GUIStyleManager.ModuleToolbarButton);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space();

            // Store the toolbar index value to the serialized settings file.
            activeModuleIndex.intValue = (int)activeModule;

            switch (activeModule)
            {
                case Module.Advertising:
                    AdModuleGUI();
                    break;
                case Module.InAppPurchase:
                    IAPModuleGUI();
                    break;
                case Module.GameService:
                    GameServiceModuleGUI();
                    break;
                case Module.Notification:
                    NotificationModuleGUI();
                    break;
            }

            // Apply changes to the serializedProperty - always do this in the end of OnInspectorGUI.
            serializedObject.ApplyModifiedProperties();
        }

        #endregion
    }
}
