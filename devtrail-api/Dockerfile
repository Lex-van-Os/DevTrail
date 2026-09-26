FROM mcr.microsoft.com/dotnet/sdk:10.0.400 AS build
WORKDIR /src

COPY devtrail-api/devtrail-api.csproj devtrail-api/
COPY devtrail-core/devtrail-core.csproj devtrail-core/

RUN dotnet restore devtrail-api/devtrail-api.csproj

COPY devtrail-api/ devtrail-api/
COPY devtrail-core/ devtrail-core/

RUN dotnet publish devtrail-api/devtrail-api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0.11 AS final
WORKDIR /app
COPY --from=build /app/publish .
USER app
EXPOSE 8080
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s \
    CMD bash -c 'exec 3<>/dev/tcp/localhost/8080 && printf "GET /health HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n" >&3 && head -n 1 <&3 | grep -q "200"'
ENTRYPOINT ["dotnet", "devtrail-api.dll"]
