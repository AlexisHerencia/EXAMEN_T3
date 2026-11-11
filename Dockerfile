FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/ExamenT3.Api/ExamenT3.Api.csproj src/ExamenT3.Api/
RUN dotnet restore src/ExamenT3.Api/ExamenT3.Api.csproj

COPY src/ExamenT3.Api/ src/ExamenT3.Api/
RUN dotnet publish src/ExamenT3.Api/ExamenT3.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

ENTRYPOINT ["dotnet", "ExamenT3.Api.dll"]
