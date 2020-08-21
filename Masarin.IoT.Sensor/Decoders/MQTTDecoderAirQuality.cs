
using Masarin.IoT.Sensor.Messages;
using System;
using System.Buffers.Binary;

namespace Masarin.IoT.Sensor
{
    class MQTTDecoderAirQuality : MQTTDecoder
    {
        private const string AirQualityTopic = "2/payload";

        private readonly IMessageQueue _messageQueue = null;

        public MQTTDecoderAirQuality(IMessageQueue messageQueue)
        {
            _messageQueue = messageQueue;
        }

        private double ConvertNO2(double nonCorrectedNO2, string device)
        {
            double correction = 0.0; // TODO: Device dependent correction value
            double no2 = Math.Round(
                Math.Max(nonCorrectedNO2 / 100.0 - 100.0 + correction, 0),
                2);

            Console.WriteLine($"Converted NO2 value {nonCorrectedNO2} to {no2} ...");
            return no2;
        }

        private double ConvertTemperature(double temp, string device)
        {
            if (device != "8121069065166743827")
            {
                temp -= 2732;
            }

            // Compensate for wrap around when sub zero temperatures are sent as uint16
            if (temp > 1000)
            {
                temp -= 65536;
            }

            return temp / 10.0;
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {

            //8121069065192625893   Circle K            62.387146,17.2948968
            //8121069065166743827   Skolhusallen        62.389056,17.2993245
            //8121069065154928350   Storgatan           62.3919559,17.294769
            //8121069065048831975   Universitetsallen   62.394618,17.2894867
            //8121069065164496276   Parkgatan           62.385353,17.3108438
            //8121069065126998713   Köpmangatan         62.38861,17.3083424

            if (topic != AirQualityTopic)
            {
                return;
            }

            ReadOnlySpan<byte> span = payload;
            double latitude = 0;
            double longitude = 0;

            if (device == "8121069065192625893")
            {
                latitude = 62.387146;
                longitude = 17.2948968;
            }
            else if (device == "8121069065166743827")
            {
                latitude = 62.389056;
                longitude = 17.2993245;
            }
            else if (device == "8121069065154928350")
            {
                latitude = 62.3919559;
                longitude = 17.294769;
            }
            else if (device == "8121069065048831975")
            {
                latitude = 62.394618;
                longitude = 17.2894867;
            }
            else if (device == "8121069065164496276")
            {
                latitude = 62.385353;
                longitude = 17.3108438;
            }
            else if (device == "8121069065126998713")
            {
                latitude = 62.38861;
                longitude = 17.3083424;
            }
            else
            {
                Console.WriteLine("Unknown air quality sensor " + device + ". Position unknown!");
                return;
            }
            
            IoTHubMessageOrigin origin = new IoTHubMessageOrigin(device, latitude, longitude);
            //pack('HHhHHH', pm10, pm25, (DTH_temp + 2732), DTH_humi, sensor_error, NO2ppm)

            double pm10 = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(start: 0, length: 2));
            _messageQueue.PostMessage(new TelemetryPM10(origin, timestamp, pm10 / 10.0));

            double pm25 = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(start: 2, length: 2));
            _messageQueue.PostMessage(new TelemetryPM25(origin, timestamp, pm25 / 10.0));

            _messageQueue.PostMessage(new TelemetryTemperature(
                origin, timestamp, 
                ConvertTemperature(
                    BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(start: 4, length: 2)),
                    device
                )));

            int humidity = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(start: 6, length: 2));
            _messageQueue.PostMessage(new TelemetryHumidity(origin, timestamp, (int)(humidity / 10.0)));

            if (device != "8121069065126998713")
            {
                var no2StartIndex = (device != "8121069065166743827") ? 10 : 8;
                double no2 = BinaryPrimitives.ReadUInt16LittleEndian(span.Slice(start: no2StartIndex, length: 2));
                _messageQueue.PostMessage(new TelemetryNO2(origin, timestamp, ConvertNO2(no2, device)));
            }
        }
    }
}
