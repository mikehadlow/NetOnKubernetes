FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim AS runtime
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0-bullseye-slim AS sdk

ARG build_number=0.0.1

WORKDIR /app

COPY src/ .

# restore nuget packages
RUN dotnet restore

# build
RUN dotnet build --no-restore "-p:Version=${build_number}"

# test
# RUN dotnet test --no-build GreetingService.Tests/GreetingService.Tests.csproj

# publish
RUN dotnet publish --no-build -o output

FROM runtime AS runtime
# ENV ASPNETCORE_URLS=http://+:5432

WORKDIR /app
COPY --from=sdk /app/output/ ./
ENTRYPOINT ["./GreetingsService"]