using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SocialPlatforms;

#if UNITY_IOS
using UnityEngine.SocialPlatforms.GameCenter;
#endif

#if UNITY_ANDROID && EM_GPGS
using GooglePlayGames;
#endif

namespace EasyMobile
{
    public class GameServiceManager : MonoBehaviour
    {
        public static GameServiceManager Instance { get; private set; }

        public static event Action UserAuthenticated = delegate {};

        /// <summary>
        /// The local or currently logged in user.
        /// Returns null if the user has not logged in.
        /// </summary>
        /// <value>The local user.</value>
        public static ILocalUser LocalUser
        {
            get
            {
                if (IsInitialized())
                {
                    return Social.localUser;
                }
                else
                {
                    return null;
                }
            }
        }

        struct LoadScoreRequest
        {
            public bool useLeaderboardDefault;
            public bool loadLocalUserScore;
            public string leaderboardName;
            public string leaderboardId;
            public int fromRank;
            public int scoreCount;
            public TimeScope timeScope;
            public UserScope userScope;
            public Action<string, IScore[]> callback;
        }

        private static bool isLoadingScore = false;
        private static List<LoadScoreRequest> loadScoreRequests = new List<LoadScoreRequest>();
            
        #if UNITY_ANDROID
        private const string ANDROID_LOGIN_REQUEST_NUMBER_PPKEY = "SGLIB_ANDROID_LOGIN_REQUEST_NUMBER";
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
            // Init the module if automatic init is enabled.
            if (EM_Settings.GameService.IsAutoInit)
            {
                StartCoroutine(CRAutoInit(EM_Settings.GameService.AutoInitDelay));
            }   
        }

        IEnumerator CRAutoInit(float delay)
        {
            yield return new WaitForSeconds(delay);
            ManagedInit();
        }

        /// <summary>
        /// Internally calls the Init() method. If the user hasn't logged in
        /// to the service, a login UI will popup. Otherwise, it will initialize silently.
        /// On iOS, the OS automatically shows the login popup when the app gets focus for the first 3 times.
        /// Subsequent init calls will be ignored.
        /// On Android, if the user dismisses the login popup for a number of times determined
        /// by AndroidMaxLoginRequests, we'll stop showing it (all subsequent init calls will be ignored).
        /// </summary>
        /// <param name="delay">Delay.</param>
        public static void ManagedInit()
        {
            #if UNITY_IOS
            if (!IsInitialized())
                Init();
            #elif UNITY_ANDROID
            if (!IsInitialized())
            {
                int loginRequestNumber = PlayerPrefs.GetInt(ANDROID_LOGIN_REQUEST_NUMBER_PPKEY, 0);

                if (loginRequestNumber < EM_Settings.GameService.AndroidMaxLoginRequests || EM_Settings.GameService.AndroidMaxLoginRequests <= 0)
                {
                    loginRequestNumber++;
                    PlayerPrefs.SetInt(ANDROID_LOGIN_REQUEST_NUMBER_PPKEY, loginRequestNumber);
                    PlayerPrefs.Save();
                    Init();
                }
                else
                {
                    if (Debug.isDebugBuild)
                        Debug.Log("Game Service Init FAILED: AndroidMaxLoginRequests exceeded. Requests attempted: " + loginRequestNumber);
                }
            }
            #endif
        }

        /// <summary>
        /// Initializes the service. This is required before any other actions can be done e.g reporting scores.
        /// During the initialization process, a login popup will show up if the user hasn't logged in, otherwise
        /// the process will carry on silently.
        /// Note that on iOS, the login popup will show up automatically when the app gets focus for the first 3 times
        /// while subsequent authentication calls will be ignored.
        /// </summary>
        public static void Init()
        {
            // Authenticate and register a ProcessAuthentication callback
            // This call needs to be made before we can proceed to other calls in the Social API
            #if UNITY_IOS
            GameCenterPlatform.ShowDefaultAchievementCompletionBanner(true);
            Social.localUser.Authenticate(ProcessAuthentication);

            #elif UNITY_ANDROID && EM_GPGS
            if (Social.Active != PlayGamesPlatform.Instance)
            {
                PlayGamesPlatform.Activate();
            }
            PlayGamesPlatform.DebugLogEnabled = EM_Settings.GameService.IsGPGSDebug;

            Social.localUser.Authenticate(ProcessAuthentication);
            #elif UNITY_ANDROID && !EM_GPGS
            if (Debug.isDebugBuild)
                Debug.LogError("SDK missing. Please import Google Play Games plugin for Unity.");
            #else
            if (Debug.isDebugBuild)
                Debug.Log("Init FAILED: platform not supported.");
            #endif
        }

        /// <summary>
        /// Determines whether the service is initialized (user is authenticated) and ready to use.
        /// </summary>
        /// <returns><c>true</c> if initialized; otherwise, <c>false</c>.</returns>
        public static bool IsInitialized()
        {
            return Social.localUser.authenticated;
        }

        /// <summary>
        /// Shows the leaderboard UI.
        /// </summary>
        public static void ShowLeaderboardUI()
        {
            if (IsInitialized())
                Social.ShowLeaderboardUI();
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("ShowLeaderboardUI FAILED: user is not logged in.");
            }
        }

        /// <summary>
        /// Shows the achievements UI.
        /// </summary>
        public static void ShowAchievementsUI()
        {
            if (IsInitialized())
                Social.ShowAchievementsUI();
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("ShowAchievementsUI FAILED: user is not logged in.");                    
            }
        }

        /// <summary>
        /// Reports the given score to the leaderboard with the specified name.
        /// </summary>
        /// <param name="score">Score.</param>
        /// <param name="leaderboardName">Leaderboard name.</param>
        public static void ReportScore(long score, string leaderboardName)
        {
            Leaderboard ldb = GetLeaderboardByName(leaderboardName);

            if (ldb != null)
            {
                DoReportScore(score, ldb.Id);
            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("ReportScore FAILED: unknown leaderboard name.");
            }
        }

        /// <summary>
        /// Reveals the hidden achievement with the specified name.
        /// </summary>
        /// <param name="achievementName">Achievement name.</param>
        public static void RevealAchievement(string achievementName)
        {
            Achievement acm = GetAchievementByName(achievementName);

            if (acm != null)
            {
                DoReportAchievementProgress(acm.Id, 0.0f);
            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("RevealAchievement FAILED: unknown achievement name.");
            }
        }

        /// <summary>
        /// Unlocks the achievement with the specified name.
        /// </summary>
        /// <param name="achievementName">Achievement name.</param>
        public static void UnlockAchievement(string achievementName)
        {
            Achievement acm = GetAchievementByName(achievementName);

            if (acm != null)
            {
                DoReportAchievementProgress(acm.Id, 100.0f);
            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("UnlockAchievement FAILED: unknown achievement name.");
            }
        }

        /// <summary>
        /// Reports the progress of the incremental achievement with the specified name.
        /// </summary>
        /// <param name="achievementName">Achievement name.</param>
        /// <param name="progress">Progress.</param>
        public static void ReportAchievementProgress(string achievementName, double progress)
        {
            Achievement acm = GetAchievementByName(achievementName);

            if (acm != null)
            {
                DoReportAchievementProgress(acm.Id, progress);
            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("ReportAchievementProgress FAILED: unknown achievement name.");
            }
        }

        /// <summary>
        /// Loads all friends of the authenticated users.
        /// Internally it will populate the LocalUsers.friends array and invoke the 
        /// callback with this array if the loading succeeded. 
        /// If the loading failed, the callback will be invoked with an empty array.
        /// If the LocalUsers.friends array is already populated then the callback will be invoked immediately
        /// without any loading request being made. 
        /// </summary>
        /// <param name="callback">Callback.</param>
        public static void LoadFriends(Action<IUserProfile[]> callback)
        {
            if (!IsInitialized())
            {
                if (Debug.isDebugBuild)
                    Debug.Log("LoadFriends FAILED: user is not logged in.");

                return;
            }

            if (Social.localUser.friends != null && Social.localUser.friends.Length > 0)
            {
                callback(Social.localUser.friends);
            }
            else
            {
                Social.localUser.LoadFriends(success =>
                    {
                        if (success)
                        {
                            callback(Social.localUser.friends);
                        }
                        else
                        {
                            if (Debug.isDebugBuild)
                                Debug.Log("LoadFriends FAILED: could not load friends.");

                            callback(new IUserProfile[0]);
                        }
                    });
            }
        }

        /// <summary>
        /// Load the user profiles associated with the given array of user IDs.
        /// </summary>
        /// <param name="userIds">User identifiers.</param>
        /// <param name="callback">Callback.</param>
        public static void LoadUsers(string[] userIds, Action<IUserProfile[]> callback)
        {
            if (!IsInitialized())
            {
                if (Debug.isDebugBuild)
                    Debug.Log("LoadUsers FAILED: user is not logged in.");

                return;
            }

            Social.LoadUsers(userIds, callback);
        }

        /// <summary>
        /// Loads a set of scores using the default parameters of the given leaderboard.
        /// This returns the 25 scores that are around the local player's score
        /// on the Global userScope and AllTime timeScope.
        /// Note that each load score request is added into a queue and the
        /// next request is called after the callback of previous request has been invoked.
        /// </summary>
        /// <param name="leaderboardName">Leaderboard name.</param>
        /// <param name="callback">Callback receives the leaderboard name and an array of loaded scores.</param>
        public static void LoadScores(string leaderboardName, Action<string, IScore[]> callback)
        {
            if (!IsInitialized())
            {
                if (Debug.isDebugBuild)
                    Debug.Log("LoadScores FAILED: user is not logged in.");

                return;
            }

            Leaderboard ldb = GetLeaderboardByName(leaderboardName);

            if (ldb == null)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("LoadScores FAILED: unknown leaderboard name.");

                return;
            }

            // Create new request
            LoadScoreRequest request = new LoadScoreRequest();
            request.leaderboardName = ldb.Name;
            request.leaderboardId = ldb.Id;
            request.callback = callback;
            request.useLeaderboardDefault = true;
            request.loadLocalUserScore = false;

            // Add request to the queue
            loadScoreRequests.Add(request);

            DoNextLoadScoreRequest();
        }

        /// <summary>
        /// Loads the set of scores from the specified leaderboard within the specified timeScope and userScope.
        /// The range is defined by starting position fromRank and the number of scores to retrieve scoreCount.
        /// Note that each load score request is added into a queue and the
        /// next request is called after the callback of previous request has been invoked.
        /// </summary>
        /// <param name="leaderboardName">Leaderboard name.</param>
        /// <param name="fromRank">The rank of the first score to load.</param>
        /// <param name="scoreCount">The total number of scores to load.</param>
        /// <param name="timeScope">Time scope.</param>
        /// <param name="userScope">User scope.</param>
        /// <param name="callback">Callback receives the leaderboard name and an array of loaded scores.</param>
        public static void LoadScores(string leaderboardName, int fromRank, int scoreCount, TimeScope timeScope, UserScope userScope, Action<string, IScore[]> callback)
        {
            // IMPORTANT: On Android, the fromRank argument is ignored and the score range always starts at 1.
            // (This is not the intended behavior according to the SocialPlatform.Range documentation, and may simply be
            // a bug of the current (0.9.34) GooglePlayPlatform implementation).
            if (!IsInitialized())
            {
                if (Debug.isDebugBuild)
                    Debug.Log("LoadScores FAILED: user is not logged in.");

                return;
            }

            Leaderboard ldb = GetLeaderboardByName(leaderboardName);

            if (ldb == null)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("LoadScores FAILED: unknown leaderboard name.");

                return;
            }

            // Create new request
            LoadScoreRequest request = new LoadScoreRequest();
            request.leaderboardName = ldb.Name;
            request.leaderboardId = ldb.Id;
            request.callback = callback;
            request.useLeaderboardDefault = false;
            request.loadLocalUserScore = false;
            request.fromRank = fromRank;
            request.scoreCount = scoreCount;
            request.timeScope = timeScope;
            request.userScope = userScope;

            // Add request to the queue
            loadScoreRequests.Add(request);

            DoNextLoadScoreRequest();
        }

        /// <summary>
        /// Loads the local user's score on the given leaderboard.
        /// Note that each load score request is added into a queue and the
        /// next request is called after the callback of previous request has been invoked.
        /// </summary>
        /// <param name="leaderboardName">Leaderboard name.</param>
        /// <param name="callback">Callback receives the leaderboard name and the loaded score.</param>
        public static void LoadLocalUserScore(string leaderboardName, Action<string, IScore> callback)
        {
            if (!IsInitialized())
            {
                if (Debug.isDebugBuild)
                    Debug.Log("LoadLocalUserScore FAILED: user is not logged in.");

                return;
            }

            Leaderboard ldb = GetLeaderboardByName(leaderboardName);

            if (ldb == null)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("LoadLocalUserScore FAILED: unknown leaderboard name.");

                return;
            }

            // Create new request
            LoadScoreRequest request = new LoadScoreRequest();
            request.leaderboardName = ldb.Name;
            request.leaderboardId = ldb.Id;
            request.callback = delegate(string ldbName, IScore[] scores)
            {
                if (scores != null)
                    callback(ldbName, scores[0]);
                else
                    callback(ldbName, null);
            };
            request.useLeaderboardDefault = false;
            request.loadLocalUserScore = true;
            request.fromRank = -1;
            request.scoreCount = -1;
            request.timeScope = TimeScope.AllTime;
            request.userScope = UserScope.FriendsOnly;

            // Add request to the queue
            loadScoreRequests.Add(request);

            DoNextLoadScoreRequest();
        }

        /// <summary>
        /// Returns a leaderboard it one with a leaderboardName was declared before within leaderboards array.
        /// </summary>
        /// <returns>The leaderboard by name.</returns>
        /// <param name="leaderboardName">Leaderboard name.</param>
        public static Leaderboard GetLeaderboardByName(string leaderboardName)
        {
            foreach (Leaderboard ldb in EM_Settings.GameService.Leaderboards)
            {
                if (ldb.Name.Equals(leaderboardName))
                    return ldb;
            }

            return null;
        }

        /// <summary>
        /// Returns an achievement it one with an achievementName was declared before within achievements array.
        /// </summary>
        /// <returns>The achievement by name.</returns>
        /// <param name="achievementName">Achievement name.</param>
        public static Achievement GetAchievementByName(string achievementName)
        {
            foreach (Achievement acm in EM_Settings.GameService.Achievements)
            {
                if (acm.Name.Equals(achievementName))
                    return acm;
            }

            return null;
        }

        #region Private methods

        static void DoReportScore(long score, string leaderboardId)
        {
            if (!IsInitialized())
            {
                if (Debug.isDebugBuild)
                    Debug.Log("DoReportScore FAILED: user is not logged in.");

                return;
            }

            if (Debug.isDebugBuild)
                Debug.Log("Reporting score: " + score);

            Social.ReportScore(
                score,
                leaderboardId,
                (bool success) =>
                {
                    if (Debug.isDebugBuild)
                        Debug.Log(success ? "Score reported successfully." : "Failed to report score.");
                }
            );
        }

        // Progress of 0.0% means reveal the achievement.
        // Progress of 100.0% means unlock the achievement.
        static void DoReportAchievementProgress(string achievementId, double progress)
        {
            if (!IsInitialized())
            {
                if (Debug.isDebugBuild)
                    Debug.Log("DoReportAchievementProgress FAILED: user is not logged in.");

                return;
            }

            if (Debug.isDebugBuild)
                Debug.Log("Reporting progress of " + progress + "% for achievement: " + achievementId);

            Social.ReportProgress(
                achievementId, 
                progress, 
                (bool success) =>
                { 
                    if (Debug.isDebugBuild)
                        Debug.Log(success ? "Successfully reported progress of " + progress + "% for achievement: " + achievementId : "Failed to report progress for achievement: " + achievementId);
                }
            );
        }

        static void DoNextLoadScoreRequest()
        {
            LoadScoreRequest request;

            if (isLoadingScore)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("DoNextLoadScoreRequest POSTPONED: is loading another request.");

                return;
            }

            if (loadScoreRequests.Count == 0)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("DoNextLoadScoreRequest DONE: no more requests in queue.");

                return;
            }

            if (Debug.isDebugBuild)
                Debug.Log("Performing next score loading request...");

            isLoadingScore = true;
            request = loadScoreRequests[0]; // fetch the next request
            loadScoreRequests.RemoveAt(0);  // then remove it from the queue

            // Now create a new leaderboard and start loading scores
            ILeaderboard ldb = Social.CreateLeaderboard();
            ldb.id = request.leaderboardId;

            if (request.useLeaderboardDefault)
            {
                // The current iOS implementation of ISocialPlatform behaves weirdly with Social.LoadScores.
                // Experiment showed that only the first score on the leaderboard was returned.
                // On Android scores were returned properly.
                // We'll have different code for the two platforms in an attempt to provide consistent behavior from the outside.
                #if UNITY_ANDROID
                // On Android, we'll use LoadScores directly from Social.
                Social.LoadScores(ldb.id, (IScore[] scores) =>
                    {
                        if (Debug.isDebugBuild)
                        {
                            Debug.Log("Successfully loaded default set of scores from leaderboard: " + ldb.id);
                            Debug.Log("Got " + scores.Length + " scores.");
                        }

                        request.callback(request.leaderboardName, scores);

                        // Load next request
                        isLoadingScore = false;
                        DoNextLoadScoreRequest();
                    });
                #elif UNITY_IOS
                // On iOS, we use LoadScores from ILeaderboard with default parameters.
                ldb.LoadScores((bool success) =>
                    {
                        if (Debug.isDebugBuild)
                        {
                            Debug.Log("Successfully loaded default set of scores from leaderboard: " + ldb.id);
                            Debug.Log("Got " + ldb.scores.Length + " scores.");
                        }

                        request.callback(request.leaderboardName, ldb.scores);

                        // Load next request
                        isLoadingScore = false;
                        DoNextLoadScoreRequest();
                    });

                #endif
            }
            else
            {
                ldb.timeScope = request.timeScope;
                ldb.userScope = request.userScope;

                if (request.fromRank > 0 && request.scoreCount > 0)
                {
                    ldb.range = new Range(request.fromRank, request.scoreCount);
                }

                ldb.LoadScores((bool success) =>
                    {    
                        if (request.loadLocalUserScore)
                        { 
                            if (Debug.isDebugBuild)
                            {
                                Debug.Log("Successfully loaded local user score from leaderboard: " + ldb.id);
                            }

                            IScore[] returnScores = new IScore[] { ldb.localUserScore };
                            request.callback(request.leaderboardName, returnScores);
                        }
                        else
                        {
                            if (Debug.isDebugBuild)
                            {
                                Debug.Log("Successfully loaded custom set of scores from leaderboard: " + ldb.id);
                                Debug.Log("Got " + ldb.scores.Length + " scores:");
                            }

                            request.callback(request.leaderboardName, ldb.scores);
                        }

                        // Load next request
                        isLoadingScore = false;
                        DoNextLoadScoreRequest();
                    });
            }
        }

        #endregion

        #region Authentication listeners

        // This function gets called when Authenticate completes
        // Note that if the operation is successful, Social.localUser will contain data from the server.
        static void ProcessAuthentication(bool success)
        {
            if (success)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("User authenticated to GameServiceManager, checking achievements...");

                // Fire event
                UserAuthenticated();

                #if UNITY_ANDROID
                // Reset login request number
                PlayerPrefs.SetInt(ANDROID_LOGIN_REQUEST_NUMBER_PPKEY, 0);
                PlayerPrefs.Save();
                #endif

                // Request loaded achievements, and register a callback for processing them
                Social.LoadAchievements(ProcessLoadedAchievements);
            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("Failed to authenticate user to GameServiceManager.");
            }
        }

        // This function gets called when the LoadAchievement call completes
        static void ProcessLoadedAchievements(IAchievement[] achievements)
        {
            if (achievements.Length == 0)
            {
                if (Debug.isDebugBuild)
                    Debug.Log("No achievements found.");
            }
            else
            {
                if (Debug.isDebugBuild)
                    Debug.Log("Got " + achievements.Length + " achievements.");
            }

        }

        #endregion
    }
}
