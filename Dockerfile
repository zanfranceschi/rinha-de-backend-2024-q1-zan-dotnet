FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

COPY ./src ./
RUN apt-get update
RUN apt-get install --no-install-recommends -y clang zlib1g-dev
RUN dotnet restore
RUN dotnet publish -c Release -o out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["./rinha-de-backend-2024-q1-zan-dotnet"]

