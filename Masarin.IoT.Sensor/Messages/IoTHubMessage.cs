using Newtonsoft.Json;

namespace Masarin.IoT.Sensor
{
	public class IoTHubMessage : IIoTHubMessage
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
}
