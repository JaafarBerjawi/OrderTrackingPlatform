FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore "./Frontend.App.Client/Frontend.App.Client.csproj"

WORKDIR "/src/Frontend.App.Client"
RUN dotnet build "./Frontend.App.Client.csproj" -c %BUILD_CONFIGURATION% -o /app/build

FROM build AS publish
RUN dotnet publish "Frontend.App.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
# Sets the working directory in the final image.
WORKDIR /app
# Copies the published application files from the 'publish' stage (/app/publish) into the final image.
COPY --from=publish /app/publish .
# Defines the command to run when the container starts. This will execute the application.
ENTRYPOINT ["dotnet", "Frontend.App.Client.dll"]