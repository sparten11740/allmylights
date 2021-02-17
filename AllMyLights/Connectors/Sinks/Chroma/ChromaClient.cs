using Newtonsoft.Json;
using NLog;
using System.Collections.Generic;
using System.Drawing;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace AllMyLights.Connectors.Sinks.Chroma
{
    public class ChromaClient : IChromaClient
    {
        public const string CHROMA_INITIALIZATION_ENDPOINT = "http://localhost:54235/razer/chromasdk";
        public readonly static StringContent CHROMA_INITIALIZATION_BODY = new StringContent(JsonConvert.SerializeObject(new ChromaApp()
            {
                Title = "AllMyLights",
                Description = "Syncing your Razer devices with the remainder of your RGB peripherals",
                Author = new ChromaApp.ChromaAuthor
                {
                    Name = "JW",
                    Contact = "https://github.com/sparten11740/allmylights"
                },
                SupportedDevices = new string[]
                {
                    "keyboard",
                    "mouse",
                    "headset",
                    "mousepad",
                    "keypad",
                    "chromalink"
                },
                Category = "application"
            }), Encoding.UTF8, "application/json");

        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public string SessionUri { get; set; }
        private HttpClient HttpClient { get; }

        public ChromaClient(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public async Task<InitializationResponse> InitializeAsync()
        {
            var response = await HttpClient.PostAsync(CHROMA_INITIALIZATION_ENDPOINT, CHROMA_INITIALIZATION_BODY);
            var content = await response.Content.ReadAsStringAsync();
            var session = JsonConvert.DeserializeObject<InitializationResponse>(content);

            if (session.Error != null)
            {
                Logger.Error(session.Error);
                throw new ChromaException(session.Error);
            }

            SessionUri = session.Uri;

            return session;
        }

        public async Task SendHeartbeatAsync()
        {
            await HttpClient.PutAsync($"{SessionUri}/heartbeat", new StringContent(""));
        }

        public async Task<UpdateResponse> ApplyStaticEffectAsync(string device, Color color)
        {
            try
            {
                var body = JsonConvert.SerializeObject(new ChromaEffect()
                {
                    Param = new Dictionary<string, int>() { { "color", color.ToBgrDecimal() } }
                });
                var content = new StringContent(body, Encoding.UTF8, "application/json");

                var response = await HttpClient.PutAsync($"{SessionUri}/{device}", content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                Logger.Debug($"{nameof(ApplyStaticEffectAsync)}: Put {body} to {SessionUri}/{device} returned {responseBody}");

                var updateResponse = JsonConvert.DeserializeObject<UpdateResponse>(responseBody);
                if (updateResponse.Result != 0 || updateResponse.Error != null)
                {
                    throw new ChromaException(updateResponse.Error);
                }

                return updateResponse;
            }
            catch(HttpRequestException e)
            {
                Logger.Error($"{nameof(ApplyStaticEffectAsync)}: Put request to {SessionUri}/{device} with {color} failed unexpectedly: {e.Message}");
                throw e;
            }
            catch (ChromaException e)
            {
                Logger.Error(e.Message);
                throw e;
            }
        }

        public async Task<UpdateResponse> UpdateAsync(string device, string payload)
        {
            try
            {
                var content = new StringContent(payload, Encoding.UTF8, "application/json");

                var response = await HttpClient.PutAsync($"{SessionUri}/{device}", content);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                Logger.Debug($"{nameof(UpdateAsync)}: Put {payload} to {SessionUri}/{device} returned {responseBody}");
        
                
                
                var updateResponse = JsonConvert.DeserializeObject<UpdateResponse>(responseBody);

                if(updateResponse.Result != 0)
                {
                    throw new ChromaException(updateResponse.Error);
                }

                return updateResponse;
            }
            catch (HttpRequestException e)
            {
                Logger.Error($"{nameof(UpdateAsync)}: Put request to {SessionUri}/{device} failed unexpectedly: {e.Message}");
                throw e;
            }
            catch (ChromaException e)
            {
                Logger.Error(e.Message);
                throw e;
            }
        }
    }
}
