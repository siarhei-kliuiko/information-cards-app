using InformationCardClient.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace InformationCardClient.Helpers
{
    internal class HttpResponseDataSet<T>
    {
        public T Data { get; set; }
        public string ErrorMessage { get; set; }
    }

    internal static class HttpService
    {
        private static HttpClient client = new HttpClient();

        static HttpService()
        {
            client.BaseAddress = new Uri("https://localhost:7271");
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        private static async Task<string> PerformDataChangingApiCall(Func<Task<HttpResponseMessage>> func)
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
            string errorMessage = null;
            HttpResponseMessage response = null;
            try
            {
                response = await func();
            }
            catch (Exception ex)
            {
                errorMessage = ex.Message;
            }

            if (response != null)
            {
                var jsonText = await response.Content.ReadAsAsync<string>();
                if (!response.IsSuccessStatusCode)
                {
                    if (!string.IsNullOrEmpty(jsonText))
                    {
                        errorMessage = jsonText;
                    }
                    else if (!string.IsNullOrEmpty(response.ReasonPhrase))
                    {
                        errorMessage = response.ReasonPhrase;
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return errorMessage;

        }

        internal static async Task<string> AddCardAsync(InformationCard newCard)
        {
            var errorMessage = await PerformDataChangingApiCall(() => client.PostAsJsonAsync("cards", newCard));
            return errorMessage;
        }

        internal static async Task<HttpResponseDataSet<List<InformationCard>>> GetCardsAsync()
        {
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
            var responseData = new HttpResponseDataSet<List<InformationCard>>();
            HttpResponseMessage response = null;
            try
            {
                response = await client.GetAsync("cards");
            }
            catch (Exception ex)
            {
                responseData.ErrorMessage = ex.Message;
            }

            if (response != null)
            {
                var jsonText = await response.Content.ReadAsAsync<string>();
                if (response.IsSuccessStatusCode)
                {
                    responseData.Data = JsonConvert.DeserializeObject<List<InformationCard>>(jsonText);
                }
                else
                {
                    if (!string.IsNullOrEmpty(jsonText))
                    {
                        responseData.ErrorMessage = jsonText;
                    }
                    else if (!string.IsNullOrEmpty(response.ReasonPhrase))
                    {
                        responseData.ErrorMessage = response.ReasonPhrase;
                    }
                }
            }

            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return responseData;
        }

        internal static async Task<string> RemoveCardAsync(InformationCard card)
        {
            var errorMessage = await PerformDataChangingApiCall(() => client.DeleteAsync($"cards/{card.Id}"));
            return errorMessage;
        }

        internal static async Task<string> UpdateCardAsync(InformationCard card)
        {
            var errorMessage = await PerformDataChangingApiCall(() => client.PutAsJsonAsync("cards", card));
            return errorMessage;
        }
    }
}
