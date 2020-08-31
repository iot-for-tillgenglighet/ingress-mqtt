using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;

namespace Fiware
{
    public class ContextBroker : IFiwareContextBroker
    {
        public ContextBroker()
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