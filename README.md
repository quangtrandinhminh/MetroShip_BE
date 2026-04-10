# MetroShip Backend Service

> An enterprise-grade backend service for a nationwide logistics and consignment management platform, built with .NET Core.
> 

## Project Overview

MetroShip is a comprehensive logistics backend designed to handle complex consignment operations, from order booking and smart routing to real-time tracking and payment processing. The system is architected to be highly scalable, maintainable, and efficient at processing concurrent data.

## Technical Highlights 

- **Smart Logistics Routing (Graph Algorithms):** Modeled the nationwide transit network as a weighted graph.
    - Implemented **Dijkstra's Algorithm** and **BFS** to dynamically calculate the shortest and most cost-effective delivery routes based on dynamic pricing configurations and distance tiers.
- **Architecture:** Strictly follows **N-Layer Architecture** separating WebAPI, Business Services, Data Access (Repository), and Utilities.
    - Applied **Repository** and **Unit of Work** design patterns to manage database transactions safely and abstract EF Core logic.
- **Real-Time Data Processing:** Integrated **SignalR** and **Firebase Realtime Database** to push real-time location updates of trains/consignments and broadcast system notifications to clients instantaneously.
- **Automated CI/CD Pipeline:** Configured **GitHub Actions** for continuous integration and deployment. The pipeline automatically builds Docker images and deploys containerized applications to a Linux (DigitalOcean) server, ensuring minimal downtime.

## Tech Stack

**Core & Framework**

- .NET 8 / C# 12
- ASP.NET Core Web API
- Entity Framework Core (Code-First)

**Database & Caching**

- PostgreSQL (Relational Data)
- Firebase Realtime Database (Geo-tracking Data)
- In-Memory Cache (Graph Data caching)

**Integrations & Background Services**

- **Payment Gateways:** VNPay & PayOS (with Webhook handling)
- **Real-time:** SignalR
- **Media Storage:** Cloudinary
- **Background Jobs:** Quartz.NET / Native Background Services (for auto-handling payment status, shipment status, train scheduling, email sending)
- **Authentication:** JWT (JSON Web Tokens) with Role-based access control.

**DevOps**

- Docker & Docker Compose
- GitHub Actions
- Linux (DigitalOcean)

## Project Structure

```
MetroShip_BE/
├── src/
│   ├── WebAPI/           # Controllers, Middlewares, SignalR Hubs, Startup configuration
│   ├── Service/          # Business logic, Graph algorithms, DTOs, Validation (FluentValidation)
│   ├── Repository/       # EF Core DbContext, Migrations, Repositories, Unit of Work
│   ├── Utility/          # Global constants, Enums, Exceptions, Helpers
│   └── Test/             # Unit testing configurations
├── doc/                  # Postman collections, database seed scripts, Simulator scripts
├── .github/workflows/    # CI/CD pipeline definitions
```

## Core Modules

1. **Shipment & Parcel Management:** Handles the entire lifecycle of a parcel, from Awaiting Drop-off to Delivered, including volumetric weight calculations and category-based insurance policies.
2. **Itinerary & Scheduling:** Manages Metro Trains, Stations, and Routes. Dynamically assigns shipments to available Metro Time Slots ensuring optimal capacity utilization.
3. **Transaction & Payment:** Secure processing of transactions via third-party APIs, utilizing database transactions to prevent race conditions during webhook callbacks.
4. **GPS Simulator:** Includes a custom web-based simulator (`doc/metro-gps-simulator`) to mock real-time train movements and trigger live tracking updates.

## Getting Started (Collaboration & Local Setup)

**Prerequisites:**

- .NET 8 SDK
- Docker Desktop
- Git

**1. Clone the Repository (Development Branch)**
For collaboration, please ensure you clone the repository and switch to the `dev` branch:

```
git clone https://github.com/quangtrandinhminh/MetroShip_BE.git
cd MetroShip_BE
git checkout dev
```

**2. Environment Configuration (.env)**
The application relies heavily on Environment Variables for secure configuration of third-party services. Create a `.env` file in the root directory and populate it with the following required keys:

```
# System Settings
APP_NAME=MetroShip
SYSTEM_DOMAIN=http://localhost:5000
SYSTEM_SECRET_KEY=your_jwt_secret_key_here
SYSTEM_SECRET_CODE=your_secret_code_here

# VNPay Integration
VNPAY_TMN_CODE=
VNPAY_HASH_SECRET=
VNPAY_BASE_URL=
VNPAY_VERSION=
VNPAY_CURR_CODE=
VNPAY_LOCALE=

# PayOS Integration
PAYOS_API_KEY=
PAYOS_CHECKSUM_KEY=
PAYOS_CLIENT_ID=

# VietQR Integration
VIETQR_API_KEY=
VIETQR_CLIENT_ID=

# SMTP / Email Configuration
SMTP_HOST=
SMTP_PORT=
SMTP_ENABLE_SSL=true
SMTP_USING_CREDENTIAL=true
SMTP_USERNAME=
SMTP_PASSWORD=
SMTP_FROM_ADDRESS=
SMTP_FROM_DISPLAY_NAME=MetroShip Support

# Other Third-party Integrations
GOOGLE_CLIENT_ID=
GOOGLE_CLIENT_SECRET=
CLOUDINARY_URL=
TWILIO_ACCOUNT_SID=
TWILIO_AUTH_TOKEN=
TWILIO_PHONE_NUMBER=
```

**3. Run with Docker Compose**
The easiest way to spin up the database and the API locally (Docker will automatically pick up your `.env` file):

```
docker-compose up -d --build
```

**4. Apply Database Migrations (If running without Docker)**
If you are running the project via IIS Express or Kestrel directly, ensure your local database is running and apply EF Core migrations:

```
dotnet ef database update --project src/Repository --startup-project src/WebAPI
```

## API Documentation

Once the application is running, the Swagger UI can be accessed at:
`http://localhost:<port>/swagger`

*(A complete Postman Collection is also available in the `/doc` directory).*
