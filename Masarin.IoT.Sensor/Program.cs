
using MQTTnet;
using MQTTnet.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Masarin.IoT.Sensor
{
    public interface IIoTHubMessage
    {
        string Topic { get; }
    }

    class IoTHubMessageOrigin
    {
        public string Device { get; }
        public double? Latitude { get; }
        public double? Longitude { get; }

        public IoTHubMessageOrigin(string device)
        {
            Device = device;
        }

        public IoTHubMessageOrigin(double latitude, double longitude)
        {
            Latitude = latitude;
            Longitude = longitude;
        }

        public IoTHubMessageOrigin(string device, double latitude, double longitude)
        {
            Device = device;
            Latitude = latitude;
            Longitude = longitude;
        }
    }

    class IoTHubMessage : IIoTHubMessage
    {
        [JsonIgnore]
        public string Topic { get; }

        public IoTHubMessageOrigin Origin { get; }
        public string Timestamp { get; }

        public IoTHubMessage(IoTHubMessageOrigin origin, string timestamp, string topic)
        {
            Origin = origin;
            Timestamp = timestamp;
            Topic = topic;
        }
    }

    class SensorStatusMessage : IoTHubMessage
    {
        public double Volt { get; }

        public SensorStatusMessage(IoTHubMessageOrigin origin, string timestamp, double volts) : base(origin, timestamp, "sensor.status")
        {
            Volt = volts;
        }
    }

    class VehicleMovementMessage : IoTHubMessage
    {
        public double Heading { get; }
        public double Speed { get; }
        public string Type { get; set; }

        public VehicleMovementMessage(IoTHubMessageOrigin origin, string timestamp, double heading, double speed) : base(origin, timestamp, "vehicle.movement")
        {
            Heading = heading;
            Speed = speed;
        }
    }

    class BicycleMovementMessage : VehicleMovementMessage
    {
        public BicycleMovementMessage(IoTHubMessageOrigin origin, string timestamp, double heading, double speed) : base(origin, timestamp, heading, speed)
        {
            Type = "bicycle";
        }
    }

    class CarMovementMessage : VehicleMovementMessage
    {
        public CarMovementMessage(IoTHubMessageOrigin origin, string timestamp, double heading, double speed) : base(origin, timestamp, heading, speed)
        {
            Type = "car";
        }
    }

    class TelemetryLight : IoTHubMessage
    {
        public double Lux { get; }
        public double UvIndex { get; }

        public TelemetryLight(IoTHubMessageOrigin origin, string timestamp, double lux, double uvIndex) : base(origin, timestamp, "telemetry.light")
        {
            Lux = Math.Round(lux, 2);
            UvIndex = Math.Round(uvIndex, 2);
        }
    }

    class TelemetryTemperature : IoTHubMessage
    {
        public double Temp { get; }

        public TelemetryTemperature(IoTHubMessageOrigin origin, string timestamp, double temperature) : base(origin, timestamp, "telemetry.temperature")
        {
            Temp = temperature;
        }
    }

    class TelemetryWind : IoTHubMessage
    {
        public double Speed { get; }
        public int Direction { get; }

        public TelemetryWind(IoTHubMessageOrigin origin, string timestamp, double speed, int direction) : base(origin, timestamp, "telemetry.wind")
        {
            Speed = speed;
            Direction = direction;
        }
    }

    class TelemetryHumidity : IoTHubMessage
    {
        public int Humidity { get; }

        public TelemetryHumidity(IoTHubMessageOrigin origin, string timestamp, int humidity) : base(origin, timestamp, "telemetry.humidity")
        {
            Humidity = humidity;
        }
    }

    class TelemetryPressure : IoTHubMessage
    {
        public long Pressure { get; }

        public TelemetryPressure(IoTHubMessageOrigin origin, string timestamp, long pressure) : base(origin, timestamp, "telemetry.pressure")
        {
            Pressure = pressure;
        }
    }

    class TelemetryHeight : IoTHubMessage
    {
        public int Height { get; }

        public TelemetryHeight(IoTHubMessageOrigin origin, string timestamp, int height) : base(origin, timestamp, "telemetry.height")
        {
            Height = height;
        }
    }
    public interface IMessageQueue
    {
        void PostMessage(IIoTHubMessage message);
    }

    class LoggingMQWrapper : IMessageQueue
    {
        private readonly JsonSerializerSettings _serializerSettings;

        public LoggingMQWrapper()
        {
            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public void PostMessage(IIoTHubMessage message)
        {
            if (message != null)
            {
                string json = JsonConvert.SerializeObject(message, _serializerSettings);
                Console.WriteLine(json);
            }
        }
    }

    class RabbitMQWrapper : IMessageQueue
    {
        private RabbitMQ.Client.IConnection _rmqConnection;
        private RabbitMQ.Client.IModel _rmqModel;

        private const string _exchangeName = "iot-msg-exchange-topic";

        private readonly JsonSerializerSettings _serializerSettings;

        public RabbitMQWrapper(RabbitMQ.Client.IConnection connection)
        {
            _rmqConnection = connection;
            _rmqModel = _rmqConnection.CreateModel();

            _rmqModel.ExchangeDeclare(_exchangeName, ExchangeType.Topic);

            _serializerSettings = new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public void PostMessage(IIoTHubMessage message)
        {
            if (message != null)
            {
                string json = JsonConvert.SerializeObject(message, _serializerSettings);
                byte[] messageBodyBytes = Encoding.UTF8.GetBytes(json);

                _rmqModel.BasicPublish(_exchangeName, message.Topic, null, messageBodyBytes);
            }
        }
    }

    interface IMQTTDecoder
    {
        void Decode(string timestamp, string device, string topic, byte[] payload);
    }

    abstract class MQTTDecoder : IMQTTDecoder
    {

        public MQTTDecoder()
        {
        }

        public static string PayloadToHex(byte[] payload)
        {
            StringBuilder hex = new StringBuilder(payload.Length * 2);
            foreach (byte b in payload)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        abstract public void Decode(string timestamp, string device, string topic, byte[] payload);
    }

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

    

    class MQTTDecoderSnowHeight : MQTTDecoder
    {
        private const string UplinkPort1Topic = "1/payload";

        private readonly IMessageQueue _messageQueue = null;

        public MQTTDecoderSnowHeight(IMessageQueue messageQueue)
        {
            _messageQueue = messageQueue;
        }

        public override void Decode(string timestamp, string device, string topic, byte[] payload)
        {
            if (topic == UplinkPort1Topic)
            {
                ReadOnlySpan<byte> span = payload;
                /*
                    uint8_t   Battery [%]
                    uint16_t  raw Distance [mm]
                    uint16_t  Angle [deg]
                    unit16_t  Vertical distance [mm] 
                    unit16_t  Snow height [mm]
                    unit16_t  Laser signal strength [0-400] (lower is better) 
                    uint8_t   Laser sensor status
                    int16_t   Temperature [°C]
                    unit8_t   Humidity [%]
                    unit32_t  Pressure [Pa]

                    DEVEUI:s
                    1199411787624306471
                    1199411787624306472
                    1199411787624306473
                    1199411787624306480
                    1199411787624306481
                    1199411787624306482
                    1199411787624306483
                    1199411787624306484
                    1199411787624306485
                    1199411787624306486
                */
                double latitude = 1.348364;
                double longitude = 1.016056;

               

  
                if (device == "1199411787624306471")
                {
                    latitude = 62.348364;
                    longitude = 17.016056;
                }
                else if (device == "1199411787624306472")
                {
                    latitude = 1.348364;
                    longitude = 1.016056;
                }
                else if (device == "1199411787624306473")
                {
                    latitude = 1.348364;
                    longitude = 1.016056;
                }
                else if (device == "1199411787624306480")
                {
                    latitude = 1.348364;
                    longitude = 1.016056;
                }
                else if (device == "1199411787624306481")
                {
                    latitude = 1.348364;
                    longitude = 1.016056;
                }
                else if (device == "1199411787624306482")
                {
                    latitude = 1.348364;
                    longitude = 1.016056;
                }
                else if (device == "1199411787624306483")
                {
                    latitude = 1.348364;
                    longitude = 1.016056;
                }
                else if (device == "1199411787624306484")
                {
                    latitude = 1.348364;
                    longitude = 1.016056;
                }
                else if (device == "1199411787624306485")
                {
                    latitude = 1.348364;
                    longitude = 1.016056;
                }
                else if (device == "1199411787624306486")
                {
                    latitude = 1.348364;
                    longitude = 1.016056;
                }


                IoTHubMessageOrigin origin = new IoTHubMessageOrigin(device, latitude, longitude);

                int battery = payload[0];  
                IoTHubMessageOrigin originWoPosition = new IoTHubMessageOrigin(device);
                _messageQueue.PostMessage(new SensorStatusMessage(originWoPosition, timestamp, 1100+battery*5));

                int snowheight = payload[7] << 8 + payload[8];
                IoTHubMessageOrigin originWoDevice = new IoTHubMessageOrigin(latitude / 10000000.0, longitude / 10000000.0);
                _messageQueue.PostMessage(new TelemetryHeight(originWoDevice, timestamp, snowheight));

                int temperature = ((payload[12] << 8 + payload[13]) - 100) / 10;
                originWoDevice = new IoTHubMessageOrigin(latitude / 10000000.0, longitude / 10000000.0);
                _messageQueue.PostMessage(new TelemetryTemperature(originWoDevice, timestamp, temperature));

                int humidity = payload[14];
                originWoDevice = new IoTHubMessageOrigin(latitude / 10000000.0, longitude / 10000000.0);
                _messageQueue.PostMessage(new TelemetryHumidity(originWoDevice, timestamp, humidity));

                int pressure = (payload[15] << 24 + payload[16] << 16 + payload[17] << 8 + payload[18]);
                originWoDevice = new IoTHubMessageOrigin(latitude / 10000000.0, longitude / 10000000.0);
                _messageQueue.PostMessage(new TelemetryPressure(originWoDevice, timestamp, pressure));
            }
        }
    }



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

    interface IMQTTDecoderRegistry
    {
        IMQTTDecoder GetDecoderForNode(string node, string path);
    }

    class MQTTDecoderRegistry : IMQTTDecoderRegistry
    {
        private readonly MQTTDecoderAurorasWS _weatherDecoder;
        private readonly MQTTDecoderWinterCycle _bicycleDecoder;
        private readonly MQTTDecoderIcomit _avlDecoder;
        private readonly MQTTDecoderSnowHeight _snowHeightDecoder;
        private readonly MQTTNullDecoder _nullDecoder;

        public MQTTDecoderRegistry(IMessageQueue messageQueue)
        {
            _bicycleDecoder = new MQTTDecoderWinterCycle(messageQueue);
            _avlDecoder = new MQTTDecoderIcomit(messageQueue);
            _weatherDecoder = new MQTTDecoderAurorasWS(messageQueue);
            _snowHeightDecoder = new MQTTDecoderSnowHeight(messageQueue);
            _nullDecoder = new MQTTNullDecoder();
        }

        public IMQTTDecoder GetDecoderForNode(string node, string path)
        {
            if (node.StartsWith("360295"))
            {
                return _weatherDecoder;
            }
            else if (node.StartsWith("812106"))
            {
                return _bicycleDecoder;
            }
            else if (node == "icomit")
            {
                return _avlDecoder;
            }
            else if (node.StartsWith("11994117876243064"))
            {
                return _snowHeightDecoder;
            }
            else
            {
                return _nullDecoder;
            }
        }

    }

    class Program
    {
        // Create a termination event that we can use to signal that the app should shut down
        static bool itIsNotTimeToShutDown = true;
        static EventWaitHandle terminationEvent = new EventWaitHandle(false, EventResetMode.AutoReset);

        static void Main(string[] args)
        {
            IMessageQueue messageQueue = new LoggingMQWrapper();

            try
            {
                ConnectionFactory rmqFactory = new ConnectionFactory
                {
                    HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST"),
                    UserName = Environment.GetEnvironmentVariable("RABBITMQ_USER"),
                    Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")
                };

                if (rmqFactory.HostName != null)
                {
                    Console.WriteLine($"Connecting to RabbitMQ host {rmqFactory.HostName} as {rmqFactory.UserName} ...");
                    messageQueue = new RabbitMQWrapper(rmqFactory.CreateConnection());
                }
            }
            catch(Exception e)
            {
                Console.WriteLine($"RabbitMQ Exception: {e.Message}");
            }

            MQTTDecoderRegistry decoders = new MQTTDecoderRegistry(messageQueue);

            //TestParseData(decoders);

            var mqttFactory = new MqttFactory();
            var client = mqttFactory.CreateMqttClient();

            var mqttUsername = Environment.GetEnvironmentVariable("MQTT_USER");
            var mqttPassword = Environment.GetEnvironmentVariable("MQTT_PASSWORD");
            var mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST");

            var builder = new MqttClientOptionsBuilder()
                .WithClientId(Guid.NewGuid().ToString())
                .WithTcpServer(mqttHost);

            if (mqttUsername != null)
            {
                builder = builder.WithCredentials(mqttUsername, mqttPassword);
            }

            var options = builder.Build();

            client.Connected += async (s, e) =>
            {
                Console.WriteLine("Connected! Subscribing to root topic ...");
                await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("#").Build());
            };

            client.ApplicationMessageReceived += async (sender, e) =>
            {
                try
                {
                    ParseMessagePayload(DateTime.Now.ToUniversalTime(), e.ApplicationMessage.Topic, e.ApplicationMessage.Payload, decoders);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception caught when calling ParseMessagePayload: {ex.Message}");
                }
            };

            client.Disconnected += async (sender, e) =>
            {
                Console.WriteLine("### MQTT Server dropped connection. Sleeping and reconnecting ... ###");
                await Task.Delay(TimeSpan.FromSeconds(5));

                try
                {
                    await client.ConnectAsync(options);
                    Console.WriteLine("Successfully reconnected to server.");
                }
                catch
                {
                    Console.WriteLine("### RECONNECTING FAILED ###");

                    itIsNotTimeToShutDown = false;
                    terminationEvent.Set();
                }
            };

            Console.WriteLine($"Connecting to MQTT host {mqttHost} as {mqttUsername} ...");
            client.ConnectAsync(options);

            while (itIsNotTimeToShutDown)
            {
                terminationEvent.WaitOne();
            }

            Console.WriteLine("### SHUTTING DOWN ###");
        }

        static void TestParseData(IMQTTDecoderRegistry decoders)
        {
            string[] lines = System.IO.File.ReadAllLines(@" <enter path here> ");
            foreach (var line in lines)
            {
                string[] parts = line.Split('\t');
                if (parts.Length == 4)
                {
                    string timestamp = parts[0];
                    string topic = parts[1] + "/" + parts[2];
                    DateTime dt = DateTime.Parse(timestamp);
                    ParseMessagePayload(dt, topic, ParseHex(parts[3]), decoders);
                }
            }
        }

        static byte[] ParseHex(string hexString)
        {
            int length = hexString.Length / 2;
            byte[] ret = new byte[length];
            for (int i = 0, j = 0; i < length; i++)
            {
                int high = ParseNybble(hexString[j++]);
                int low = ParseNybble(hexString[j++]);
                ret[i] = (byte)((high << 4) | low);
            }

            return ret;
        }

        static int ParseNybble(char c)
        {
            if (c >= '0' && c <= '9')
            {
                return c - '0';
            }
            c = (char)(c & ~0x20);
            if (c >= 'A' && c <= 'F')
            {
                return c - ('A' - 10);
            }
            throw new ArgumentException("Invalid nybble: " + c);
        }

        static void ParseMessagePayload(DateTime dt, string topic, byte[] payload, IMQTTDecoderRegistry decoders)
        {
            string[] topicPath = topic.Split('/');
            var node = topicPath[0];
            var path = String.Join("/", topicPath, 1, topicPath.Length - 1);

            var hex = MQTTDecoder.PayloadToHex(payload);
            var timestamp = dt.ToString("yyyy-MM-ddTHH:mm:ssZ");

            var logstring = $"{timestamp}\t{node}\t{path}\t{hex}";
            Console.WriteLine(logstring);

            if (node.StartsWith("node_") && node.Length > 5)
            {
                node = node.Substring(5);
            }

            IMQTTDecoder decoder = decoders.GetDecoderForNode(node, path);
            decoder.Decode(timestamp, node, path, payload);
        }
    }
}
