using UnityEngine;
using System.Collections;

namespace EasyMobile
{
    public static class MobileNativeUI
    {

        #region Alerts

        /// <summary>
        /// Shows an alert with 3 buttons.
        /// </summary>
        /// <returns>The three buttons alert.</returns>
        /// <param name="title">Title.</param>
        /// <param name="message">Message.</param>
        /// <param name="button1">Button1.</param>
        /// <param name="button2">Button2.</param>
        /// <param name="button3">Button3.</param>
        public static MobileNativeAlert ShowThreeButtonAlert(string title, string message, string button1, string button2, string button3)
        {
            return MobileNativeAlert.ShowThreeButtonAlert(title, message, button1, button2, button3);
        }

        /// <summary>
        /// Shows an alert with 2 buttons.
        /// </summary>
        /// <returns>The two buttons alert.</returns>
        /// <param name="title">Title.</param>
        /// <param name="message">Message.</param>
        /// <param name="button1">Button1.</param>
        /// <param name="button2">Button2.</param>
        public static MobileNativeAlert ShowTwoButtonAlert(string title, string message, string button1, string button2)
        {
            return MobileNativeAlert.ShowTwoButtonAlert(title, message, button1, button2);
        }

        /// <summary>
        /// Shows a one-button alert with a custom button label.
        /// </summary>
        /// <returns>The one button alert.</returns>
        /// <param name="title">Title.</param>
        /// <param name="message">Message.</param>
        /// <param name="button">Button.</param>
        public static MobileNativeAlert Alert(string title, string message, string button)
        {
            return MobileNativeAlert.ShowOneButtonAlert(title, message, button);
        }

        /// <summary>
        /// Shows a one-button alert with the default "OK" button.
        /// </summary>
        /// <returns>The alert.</returns>
        /// <param name="title">Title.</param>
        /// <param name="message">Message.</param>
        public static MobileNativeAlert Alert(string title, string message)
        {
            return MobileNativeAlert.Alert(title, message);
        }

        #endregion


        #region Android Toasts

        #if UNITY_ANDROID
        /// <summary>
        /// Shows a toast message (Android only).
        /// </summary>
        /// <param name="message">Message.</param>
        public static void ShowToast(string message)
        {   
            MobileNativeAlert.ShowToast(message);
        }
        #endif

        #endregion
    }
}