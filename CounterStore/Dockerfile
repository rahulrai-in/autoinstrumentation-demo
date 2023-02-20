FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS base
WORKDIR /app
# Directory for OpenTelemetry AI binaries
ENV OTEL_DOTNET_AUTO_HOME=/otel-ai
EXPOSE 8081

ENV ASPNETCORE_URLS=http://+:8081

FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src
COPY ["CounterStore/CounterStore.csproj", "CounterStore/"]
RUN dotnet restore "CounterStore/CounterStore.csproj"
COPY . .
WORKDIR "/src/CounterStore"
RUN dotnet build "CounterStore.csproj" -c Release -o /app/build

FROM build AS publish
ENV OTEL_DOTNET_AUTO_HOME=/otel-ai
RUN dotnet publish "CounterStore.csproj" -c Release -o /app/publish /p:UseAppHost=false
# Helper script to intall the dependencies and instrument the application
RUN curl -sSfL https://raw.githubusercontent.com/open-telemetry/opentelemetry-dotnet-instrumentation/v0.5.0/otel-dotnet-auto-install.sh -O
RUN sh otel-dotnet-auto-install.sh

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
# Add OTEL binaries to the published app
COPY --from=publish $OTEL_DOTNET_AUTO_HOME $OTEL_DOTNET_AUTO_HOME
ENTRYPOINT ["dotnet", "CounterStore.dll"]