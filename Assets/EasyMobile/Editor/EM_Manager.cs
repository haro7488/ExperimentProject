using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using SgLib.Editor;

namespace EasyMobile.Editor
{
    [InitializeOnLoad]
    public class EM_Manager : AssetPostprocessor
    {
        // This static constructor will automatically run thanks to the InitializeOnLoad attribute.
        static EM_Manager()
        {
            EditorApplication.update += Initialize;
        }

        // This is called by Unity after importing of any number of assets is complete.
        static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            // Here we know which files have been updated.
        }

        private static void Initialize()
        {
            EditorApplication.update -= Initialize;

            // Import the native package if it has been imported before.
            if (!IsNativeCodeImported())
            {
                ImportNativeCode();
            }

            // Define a global symbol indicating the existence of EasyMobile
            GlobalDefineManager.SDS_AddDefine(EM_ScriptingSymbols.EasyMobile, EditorUserBuildSettings.selectedBuildTargetGroup);

            // Create the EM_Settings scriptable object if it doesn't exist.
            CreateSettingsAsset();

            // Create the EasyMobile prefab if it doesn't exist.
            CreateMainPrefab();

            // Regularly check for module prerequisites to avoid issues caused
            // by inadvertent changes, e.g remove components from prefab or delete scripting symbol.
            CheckModules();
        }

        [MenuItem("Window/Easy Mobile/Settings", false)]
        public static void MenuOpenSettings()
        {
            EM_Settings instance = EM_Settings.LoadSettingsAsset();

            if (instance == null)
            {
                instance = CreateSettingsAsset();
            }

            Selection.activeObject = instance;
        }

        [MenuItem("Window/Easy Mobile/Create EasyMobile.prefab", false)]
        public static void MenuCreateMainPrefab()
        {
            CreateMainPrefab(true);
            CheckModules();
        }

        [MenuItem("Window/Easy Mobile/Documentation", false)]
        public static void OpenDocumentation()
        {
            Application.OpenURL(EM_Constants.DocumentationURL);
        }

        private static EM_Settings CreateSettingsAsset()
        {
            // Stop if the asset is already created.
            EM_Settings instance = EM_Settings.LoadSettingsAsset();
            if (instance != null)
            {
                return instance;
            }

            // Create Resources folder if it doesn't exist.
            FileIO.EnsureFolderExists(EM_Constants.ResourcesFolder);

            // Now create the asset inside the Resources folder.
            instance = EM_Settings.Instance; // this will create a new instance of the EMSettings scriptable.
            AssetDatabase.CreateAsset(instance, EM_Constants.SettingsAssetPath);    
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("EM_Settings.asset is created at " + EM_Constants.SettingsAssetPath);

            return instance;
        }

        private static GameObject CreateMainPrefab(bool showAlert = false)
        {
            // Stop if the prefab is already created.
            string prefabPath = EM_Constants.MainPrefabPath;
            GameObject existingPrefab = EM_EditorUtil.GetMainPrefab();

            if (existingPrefab != null)
            {
                if (showAlert)
                {
                    EM_EditorUtil.Alert("Prefab Exists", "EasyMobile.prefab already exists at " + prefabPath);
                }

                return existingPrefab;
            }

            // Make sure the containing folder exists.
            FileIO.EnsureFolderExists(EM_Constants.MainPrefabFolder);

            // Create a temporary gameObject and then create the prefab from it.
            GameObject tmpObj = new GameObject(EM_Constants.MainPrefabName);

            // Add PrefabManager component.
            tmpObj.AddComponent<EM_PrefabManager>();

            // Generate the prefab from the temporary game object.
            GameObject prefabObj = PrefabUtility.CreatePrefab(prefabPath, tmpObj);
            GameObject.DestroyImmediate(tmpObj);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            if (showAlert)
            {
                EM_EditorUtil.Alert("Prefab Created", "EasyMobile.prefab is created at " + prefabPath);
            }
            else
            {
                Debug.Log("EasyMobile.prefab is created at " + prefabPath);
            }

            return prefabObj;
        }

        private static bool IsNativeCodeImported()
        {
            return EM_ProjectSettings.Instance.GetBool(EM_Constants.PSK_ImportedNativeCode, false);
        }

        private static void ImportNativeCode()
        {
            AssetDatabase.ImportPackage(EM_Constants.NativePackagePath, false);
            EM_ProjectSettings.Instance.Set(EM_Constants.PSK_ImportedNativeCode, true);
        }

        // Makes that everything is set up properly so that all modules function as expected.
        internal static void CheckModules()
        {
            GameObject mainPrefab = EM_EditorUtil.GetMainPrefab();

            // Advertising module.
            if (EM_Settings.IsAdModuleEnable)
            {
                EnableAdModule(mainPrefab);
            }
            else
            {
                DisableAdModule(mainPrefab);
            }

            // IAP module.
            if (EM_Settings.IsIAPModuleEnable)
            {
                EnableIAPModule(mainPrefab);
            }
            else
            {
                DisableIAPModule(mainPrefab);
            }

            // Game Service module.
            if (EM_Settings.IsGameServiceModuleEnable)
            {
                EnableGameServiceModule(mainPrefab);
            }
            else
            {
                DisableGameServiceModule(mainPrefab);
            }

            // Notification module
            if (EM_Settings.IsNotificationModuleEnable)
            {
                EnableNotificationModule(mainPrefab);
            }
            else
            {
                DisableNotificationModule(mainPrefab);
            }
        }

        internal static void EnableAdModule(GameObject mainPrefab)
        {
            EM_EditorUtil.AddModuleToPrefab<AdManager>(mainPrefab);

            // Check ad network plugins' availability and define appropriate scripting symbols.
            List<string> symbols = new List<string>();
            bool isAdMobAvail = EM_ExternalPluginManager.IsAdMobAvail();
            if (isAdMobAvail)
            {
                symbols.Add(EM_ScriptingSymbols.AdMob);
            }

            bool isChartboostAvail = EM_ExternalPluginManager.IsChartboostAvail();
            if (isChartboostAvail)
            {
                symbols.Add(EM_ScriptingSymbols.Chartboost);
            }

            bool isHeyzapAvail = EM_ExternalPluginManager.IsHeyzapAvail();
            if (isHeyzapAvail)
            {
                symbols.Add(EM_ScriptingSymbols.Heyzap);
            }

            GlobalDefineManager.SDS_AddDefines(symbols.ToArray(), EditorUserBuildSettings.selectedBuildTargetGroup);
        }

        internal static void DisableAdModule(GameObject mainPrefab)
        {
            EM_EditorUtil.RemoveModuleFromPrefab<AdManager>(mainPrefab);

            // Remove associated scripting symbols on all platforms if any was defined on that platform.
            GlobalDefineManager.SDS_RemoveDefinesOnAllPlatforms(
                new string[] { EM_ScriptingSymbols.AdMob, EM_ScriptingSymbols.Chartboost, EM_ScriptingSymbols.Heyzap }
            );
        }

        internal static void EnableIAPModule(GameObject mainPrefab)
        {
            EM_EditorUtil.AddModuleToPrefab<IAPManager>(mainPrefab);

            // Check if UnityIAP is enable and act accordingly.
            bool isUnityIAPAvail = EM_ExternalPluginManager.IsUnityIAPAvail();
            if (isUnityIAPAvail)
            {
                // Generate dummy AppleTangle and GoogleTangle classes if they don't exist.
                // Note that AppleTangle and GooglePlayTangle only get compiled on following platforms,
                // therefore the compilational condition is needed, otherwise the code will repeat forever.
                #if UNITY_ANDROID || UNITY_IPHONE || UNITY_STANDALONE_OSX || UNITY_TVOS
                if (!EM_EditorUtil.AppleTangleClassExists())
                {
                    EM_EditorUtil.GenerateDummyAppleTangleClass();
                }

                if (!EM_EditorUtil.GooglePlayTangleClassExists())
                {
                    EM_EditorUtil.GenerateDummyGooglePlayTangleClass();
                }
                #endif

                GlobalDefineManager.SDS_AddDefine(EM_ScriptingSymbols.UnityIAP, EditorUserBuildSettings.selectedBuildTargetGroup);
            }
        }

        internal static void DisableIAPModule(GameObject mainPrefab)
        { 
            EM_EditorUtil.RemoveModuleFromPrefab<IAPManager>(mainPrefab);

            // Remove associated scripting symbol on all platforms it was defined.
            GlobalDefineManager.SDS_RemoveDefineOnAllPlatforms(EM_ScriptingSymbols.UnityIAP);
        }

        internal static void EnableGameServiceModule(GameObject mainPrefab)
        {
            EM_EditorUtil.AddModuleToPrefab<GameServiceManager>(mainPrefab);

            // Check if Google Play Games plugin is available.
            bool isGPGSAvail = EM_ExternalPluginManager.IsGPGSAvail();
            if (isGPGSAvail)
            {
                // We won't use Google Play Game Services on iOS, so we'll define NO_GPGS symbol to disable it.
                GlobalDefineManager.SDS_AddDefine(EM_ScriptingSymbols.NoGooglePlayGames, BuildTargetGroup.iOS);

                // Define EM_GPGS symbol on Android platform
                GlobalDefineManager.SDS_AddDefine(EM_ScriptingSymbols.GooglePlayGames, BuildTargetGroup.Android);
            }
        }

        internal static void DisableGameServiceModule(GameObject mainPrefab)
        {
            EM_EditorUtil.RemoveModuleFromPrefab<GameServiceManager>(mainPrefab);

            // Removed associated scripting symbols if any was defined.
            // Note that we won't remove the NO_GPGS symbol automatically on iOS.
            // Rather we'll let the user delete it manually if they want.
            // This helps prevent potential build issues on iOS due to GPGS dependencies.
            GlobalDefineManager.SDS_RemoveDefineOnAllPlatforms(EM_ScriptingSymbols.GooglePlayGames);
        }

        internal static void EnableNotificationModule(GameObject mainPrefab)
        {
            EM_EditorUtil.AddModuleToPrefab<NotificationManager>(mainPrefab);

            // Check if OneSignal is available.
            bool isOneSignalAvail = EM_ExternalPluginManager.IsOneSignalAvail();
            if (isOneSignalAvail)
            {
                GlobalDefineManager.SDS_AddDefine(EM_ScriptingSymbols.OneSignal, EditorUserBuildSettings.selectedBuildTargetGroup);
            }
        }

        internal static void DisableNotificationModule(GameObject mainPrefab)
        {
            EM_EditorUtil.RemoveModuleFromPrefab<NotificationManager>(mainPrefab);

            // Remove associated scripting symbol on all platforms it was defined.
            GlobalDefineManager.SDS_RemoveDefineOnAllPlatforms(EM_ScriptingSymbols.OneSignal);
        }
    }
}
