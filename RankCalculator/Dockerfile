FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

COPY *.sln .
COPY ./RankCalculator/*.csproj ./RankCalculator/
COPY ./MessageBroker/*.csproj ./MessageBroker/
COPY ./DatabaseService/*.csproj ./DatabaseService/
RUN dotnet restore ./RankCalculator/RankCalculator.csproj

COPY ./RankCalculator ./RankCalculator
COPY ./MessageBroker ./MessageBroker/
COPY ./DatabaseService ./DatabaseService/

RUN dotnet publish ./RankCalculator/RankCalculator.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "RankCalculator.dll"]