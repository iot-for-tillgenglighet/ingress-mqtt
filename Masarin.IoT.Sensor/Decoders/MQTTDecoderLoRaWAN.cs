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
        private readonly IContextBrokerProxy _fiwareContextBroker = null;
        public MQTTDecoderLoRaWAN(IContextBrokerProxy fiwareContextBroker)
        {
            _fiwareContextBroker = fiwareContextBroker;
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {
            string json = Encoding.UTF8.GetString(payload);
            var data = JsonConvert.DeserializeObject<dynamic>(json);
            var deviceName = Convert.ToString(data.deviceName);
            var obj = data["object"];
            var present = obj.present;
            
            if (deviceName.Contains("sn-elt-livboj-"))
            {
                string value = "on";

                if (present == false)
                {
                    value = "off";
                }

                var message = new Fiware.DeviceMessage(deviceName, value);

                _fiwareContextBroker.PostMessage(message);
            } 
            else if (deviceName.Contains("sk-elt-temp-"))
            {
                double value = obj.externalTemperature;

                string stringValue = $"t%3D{value}";

                var message = new Fiware.DeviceMessage(deviceName, stringValue);
                
                _fiwareContextBroker.PostMessage(message);
            }

            Console.WriteLine($"Got message from deviceName {deviceName}: {json}");

        }
    }
}
