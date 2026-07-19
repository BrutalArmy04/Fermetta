# ---------- Build stage ----------
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Restore first, alone: this layer is cached as long as the csproj doesn't change,
# so code-only edits rebuild in seconds instead of re-downloading packages.
COPY Fermetta/Fermetta/Fermetta.csproj Fermetta/Fermetta/
RUN dotnet restore Fermetta/Fermetta/Fermetta.csproj

# Now the rest of the source
COPY . .
RUN dotnet publish Fermetta/Fermetta/Fermetta.csproj -c Release -o /app/publish /p:UseAppHost=false

# ---------- Runtime stage ----------
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# The aspnet:9.0 image listens on 8080 by default (ASPNETCORE_HTTP_PORTS=8080)
EXPOSE 8080

ENTRYPOINT ["dotnet", "Fermetta.dll"]
