FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY backend/DevicePulse.Api/DevicePulse.Api.csproj backend/DevicePulse.Api/
RUN dotnet restore backend/DevicePulse.Api/DevicePulse.Api.csproj
COPY backend/DevicePulse.Api/ backend/DevicePulse.Api/
RUN dotnet publish backend/DevicePulse.Api/DevicePulse.Api.csproj \
    --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
RUN mkdir /data && chown app:app /data
COPY --from=build --chown=app:app /app/publish .
USER app
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ConnectionStrings__DevicePulse="Data Source=/data/devicepulse.db;Default Timeout=30"
EXPOSE 8080
VOLUME ["/data"]
ENTRYPOINT ["dotnet", "DevicePulse.Api.dll"]
