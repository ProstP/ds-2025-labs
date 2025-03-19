FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

COPY *.csproj ./
RUN dotnet restore

COPY ./Pages ./Pages
COPY ./Properties ./Properties
COPY ./wwwroot ./wwwroot
COPY appsettings.json appsettings.json
COPY appsettings.Development.json appsettings.Development.json
COPY Program.cs Program.cs

RUN dotnet publish -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

COPY --from=build /app .

ENTRYPOINT ["dotnet", "Valuator.dll"]
