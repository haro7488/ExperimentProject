using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;
using SgLib.UI;

namespace EasyMobile.Demo
{
    public class GameServiceDemo : MonoBehaviour
    {
        public GameObject curtain;
        public GameObject isAutoInitInfo;
        public GameObject isInitializedInfo;
        public Text selectedAchievementInfo;
        public Text selectedLeaderboardInfo;
        public InputField scoreInput;
        public DemoUtils demoUtils;
        public GameObject scrollableListPrefab;

        Achievement selectedAchievement;
        Leaderboard selectedLeaderboard;

        void Start()
        {
            curtain.SetActive(!EM_Settings.IsGameServiceModuleEnable);
        }

        void Update()
        {
            // Check if autoInit is on.
            if (EM_Settings.GameService.IsAutoInit)
            {
                demoUtils.DisplayBool(isAutoInitInfo, true, "Auto Initialization: ON");
            }
            else
            {
                demoUtils.DisplayBool(isAutoInitInfo, false, "Auto Initialization: OFF");
            }

            // Check if the module is initalized.
            if (GameServiceManager.IsInitialized())
            {
                demoUtils.DisplayBool(isInitializedInfo, true, "isInitialized: TRUE");  
            }
            else
            {
                demoUtils.DisplayBool(isInitializedInfo, false, "isInitialized: FALSE");
            }
        }

        public void Init()
        {
            if (GameServiceManager.IsInitialized())
            {
                MobileNativeUI.Alert("Alert", "The module is already initialized.");
            }
            else
            {
                GameServiceManager.Init();
            }
        }

        public void ShowLeaderboardUI()
        {
            if (GameServiceManager.IsInitialized())
            {
                GameServiceManager.ShowLeaderboardUI();
            }
            else
            {
                #if UNITY_ANDROID
                GameServiceManager.Init();
                #elif UNITY_IOS
                MobileNativeUI.Alert("Service Unavailable", "The user is not logged in.");
                #else
                Debug.Log("Cannot show leaderboards: platform not supported.");
                #endif
            }
        }

        public void ShowAchievementUI()
        {            
            if (GameServiceManager.IsInitialized())
            {
                GameServiceManager.ShowAchievementsUI();
            }
            else
            {
                #if UNITY_ANDROID
                GameServiceManager.Init();
                #elif UNITY_IOS
                MobileNativeUI.Alert("Service Unavailable", "The user is not logged in.");
                #else
                Debug.Log("Cannot show achievements: platform not supported.");
                #endif
            }
        }

        public void SelectAchievement()
        {
            var achievements = EM_Settings.GameService.Achievements;

            if (achievements == null || achievements.Length == 0)
            {
                MobileNativeUI.Alert("Alert", "You haven't added any achievement. Please go to Window > Easy Mobile > Settings and add some.");
                selectedAchievement = null;
                return;
            }
                                
            var items = new Dictionary<string, string>();

            foreach (Achievement acm in achievements)
            {
                items.Add(acm.Name, acm.Id);
            }

            var scrollableList = ScrollableList.Create(scrollableListPrefab, "ACHIEVEMENTS", items);
            scrollableList.ItemSelected += OnAchievementSelected;
        }

        public void UnlockAchievement()
        {
            if (!GameServiceManager.IsInitialized())
            {
                MobileNativeUI.Alert("Alert", "You need to initialize the module first.");
                return;
            }

            if (selectedAchievement != null)
            {
                GameServiceManager.UnlockAchievement(selectedAchievement.Name);
            }
            else
            {
                MobileNativeUI.Alert("Alert", "Please select an achievement to unlock.");
            }
        }

        public void SelectLeaderboard()
        {
            var leaderboards = EM_Settings.GameService.Leaderboards;

            if (leaderboards == null || leaderboards.Length == 0)
            {
                MobileNativeUI.Alert("Alert", "You haven't added any leaderboard. Please go to Window > Easy Mobile > Settings and add some.");
                selectedAchievement = null;
                return;
            }

            var items = new Dictionary<string, string>();

            foreach (Leaderboard ldb in leaderboards)
            {
                items.Add(ldb.Name, ldb.Id);
            }

            var scrollableList = ScrollableList.Create(scrollableListPrefab, "LEADERBOARDS", items);
            scrollableList.ItemSelected += OnLeaderboardSelected;
        }

        public void ReportScore()
        {
            if (!GameServiceManager.IsInitialized())
            {
                MobileNativeUI.Alert("Alert", "You need to initialize the module first.");
                return;
            }

            if (selectedLeaderboard == null)
            {
                MobileNativeUI.Alert("Alert", "Please select a leaderboard to report score to.");
            }
            else
            {
                if (string.IsNullOrEmpty(scoreInput.text))
                {
                    MobileNativeUI.Alert("Alert", "Please enter a score to report.");
                }
                else
                {
                    int score = System.Convert.ToInt32(scoreInput.text);
                    GameServiceManager.ReportScore(score, selectedLeaderboard.Name);
                    MobileNativeUI.Alert("Alert", "Reported score " + score + " to leaderboard \"" + selectedLeaderboard.Name + "\".");
                }
            }
        }

        public void LoadLocalUserScore()
        {
            if (!GameServiceManager.IsInitialized())
            {
                MobileNativeUI.Alert("Alert", "You need to initialize the module first.");
                return;
            }

            if (selectedLeaderboard == null)
            {
                MobileNativeUI.Alert("Alert", "Please select a leaderboard to load score from.");
            }
            else
            {
                GameServiceManager.LoadLocalUserScore(selectedLeaderboard.Name, OnLocalUserScoreLoaded);
            }
        }

        public void LoadFriends()
        {
            if (!GameServiceManager.IsInitialized())
            {
                MobileNativeUI.Alert("Alert", "You need to initialize the module first.");
                return;
            }

            GameServiceManager.LoadFriends(OnFriendsLoaded);
        }

        void OnAchievementSelected(ScrollableList list, string title, string subtitle)
        {
            list.ItemSelected -= OnAchievementSelected;
            selectedAchievement = GameServiceManager.GetAchievementByName(title);
            selectedAchievementInfo.text = "Selected achievement: " + title;
        }

        void OnLeaderboardSelected(ScrollableList list, string title, string subtitle)
        {
            list.ItemSelected -= OnLeaderboardSelected;
            selectedLeaderboard = GameServiceManager.GetLeaderboardByName(title);
            selectedLeaderboardInfo.text = "Selected leaderboard: " + title;
        }

        void OnLocalUserScoreLoaded(string leaderboardName, IScore score)
        {
            if (score != null)
            {
                MobileNativeUI.Alert("Local User Score Loaded", "Your score on leaderboard \"" + leaderboardName + "\" is " + score.value);
            }
            else
            {
                MobileNativeUI.Alert("Local User Score Load Failed", "You don't have any score reported to leaderboard \"" + leaderboardName + "\".");
            }
        }

        void OnFriendsLoaded(IUserProfile[] friends)
        {
            if (friends.Length > 0)
            {
                var items = new Dictionary<string, string>();

                foreach (IUserProfile user in friends)
                {
                    items.Add(user.userName, user.id);
                }

                ScrollableList.Create(scrollableListPrefab, "FRIEND LIST", items);
            }
            else
            {
                MobileNativeUI.Alert("Load Friends Result", "Couldn't find any friend.");
            }
        }
    }
}

