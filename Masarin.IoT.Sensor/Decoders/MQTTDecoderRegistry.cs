namespace Masarin.IoT.Sensor
{
	class MQTTDecoderRegistry : IMQTTDecoderRegistry
    {
        private readonly MQTTDecoderAurorasWS _weatherDecoder;
        private readonly MQTTDecoderWinterCycle _bicycleDecoder;
        private readonly MQTTDecoderIcomit _avlDecoder;
        private readonly MQTTDecoderSnowdepth _snowdepthDecoder;
        private readonly MQTTNullDecoder _nullDecoder;

        public MQTTDecoderRegistry(IMessageQueue messageQueue)
        {
            _bicycleDecoder = new MQTTDecoderWinterCycle(messageQueue);
            _avlDecoder = new MQTTDecoderIcomit(messageQueue);
            _weatherDecoder = new MQTTDecoderAurorasWS(messageQueue);
            _snowdepthDecoder = new MQTTDecoderSnowdepth(messageQueue);
            _nullDecoder = new MQTTNullDecoder();
        }

        public IMQTTDecoder GetDecoderForNode(string node, string path)
        {
            if (node.StartsWith("360295"))
            {
                return _weatherDecoder;
            }
            else if (node.StartsWith("812106"))
            {
                return _bicycleDecoder;
            }
            else if (node == "icomit")
            {
                return _avlDecoder;
            }
            else if (node.StartsWith("10a52aaa8"))
            {
                return _snowdepthDecoder;
            }
            else
            {
                return _nullDecoder;
            }
        }

    }
}
