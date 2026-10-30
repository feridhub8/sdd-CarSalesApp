FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json ./
COPY services/ms-listing/src/WebApi/WebApi.csproj services/ms-listing/src/WebApi/
RUN dotnet restore services/ms-listing/src/WebApi/WebApi.csproj

COPY services/ms-listing/src/WebApi/ services/ms-listing/src/WebApi/
RUN dotnet publish services/ms-listing/src/WebApi/WebApi.csproj -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

USER $APP_UID
EXPOSE 8080

ENTRYPOINT ["dotnet", "WebApi.dll"]
