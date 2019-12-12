namespace Masarin.IoT.Sensor
{
	class TelemetryHumidity : IoTHubMessage
    {
        public int Humidity { get; }

        public TelemetryHumidity(IoTHubMessageOrigin origin, string timestamp, int humidity) : base(origin, timestamp, "telemetry.humidity")
        {
            Humidity = humidity;
        }
    }
}
