
using Masarin.IoT.Sensor.Messages;
using System;
using System.Buffers.Binary;

namespace Masarin.IoT.Sensor
{
	class MQTTDecoderWinterCycle : MQTTDecoder
    {
        private const string UplinkPort1Topic = "1/payload";

        private readonly IMessageQueue _messageQueue = null;

        public MQTTDecoderWinterCycle(IMessageQueue messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {
            if (topic == UplinkPort1Topic)
            {
                ReadOnlySpan<byte> span = payload;

                double millivolts = payload[10];
                IoTHubMessageOrigin originWoPosition = new IoTHubMessageOrigin(device);
                _messageQueue.PostMessage(new SensorStatusMessage(originWoPosition, timestamp, millivolts * 0.025));

                double latitude = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(start: 0, length: 4));
                double longitude = BinaryPrimitives.ReadInt32LittleEndian(span.Slice(start: 4, length: 4));
                bool inTrip = (payload[8] & 0x80) != 0;
                bool lastFixFailed = (payload[8] & 0x40) != 0;
                double heading = (payload[8] & 0x3F); // Maska bort de första två bitarna för att få heading
                double speed = payload[9];

                // TODO: Bestäm vad som är ett bra filter här egentligen ...
                if (speed > 0)
                {
                    IoTHubMessageOrigin originWoDevice = new IoTHubMessageOrigin(latitude / 10000000.0, longitude / 10000000.0);
                    _messageQueue.PostMessage(new BicycleMovementMessage(originWoDevice, timestamp, heading * 5.625, speed));
                }
            }
        }
    }
}
