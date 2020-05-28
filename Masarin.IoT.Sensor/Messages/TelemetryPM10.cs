namespace Masarin.IoT.Sensor
{
	class TelemetryPM10 : IoTHubMessage
    {
        public double PM10 { get; }

        public TelemetryPM10(IoTHubMessageOrigin origin, string timestamp, double pm10) : base(origin, timestamp, "telemetry.pm10")
        {
            PM10 = pm10;
        }
    }
}
