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
            //Arrange          
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object);
            
            byte[] bytes = { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var data = Convert.ToBase64String(bytes);
            var payload = "{\"data\":\"" + data + "\"}";
            
            //Act
            decoder.Decode("2020-08-19T14:22:36Z", "ff", "", Encoding.UTF8.GetBytes(payload));

            //Assert
            mockMQ.Verify(foo => foo.PostMessage(It.IsAny<TelemetrySnowdepth>()), Times.Never());
        }
        
        
        [Fact]
        public void IfNotSummer_SnowdepthShouldSend()
        {
            //Arrange          
            var mockMQ = new Mock<IMessageQueue>();
            var decoder = new MQTTDecoderSnowdepth(mockMQ.Object);
            
            byte[] bytes = { 2, 4, 6, 8, 10, 12, 14, 16, 18, 20, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            var data = Convert.ToBase64String(bytes);
            var payload = "{\"data\":\"" + data + "\"}";
            
            //Act
            decoder.Decode("2020-11-19T14:22:36Z", "ff", "", Encoding.UTF8.GetBytes(payload));

            //Assert
            mockMQ.Verify(foo => foo.PostMessage(It.IsAny<TelemetrySnowdepth>()), Times.Once());
        }
    }
}
