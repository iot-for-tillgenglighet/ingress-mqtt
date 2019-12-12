using System;
using System.Collections.Generic;
using System.Text;

namespace Masarin.IoT.Sensor.Messages
{
	class SensorStatusMessage : IoTHubMessage
	{
		public double Volt { get; }

		public SensorStatusMessage(IoTHubMessageOrigin origin, string timestamp, double volts) : base(origin, timestamp, "sensor.status")
		{
			Volt = volts;
		}
	}
}
