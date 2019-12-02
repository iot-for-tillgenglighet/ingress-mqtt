using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using RabbitMQ.Client;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Masarin.IoT.Sensor
{
	public interface IIoTHubMessage
    {
        string Topic { get; }
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

            var mqttUsername = Environment.GetEnvironmentVariable("MQTT_USER");
            var mqttPassword = Environment.GetEnvironmentVariable("MQTT_PASSWORD");
            var mqttHost = Environment.GetEnvironmentVariable("MQTT_HOST");

			var options = new MqttClientOptionsBuilder()
				.WithTcpServer(mqttHost, 1883)
				.WithTls(new MqttClientOptionsBuilderTlsParameters
				{
					UseTls = true,
					AllowUntrustedCertificates = true,
					IgnoreCertificateChainErrors = true,
					IgnoreCertificateRevocationErrors = true
				})
				.WithCredentials(mqttUsername, mqttPassword)
				.WithClientId(Guid.NewGuid().ToString())
				.Build();

			var client = mqttFactory.CreateMqttClient();

			client.UseConnectedHandler( async (e) =>
            {
                Console.WriteLine("Connected! Subscribing to root topic ...");
                await client.SubscribeAsync(new TopicFilterBuilder().WithTopic("#").Build());
            });

            client.UseApplicationMessageReceivedHandler( e =>
            {
                try
                {
                    ParseMessagePayload(DateTime.Now.ToUniversalTime(), e.ApplicationMessage.Topic, e.ApplicationMessage.Payload, decoders);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception caught when calling ParseMessagePayload: {ex.Message}");
                }
            });

            client.UseDisconnectedHandler( async (e) =>
            {
                Console.WriteLine("### MQTT Server dropped connection. Sleeping and reconnecting ... ###");
				Console.WriteLine(e.Exception);
				Console.WriteLine(e.AuthenticateResult);
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
            });

            Console.WriteLine($"Connecting to MQTT host {mqttHost} as {mqttUsername} ...");
            client.ConnectAsync(options, CancellationToken.None);

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
