FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

# Copy csproj and dependencies
COPY Silo/* Silo/
COPY Grains/* Grains/
COPY GrainInterfaces/* GrainInterfaces/

WORKDIR /src/Silo

# Restore
RUN dotnet restore

# Release
RUN dotnet publish -c Release -o out


FROM microsoft/dotnet:2.1-runtime AS runtime
WORKDIR /src/Silo
COPY --from=build /app/Silo/out ./
ENTRYPOINT ["dotnet", "Silo.dll"]