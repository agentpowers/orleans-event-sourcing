FROM microsoft/dotnet:2.2-sdk AS build
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


FROM microsoft/dotnet:2.2-runtime AS runtime
WORKDIR /src/Silo
COPY --from=build /src/Silo/out ./
ENTRYPOINT ["dotnet", "Silo.dll"]