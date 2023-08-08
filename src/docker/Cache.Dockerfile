FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

# Copy csproj and dependencies
COPY ./examples/Caching examples/Caching
COPY ./examples/Caching.Grains examples/Caching.Grains
COPY ./EventSourcingGrains EventSourcingGrains/
COPY ./EventSourcing EventSourcing/

WORKDIR /src/examples/Caching

# Release
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS runtime
WORKDIR /src/examples/Caching
COPY --from=build /src/examples/Caching/out ./
ENTRYPOINT ["dotnet", "Caching.dll"]