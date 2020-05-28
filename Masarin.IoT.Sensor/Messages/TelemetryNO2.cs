namespace Masarin.IoT.Sensor
{
    class TelemetryNO2 : IoTHubMessage
    {
        public double NO2 { get; }

        public TelemetryNO2(IoTHubMessageOrigin origin, string timestamp, double no2) : base(origin, timestamp, "telemetry.no2")
        {
            NO2 = no2;
        }
    }
}
