
using Masarin.IoT.Sensor.Messages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Masarin.IoT.Sensor
{
	class MQTTDecoderIcomit : MQTTDecoder
    {
        class AVLProperty
        {
            public string ID { get; set; }
            public long Value { get; set; }
        }

        class TektonikaAVLData
        {
            public string IMEI { get; set; }
            public string TimeStamp { get; set; }
            public string Longitude { get; set; }
            public string Latitude { get; set; }
            public string Angle { get; set; }
            public string Speed { get; set; }

            public List<AVLProperty> OneByteIO { get; set; }
            public List<AVLProperty> TwoByteIO { get; set; }
            public List<AVLProperty> FourByteIO { get; set; }

            public bool IsIgnitionOn()
            {
                AVLProperty p = OneByteIO.Find(x => x.ID == "239");
                return (p != null && p.Value == 1);
            }

            public CarMovementMessage ToCarMovementMessage()
            {
                if (IsIgnitionOn())
                {
                    double lat, lon, heading, velocity;
                    Double.TryParse(Latitude, out lat);
                    Double.TryParse(Longitude, out lon);
                    Double.TryParse(Angle, out heading);
                    Double.TryParse(Speed, out velocity);

                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    dt = dt.AddMilliseconds(long.Parse(TimeStamp));

                    IoTHubMessageOrigin origin = new IoTHubMessageOrigin(lat / 10000000.0, lon / 10000000.0);
                    return new CarMovementMessage(origin, dt.ToString("yyyy-MM-ddTHH:mm:ssZ"), heading, velocity);
                }

                return null;
            }

            public SensorStatusMessage ToStatusMessage()
            {
                AVLProperty p = TwoByteIO.Find(x => x.ID == "67");
                if (p != null)
                {
                    double lat, lon;
                    Double.TryParse(Latitude, out lat);
                    Double.TryParse(Longitude, out lon);

                    DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    dt = dt.AddMilliseconds(long.Parse(TimeStamp));

                    IoTHubMessageOrigin origin = new IoTHubMessageOrigin($"icom-{IMEI}", lat / 10000000.0, lon / 10000000.0);
                    return new SensorStatusMessage(origin, dt.ToString("yyyy-MM-ddTHH:mm:ssZ"), p.Value / 1000.0);
                }

                return null;
            }

            public TektonikaAVLData() {}
        }

        private readonly IMessageQueue _messageQueue = null;

        public MQTTDecoderIcomit(IMessageQueue messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {
            if (topic == "test")
            {
                string json = Encoding.UTF8.GetString(payload);

                TektonikaAVLData data = JsonConvert.DeserializeObject<TektonikaAVLData>(json);
                _messageQueue.PostMessage(data.ToStatusMessage());
                _messageQueue.PostMessage(data.ToCarMovementMessage());
            }
        }
    }
}
