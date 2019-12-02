using System;

namespace Masarin.IoT.Sensor
{
	class MQTTNullDecoder : MQTTDecoder
    {
        public MQTTNullDecoder()
        {
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {
            Console.WriteLine("WARNING: Received data that could not be handled by any decoder ...");
        }
    }
}
