FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

COPY *.sln .
COPY ./Valuator/*.csproj ./Valuator/
COPY ./MessageBroker/*.csproj ./MessageBroker/
RUN dotnet restore ./Valuator/Valuator.csproj

COPY ./Valuator ./Valuator/
COPY ./MessageBroker ./MessageBroker/

RUN dotnet publish ./Valuator/Valuator.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app .
COPY --from=build /source/Valuator/appsettings*.json .

ENTRYPOINT ["dotnet", "Valuator.dll"]
