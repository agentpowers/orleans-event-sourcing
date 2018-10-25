FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src

# Copy csproj and dependencies
COPY ./API/ API/
COPY ./GrainInterfaces/ GrainInterfaces/

WORKDIR /src/API

# Restore
RUN dotnet restore

# Release
RUN dotnet publish -c Release -o out


FROM microsoft/dotnet:2.1-aspnetcore-runtime AS runtime
WORKDIR /src/API
COPY --from=build /src/API/out ./
ENTRYPOINT ["dotnet", "API.dll"]