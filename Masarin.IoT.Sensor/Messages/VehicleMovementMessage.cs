namespace Masarin.IoT.Sensor
{
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
}
