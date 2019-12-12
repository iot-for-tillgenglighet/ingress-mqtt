namespace Masarin.IoT.Sensor
{
	class TelemetrySnowdepth : IoTHubMessage
    {
        public int Snowdepth { get; }

        public TelemetrySnowdepth(IoTHubMessageOrigin origin, string timestamp, int snowdepth) : base(origin, timestamp, "telemetry.Snowdepth")
        {
            Snowdepth = snowdepth;
        }
    }
}
