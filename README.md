# Microservice_Project

A backend e-commerce system built as a set of independently deployable microservices on **.NET 10 / ASP.NET Core**. The solution demonstrates a polyglot-persistence microservices architecture with an API gateway, a Backend-for-Frontend (BFF) aggregator, synchronous communication over REST and gRPC, and asynchronous communication over RabbitMQ, all runnable through Docker Compose.

> Languages: **[English](README.md)** · [فارسی (Persian)](README.fa.md)

<p>
  <img alt=".NET" src="https://img.shields.io/badge/.NET-10-512BD4?logo=dotnet&logoColor=white">
  <img alt="C#" src="https://img.shields.io/badge/C%23-239120?logo=csharp&logoColor=white">
  <img alt="ASP.NET Core" src="https://img.shields.io/badge/ASP.NET%20Core-512BD4?logo=dotnet&logoColor=white">
  <img alt="Docker" src="https://img.shields.io/badge/Docker-2496ED?logo=docker&logoColor=white">
  <img alt="MongoDB" src="https://img.shields.io/badge/MongoDB-47A248?logo=mongodb&logoColor=white">
  <img alt="Redis" src="https://img.shields.io/badge/Redis-DC382D?logo=redis&logoColor=white">
  <img alt="PostgreSQL" src="https://img.shields.io/badge/PostgreSQL-4169E1?logo=postgresql&logoColor=white">
  <img alt="SQL Server" src="https://img.shields.io/badge/SQL%20Server-CC2927?logo=microsoftsqlserver&logoColor=white">
  <img alt="RabbitMQ" src="https://img.shields.io/badge/RabbitMQ-FF6600?logo=rabbitmq&logoColor=white">
  <img alt="gRPC" src="https://img.shields.io/badge/gRPC-2496ED?logo=grpc&logoColor=white">
  <img alt="Ocelot" src="https://img.shields.io/badge/Ocelot-API%20Gateway-5C2D91">
</p>

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Services](#services)
- [Service Communication](#service-communication)
- [Project Structure](#project-structure)
- [Architectural Layers](#architectural-layers)
- [Technology Stack](#technology-stack)
- [Features](#features)
- [Databases & Infrastructure](#databases--infrastructure)
- [API Overview](#api-overview)
- [Configuration](#configuration)
- [Docker & Containerization](#docker--containerization)
- [Getting Started](#getting-started)
- [Testing](#testing)
- [License](#license)

---

## Overview

`Microservice_Project` implements the core backend of an online shopping platform, split into focused services that each own their data and lifecycle. Instead of a single monolith, the domain is decomposed into **Catalog**, **Basket**, **Discount**, and **Ordering** services, fronted by an **Ocelot** API gateway and a **Shopping Aggregator** BFF.

The project demonstrates several concepts that are central to distributed .NET systems:

- **Database-per-service** with polyglot persistence (MongoDB, Redis, PostgreSQL, SQL Server).
- **Synchronous inter-service calls** using gRPC (Basket → Discount) and HTTP (aggregator/gateway → services).
- **Asynchronous, event-driven integration** using RabbitMQ with MassTransit (Basket → Ordering).
- **Clean Architecture** and **CQRS** in the Ordering service (MediatR, FluentValidation, AutoMapper, pipeline behaviours).
- **API gateway routing** and a **BFF aggregation** layer.
- **Containerized deployment** through Docker and Docker Compose.

The intended use case is as a reference/learning implementation of a microservices backend: a realistic checkout flow (browse catalog → build basket → apply discounts → checkout → create order) that crosses service boundaries through both request/response and messaging.

---

## Architecture

The system follows a **microservices architecture**. A client reaches the services through the **Ocelot API gateway** or the **Shopping Aggregator** BFF. Each service owns a dedicated datastore. The checkout flow is completed asynchronously via a message broker.

```mermaid
flowchart LR
    Client([Client])

    Client --> Gateway[OcelotApiGw<br/>API Gateway]
    Client --> Agg[Shopping.Aggregator<br/>BFF]

    Gateway -->|HTTP| Catalog[Catalog.Api]
    Gateway -->|HTTP| Basket[Basket.Api]

    Agg -->|HTTP| Catalog
    Agg -->|HTTP| Basket
    Agg -->|HTTP| Ordering[Ordering.Api]

    Basket -->|gRPC| DiscountGrpc[Discount.Grpc]
    DiscountApi[Discount.Api]

    Catalog --> CatalogDB[(MongoDB<br/>catalogdb)]
    Basket --> BasketDB[(Redis<br/>basketdb)]
    DiscountGrpc --> DiscountDB[(PostgreSQL<br/>discountdb)]
    DiscountApi --> DiscountDB
    Ordering --> OrderDB[(SQL Server<br/>orderdb)]

    Basket -->|publish BasketCheckoutEvent| MQ{{RabbitMQ}}
    MQ -->|consume| Ordering
```

Key architectural characteristics:

- **API Gateway (Ocelot):** Single entry point that routes upstream requests to the Catalog and Basket services.
- **BFF (Shopping.Aggregator):** Aggregates Catalog, Basket, and Ordering data via typed `HttpClient`s for client-friendly composition.
- **Database per service:** Each service keeps its own database; no shared schema between services.
- **Mixed communication:** gRPC for low-latency internal calls, HTTP for gateway/BFF, and RabbitMQ for decoupled event delivery.
- **Clean Architecture in Ordering:** Domain, Application, Infrastructure, and API layers with CQRS.

---

## Services

| Service | Responsibility | API Style | Database | Key Dependencies |
|---|---|---|---|---|
| **Catalog.Api** | Product catalog CRUD and category queries | REST | MongoDB | `MongoDB.Driver`, Repository pattern |
| **Basket.Api** | Shopping cart management and checkout initiation | REST | Redis | `StackExchange.Redis` cache, gRPC client, MassTransit, AutoMapper |
| **Discount.Api** | Coupon/discount CRUD | REST | PostgreSQL | `Dapper`, `Npgsql` |
| **Discount.Grpc** | Discount lookups for internal callers | gRPC | PostgreSQL | `Grpc.AspNetCore`, `Dapper`, `Npgsql` |
| **Ordering.Api** | Order checkout, listing, update, delete | REST + event consumer | SQL Server | EF Core, MediatR (CQRS), MassTransit, AutoMapper, FluentValidation |
| **OcelotApiGw** | API gateway / request routing | HTTP | – | `Ocelot`, `Ocelot.Cache.CacheManager` |
| **Shopping.Aggregator** | BFF aggregating Catalog/Basket/Ordering | REST | – | typed `HttpClient` |

### Catalog.Api
Manages products stored in MongoDB. Exposes REST endpoints to list products, fetch by id, filter by category, and create/update/delete products. Uses a repository (`IProductRepository`) over `MongoDB.Driver`.

### Basket.Api
Stores each user's shopping cart in Redis using the distributed cache abstraction. When a basket is updated, it calls **Discount.Grpc** through a gRPC client to apply coupon amounts to item prices. On checkout it maps the basket to a `BasketCheckoutEvent` and publishes it to RabbitMQ via MassTransit, then clears the basket.

### Discount.Api & Discount.Grpc
Both manage coupons persisted in PostgreSQL via **Dapper** and **Npgsql**, and both migrate/seed the database on startup. `Discount.Api` exposes a REST interface; `Discount.Grpc` exposes the same domain over gRPC (defined in `Protos/Discount.proto`) and is the one consumed internally by Basket.

### Ordering.Api
Built with **Clean Architecture** and **CQRS**. The API layer delegates to MediatR commands/queries in the Application layer (`CheckoutOrder`, `UpdateOrder`, `DeleteOrder`, `GetOrdersList`). Requests pass through validation and exception pipeline behaviours. Orders are persisted to SQL Server through EF Core (`OrderContext`) with automatic migration and seeding. The service also hosts a **MassTransit consumer** (`BasketCheckoutConsumer`) that turns incoming `BasketCheckoutEvent` messages into checkout commands.

### OcelotApiGw
An **Ocelot** gateway that routes upstream paths (Catalog and Basket routes) to the corresponding downstream services, with an in-memory cache handle registered through `Ocelot.Cache.CacheManager`.

### Shopping.Aggregator
A **Backend-for-Frontend** that composes data from Catalog, Basket, and Ordering using strongly-typed `HttpClient`s configured from `ApiSettings`.

---

## Service Communication

The solution uses three communication mechanisms:

| Mechanism | From → To | Purpose |
|---|---|---|
| **HTTP / REST** | Client → OcelotApiGw → Catalog/Basket | External routing through the gateway |
| **HTTP / REST** | Shopping.Aggregator → Catalog/Basket/Ordering | BFF data aggregation |
| **gRPC** | Basket.Api → Discount.Grpc | Low-latency discount lookup during basket updates |
| **AMQP (RabbitMQ + MassTransit)** | Basket.Api → Ordering.Api | Asynchronous checkout via `BasketCheckoutEvent` |

**Checkout flow (event-driven):**

1. `Basket.Api` receives a checkout request and builds a `BasketCheckoutEvent`.
2. The event is published to RabbitMQ on the `basketcheckout-queue` (constant defined in `EventBus.Messages`).
3. `Ordering.Api`'s `BasketCheckoutConsumer` consumes the event, maps it to a `CheckoutOrderCommand`, and dispatches it through MediatR to create the order.

The shared integration event contracts live in the `BuildingBlocks/EventBus.Messages` project so producers and consumers reference the same message types.

---

## Project Structure

```text
Microservice_Project/
├── ApiGateways/
│   ├── OcelotApiGw/              # Ocelot API gateway (routing + cache)
│   └── Shopping.Aggregator/      # BFF aggregating Catalog/Basket/Ordering
├── BuildingBlocks/
│   └── EventBus.Messages/        # Shared integration events & constants
├── Services/
│   ├── Catalog/
│   │   └── Catalog.Api/          # Product catalog (MongoDB)
│   ├── Basket/
│   │   └── Basket.Api/           # Shopping cart (Redis, gRPC client, MassTransit)
│   ├── Discount/
│   │   ├── Discount.Api/         # Coupon REST API (PostgreSQL, Dapper)
│   │   └── Discount.Grpc/        # Coupon gRPC service (PostgreSQL, Dapper)
│   └── Ordering/
│       ├── Ordering.Api/         # Order API + RabbitMQ consumer
│       ├── Ordering.Application/  # CQRS: commands, queries, behaviours, mappings
│       ├── Ordering.Domain/       # Domain entities
│       ├── Ordering.Infrastructure/ # EF Core persistence, repositories, email
│       └── Common/
│           └── Ordering.Common.Domain/ # Shared domain base types
├── docker-compose.yml            # Service & infrastructure definitions
├── docker-compose.override.yml   # Ports, volumes, environment
├── Microservice_Project.slnx     # Solution file
└── README.md
```

---

## Architectural Layers

The **Ordering** service is organized using Clean Architecture. Each layer has a single responsibility and depends only inward:

| Layer | Project | Responsibility |
|---|---|---|
| **API** | `Ordering.Api` | HTTP endpoints, RabbitMQ consumer, DI composition |
| **Application** | `Ordering.Application` | CQRS commands/queries (MediatR), validation, mapping, pipeline behaviours |
| **Domain** | `Ordering.Domain`, `Ordering.Common.Domain` | Entities and shared domain base types |
| **Infrastructure** | `Ordering.Infrastructure` | EF Core `DbContext`, repositories, migrations/seed, email service |

The other services (Catalog, Basket, Discount) follow a lighter, self-contained layout (controllers, entities, repository, context) appropriate to their smaller scope.

---

## Technology Stack

**Languages**
- C# (primary)
- Protocol Buffers (gRPC contracts)
- Dockerfile

**Frameworks & Runtime**
- .NET 10 / ASP.NET Core
- Entity Framework Core (Ordering)
- Dapper (Discount)

**Databases**
- MongoDB — Catalog
- Redis — Basket
- PostgreSQL — Discount
- SQL Server — Ordering

**Messaging**
- RabbitMQ with MassTransit

**Communication**
- REST / HTTP
- gRPC

**Architecture & Patterns**
- Microservices, API Gateway (Ocelot), BFF (Aggregator)
- Clean Architecture, CQRS (MediatR), Repository pattern, Dependency Injection
- Event-driven integration

**Libraries**
- MediatR, AutoMapper, FluentValidation
- Swashbuckle / Swagger (OpenAPI)
- Newtonsoft.Json

**Containerization & Tooling**
- Docker, Docker Compose
- pgAdmin, Portainer (management UIs)

---

## Features

- Product catalog with CRUD and category filtering (MongoDB).
- User shopping baskets backed by Redis distributed cache.
- Real-time discount application to basket items via gRPC.
- Coupon management exposed over both REST and gRPC.
- Order checkout, listing, update, and deletion using CQRS.
- Event-driven checkout: baskets publish events consumed by the Ordering service.
- Centralized routing through an Ocelot API gateway.
- Data aggregation through a Backend-for-Frontend service.
- Swagger/OpenAPI documentation for the HTTP APIs.
- Full local orchestration with Docker Compose, including databases and management tools.

---

## Databases & Infrastructure

| Component | Image | Used by | Purpose |
|---|---|---|---|
| **catalogdb** | `mongo` | Catalog.Api | Product documents |
| **basketdb** | `redis` | Basket.Api | Shopping cart cache |
| **discountdb** | `postgres` | Discount.Api / Discount.Grpc | Coupons |
| **orderdb** | `mcr.microsoft.com/mssql/server` | Ordering.Api | Orders |
| **rabbitmq** | `rabbitmq` | Basket ↔ Ordering | Message broker for integration events |
| **pgadmin** | `dpage/pgadmin4` | – | PostgreSQL management UI |
| **portainer** | `portainer/portainer-ce` | – | Container management UI |

Each application service owns exactly one database, following the database-per-service principle. RabbitMQ provides the asynchronous backbone between Basket and Ordering.

---

## API Overview

APIs are documented with **Swagger / OpenAPI** (available in the Development environment). Major endpoints:

**Catalog.Api** — `api/v1/Product`
- `GET /` – list products
- `GET /{id}` – product by id
- `GET /GetProductsByCategory/{category}` – filter by category
- `POST /` · `PUT /` · `DELETE /{id}`

**Basket.Api** — `api/v1/Basket`
- `GET /{userName}` – get a user's basket
- `POST /` – create/update basket (applies discounts via gRPC)
- `DELETE /{userName}` – delete basket
- `POST /Checkout` – publish checkout event

**Discount.Api** — `api/v1/Discount`
- `GET /{productName}` · `POST /` · `PUT /` · `DELETE /{productName}`

**Discount.Grpc** — `DiscountProto` service
- `GetDiscount`, `CreateDiscount`, `UpdateDiscount`, `DeleteDiscount`

**Ordering.Api** — `api/v1/Order`
- `GET /{userName}` – orders by user
- `POST /` – checkout order
- `PUT /` – update order
- `DELETE /{id}` – delete order

---

## Configuration

Configuration is provided through `appsettings.json` files and environment variables (set in `docker-compose.override.yml`). Do **not** commit real credentials — use placeholders when documenting.

Key settings per service:

| Service | Setting | Example (placeholder) |
|---|---|---|
| Catalog.Api | `MongoSettings:ConnectionString` | `mongodb://catalogdb:27017` |
| Basket.Api | `CacheSettings:ConnectionString` | `basketdb:6379` |
| Basket.Api | `GrpcSettings:DiscountUrl` | `http://discount.grpc` |
| Basket.Api / Ordering.Api | `EventBusSettings:HostAddress` | `amqp://YOUR_USER:YOUR_PASSWORD@rabbitmq:5672` |
| Discount.Api / Discount.Grpc | `ConnectionSetting:ConnectionString` | `Server=discountdb;Port=5432;Database=DiscountDb;Username=YOUR_USER;Password=YOUR_PASSWORD;` |
| Ordering.Api | `ConnectionStrings:OrderingConnectionString` | `Server=orderdb;Database=OrderDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;` |

Example connection string placeholders:

```text
YOUR_CONNECTION_STRING
YOUR_USER
YOUR_PASSWORD
```

> When running through Docker Compose, container-to-container hostnames (`catalogdb`, `basketdb`, `discountdb`, `orderdb`, `rabbitmq`, `discount.grpc`) are used instead of `localhost`.

---

## Docker & Containerization

Every application service has its own `Dockerfile` (multi-stage build on the .NET 10 SDK/ASP.NET runtime images). `docker-compose.yml` defines the services and infrastructure, and `docker-compose.override.yml` supplies ports, volumes, and environment variables.

Published host ports (from `docker-compose.override.yml`):

| Component | Host Port | Container Port |
|---|---|---|
| Catalog.Api | 8000 | 8080 |
| Basket.Api | 8001 | 8080 |
| Discount.Api | 8002 | 8080 |
| Discount.Grpc | 8003 | 8080 |
| Ordering.Api | 8004 | 8080 |
| Shopping.Aggregator | 8005 | 8080 |
| OcelotApiGw | 8010 | 80 |
| MongoDB | 27017 | 27017 |
| Redis | 6379 | 6379 |
| PostgreSQL | 5432 | 5432 |
| SQL Server | 1433 | 1433 |
| RabbitMQ | 5672 / 15672 | 5672 / 15672 |
| pgAdmin | 5050 | 80 |
| Portainer | 10000 / 10001 | 8000 / 9000 |

Volumes: `mongo_data`, `postgres_data`, `pgadmin_data`, `portainer_data`.

Start everything:

```bash
docker compose up -d
```

Stop and remove containers:

```bash
docker compose down
```

Rebuild images after code changes:

```bash
docker compose up -d --build
```

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) and Docker Compose

### 1. Clone the repository

```bash
git clone https://github.com/MohammadHasanp/Microservice_Project.git
cd Microservice_Project
```

### 2. Configuration

Review the `appsettings.json` of each service and the environment variables in `docker-compose.override.yml`. Replace any credentials with your own values.

### 3. Run with Docker Compose (recommended)

```bash
docker compose up -d --build
```

This starts the databases, RabbitMQ, management tools, and all services. Databases for Discount and Ordering are migrated/seeded automatically on startup.

Once running, explore each API through Swagger, e.g. `http://localhost:8000/swagger` (Catalog), `http://localhost:8001/swagger` (Basket), and the RabbitMQ management UI at `http://localhost:15672`.

### 4. Run a single service locally (optional)

```bash
dotnet restore
dotnet run --project Services/Catalog/Catalog.Api/Catalog.Api.csproj
```

When running outside Docker, point the connection settings at your local database/broker hosts (`localhost`).

### 5. Build the whole solution

```bash
dotnet build Microservice_Project.slnx
```

---

## Testing

This repository does not currently include automated test projects. Contributions adding unit or integration tests are welcome.

---

**Repository owner:** [@MohammadHasanp](https://github.com/MohammadHasanp)
