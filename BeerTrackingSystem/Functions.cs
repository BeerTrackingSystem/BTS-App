using BeerTrackingSystem.Components.Pages;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System.ComponentModel;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace BeerTrackingSystem
{
    internal class WebCall
    {
        public static async Task<(bool, string)> MakeApiCall(MultipartFormDataContent formData)
        {
            try
            {
                // Erstellen einer HttpClient-Instanz
                using HttpClient client = new();


                // URL, an die die Anfrage gesendet werden soll
                string url = Preferences.Default.Get("api_url", "Unknown URL");

                // Erstellen der Formulardaten
                var callData = formData;
                callData.Add(new StringContent(Preferences.Default.Get("api_key", "Unknown URL")), "apikey");

                var calltypeContent = formData.FirstOrDefault(p => p.Headers.ContentDisposition?.Name?.Trim('"') == "calltype");
                var callitemContent = formData.FirstOrDefault(p => p.Headers.ContentDisposition?.Name?.Trim('"') == "callitem");
                if (calltypeContent != null && callitemContent != null)
                {
                    var calltype = calltypeContent.ReadAsStringAsync().Result;
                    var callitem = callitemContent.ReadAsStringAsync().Result;
                    if (calltype == "set" || callitem == "sessionstate" || callitem == "logoff")
                    {
                        callData.Add(new StringContent(Preferences.Default.Get("sessionid", "Unknown SessionID")), "sessionid");
                        callData.Add(new StringContent(Preferences.Default.Get("user", "Unknown User")), "user");
                    }
                }


                HttpResponseMessage response = await client.PostAsync(url, callData);

                // Überprüfen der Antwort des Servers
                if (response.IsSuccessStatusCode)
                {
                    // Erfolgreiche Antwort verarbeiten
                    string responseContent = await response.Content.ReadAsStringAsync();

                    return (response.IsSuccessStatusCode, responseContent);
                }
                else
                {
                    // Fehlerbehandlung
                    string responseError = ("Fehler beim Webaufruf. Statuscode: " + response.StatusCode);
                    return (response.IsSuccessStatusCode, responseError);
                }
            }
            catch (Exception ex)
            {
                // Fehlerbehandlung
                string responseError = ("Fehler: " + ex.Message);
                return (false, responseError);
            }
        }//WebCall
    }
    internal class Popups
    {
        public static bool ShowAuthErrorPopup = false;
        public static bool ShowGetErrorPopup = false;
        public static bool ShowSetErrorPopup = false;
        public static bool ShowConfirmPopup = false;
        public static string ConfirmPopupTitle = "Confirmation";
        public static string ConfirmPopupContent = "";
        public static void OpenAuthErrorPopup()
        {
                ShowAuthErrorPopup = true;
        }
        public static void CloseAuthErrorPopup()
        {
            ShowAuthErrorPopup = false;
        }
        public static void OpenGetErrorPopup()
        {
            if (Preferences.Default.Get("api_url", "Unknown URL") != "")
            {
                ShowGetErrorPopup = true;
            }
        }

        public static void CloseGetErrorPopup()
        {
            ShowGetErrorPopup = false;
        }
        public static void OpenSetErrorPopup()
        {
            ShowSetErrorPopup = true;
        }

        public static void CloseSetErrorPopup()
        {
            ShowSetErrorPopup = false;
        }

        public static void OpenConfirmPopup()
        {
            ShowConfirmPopup = true;
        }
        public static void CloseConfirmPopup()
        {
            ConfirmPopupContent = "";
            ShowConfirmPopup = false;
        }
    }
    internal class ApiAuths
    {
        public static bool ResponseStatus;
        public static string ResponseContent = "";
        public static string ResponseError = "";
        public static async Task AuthLogoff()
        {
            var formData = new MultipartFormDataContent
                {
                    { new StringContent("auth"), "calltype" },
                    { new StringContent("logoff"), "callitem" }
                };
            _ = await WebCall.MakeApiCall(formData);

            Preferences.Default.Remove("user");
            Preferences.Default.Remove("sessionid");
            ApiGets.SessionState = false;
        }
        public static async Task AuthLogin(string user, string password)
        {
            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent("auth"), "calltype" },
                    { new StringContent("login"), "callitem" },
                    { new StringContent(user), "loginuser" },
                    { new StringContent(password), "loginpassword" }
                };
                var Response = await WebCall.MakeApiCall(formData);

                ApiAuths.ResponseStatus = Response.Item1;

                if (ApiAuths.ResponseStatus)
                {
                    Preferences.Default.Set("user", user);
                    Preferences.Default.Set("sessionid", Response.Item2);
                    ApiGets.SessionState = true;
                }
                else
                {
                    ApiAuths.ResponseError = Response.Item2;
                    Popups.OpenAuthErrorPopup();
                }
            }
            else
            {
                ApiAuths.ResponseError = "Please correct your input!";
                Popups.OpenAuthErrorPopup();
            }
        }
    }
    internal class ApiGets
    {
        public static bool ResponseStatus;
        public static bool SessionState;
        public static string ResponseContent = "";
        public static string ResponseError = "";

        public static async Task GetAllUsers()
        {
            var formData = new MultipartFormDataContent
                {
                    { new StringContent("get"), "calltype" },
                    { new StringContent("allusers"), "callitem" }
                };

            var Response = await WebCall.MakeApiCall(formData);

            ApiGets.ResponseStatus = Response.Item1;

            if (ResponseStatus)
            {
                Preferences.Default.Set("all_users", Response.Item2);
            }
            else
            {
                ApiGets.ResponseError = Response.Item2;
                Popups.OpenGetErrorPopup();
            }
        }
        public static async Task GetCurrentStock()
        {
            var formData = new MultipartFormDataContent
                {
                    { new StringContent("get"), "calltype" },
                    { new StringContent("currentstock"), "callitem" }
                };

            var Response = await WebCall.MakeApiCall(formData);

            ApiGets.ResponseStatus = Response.Item1;

            if (ResponseStatus)
            {
                Preferences.Default.Set("current_stock", Response.Item2);
                Preferences.Default.Set("last_update", DateTime.Now.ToString());
            }
            else
            {
                ApiGets.ResponseError = Response.Item2;
                Popups.OpenGetErrorPopup();
            }
        }
        public static async Task GetSessionState()
        {
            var formData = new MultipartFormDataContent
                {
                    { new StringContent("get"), "calltype" },
                    { new StringContent("sessionstate"), "callitem" }
                };

            var Response = await WebCall.MakeApiCall(formData);

            ApiGets.ResponseStatus = Response.Item1;

            if (ApiGets.ResponseStatus)
            {
                if (Response.Item2 == "1")
                {
                    ApiGets.SessionState = true;
                }
                else
                {
                    ApiGets.SessionState = false;
                }
            }
        }
        public static async Task GetCurrentStrikes()
        {
            var formData = new MultipartFormDataContent
                {
                    { new StringContent("get"), "calltype" },
                    { new StringContent("currentstrikes"), "callitem" }
                };

            var Response = await WebCall.MakeApiCall(formData);

            ApiGets.ResponseStatus = Response.Item1;

            if (ResponseStatus)
            {
                Preferences.Default.Set("current_strikes", Response.Item2);
                Preferences.Default.Set("last_update", DateTime.Now.ToString());
            }
            else
            {
                ApiGets.ResponseError = Response.Item2;
                Popups.OpenGetErrorPopup();
            }
        }
        public static async Task GetMisc()
        {
            var formData = new MultipartFormDataContent
                {
                    { new StringContent("get"), "calltype" },
                    { new StringContent("misc"), "callitem" }
                };

            var Response = await WebCall.MakeApiCall(formData);

            ApiGets.ResponseStatus = Response.Item1;

            if (ResponseStatus)
            {
                ResponseContent = Response.Item2;
                var deserializedData = JsonSerializer.Deserialize<List<Dictionary<string, object>>>(Response.Item2);
                if (deserializedData != null)
                {
                    foreach (var item in deserializedData)
                    {
                        Preferences.Default.Set(item["object"].ToString() + "_" + item["attribute"].ToString(), item["value"].ToString());
                    }
                }
                Preferences.Default.Set("last_update", DateTime.Now.ToString());
            }
            else
            {
                ApiGets.ResponseError = Response.Item2;
                Popups.OpenGetErrorPopup();
            }
        }
        public static async Task GetMotd()
        {
            var formData = new MultipartFormDataContent
                {
                    { new StringContent("get"), "calltype" },
                    { new StringContent("motd"), "callitem" }
                };

            var Response = await WebCall.MakeApiCall(formData);

            ApiGets.ResponseStatus = Response.Item1;

            if (ResponseStatus)
            {
                Preferences.Default.Set("motd", Response.Item2);
                Preferences.Default.Set("last_update", DateTime.Now.ToString());
            }
            else
            {
                ApiGets.ResponseError = Response.Item2;
                Popups.OpenGetErrorPopup();
            }
        }
        public static async Task GetPendingAddStrikes()
        {
            var formData = new MultipartFormDataContent
                {
                    { new StringContent("get"), "calltype" },
                    { new StringContent("pendingaddstrikes"), "callitem" }
                };

            var Response = await WebCall.MakeApiCall(formData);

            ApiGets.ResponseStatus = Response.Item1;

            if (ResponseStatus)
            {
                Preferences.Default.Set("pendingaddstrikes", Response.Item2);
                Preferences.Default.Set("last_update", DateTime.Now.ToString());
            }
            else
            {
                ApiGets.ResponseError = Response.Item2;
                Popups.OpenGetErrorPopup();
            }
        }
        public static async Task GetPendingDelStrikes()
        {
            var formData = new MultipartFormDataContent
                {
                    { new StringContent("get"), "calltype" },
                    { new StringContent("pendingdelstrikes"), "callitem" }
                };

            var Response = await WebCall.MakeApiCall(formData);

            ApiGets.ResponseStatus = Response.Item1;

            if (ResponseStatus)
            {
                Preferences.Default.Set("pendingdelstrikes", Response.Item2);
                Preferences.Default.Set("last_update", DateTime.Now.ToString());
            }
            else
            {
                ApiGets.ResponseError = Response.Item2;
                Popups.OpenGetErrorPopup();
            }
        }
    }
    internal class ApiSets
    {
        public static bool ResponseStatus;
        public static string ResponseContent = "";
        public static string ResponseError = "";

        public static async Task SetCurrentStock(string newStock)
        {
            if (!string.IsNullOrEmpty(newStock))
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent("set"), "calltype" },
                    { new StringContent("currentstock"), "callitem" },
                    { new StringContent(newStock), "value" }
                };
                var Response = await WebCall.MakeApiCall(formData);

                ApiSets.ResponseStatus = Response.Item1;

                if (ApiSets.ResponseStatus)
                {
                    await ApiGets.GetCurrentStock();
                }
                else
                {
                    ApiSets.ResponseError = Response.Item2;
                    Popups.OpenSetErrorPopup();
                }
                Stock.newStock = "";
            }
            else
            {
                ApiSets.ResponseError = "Please correct your input!";
                Popups.OpenSetErrorPopup();
            }
        }
        public static async Task SetAddStrike(string malefactor, string reason)
        {
            if (!string.IsNullOrEmpty(malefactor) && !string.IsNullOrEmpty(reason))
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent("set"), "calltype" },
                    { new StringContent("addstrike"), "callitem" },
                    { new StringContent(malefactor), "malefactor" },
                    { new StringContent(reason), "reason" }
                };
                var Response = await WebCall.MakeApiCall(formData);

                ApiSets.ResponseStatus = Response.Item1;

                if (ApiSets.ResponseStatus)
                {
                    await ApiGets.GetPendingAddStrikes();
                }
                else
                {
                    ApiSets.ResponseError = Response.Item2;
                    Popups.OpenSetErrorPopup();
                }
                Strikes_Add.malefactor = "";
                Strikes_Add.reason = "";
            }
            else
            {
                ApiSets.ResponseError = "Please correct your input!";
                Popups.OpenSetErrorPopup();
            }
        }
        public static async Task SetDelStrike(string malefactor, string reason)
        {
            if (!string.IsNullOrEmpty(malefactor) && !string.IsNullOrEmpty(reason))
            {
                var formData = new MultipartFormDataContent
                {
                    { new StringContent("set"), "calltype" },
                    { new StringContent("delstrike"), "callitem" },
                    { new StringContent(malefactor), "malefactor" },
                    { new StringContent(reason), "reason" }
                };
                var Response = await WebCall.MakeApiCall(formData);

                ApiSets.ResponseStatus = Response.Item1;

                if (ApiSets.ResponseStatus)
                {
                    await ApiGets.GetPendingDelStrikes();
                }
                else
                {
                    ApiSets.ResponseError = Response.Item2;
                    Popups.OpenSetErrorPopup();
                }
                Strikes_Add.malefactor = "";
                Strikes_Add.reason = "";
            }
            else
            {
                ApiSets.ResponseError = "Please correct your input!";
                Popups.OpenSetErrorPopup();
            }
        }
    }
    internal class Misc
    {
        public static bool darkmode = false;
        public static void SaveVariables(Dictionary<string, string> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> pair in keyValuePairs)
            {
                Preferences.Default.Set(pair.Key, pair.Value);
            }
        }
        public static void SetDarkMode()
        {
            if (Preferences.Default.Get("darkmode", "false") == "false")
            {
                Preferences.Default.Set("darkmode", "true");
            }
            else
            {
                Preferences.Default.Set("darkmode", "false");
            }
        }
        public static void GetDarkMode()
        {
            if (Preferences.Default.Get("darkmode", "false") == "false")
            {
                Misc.darkmode = false;
            }
            else
            {
                Misc.darkmode = true;
            }
        }
        public static bool GetDarkModeBool()
        {
            if (Preferences.Default.Get("darkmode", "false") == "false")
            {
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}