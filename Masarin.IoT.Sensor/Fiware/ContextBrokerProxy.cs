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
        public ContextBrokerProxy()
        {

        }

        public void PostMessage(DeviceMessage message)
        {
            var settings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };

            var json = JsonConvert.SerializeObject(message, settings);
            

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var url = $"https://iotsundsvall.se/ngsi-ld/entities/{message.Id}/attrs/";

            using var client = new HttpClient();

            //client.BaseAddress = new Uri("http://domän.se/api/");
            
            // Post
            Patch(client, url, data);
                
            // Get

        }

        private static void Post(HttpClient client, string url, StringContent data)
        {
            var responseTask = client.PostAsync(url, data);
            var responseMessage = responseTask.GetAwaiter().GetResult();
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Satan!");
            }
        }

        private static void Patch(HttpClient client, string url, StringContent data)
        {
            var responseTask = client.PatchAsync(url, data);
            var responseMessage = responseTask.GetAwaiter().GetResult();
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Satan!");
            }
        }

        private static DeviceMessage Get(HttpClient client, string url, StringContent data)
        {
            var responseTask = client.GetAsync(url);
            var responseMessage = responseTask.GetAwaiter().GetResult();
            if (!responseMessage.IsSuccessStatusCode)
            {
                throw new HttpRequestException("Satan!");
            }

            var content = responseMessage.Content.ReadAsStringAsync().GetAwaiter().GetResult();

            return JsonConvert.DeserializeObject<DeviceMessage>(content);
        }
    }
}
