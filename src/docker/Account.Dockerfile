FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build
WORKDIR /src

#TODO: split nuget restore and build into multiple steps

# Copy csproj and dependencies
COPY ./examples/Account examples/Account
COPY ./examples/Account.Grains examples/Account.Grains
COPY ./EventSourcingGrains EventSourcingGrains/
COPY ./EventSourcing EventSourcing/

WORKDIR /src/examples/Account

# Release
RUN dotnet publish -c Release -o out


FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine AS runtime
WORKDIR /src/examples/Account
COPY --from=build /src/examples/Account/out ./
ENTRYPOINT ["dotnet", "Account.dll"]