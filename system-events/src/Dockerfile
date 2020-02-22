FROM mcr.microsoft.com/dotnet/core/aspnet:3.1 AS base
WORKDIR /app
EXPOSE 80

# Restore only
FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
COPY src/*.csproj /all/src/
COPY tests/Unit/*.csproj /all/tests/Unit/

WORKDIR /all/src
RUN dotnet restore

WORKDIR /all/tests/Unit
RUN dotnet restore

# Build Source Code
COPY src /all/src
COPY tests/Unit /all/tests/Unit
RUN dotnet build --no-restore -c Release -o /app

WORKDIR /all/tests/Unit
RUN dotnet test --no-restore 

FROM build AS publish
RUN dotnet publish --no-restore -c Release -o /app

# Build runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "SystemEvents.dll"]