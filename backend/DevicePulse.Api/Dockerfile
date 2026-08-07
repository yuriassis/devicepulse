FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /src
COPY backend/ ./backend/
RUN dotnet restore backend/DevicePulse.Api/DevicePulse.Api.csproj && dotnet publish backend/DevicePulse.Api/DevicePulse.Api.csproj -c Release -o /app --no-restore
FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_URLS=http://+:8080
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet","DevicePulse.Api.dll"]
