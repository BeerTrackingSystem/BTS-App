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
                string url = "https://bts.mditsa.de/api/";

                // Erstellen der Formulardaten
                var callData = formData;
                callData.Add(new StringContent("G4LelS"), "apikey");

                var calltypeContent = formData.FirstOrDefault(p => p.Headers.ContentDisposition?.Name?.Trim('"') == "calltype");
                if (calltypeContent != null)
                {
                    var calltype = calltypeContent.ReadAsStringAsync().Result;
                    if (calltype == "set")
                    {
                        callData.Add(new StringContent("fBRNVE-YrE1P7bkl+Qkx-bqa7cnC.UY9PgVupir4J388BgdDS!"), "sessionid");
                        callData.Add(new StringContent("Test 1"), "user");
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
        }
    }
    internal class Popups
    {
        public static bool ShowErrorPopup = false;
        public static void OpenErrorPopup()
        {
            ShowErrorPopup = true;
        }

        public static void CloseErrorPopup()
        {
            ShowErrorPopup = false;
        }
    }
}