FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY api-gateway/src/WebApi/WebApi.csproj api-gateway/src/WebApi/
RUN dotnet restore api-gateway/src/WebApi/WebApi.csproj

COPY api-gateway/src/WebApi/ api-gateway/src/WebApi/
RUN dotnet publish api-gateway/src/WebApi/WebApi.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApi.dll"]
