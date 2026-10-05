# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Publish the Client (Blazor WASM) first
RUN dotnet publish EVoteUG.Client/EVoteUG.Client.csproj -c Release -o /app/client-publish

# Publish the Api
RUN dotnet publish EVoteUG.Api/EVoteUG.Api.csproj -c Release -o /app/publish

# Copy the Client's compiled static files into the Api's wwwroot
RUN mkdir -p /app/publish/wwwroot && cp -r /app/client-publish/wwwroot/. /app/publish/wwwroot/

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "EVoteUG.Api.dll"]
