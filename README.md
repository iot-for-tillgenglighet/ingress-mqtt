# ingress-mqtt
An ingress service that connects to MQTT endpoints and pulls data into the platform

## Deprecation Notice

This repository is deprecated and the code has now moved to https://github.com/diwise/ingress-mqtt

# Building and tagging with Docker

`docker build -f Dockerfile -t iot-for-tillgenglighet/ingress-mqtt:latest .`

# Running locally with Docker

`docker run -it -e MQTT_HOST='<insert MQTT host here>' iot-for-tillgenglighet/ingress-mqtt`
