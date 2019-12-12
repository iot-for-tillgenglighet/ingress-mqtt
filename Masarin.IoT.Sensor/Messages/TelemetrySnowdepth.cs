namespace Masarin.IoT.Sensor
{
	class TelemetrySnowdepth : IoTHubMessage
    {
        public double Snowdepth { get; }

        public TelemetrySnowdepth(IoTHubMessageOrigin origin, string timestamp, double snowdepth) : base(origin, timestamp, "telemetry.Snowdepth")
        {
            Snowdepth = snowdepth;
        }
    }
}
