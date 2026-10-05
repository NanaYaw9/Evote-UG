# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything
COPY . .

# Publish the Blazor WebAssembly Client
RUN dotnet publish EVoteUG.Client/EVoteUG.Client.csproj -c Release -o /app/client-publish

# Check that the required Blazor framework file was published
RUN ls -la /app/client-publish/wwwroot/_framework/icudt_EFIGS.dat

# Publish the API
RUN dotnet publish EVoteUG.Api/EVoteUG.Api.csproj -c Release -o /app/publish

# Copy the Client's compiled static files into the API's wwwroot
RUN mkdir -p /app/publish/wwwroot && cp -r /app/client-publish/wwwroot/. /app/publish/wwwroot/

# Check that the required file was copied into the API output
RUN ls -la /app/publish/wwwroot/_framework/icudt_EFIGS.dat

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "EVoteUG.Api.dll"]
