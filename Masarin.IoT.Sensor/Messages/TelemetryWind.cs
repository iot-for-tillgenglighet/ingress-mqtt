namespace Masarin.IoT.Sensor
{
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
}
