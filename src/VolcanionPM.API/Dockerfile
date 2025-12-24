# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["src/VolcanionPM.API/VolcanionPM.API.csproj", "src/VolcanionPM.API/"]
COPY ["src/VolcanionPM.Application/VolcanionPM.Application.csproj", "src/VolcanionPM.Application/"]
COPY ["src/VolcanionPM.Domain/VolcanionPM.Domain.csproj", "src/VolcanionPM.Domain/"]
COPY ["src/VolcanionPM.Infrastructure/VolcanionPM.Infrastructure.csproj", "src/VolcanionPM.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/VolcanionPM.API/VolcanionPM.API.csproj"

# Copy all source files
COPY . .

# Build the application
WORKDIR "/src/src/VolcanionPM.API"
RUN dotnet build "VolcanionPM.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "VolcanionPM.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user for security (use 1001 to avoid conflicts)
RUN groupadd -f -g 1001 appuser && \
    useradd -r -u 1001 -g appuser appuser

# Create logs directory
RUN mkdir -p /app/logs && \
    chown -R appuser:appuser /app/logs

# Copy published application
COPY --from=publish /app/publish .

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health/live || exit 1

# Set entrypoint
ENTRYPOINT ["dotnet", "VolcanionPM.API.dll"]
