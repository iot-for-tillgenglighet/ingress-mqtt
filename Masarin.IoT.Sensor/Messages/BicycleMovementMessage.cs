namespace Masarin.IoT.Sensor
{
	class BicycleMovementMessage : VehicleMovementMessage
    {
        public BicycleMovementMessage(IoTHubMessageOrigin origin, string timestamp, double heading, double speed) : base(origin, timestamp, heading, speed)
        {
            Type = "bicycle";
        }
    }
}
