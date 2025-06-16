# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build-env
WORKDIR /app

# Copy all project files and restore dependencies
COPY SignaliteWebAPI/*.csproj ./SignaliteWebAPI/
COPY SignaliteWebAPI.Application/*.csproj ./SignaliteWebAPI.Application/
COPY SignaliteWebAPI.Infrastructure/*.csproj ./SignaliteWebAPI.Infrastructure/
COPY SignaliteWebAPI.Domain/*.csproj ./SignaliteWebAPI.Domain/

# If you have a solution file, copy it too (optional but recommended)
COPY *.sln ./

# Restore dependencies for the API project (which should restore all dependencies)
RUN dotnet restore ./SignaliteWebAPI/
# Copy everything else and build
COPY . ./
RUN dotnet publish ./SignaliteWebAPI/ -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build-env /app/out .

# Create a non-root user
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Expose port
EXPOSE 8080
# Set environment variable for ASP.NET Core to listen on all interfaces
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SignaliteWebAPI.dll"]