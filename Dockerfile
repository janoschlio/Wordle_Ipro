# ---------- Build-Stage ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BlazorServerApp/BlazorServerApp.csproj BlazorServerApp/
RUN dotnet restore BlazorServerApp/BlazorServerApp.csproj

COPY BlazorServerApp/ BlazorServerApp/
RUN dotnet publish BlazorServerApp/BlazorServerApp.csproj \
    -c Release \
    -o /app/publish \
    /p:UseAppHost=false

# ---------- Runtime-Stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

COPY --from=build /app/publish .

ENV ConnectionStrings__WordleDb="Data Source=/app/data/wordle.db"
RUN mkdir -p /app/data && chown -R $APP_UID:$APP_UID /app/data

ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

USER $APP_UID
ENTRYPOINT ["dotnet", "BlazorServerApp.dll"]
