# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files to cache restore
COPY ["AbstractPayments.slnx", "./"]
COPY ["AbstractPayments.Core/AbstractPayments.Core.csproj", "AbstractPayments.Core/"]
COPY ["AbstractPayments.Sandbox/AbstractPayments.Sandbox.csproj", "AbstractPayments.Sandbox/"]
COPY ["AbstractPayments.Tests/AbstractPayments.Tests.csproj", "AbstractPayments.Tests/"]
COPY ["AbstractPayments.Benchmarks/AbstractPayments.Benchmarks.csproj", "AbstractPayments.Benchmarks/"]

RUN dotnet restore

# Copy the rest of the source code
COPY . .

# Build and publish the Sandbox
WORKDIR "/src/AbstractPayments.Sandbox"
RUN dotnet publish -c Release -o /app/publish

# Runtime Stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Expose port for the Minimal API
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "AbstractPayments.Sandbox.dll"]