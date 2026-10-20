# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file first to leverage Docker layer caching
COPY src/MyTestApp/MyTestApp.csproj src/MyTestApp/
RUN dotnet restore src/MyTestApp/MyTestApp.csproj

# Copy the remaining source files and compile
COPY src/MyTestApp/ src/MyTestApp/
WORKDIR /src/src/MyTestApp
RUN dotnet publish MyTestApp.csproj -c Release -o /app/publish --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ARG GIT_COMMIT_SHA=""
ARG GIT_BRANCH=""
ARG GIT_TAG=""
ARG BUILD_TIME=""

ENV GIT_COMMIT_SHA=${GIT_COMMIT_SHA}
ENV GIT_BRANCH=${GIT_BRANCH}
ENV GIT_TAG=${GIT_TAG}
ENV BUILD_TIME=${BUILD_TIME}

# Run as non-root user (standard built-in non-root user in .NET 8+ container images)
USER $APP_UID

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "MyTestApp.dll"]
