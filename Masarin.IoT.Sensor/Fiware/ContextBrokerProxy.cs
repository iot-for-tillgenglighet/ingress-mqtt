using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;

namespace Fiware
{
    public class ContextBrokerProxy : IContextBrokerProxy
    {
        private readonly HttpClient _client = null;
        private readonly string _contextBrokerURL = null;

        public ContextBrokerProxy(string contextBrokerURL)
        {
            _client = new HttpClient();
            _contextBrokerURL = contextBrokerURL;
        }

        public void PostMessage(DeviceMessage message)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(message, settings);
            
            var data = new StringContent(json, Encoding.UTF8, "application/json+ld");

            var url = $"{_contextBrokerURL}/ngsi-ld/v1/entities/{message.Id}/attrs/";

            Patch(_client, url, data);
        }

        private static void Patch(HttpClient client, string url, StringContent data)
        {
            var responseTask = client.PatchAsync(url, data);
            var responseMessage = responseTask.GetAwaiter().GetResult();
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Failed to patch entity attributes.");
            }
        }
    }
}
