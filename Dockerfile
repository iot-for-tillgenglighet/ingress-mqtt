FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /app
COPY *.sln .
COPY Masarin.IoT.Sensor/*.csproj ./Masarin.IoT.Sensor/
COPY Masarin.IoT.Sensor.Tests/*.csproj ./Masarin.IoT.Sensor.Tests/
RUN dotnet restore

# copy full solution over
COPY . .
RUN dotnet build
FROM build AS testrunner
WORKDIR /app/Masarin.IoT.Sensor.Tests
CMD ["dotnet", "test", "--logger:trx"]

# run the unit tests
FROM build AS test
WORKDIR /app/Masarin.IoT.Sensor.Tests
RUN dotnet test --logger:trx

# publish the API
FROM build AS publish
WORKDIR /app/Masarin.IoT.Sensor/
RUN dotnet publish -c Release -o out

# run the api
FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
WORKDIR /app
COPY --from=publish /app/Masarin.IoT.Sensor/out ./
EXPOSE 80
ENTRYPOINT ["dotnet", "Masarin.IoT.Sensor.dll"]