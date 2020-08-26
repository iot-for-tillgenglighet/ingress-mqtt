
using Masarin.IoT.Sensor.Messages;
using Newtonsoft.Json;
using System;
using System.Buffers.Binary;
using System.Text;

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
            var devEUI = data.deviceName;
            Console.WriteLine($"Got message from devEUI {devEUI}: {json}");

            // TODO: Repackage the contents as a NGSI-LD Device message and forward it to the NGSI Context Broker
        }
    }
}
