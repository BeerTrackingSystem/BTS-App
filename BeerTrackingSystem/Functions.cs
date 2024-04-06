using BeerTrackingSystem.Components.Pages;
using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace BeerTrackingSystem
{
    internal class WebCall
    {
        public static async Task<(bool,string)> MakeApiCall(MultipartFormDataContent formData)
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
                    if (calltype == "set" || callitem == "sessionstate")
                    {
                        callData.Add(new StringContent(Preferences.Default.Get("sessionid", "Unknown SessionID")), "sessionid");
                        callData.Add(new StringContent(Preferences.Default.Get("user", "Unknown User")), "user");
                    }
                }

                // Durchführen des POST-Webaufrufs mit Formulardaten
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
        public static bool ShowGetErrorPopup = false;
        public static bool ShowSetErrorPopup = false;
        public static void OpenGetErrorPopup()
        {
            ShowGetErrorPopup = true;
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
    }
    internal class ApiGets
    {
        public static bool ResponseStatus;
        public static bool SessionState;
        public static string ResponseContent = "";
        public static string ResponseError = "";
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
    }
    internal class Misc
    {
        public static void SaveVariables(Dictionary<string, string> keyValuePairs)
        {
            foreach (KeyValuePair<string, string> pair in keyValuePairs)
            {
                Preferences.Default.Set(pair.Key, pair.Value);
            }
        }
    }
}