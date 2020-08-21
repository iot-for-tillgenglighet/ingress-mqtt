using System;
using System.Text;
using Xunit;
using Masarin.IoT.Sensor;
using Moq;

namespace Masarin.IoT.Sensor.Tests
{
    public class MQTTDecoderSnowdepthTests
    {
        [Fact]
        public void IfSummer_SnowdepthShouldNotSend()
        {
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object);
            
            byte[] bytes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 12, 13, 14, 15, 16, 17, 18 };
            
            decoder.Decode("2020-08-19T14:22:36Z", "ff", "", CreateTestPayload(bytes));

            mockMQ.Verify(foo => foo.PostMessage(It.IsAny<TelemetrySnowdepth>()), Times.Never());
        }
        
        
        [Fact]
        public void IfNotSummer_SnowdepthShouldSend()
        {
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object);
            
            byte[] bytes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 12, 13, 14, 15, 16, 17, 18 };

            decoder.Decode("2020-11-19T14:22:36Z", "ff", "", CreateTestPayload(bytes));

            mockMQ.Verify(foo => foo.PostMessage(It.IsAny<TelemetrySnowdepth>()), Times.Once());
        }

        [Fact]
        public void IfDate_IsWrongFormat()
        {
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object);
            
            byte[] bytes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 0, 12, 13, 14, 15, 16, 17, 18 };

            Assert.Throws<System.FormatException>(() => decoder.Decode("gurka", "ff", "", CreateTestPayload(bytes)));
        }

        [Fact]
        public void IfSensor_IsNotOkay()
        {   
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object);

            //The byte array is needed to form the payload that PostMessage sends. Without it, the test does not work.
            //The 11th byte (11), should be 0 if the sensor is okay (see previous tests).
            byte[] bytes = { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };

            decoder.Decode("2020-11-19T14:22:36Z", "ff", "", CreateTestPayload(bytes));

            mockMQ.Verify(foo => foo.PostMessage(It.IsAny<TelemetrySnowdepth>()), Times.Never());
        }

        [Fact]
        public void IfSnowdepth_ReceivedMatchesSent()
        {
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object);
            byte[] bytes = { 0, 1, 2, 3, 4, 5, 6, 1, 244, 9, 10, 0, 12, 13, 14, 15, 16, 17, 18 };

            decoder.Decode("2020-11-19T14:22:36Z", "ff", "", CreateTestPayload(bytes));

            mockMQ.Verify(ms => ms.PostMessage( 
                It.Is<TelemetrySnowdepth>(mo => mo.Depth == 50)
            ), Times.Once());
        }

        private static byte[] CreateTestPayload(byte[] bytes)
        {
            var data = Convert.ToBase64String(bytes);
            var payload = "{\"data\":\"" + data + "\"}";
            return Encoding.UTF8.GetBytes(payload);
        }
    }
}
