using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

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
            Console.WriteLine(json);
        }
    }
}