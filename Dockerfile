# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy everything (simplest approach for a multi-project solution)
COPY . .

# Restore and publish the Api project (this pulls in Core, Infrastructure, Shared automatically via project references)
RUN dotnet publish EVoteUG.Api/EVoteUG.Api.csproj -c Release -o /app/publish

# Runtime stage (smaller image, no SDK needed to just run the app)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Render provides the PORT environment variable; ASP.NET Core needs to listen on it
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000

ENTRYPOINT ["dotnet", "EVoteUG.Api.dll"]
