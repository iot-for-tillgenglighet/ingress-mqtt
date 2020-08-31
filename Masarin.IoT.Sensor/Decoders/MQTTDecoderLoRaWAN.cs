using Masarin.IoT.Sensor.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Buffers.Binary;
using System.Text;

namespace Masarin.IoT.Sensor

{
    public class FiwareValueProperty 
    {
        public string Type { get; }
        public string Value { get; }

        public FiwareValueProperty(string value)
        {
            Type = "Property";
            Value = value;
        }
    }
    public class FiwareDeviceMessage
    {
        public string Id { get; }
        public string Type { get; }
        public FiwareValueProperty Value { get; }

        public FiwareDeviceMessage(string id, string value)
        {
            Id = "urn:ngsi-ld:Device:" + id;
            Type = "Device";
            Value = new FiwareValueProperty(value);
        }

    }
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

                var message = new FiwareDeviceMessage(deviceName, value);

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
