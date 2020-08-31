using Masarin.IoT.Sensor.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Buffers.Binary;
using System.Text;
using Fiware;

namespace Masarin.IoT.Sensor

{
    public class MQTTDecoderLoRaWAN : MQTTDecoder
    {
        public MQTTDecoderLoRaWAN(string ngsiContextBrokerURL)
        {
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {
            string json = Encoding.UTF8.GetString(payload);
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            var deviceName = Convert.ToString(data.deviceName);
            var obj = data["object"];
            var present = obj.present;
            
            if (deviceName.Contains("livboj"))
            {
                string value = "on";

                if (present == false)
                {
                    value = "off";
                }

                var message = new Fiware.DeviceMessage(deviceName, value);

                var settings = new JsonSerializerSettings
                {
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    NullValueHandling = NullValueHandling.Ignore
                };

                json = JsonConvert.SerializeObject(message, settings);
                Console.WriteLine(json);
            }

            Console.WriteLine($"Got message from deviceName {deviceName}: {json}");

            // TODO: forward device message to the NGSI Context Broker
        }
    }
}
