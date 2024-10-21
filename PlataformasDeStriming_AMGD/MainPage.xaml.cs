using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;


namespace PlataformasDeStriming_AMGD
{
    public partial class MainPage : ContentPage
    {
        // Constantes deben estar dentro de la clase
        private const string Client_ID = "lt3p7af6vamltum6bkp5s3qf94gjwi"; // Reemplaza con tu Client ID
        private const string Access_Token = "nfkzkeyle69a2lgnji89ld303c9pds"; // Reemplaza con tu Access Token

        public MainPage()
        {
            InitializeComponent();
        }

        private async void OnSearchButtonClicked(object sender, EventArgs e)
        {
            // Cambia para orientar al usuario
            Boton.BackgroundColor = Color.DarkSlateGray;
            streamStatusLabel.Text = "........................";

            string streamerName = streamerNameEntry.Text?.Trim(); // Obtiene el nombre del streamer del campo de entrada
            if (string.IsNullOrWhiteSpace(streamerName))
            {
                await DisplayAlert("Error", "Por favor ingresa un nombre de streamer.", "OK");
                Boton.BackgroundColor = Color.Aqua;
                return;
            }

            // Llamada a la API de Twitch para obtener información sobre el streamer
            var streamerLiveData = await GetStreamerLiveStatusAsync(streamerName);

            if (streamerLiveData != null && streamerLiveData.Data.Length > 0)
            {
                var stream = streamerLiveData.Data[0];

                streamStatusLabel.Text = $"{stream.UserName} está transmitiendo ahora!";
                streamStatusLabel.TextColor = Color.Green;
                streamStatusLabel.IsVisible = true;

                // Usa la URL para buscar al streamer
                string streamUrl = $"https://www.twitch.tv/{stream.UserName}";
                streamWebView.Source = streamUrl;
                streamWebView.IsVisible = true;

                Boton.BackgroundColor = Color.Aqua;
                Boton2.IsVisible = true;
            }
            else
            {
                // Si el streamer no está en directo o no existe
                streamStatusLabel.Text = "El streamer está offline o no existe.";
                streamStatusLabel.TextColor = Color.Red;
                streamStatusLabel.IsVisible = true;
                streamWebView.IsVisible = false;

                Boton.BackgroundColor = Color.Aqua;
                Boton2.IsVisible = false;
            }
        }

        private async void CloseWebViewClicked(object sender, EventArgs e)
        {
            streamWebView.IsVisible = false;
            Boton2.IsVisible = false;
            streamStatusLabel.Text = "";
        }

        // Método para obtener el estado en vivo del streamer con manejo de SSL ignorado
        private async Task<StreamResponse> GetStreamerLiveStatusAsync(string streamerName)
        {
            string url = $"https://api.twitch.tv/helix/streams?user_login={streamerName}";

            using (var httpClientHandler = new HttpClientHandler())
            {
                // Permite todos los certificados SSL (NO recomendado para producción)
                httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, sslPolicyErrors) => true;

                using (var httpClient = new HttpClient(httpClientHandler))
                {
                    httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {Access_Token}");
                    httpClient.DefaultRequestHeaders.Add("Client-ID", Client_ID);

                    var response = await httpClient.GetAsync(url);

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonResponse = await response.Content.ReadAsStringAsync();
                        return JsonConvert.DeserializeObject<StreamResponse>(jsonResponse);
                    }
                    else
                    {
                        await DisplayAlert("Error", "Error al verificar el estado del streamer. Intenta de nuevo.", "OK");
                        return null;
                    }
                }
            }
        }
    }

    // Clases para convertir y usar la respuesta de la API de Twitch
    public class StreamResponse
    {
        public StreamData[] Data { get; set; }
    }

    public class StreamData
    {
        [JsonProperty("user_name")]
        public string UserName { get; set; }
    }
}

