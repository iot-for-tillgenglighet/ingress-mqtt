
using Masarin.IoT.Sensor.Messages;
using System;
using System.Buffers.Binary;

namespace Masarin.IoT.Sensor
{
	class MQTTDecoderAurorasWS : MQTTDecoder
    {
        private const string WeatherTopic = "6/payload";

        private readonly IMessageQueue _messageQueue = null;

        public MQTTDecoderAurorasWS(IMessageQueue messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {
            if (payload.Length != 19 || topic != WeatherTopic)
            {
                return;
            }

            ReadOnlySpan<byte> span = payload;

            // TODO: Denna hårdkodning gäller endast för den väderstation som sitter på MIUN, vi kommer
            //       att behöva hantera sensorposition på ett intelligentare sätt framöver när de blir fler.
            double latitude = 62.391944;
            double longitude = 17.285917;
            IoTHubMessageOrigin origin = new IoTHubMessageOrigin(device, latitude, longitude);

            double millivolts = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(start: 17, length: 2));
            _messageQueue.PostMessage(new SensorStatusMessage(origin, timestamp, millivolts / 1000.0));

            double temperature = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(start: 0, length: 2));
            _messageQueue.PostMessage(new TelemetryTemperature(origin, timestamp, (temperature - 4000) / 100.0));

            const double kmphToMpS = 1000.0 / 60.0 / 60.0;
            double windspeed = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(start: 9, length: 2));
            windspeed = Math.Round(windspeed * kmphToMpS / 100.0, 2);

            int winddirection = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(start: 11, length: 2));
            _messageQueue.PostMessage(new TelemetryWind(origin, timestamp, windspeed, winddirection));

            double lux = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(start: 13, length: 2));
            double uvidx = BinaryPrimitives.ReadUInt16BigEndian(span.Slice(start: 15, length: 2));
            _messageQueue.PostMessage(new TelemetryLight(origin, timestamp, lux * 10, uvidx / 100.0));
        }
    }
}
