using System.Collections.Generic;
using UnityEngine;
#if UNITY_WEBGL && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

namespace RGN.Modules.Telegram
{
    internal static class TelegramJavascriptBridge
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        [DllImport("__Internal")]
        private static extern string PLAY_GetInitParamsJs();

        [DllImport("__Internal")]
        private static extern void PLAY_OpenLinkJs(string url);

        [DllImport("__Internal")]
        private static extern void PLAY_OpenTelegramLinkJs(string url);
#endif

        public static TelegramInitParams GetInitParams()
        {
            try
            {
                string rawInitParams = "{}";
#if UNITY_WEBGL && !UNITY_EDITOR
                rawInitParams = PLAY_GetInitParamsJs();
#endif
                IDictionary<string, object> initParamsJson = RGNCoreBuilder.I.Dependencies.Json
                    .FromJsonAsDictionary(rawInitParams);

                TelegramInitParams initParams = new TelegramInitParams();

                if (initParamsJson.TryGetValue("tgWebAppData", out object rawTgWebAppData))
                {
                    initParams.AppData = GetAppData((IDictionary<string, object>)rawTgWebAppData);
                }

                if (initParamsJson.TryGetValue("tgWebAppVersion", out object appVersion))
                {
                    initParams.AppVersion = (string)appVersion;
                }

                if (initParamsJson.TryGetValue("tgWebAppPlatform", out object appPlatform))
                {
                    initParams.AppPlatform = (string)appPlatform;
                }

                return initParams;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TelegramSdkJs]: Failed to get telegram's init params: {e.Message}");
                return null;
            }
        }

        private static TelegramAppData GetAppData(IDictionary<string, object> appDataJson)
        {
            var appData = new TelegramAppData();
            try
            {
                if (appDataJson.TryGetValue("user", out object rawUser))
                {
                    var userJson = (IDictionary<string, object>)rawUser;
                    appData.User = new TelegramUserData();

                    if (userJson.TryGetValue("id", out object id))
                    {
                        appData.User.Id = System.Convert.ToInt64(id);
                    }

                    if (userJson.TryGetValue("first_name", out object firstName))
                    {
                        appData.User.FirstName = (string)firstName;
                    }

                    if (userJson.TryGetValue("last_name", out object lastName))
                    {
                        appData.User.LastName = (string)lastName;
                    }

                    if (userJson.TryGetValue("username", out object userName))
                    {
                        appData.User.UserName = (string)userName;
                    }

                    if (userJson.TryGetValue("language_code", out object languageCode))
                    {
                        appData.User.LanguageCode = (string)languageCode;
                    }

                    if (userJson.TryGetValue("allows_write_to_pm", out object allowsWriteToPm))
                    {
                        appData.User.AllowsWriteToPm = System.Convert.ToBoolean(allowsWriteToPm);
                    }

                    if (userJson.TryGetValue("photo_url", out object photoUrl))
                    {
                        appData.User.PhotoUrl = (string)photoUrl;
                    }
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TelegramSdkJs]: Failed to parse telegram's app data: {e.Message}");
            }
            return appData;
        }

        public static void OpenLink(string url)
        {
            try
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                PLAY_OpenLinkJs(url);
#else
                Application.OpenURL(url);
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TelegramSdkJs]: Failed to open link: {e.Message}");
            }
        }

        public static void OpenTelegramLink(string url)
        {
            try
            {
#if UNITY_WEBGL && !UNITY_EDITOR
                PLAY_OpenTelegramLinkJs(url);
#else
                Application.OpenURL(url);
#endif
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[TelegramSdkJs]: Failed to open telegram link: {e.Message}");
            }
        }
    }
}
