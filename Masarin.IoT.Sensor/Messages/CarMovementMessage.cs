namespace Masarin.IoT.Sensor
{
	class CarMovementMessage : VehicleMovementMessage
    {
        public CarMovementMessage(IoTHubMessageOrigin origin, string timestamp, double heading, double speed) : base(origin, timestamp, heading, speed)
        {
            Type = "car";
        }
    }
}
