FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

COPY AccessConsole.Api.csproj ./

RUN dotnet restore AccessConsole.Api.csproj

COPY . .

RUN dotnet publish AccessConsole.Api.csproj \
    --configuration Release \
    --output /app/publish \
    --no-restore \
    /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime

WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://0.0.0.0:8080

EXPOSE 8080

USER $APP_UID

ENTRYPOINT ["dotnet", "AccessConsole.Api.dll"]
