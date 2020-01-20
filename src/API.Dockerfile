FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
WORKDIR /src

# Copy csproj and dependencies
COPY ./API/ API/
COPY ./Grains Grains/
COPY ./GrainInterfaces/ GrainInterfaces/

WORKDIR /src/API

# Release
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine AS runtime
WORKDIR /src/API
COPY --from=build /src/API/out ./
ENTRYPOINT ["dotnet", "API.dll"]