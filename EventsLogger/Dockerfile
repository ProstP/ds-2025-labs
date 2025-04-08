FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

COPY *.sln .
COPY ./EventsLogger/*.csproj ./EventsLogger/
COPY ./MessageBroker/*.csproj ./MessageBroker/
RUN dotnet restore ./EventsLogger/EventsLogger.csproj

COPY ./EventsLogger ./EventsLogger
COPY ./MessageBroker ./MessageBroker/

RUN dotnet publish ./EventsLogger/EventsLogger.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "EventsLogger.dll"]