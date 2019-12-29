FROM mcr.microsoft.com/dotnet/core/sdk:3.0-alpine AS build
WORKDIR /src

# Copy csproj and dependencies
COPY ./Silo/ Silo/
COPY ./Grains/ Grains/
COPY ./GrainInterfaces/ GrainInterfaces/

WORKDIR /src/Silo

# Restore
RUN dotnet restore

# Release
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/core/runtime:3.0-alpine AS runtime
WORKDIR /src/Silo
COPY --from=build /src/Silo/out ./
ENTRYPOINT ["dotnet", "Silo.dll"]