# Order Tracking Platform

## Overview

This project is a microservices-based Order Tracking Platform. It allows users to manage products, place orders, and track their status. The platform is built using .NET for the backend services and a client application for the user interface. All services are containerized using Docker.

## Microservices

The platform consists of the following microservices:

*   **APIGateway (`APIGateway/APIGateway.Web`)**:
    *   The single entry point for all client requests.
    *   Routes requests to the appropriate downstream services.
    *   Handles cross-cutting concerns like authentication and logging (though authentication is primarily handled by AuthService).
    *   Built using YARP (Yet Another Reverse Proxy).

*   **AuthService (`AuthService/AuthService.Web`)**:
    *   Manages user authentication and authorization.
    *   Handles user registration, login, and token generation.
    *   Provides endpoints for verifying user identities.

*   **ProductService (`ProductService/ProductService.API`)**:
    *   Manages the product catalog.
    *   Provides CRUD (Create, Read, Update, Delete) operations for products.
    *   Stores product information.

*   **OrderService (`OrderService/OrderService.API`)**:
    *   Manages customer orders.
    *   Handles order creation, updates, and deletes.
    *   Interacts with ProductService to verify product availability.

*   **Frontend.App (`Frontend.App/Frontend.App.Client`)**:
    *   The client-side user interface for interacting with the platform.
    *   Allows users to browse products, place orders, and view order history.
    *   Communicates with the backend services via the APIGateway.
    *   This is a Razor Pages application.

## Development Environment Setup

To set up the development environment, you will need the following:

1.  **.NET SDK**:
    *   Ensure you have the .NET 8 SDK installed. You can download it from the [official .NET website](https://dotnet.microsoft.com/download/dotnet/8.0).
    *   Verify installation by running `dotnet --version` in your terminal.

2.  **Docker**:
    *   Docker is required to build and run the containerized services.
    *   Install Docker Desktop (for Windows or macOS) or Docker Engine (for Linux). You can find installation instructions on the [official Docker website](https://www.docker.com/get-started).
    *   Ensure the Docker daemon is running.

3.  **IDE (Optional but Recommended)**:
    *   An IDE like Visual Studio 2022, JetBrains Rider, or VS Code with the C# extension can be helpful for code development.

## Build and Run the Application

The entire platform can be built and run using Docker Compose.

1.  **Clone the repository**:
    ```bash
    git clone <repository-url>
    cd <repository-directory>
    ```

2.  **Build and run with Docker Compose**:
    *   Open a terminal in the root directory of the project (where the `docker-compose.yml` file is located).
    *   Run the following command:
        ```bash
        docker-compose up --build
        ```
    *   This command will:
        *   Pull the necessary base images.
        *   Build the Docker images for each service defined in `docker-compose.yml`.
        *   Create and start containers for all services.
        *   The `--build` flag ensures that the images are rebuilt if there are any changes in the Dockerfiles or source code.

3.  **Stopping the application**:
    *   To stop the application, press `Ctrl+C` in the terminal where `docker-compose up` is running.
    *   To stop and remove the containers, you can run:
        ```bash
        docker-compose down
        ```

## Basic Usage

Once the application is running, you can access the frontend:

*   **Frontend Application**: Open your web browser and navigate to `http://localhost:5000`.

You can then use the frontend to:
*   Log in as a user (via AuthService).
*   Place orders (managed by OrderService).

The API Gateway is accessible at `http://localhost:8080`, but direct interaction is typically done through the frontend or API testing tools for specific service endpoints (e.g., using Postman).

Individual services (if their ports were exposed directly, which they are not by default in the provided `docker-compose.yml` for services other than the gateway and frontend) would be on their respective container ports, but all interaction is designed to go through the API Gateway.
The PostgreSQL database is available on port `5432` for direct inspection if needed, using the credentials in `docker-compose.yml`.