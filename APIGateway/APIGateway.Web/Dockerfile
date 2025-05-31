# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Stage 1: Base image for running the application
# This stage sets up the basic environment for the ASP.NET Core application.
# It's used for both fast mode debugging in Visual Studio and as the base for the final production image.
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
# Sets the user for running the application. $APP_UID is typically set by the container runtime.
USER $APP_UID
# Sets the working directory in the container.
WORKDIR /app
# Exposes port 8080 and 8081 for the application to listen on.
EXPOSE 8080
EXPOSE 8081


# Stage 2: Build stage
# This stage compiles the .NET project.
# It uses the .NET SDK image which includes tools for building and publishing .NET applications.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
# Defines a build argument for the build configuration (e.g., Release or Debug).
ARG BUILD_CONFIGURATION=Release
# Sets the working directory for build operations.
WORKDIR /src
# Copies the .csproj file into the container and places it in the APIGateway.Web/ directory.
# This is done first to leverage Docker's layer caching for faster subsequent builds if only code changes.
COPY ["APIGateway.Web.csproj", "APIGateway.Web/"]
# Restores NuGet packages for the project.
RUN dotnet restore "./APIGateway.Web/APIGateway.Web.csproj"
# Sets the working directory to the root of the source files for the COPY . . command.
WORKDIR "/src/"
# Copies all source files from the build context into the container at the current WORKDIR (/src/).
COPY . .
# Sets the working directory to the project folder to run the build command.
WORKDIR "/src/APIGateway.Web"
# Builds the project. The output is placed in /app/build.
RUN dotnet build "./APIGateway.Web.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish stage
# This stage publishes the application, creating a deployment-ready package.
# It builds upon the 'build' stage.
FROM build AS publish
# Defines a build argument for the build configuration.
ARG BUILD_CONFIGURATION=Release
# Publishes the application. The output is placed in /app/publish.
# /p:UseAppHost=false means it won't generate a native executable, just the DLL.
RUN dotnet publish "./APIGateway.Web.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final stage
# This stage creates the final, lean image for production.
# It uses the 'base' image created in Stage 1, which contains only the ASP.NET runtime.
FROM base AS final
# Sets the working directory in the final image.
WORKDIR /app
# Copies the published application files from the 'publish' stage (/app/publish) into the final image.
COPY --from=publish /app/publish .
# Defines the command to run when the container starts. This will execute the application.
ENTRYPOINT ["dotnet", "APIGateway.Web.dll"]