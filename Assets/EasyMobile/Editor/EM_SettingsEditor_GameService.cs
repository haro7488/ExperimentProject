using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

#if UNITY_ANDROID && EM_GPGS
using GooglePlayGames.Editor;
#endif

namespace EasyMobile.Editor
{
    // Partial editor class for GameService module.
    public partial class EM_SettingsEditor
    {
        const string GameServiceModuleLabel = "GAME SERVICE";
        const string GameServiceModuleIntro = "Game Service module helps you quickly implement services like leaderboards and achievements for your game. It provides a cross-platform API that works with the Game Center network on iOS and Google Play Games services on Android.";
        const string GameServiceManualInitInstruction = "You can initialize the module manually from script by calling GameServiceManager.Instance.ManagedInit() or GameServiceManager.Instance.Init() method.";
        const string AndroidGPGSImportInstruction = "Google Play Games plugin is required. Please download and import it to use this module on Android.";
        const string AndroidGPGSAvailMsg = "Google Play Games plugin is imported and ready to use.";
        const string AndroidGPGPSSetupInstruction = "Paste in the Android XML Resources from the Play Console and hit the Setup button.";
        const string GameServiceConstantGenerationIntro = "Generate the static class " + EM_Constants.RootNameSpace + "." + EM_Constants.GameServiceConstantsClassName + " that contains the constants of leaderboard and achievement names." +
                                                          " Remember to regenerate if you make changes to these names.";

        // GameServiceItem property names.
        const string GameServiceItem_NameProperty = "_name";
        const string GameServiceItem_IOSIdProperty = "_iosId";
        const string GameServiceItem_AndroidIdProperty = "_androidId";

        #if !UNITY_ANDROID || (UNITY_ANDROID && EM_GPGS)
        // Foldout bools.
        static bool isLeadeboardsFoldout = false;
        static bool isAchievementsFoldout = false;
        #endif

        // Android resources text area scroll position.
        Vector2 androidResourcesTextAreaScroll;

        void GameServiceModuleGUI()
        {
            EditorGUILayout.BeginVertical(EM_GUIStyleManager.GetCustomStyle("Module Box"));

            EditorGUI.BeginChangeCheck();

            isGameServiceModuleEnable.boolValue = EM_EditorGUI.ModuleToggle(isGameServiceModuleEnable.boolValue, GameServiceModuleLabel);

            // Update the main prefab according to the toggle state.
            if (EditorGUI.EndChangeCheck())
            {
                GameObject prefab = EM_EditorUtil.GetMainPrefab();

                if (!isGameServiceModuleEnable.boolValue)
                {                 
                    EM_Manager.DisableGameServiceModule(prefab);
                }
                else
                { 
                    EM_Manager.EnableGameServiceModule(prefab);
                }
            }

            // Now draw the GUI.
            if (!isGameServiceModuleEnable.boolValue)
            {
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(GameServiceModuleIntro, MessageType.Info);
            }
            else
            {
                #if UNITY_ANDROID && !EM_GPGS
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(AndroidGPGSImportInstruction, MessageType.Error);
                EditorGUILayout.Space();
                if (GUILayout.Button("Download Google Play Games Plugin", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
                {
                    EM_ExternalPluginManager.DownloadGooglePlayGamesPlugin();
                }
                #elif UNITY_ANDROID && EM_GPGS
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(AndroidGPGSAvailMsg, MessageType.Info);
                EditorGUILayout.Space();
                if (GUILayout.Button("Download Google Play Games Plugin", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
                {
                    EM_ExternalPluginManager.DownloadGooglePlayGamesPlugin();
                }

                // Android Google Play Games setup
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("[ANDROID] GOOGLE PLAY GAMES SETUP", EditorStyles.boldLabel);

                // GPGPS debug log
                GameServiceProperties.gpgsDebugLog.property.boolValue = EditorGUILayout.Toggle(GameServiceProperties.gpgsDebugLog.content, GameServiceProperties.gpgsDebugLog.property.boolValue);

                // Text area to input the Android resource.
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox(AndroidGPGPSSetupInstruction, MessageType.None);
                EditorGUILayout.LabelField(GameServiceProperties.androidXmlResources.content, EditorStyles.boldLabel);

                // Draw text area inside a scroll view.
                androidResourcesTextAreaScroll = GUILayout.BeginScrollView(androidResourcesTextAreaScroll, false, false, GUILayout.Height(EditorGUIUtility.singleLineHeight * 10));
                GameServiceProperties.androidXmlResources.property.stringValue = EditorGUILayout.TextArea(
                    GameServiceProperties.androidXmlResources.property.stringValue, 
                    GUILayout.Height(EditorGUIUtility.singleLineHeight * 100),
                    GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();

                EditorGUILayout.Space();

                // Replicate the "Setup" button within the Android GPGS setup window.
                if (GUILayout.Button("Setup Google Play Games", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
                { 
                    string webClientId = null;  // Web ClientId, not required for Games Services.
                    string folder = EM_Constants.GeneratedFolder;    // Folder to contain the generated id constant class.
                    string className = EM_Constants.AndroidGPGSConstantClassName;    // Name of the generated id constant class.
                    string resourceXmlData = GameServiceProperties.androidXmlResources.property.stringValue;    // The xml resources inputted.
                    string nearbySvcId = null;  // Nearby Connection Id, not supported by us.
                    bool requiresGooglePlus = false;    // Not required Google+ API.

                    try
                    {
                        if (GPGSUtil.LooksLikeValidPackageName(className))
                        {
                            SetupAndroidGPGS(webClientId, folder, className, resourceXmlData, nearbySvcId, requiresGooglePlus);
                        }
                    }
                    catch (System.Exception e)
                    {
                        GPGSUtil.Alert(
                            GPGSStrings.Error,
                            "Invalid classname: " + e.Message);
                    }
                }
                #endif

                #if !UNITY_ANDROID || (UNITY_ANDROID && EM_GPGS)
                // Auto-init config
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("AUTO-INIT CONFIG", EditorStyles.boldLabel);
                GameServiceProperties.autoInit.property.boolValue = EditorGUILayout.Toggle(GameServiceProperties.autoInit.content, GameServiceProperties.autoInit.property.boolValue);

                EditorGUI.BeginDisabledGroup(!GameServiceProperties.autoInit.property.boolValue);
                GameServiceProperties.autoInitDelay.property.floatValue = EditorGUILayout.FloatField(GameServiceProperties.autoInitDelay.content, GameServiceProperties.autoInitDelay.property.floatValue);
                EditorGUI.EndDisabledGroup();

                GameServiceProperties.androidMaxLoginRequest.property.intValue = EditorGUILayout.IntField(GameServiceProperties.androidMaxLoginRequest.content, GameServiceProperties.androidMaxLoginRequest.property.intValue);
                if (!GameServiceProperties.autoInit.property.boolValue)
                {
                    EditorGUILayout.Space();
                    EditorGUILayout.HelpBox(GameServiceManualInitInstruction, MessageType.Info);
                }

                // Leaderboard setup.
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("LEADERBOARD SETUP", EditorStyles.boldLabel);
                DrawGameServiceItemArray("Leaderboard", GameServiceProperties.leaderboards, ref isLeadeboardsFoldout);

                // Achievement setup.
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("ACHIEVEMENT SETUP", EditorStyles.boldLabel);
                DrawGameServiceItemArray("Achievement", GameServiceProperties.achievements, ref isAchievementsFoldout);

                // Constant generation.
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("CONSTANTS CLASS GENERATION", EditorStyles.boldLabel);
                EditorGUILayout.HelpBox(GameServiceConstantGenerationIntro, MessageType.None);

                EditorGUILayout.Space();
                if (GUILayout.Button("Generate Constants Class", GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
                {
                    GenerateGameServiceConstants();
                }
                #endif
            }

            EditorGUILayout.EndVertical();
        }

        #if UNITY_ANDROID && EM_GPGS
        // Replicate the "DoSetup" method of the GPGSAndroidSetupUI class.
        void SetupAndroidGPGS(string webClientId, string folder, string className, string resourceXmlData, string nearbySvcId, bool requiresGooglePlus)
        {           
            // Create the folder to store the generated cs file if it doesn't exist.
            SgLib.Editor.FileIO.EnsureFolderExists(folder);

            if (GPGSAndroidSetupUI.PerformSetup(webClientId, folder, className, resourceXmlData, nearbySvcId, requiresGooglePlus))
            {
                GPGSAndroidSetupUI.CheckBundleId();

                EditorUtility.DisplayDialog(
                    GPGSStrings.Success,
                    GPGSStrings.AndroidSetup.SetupComplete,
                    GPGSStrings.Ok);

                GPGSProjectSettings.Instance.Set(GPGSUtil.ANDROIDSETUPDONEKEY, true);
            }
            else
            {
                GPGSUtil.Alert(
                    GPGSStrings.Error,
                    "Invalid or missing XML resource data.  Make sure the data is" +
                    " valid and contains the app_id element");
            }
        }
        #endif

        // Generate a static class containing constants of leaderboard and achievement names.
        void GenerateGameServiceConstants()
        {           
            // First create a hashtable containing all the names to be stored as constants.
            SerializedProperty ldbProp = GameServiceProperties.leaderboards.property;
            SerializedProperty acmProp = GameServiceProperties.achievements.property;

            // First check if there're duplicate names.
            string duplicateLdbName = EM_EditorUtil.FindDuplicateNameInArrayProperty(ldbProp, GameServiceItem_NameProperty);
            if (!string.IsNullOrEmpty(duplicateLdbName))
            {
                EM_EditorUtil.Alert("Error: Duplicate Names", "Found duplicate leaderboard name of \"" + duplicateLdbName + "\".");
                return;
            }

            string duplicateAcmName = EM_EditorUtil.FindDuplicateNameInArrayProperty(acmProp, GameServiceItem_NameProperty);
            if (!string.IsNullOrEmpty(duplicateAcmName))
            {
                EM_EditorUtil.Alert("Error: Duplicate Names", "Found duplicate achievement name of \"" + duplicateAcmName + "\".");
                return;
            }

            // Proceed with adding resource keys.
            Hashtable resourceKeys = new Hashtable();

            // Add the leaderboard names.
            for (int i = 0; i < ldbProp.arraySize; i++)
            {
                SerializedProperty element = ldbProp.GetArrayElementAtIndex(i);
                string name = element.FindPropertyRelative(GameServiceItem_NameProperty).stringValue;

                // Ignore all items with an empty name.
                if (!string.IsNullOrEmpty(name))
                {
                    string key = "Leaderboard_" + name;
                    resourceKeys.Add(key, name);
                }
            }

            // Add the achievement names.
            for (int j = 0; j < acmProp.arraySize; j++)
            {
                SerializedProperty element = acmProp.GetArrayElementAtIndex(j);
                string name = element.FindPropertyRelative(GameServiceItem_NameProperty).stringValue;

                // Ignore all items with an empty name.
                if (!string.IsNullOrEmpty(name))
                {
                    string key = "Achievement_" + name;
                    resourceKeys.Add(key, name);
                }
            }

            if (resourceKeys.Count > 0)
            {
                // Now build the class.
                EM_EditorUtil.GenerateConstantsClass(
                    EM_Constants.GeneratedFolder,
                    EM_Constants.RootNameSpace + "." + EM_Constants.GameServiceConstantsClassName,
                    resourceKeys,
                    true
                );
            }
            else
            {
                EM_EditorUtil.Alert("Constants Class Generation", "Please fill in required information for all leaderboards and achievements.");
            }
        }

        // Draw the array of leaderboards or achievements inside a foldout and the relevant buttons.
        void DrawGameServiceItemArray(string itemType, EMProperty myProp, ref bool isFoldout)
        {
            if (myProp.property.arraySize > 0)
            { 
                EditorGUI.indentLevel++;
                isFoldout = EditorGUILayout.Foldout(isFoldout, myProp.property.arraySize + " " + myProp.content.text);
                EditorGUI.indentLevel--;

                if (isFoldout)
                {
                    //Prepare a string array of Android GPGPS ids to display in the leaderboards and achievements.
                    string[] gpgsIds = new string[gpgsIdDict.Count + 1];
                    gpgsIds[0] = EM_Constants.NoneSymbol;
                    gpgsIdDict.Keys.CopyTo(gpgsIds, 1);

                    // Index of the element on which buttons are clicked.
                    int deleteIndex = -1;
                    int moveUpIndex = -1;
                    int moveDownIndex = -1;

                    for (int i = 0; i < myProp.property.arraySize; i++)
                    {
                        SerializedProperty element = myProp.property.GetArrayElementAtIndex(i);
                        bool isDeleted = false;
                        bool isMovedUp = false;
                        bool isMovedDown = false;

                        EditorGUILayout.Space();
                        DrawGameServiceItem(
                            itemType,
                            element,
                            gpgsIds,
                            ref isDeleted,
                            ref isMovedUp,
                            ref isMovedDown,
                            i > 0,
                            i < myProp.property.arraySize - 1
                        );

                        if (isDeleted)
                            deleteIndex = i;
                        if (isMovedUp)
                            moveUpIndex = i;
                        if (isMovedDown)
                            moveDownIndex = i;
                    }

                    // Delete.
                    if (deleteIndex >= 0)
                    {
                        myProp.property.DeleteArrayElementAtIndex(deleteIndex);
                    }

                    // Move up.
                    if (moveUpIndex > 0)
                    {
                        myProp.property.MoveArrayElement(moveUpIndex, moveUpIndex - 1);
                    }

                    // Move down.
                    if (moveDownIndex >= 0 && moveDownIndex < myProp.property.arraySize - 1)
                    {
                        myProp.property.MoveArrayElement(moveDownIndex, moveDownIndex + 1);
                    }

                    // Detect duplicate names.
                    string duplicateName = EM_EditorUtil.FindDuplicateNameInArrayProperty(myProp.property, GameServiceItem_NameProperty);
                    if (!string.IsNullOrEmpty(duplicateName))
                    {
                        EditorGUILayout.Space();
                        EditorGUILayout.HelpBox("Found duplicate name of \"" + duplicateName + "\".", MessageType.Warning);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No " + itemType + " added.", MessageType.None);
            }

            EditorGUILayout.Space();
            if (GUILayout.Button("Add New " + itemType, GUILayout.Height(EM_GUIStyleManager.buttonHeight)))
            {
                // Add new leaderboard.
                AddNewGameServiceItem(myProp.property);

                // Open the foldout if it's closed.
                isFoldout = true;
            }
        }

        // Draw leaderboard or achievement item.
        void DrawGameServiceItem(string label, SerializedProperty property, string[] gpgsIds, ref bool isDeleteButton, ref bool isMoveUpButton, ref bool isMoveDownButton, bool allowMoveUp = true, bool allowMoveDown = true)
        {
            SerializedProperty name = property.FindPropertyRelative(GameServiceItem_NameProperty);
            SerializedProperty iosId = property.FindPropertyRelative(GameServiceItem_IOSIdProperty);
            SerializedProperty androidId = property.FindPropertyRelative(GameServiceItem_AndroidIdProperty);

            EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(EM_GUIStyleManager.GetCustomStyle("Item Box"));

            EditorGUILayout.LabelField(string.IsNullOrEmpty(name.stringValue) ? "New " + label : name.stringValue, EditorStyles.boldLabel);
            name.stringValue = EditorGUILayout.TextField("Name", name.stringValue);
            iosId.stringValue = EditorGUILayout.TextField("iOS Id", iosId.stringValue);
            // For Android Id, display a popup of Android leaderboards & achievements for the user to select
            // then assign its associated id to the property.
            EditorGUI.BeginChangeCheck();
            int currentIndex = Mathf.Max(System.Array.IndexOf(gpgsIds, GetNameFromId(gpgsIdDict, androidId.stringValue)), 0);                           
            int newIndex = EditorGUILayout.Popup("Android Id", currentIndex, gpgsIds);
            if (EditorGUI.EndChangeCheck())
            {
                // Position 0 is [None].
                if (newIndex == 0)
                {
                    androidId.stringValue = string.Empty;
                }
                else
                {
                    // Record the new android Id.
                    string newName = gpgsIds[newIndex];
                    androidId.stringValue = gpgsIdDict[newName];
                }
            }

            EditorGUILayout.EndVertical();

            EditorGUILayout.BeginVertical(EM_GUIStyleManager.GetCustomStyle("Tool Box"), GUILayout.Width(EM_GUIStyleManager.toolboxWidth), GUILayout.Height(EM_GUIStyleManager.toolboxHeight));

            // Move up button.
            EditorGUI.BeginDisabledGroup(!allowMoveUp);
            if (GUILayout.Button(EM_Constants.UpSymbol, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                isMoveUpButton = true;
            }
            EditorGUI.EndDisabledGroup();

            // Delete button.
            if (GUILayout.Button(EM_Constants.DeleteSymbol, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                // DeleteArrayElementAtIndex seems working fine even while iterating
                // through the array, but it's still a better idea to move it outside the loop.
                isDeleteButton = true;
            }

            // Move down button.
            EditorGUI.BeginDisabledGroup(!allowMoveDown);
            if (GUILayout.Button(EM_Constants.DownSymbol, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true)))
            {
                isMoveDownButton = true;
            }
            EditorGUI.EndDisabledGroup();

            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        void AddNewGameServiceItem(SerializedProperty property)
        {
            if (property.isArray)
            {
                property.InsertArrayElementAtIndex(property.arraySize);

                // Init values.
                SerializedProperty newProp = property.GetArrayElementAtIndex(property.arraySize - 1);
                SerializedProperty name = newProp.FindPropertyRelative(GameServiceItem_NameProperty);
                SerializedProperty iosId = newProp.FindPropertyRelative(GameServiceItem_IOSIdProperty);
                SerializedProperty androidId = newProp.FindPropertyRelative(GameServiceItem_AndroidIdProperty);
                name.stringValue = string.Empty;
                iosId.stringValue = string.Empty;
                androidId.stringValue = string.Empty;
            }
        }

        // Get leaderboard or achievement name from its id.
        string GetNameFromId(Dictionary<string, string> dict, string val)
        {
            foreach (KeyValuePair<string, string> entry in dict)
            {
                if (entry.Value.Equals(val))
                {
                    return entry.Key;
                }
            }

            return null;
        }
    }
}

