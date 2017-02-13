using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace EasyMobile.Demo
{
    public class DemoHomeController : MonoBehaviour
    {
        void OnEnable()
        {
            NotificationManager.NotificationOpened += NotificationManager_NotificationOpened;
        }

        void OnDisable()
        {
            NotificationManager.NotificationOpened -= NotificationManager_NotificationOpened;
        }

        public void AdvertisingDemo()
        {
            SceneManager.LoadScene("AdvertisingDemo");
        }

        public void InAppPurchaseDemo()
        {
            SceneManager.LoadScene("InAppPurchasingDemo");
        }

        public void GameServiceDemo()
        {
            SceneManager.LoadScene("GameServiceDemo");
        }

        public void MobileNativeUIExample()
        {
            SceneManager.LoadScene("MobileNativeUIDemo");
        }

        public void MobileNativeShareExample()
        {
            SceneManager.LoadScene("MobileNativeShareDemo");
        }

        public void Restart()
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        void Update()
        {
            #if UNITY_ANDROID
            if (Input.GetKeyUp(KeyCode.Escape))
            {   
                // Ask if user wants to exit
                MobileNativeAlert alert = MobileNativeUI.ShowTwoButtonAlert("Exit App",
                                              "Do you want to exit?",
                                              "Yes", 
                                              "No");

                if (alert != null)
                    alert.OnComplete += delegate (int button)
                    { 
                        if (button == 0)
                            Application.Quit();
                    };
            }

            #endif
        }

        // Push notification opened handler
        void NotificationManager_NotificationOpened(string message, string actionID, Dictionary<string, object> additionalData, bool isAppInFocus)
        {
            Debug.Log("Push notification received!");
            Debug.Log("Message: " + message);
            Debug.Log("isAppInFocus: " + isAppInFocus.ToString());

            if (additionalData != null)
            {
                Debug.Log("AdditionalData:");
                foreach (KeyValuePair<string, object> item in additionalData)
                {
                    Debug.Log("Key: " + item.Key + " - Value: " + item.Value.ToString());
                }

                if (additionalData.ContainsKey("newUpdate"))
                {
                    if (!isAppInFocus)
                    {
                        Debug.Log("New update available! Should open the update page now.");
                    }
                }
            }
        }
    }
}

