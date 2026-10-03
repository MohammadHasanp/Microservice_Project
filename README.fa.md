<div dir="rtl">

# Microservice_Project

یک سیستم بک‌اند فروشگاهی که به‌صورت مجموعه‌ای از میکروسرویس‌های مستقل و قابل استقرار روی **.NET 10 / ASP.NET Core** ساخته شده است. این پروژه یک معماری میکروسرویسی با پایگاه‌داده‌های متنوع (polyglot persistence) را نشان می‌دهد که شامل یک API Gateway، یک لایه‌ی تجمیع‌کننده‌ی Backend-for-Frontend (BFF)، ارتباط همگام از طریق REST و gRPC، و ارتباط ناهمگام از طریق RabbitMQ است و به‌طور کامل با Docker Compose اجرا می‌شود.

> زبان‌ها: [English](README.md) · **[فارسی (Persian)](README.fa.md)**

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

## فهرست مطالب

- [معرفی کلی](#معرفی-کلی)
- [معماری](#معماری)
- [سرویس‌ها](#سرویسها)
- [ارتباط بین سرویس‌ها](#ارتباط-بین-سرویسها)
- [ساختار پروژه](#ساختار-پروژه)
- [لایه‌های معماری](#لایههای-معماری)
- [پشته‌ی فناوری](#پشتهی-فناوری)
- [ویژگی‌ها](#ویژگیها)
- [پایگاه‌داده‌ها و زیرساخت](#پایگاهدادهها-و-زیرساخت)
- [نمای کلی API](#نمای-کلی-api)
- [پیکربندی](#پیکربندی)
- [Docker و کانتینرسازی](#docker-و-کانتینرسازی)
- [شروع به کار](#شروع-به-کار)
- [تست](#تست)
- [مجوز](#مجوز)

---

## معرفی کلی

پروژه‌ی `Microservice_Project` هسته‌ی بک‌اند یک پلتفرم خرید آنلاین را پیاده‌سازی می‌کند که به سرویس‌های متمرکز و مجزا تقسیم شده و هر سرویس مالک داده و چرخه‌ی حیات خود است. به‌جای یک برنامه‌ی یکپارچه (monolith)، دامنه به سرویس‌های **Catalog**، **Basket**، **Discount** و **Ordering** تقسیم شده که پشت یک API Gateway مبتنی بر **Ocelot** و یک BFF با نام **Shopping Aggregator** قرار گرفته‌اند.

این پروژه چند مفهوم کلیدی در سیستم‌های توزیع‌شده‌ی .NET را نشان می‌دهد:

- **پایگاه‌داده به‌ازای هر سرویس (database-per-service)** با ماندگاری متنوع (MongoDB، Redis، PostgreSQL، SQL Server).
- **فراخوانی‌های همگام بین سرویس‌ها** با استفاده از gRPC (از Basket به Discount) و HTTP (از aggregator/gateway به سرویس‌ها).
- **یکپارچه‌سازی ناهمگام و رویدادمحور** با استفاده از RabbitMQ و MassTransit (از Basket به Ordering).
- **Clean Architecture** و **CQRS** در سرویس Ordering (به کمک MediatR، FluentValidation، AutoMapper و pipeline behaviours).
- **مسیریابی از طریق API Gateway** و یک لایه‌ی تجمیع **BFF**.
- **استقرار کانتینری** از طریق Docker و Docker Compose.

کاربرد موردنظر این پروژه، به‌عنوان یک پیاده‌سازی مرجع/آموزشی از یک بک‌اند میکروسرویسی است: یک جریان واقعی خرید (مرور کاتالوگ ← ساخت سبد ← اعمال تخفیف ← تسویه‌حساب ← ایجاد سفارش) که با ترکیبی از الگوی درخواست/پاسخ و پیام‌رسانی از مرز سرویس‌ها عبور می‌کند.

---

## معماری

این سیستم از یک **معماری میکروسرویسی** پیروی می‌کند. کلاینت از طریق **Ocelot API Gateway** یا BFF با نام **Shopping Aggregator** به سرویس‌ها دسترسی پیدا می‌کند. هر سرویس مالک یک پایگاه‌داده‌ی اختصاصی است و جریان تسویه‌حساب به‌صورت ناهمگام از طریق یک message broker تکمیل می‌شود.

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

ویژگی‌های کلیدی معماری:

- **API Gateway (Ocelot):** نقطه‌ی ورود واحد که درخواست‌های ورودی را به سرویس‌های Catalog و Basket مسیریابی می‌کند.
- **BFF (Shopping.Aggregator):** داده‌های Catalog، Basket و Ordering را از طریق `HttpClient`های نوع‌دار برای ترکیب مناسب سمت کلاینت تجمیع می‌کند.
- **پایگاه‌داده به‌ازای هر سرویس:** هر سرویس پایگاه‌داده‌ی خودش را دارد و هیچ اسکیمای مشترکی بین سرویس‌ها وجود ندارد.
- **ارتباط ترکیبی:** gRPC برای فراخوانی‌های داخلی کم‌تأخیر، HTTP برای gateway/BFF و RabbitMQ برای تحویل رویدادِ جداشده (decoupled).
- **Clean Architecture در Ordering:** لایه‌های Domain، Application، Infrastructure و API به همراه CQRS.

---

## سرویس‌ها

| سرویس | مسئولیت | سبک API | پایگاه‌داده | وابستگی‌های کلیدی |
|---|---|---|---|---|
| **Catalog.Api** | مدیریت (CRUD) کاتالوگ محصولات و جست‌وجوی دسته‌بندی | REST | MongoDB | `MongoDB.Driver`، Repository pattern |
| **Basket.Api** | مدیریت سبد خرید و آغاز تسویه‌حساب | REST | Redis | کش `StackExchange.Redis`، gRPC client، MassTransit، AutoMapper |
| **Discount.Api** | مدیریت (CRUD) کوپن/تخفیف | REST | PostgreSQL | `Dapper`، `Npgsql` |
| **Discount.Grpc** | جست‌وجوی تخفیف برای فراخوانان داخلی | gRPC | PostgreSQL | `Grpc.AspNetCore`، `Dapper`، `Npgsql` |
| **Ordering.Api** | تسویه، فهرست، به‌روزرسانی و حذف سفارش | REST + مصرف‌کننده‌ی رویداد | SQL Server | EF Core، MediatR (CQRS)، MassTransit، AutoMapper، FluentValidation |
| **OcelotApiGw** | API Gateway / مسیریابی درخواست | HTTP | – | `Ocelot`، `Ocelot.Cache.CacheManager` |
| **Shopping.Aggregator** | BFF برای تجمیع Catalog/Basket/Ordering | REST | – | `HttpClient` نوع‌دار |

### Catalog.Api
محصولات ذخیره‌شده در MongoDB را مدیریت می‌کند. نقاط پایانی REST برای فهرست‌کردن محصولات، دریافت بر اساس شناسه، فیلتر بر اساس دسته‌بندی و ایجاد/به‌روزرسانی/حذف محصولات ارائه می‌دهد. از یک repository (`IProductRepository`) روی `MongoDB.Driver` استفاده می‌کند.

### Basket.Api
سبد خرید هر کاربر را در Redis با استفاده از انتزاع distributed cache ذخیره می‌کند. هنگام به‌روزرسانی سبد، از طریق یک gRPC client سرویس **Discount.Grpc** را فراخوانی می‌کند تا مبالغ کوپن را روی قیمت اقلام اعمال کند. هنگام تسویه‌حساب، سبد را به یک `BasketCheckoutEvent` نگاشت کرده و آن را از طریق MassTransit روی RabbitMQ منتشر می‌کند و سپس سبد را پاک می‌کند.

### Discount.Api و Discount.Grpc
هر دو کوپن‌ها را در PostgreSQL از طریق **Dapper** و **Npgsql** نگهداری می‌کنند و هر دو در زمان راه‌اندازی، پایگاه‌داده را migrate/seed می‌کنند. سرویس `Discount.Api` یک رابط REST ارائه می‌دهد؛ سرویس `Discount.Grpc` همان دامنه را روی gRPC (تعریف‌شده در `Protos/Discount.proto`) ارائه می‌کند و همان سرویسی است که به‌صورت داخلی توسط Basket مصرف می‌شود.

### Ordering.Api
با **Clean Architecture** و **CQRS** ساخته شده است. لایه‌ی API به دستورها/کوئری‌های MediatR در لایه‌ی Application واگذار می‌کند (`CheckoutOrder`، `UpdateOrder`، `DeleteOrder`، `GetOrdersList`). درخواست‌ها از میان pipeline behaviourهای اعتبارسنجی و مدیریت استثنا عبور می‌کنند. سفارش‌ها از طریق EF Core (`OrderContext`) در SQL Server با migration و seed خودکار نگهداری می‌شوند. این سرویس همچنین میزبان یک **مصرف‌کننده‌ی MassTransit** (`BasketCheckoutConsumer`) است که پیام‌های ورودی `BasketCheckoutEvent` را به دستورهای تسویه‌حساب تبدیل می‌کند.

### OcelotApiGw
یک gateway مبتنی بر **Ocelot** که مسیرهای ورودی (مسیرهای Catalog و Basket) را به سرویس‌های مقصد مربوطه مسیریابی می‌کند و از یک cache handle درون‌حافظه‌ای ثبت‌شده از طریق `Ocelot.Cache.CacheManager` استفاده می‌کند.

### Shopping.Aggregator
یک **Backend-for-Frontend** که داده‌ها را از Catalog، Basket و Ordering با استفاده از `HttpClient`های نوع‌دارِ پیکربندی‌شده از `ApiSettings` ترکیب می‌کند.

---

## ارتباط بین سرویس‌ها

این پروژه از سه مکانیزم ارتباطی استفاده می‌کند:

| مکانیزم | از ← به | هدف |
|---|---|---|
| **HTTP / REST** | Client ← OcelotApiGw ← Catalog/Basket | مسیریابی خارجی از طریق gateway |
| **HTTP / REST** | Shopping.Aggregator ← Catalog/Basket/Ordering | تجمیع داده در BFF |
| **gRPC** | Basket.Api ← Discount.Grpc | جست‌وجوی کم‌تأخیر تخفیف هنگام به‌روزرسانی سبد |
| **AMQP (RabbitMQ + MassTransit)** | Basket.Api ← Ordering.Api | تسویه‌حساب ناهمگام از طریق `BasketCheckoutEvent` |

**جریان تسویه‌حساب (رویدادمحور):**

1. سرویس `Basket.Api` یک درخواست تسویه‌حساب دریافت می‌کند و یک `BasketCheckoutEvent` می‌سازد.
2. این رویداد روی صف `basketcheckout-queue` در RabbitMQ منتشر می‌شود (ثابتِ تعریف‌شده در `EventBus.Messages`).
3. مصرف‌کننده‌ی `BasketCheckoutConsumer` در `Ordering.Api` این رویداد را مصرف می‌کند، آن را به یک `CheckoutOrderCommand` نگاشت کرده و از طریق MediatR برای ایجاد سفارش ارسال می‌کند.

قراردادهای رویدادهای یکپارچه‌سازی مشترک در پروژه‌ی `BuildingBlocks/EventBus.Messages` قرار دارند تا تولیدکننده و مصرف‌کننده به انواع پیام یکسانی ارجاع دهند.

---

## ساختار پروژه

<div dir="ltr">

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

</div>

---

## لایه‌های معماری

سرویس **Ordering** با استفاده از Clean Architecture سازمان‌دهی شده است. هر لایه مسئولیت واحدی دارد و تنها به سمت درون وابسته است:

| لایه | پروژه | مسئولیت |
|---|---|---|
| **API** | `Ordering.Api` | نقاط پایانی HTTP، مصرف‌کننده‌ی RabbitMQ، ترکیب DI |
| **Application** | `Ordering.Application` | دستورها/کوئری‌های CQRS (MediatR)، اعتبارسنجی، نگاشت، pipeline behaviours |
| **Domain** | `Ordering.Domain`، `Ordering.Common.Domain` | موجودیت‌ها و انواع پایه‌ی مشترک دامنه |
| **Infrastructure** | `Ordering.Infrastructure` | `DbContext` مبتنی بر EF Core، repositoryها، migration/seed، سرویس ایمیل |

سرویس‌های دیگر (Catalog، Basket، Discount) از یک چیدمان سبک‌تر و خودبسنده (controllerها، entityها، repository، context) متناسب با دامنه‌ی کوچک‌ترشان پیروی می‌کنند.

---

## پشته‌ی فناوری

**زبان‌ها**
- C# (اصلی)
- Protocol Buffers (قراردادهای gRPC)
- Dockerfile

**فریم‌ورک‌ها و Runtime**
- .NET 10 / ASP.NET Core
- Entity Framework Core (Ordering)
- Dapper (Discount)

**پایگاه‌داده‌ها**
- MongoDB — کاتالوگ
- Redis — سبد خرید
- PostgreSQL — تخفیف
- SQL Server — سفارش

**پیام‌رسانی**
- RabbitMQ به همراه MassTransit

**ارتباطات**
- REST / HTTP
- gRPC

**معماری و الگوها**
- میکروسرویس، API Gateway (Ocelot)، BFF (Aggregator)
- Clean Architecture، CQRS (MediatR)، Repository pattern، Dependency Injection
- یکپارچه‌سازی رویدادمحور

**کتابخانه‌ها**
- MediatR، AutoMapper، FluentValidation
- Swashbuckle / Swagger (OpenAPI)
- Newtonsoft.Json

**کانتینرسازی و ابزارها**
- Docker، Docker Compose
- pgAdmin، Portainer (رابط‌های مدیریتی)

---

## ویژگی‌ها

- کاتالوگ محصولات با CRUD و فیلتر بر اساس دسته‌بندی (MongoDB).
- سبد خرید کاربران با پشتیبانی از distributed cache در Redis.
- اعمال بلادرنگ تخفیف روی اقلام سبد از طریق gRPC.
- مدیریت کوپن ارائه‌شده هم روی REST و هم روی gRPC.
- تسویه، فهرست، به‌روزرسانی و حذف سفارش با استفاده از CQRS.
- تسویه‌حساب رویدادمحور: سبدها رویدادهایی منتشر می‌کنند که توسط سرویس Ordering مصرف می‌شوند.
- مسیریابی متمرکز از طریق Ocelot API Gateway.
- تجمیع داده از طریق یک سرویس Backend-for-Frontend.
- مستندسازی Swagger/OpenAPI برای APIهای HTTP.
- ارکستراسیون کامل محلی با Docker Compose، شامل پایگاه‌داده‌ها و ابزارهای مدیریتی.

---

## پایگاه‌داده‌ها و زیرساخت

| مؤلفه | Image | مورد استفاده توسط | هدف |
|---|---|---|---|
| **catalogdb** | `mongo` | Catalog.Api | اسناد محصولات |
| **basketdb** | `redis` | Basket.Api | کش سبد خرید |
| **discountdb** | `postgres` | Discount.Api / Discount.Grpc | کوپن‌ها |
| **orderdb** | `mcr.microsoft.com/mssql/server` | Ordering.Api | سفارش‌ها |
| **rabbitmq** | `rabbitmq` | Basket ↔ Ordering | message broker برای رویدادهای یکپارچه‌سازی |
| **pgadmin** | `dpage/pgadmin4` | – | رابط مدیریت PostgreSQL |
| **portainer** | `portainer/portainer-ce` | – | رابط مدیریت کانتینرها |

هر سرویس اپلیکیشنی دقیقاً مالک یک پایگاه‌داده است و از اصل database-per-service پیروی می‌کند. RabbitMQ ستون فقرات ناهمگام میان Basket و Ordering را فراهم می‌کند.

---

## نمای کلی API

APIها با **Swagger / OpenAPI** مستند شده‌اند (در محیط Development در دسترس هستند). نقاط پایانی اصلی:

**Catalog.Api** — `api/v1/Product`
- `GET /` – فهرست محصولات
- `GET /{id}` – محصول بر اساس شناسه
- `GET /GetProductsByCategory/{category}` – فیلتر بر اساس دسته‌بندی
- `POST /` · `PUT /` · `DELETE /{id}`

**Basket.Api** — `api/v1/Basket`
- `GET /{userName}` – دریافت سبد یک کاربر
- `POST /` – ایجاد/به‌روزرسانی سبد (اعمال تخفیف از طریق gRPC)
- `DELETE /{userName}` – حذف سبد
- `POST /Checkout` – انتشار رویداد تسویه‌حساب

**Discount.Api** — `api/v1/Discount`
- `GET /{productName}` · `POST /` · `PUT /` · `DELETE /{productName}`

**Discount.Grpc** — سرویس `DiscountProto`
- `GetDiscount`، `CreateDiscount`، `UpdateDiscount`، `DeleteDiscount`

**Ordering.Api** — `api/v1/Order`
- `GET /{userName}` – سفارش‌ها بر اساس کاربر
- `POST /` – تسویه‌حساب سفارش
- `PUT /` – به‌روزرسانی سفارش
- `DELETE /{id}` – حذف سفارش

---

## پیکربندی

پیکربندی از طریق فایل‌های `appsettings.json` و متغیرهای محیطی (تنظیم‌شده در `docker-compose.override.yml`) فراهم می‌شود. اطلاعات محرمانه‌ی واقعی را **commit نکنید** — هنگام مستندسازی از placeholder استفاده کنید.

تنظیمات کلیدی هر سرویس:

| سرویس | تنظیم | نمونه (placeholder) |
|---|---|---|
| Catalog.Api | `MongoSettings:ConnectionString` | `mongodb://catalogdb:27017` |
| Basket.Api | `CacheSettings:ConnectionString` | `basketdb:6379` |
| Basket.Api | `GrpcSettings:DiscountUrl` | `http://discount.grpc` |
| Basket.Api / Ordering.Api | `EventBusSettings:HostAddress` | `amqp://YOUR_USER:YOUR_PASSWORD@rabbitmq:5672` |
| Discount.Api / Discount.Grpc | `ConnectionSetting:ConnectionString` | `Server=discountdb;Port=5432;Database=DiscountDb;Username=YOUR_USER;Password=YOUR_PASSWORD;` |
| Ordering.Api | `ConnectionStrings:OrderingConnectionString` | `Server=orderdb;Database=OrderDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;` |

نمونه‌ی placeholder برای connection string:

<div dir="ltr">

```text
YOUR_CONNECTION_STRING
YOUR_USER
YOUR_PASSWORD
```

</div>

> هنگام اجرا از طریق Docker Compose، به‌جای `localhost` از نام میزبان‌های کانتینر به کانتینر (`catalogdb`، `basketdb`، `discountdb`، `orderdb`، `rabbitmq`، `discount.grpc`) استفاده می‌شود.

---

## Docker و کانتینرسازی

هر سرویس اپلیکیشنی `Dockerfile` مخصوص خود را دارد (build چندمرحله‌ای روی image‌های SDK/ASP.NET runtime نسخه‌ی .NET 10). فایل `docker-compose.yml` سرویس‌ها و زیرساخت را تعریف می‌کند و `docker-compose.override.yml` پورت‌ها، volumeها و متغیرهای محیطی را فراهم می‌کند.

پورت‌های میزبان منتشرشده (از `docker-compose.override.yml`):

| مؤلفه | پورت میزبان | پورت کانتینر |
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

Volumeها: `mongo_data`، `postgres_data`، `pgadmin_data`، `portainer_data`.

راه‌اندازی همه‌چیز:

<div dir="ltr">

```bash
docker compose up -d
```

</div>

توقف و حذف کانتینرها:

<div dir="ltr">

```bash
docker compose down
```

</div>

بازسازی image‌ها پس از تغییر کد:

<div dir="ltr">

```bash
docker compose up -d --build
```

</div>

---

## شروع به کار

### پیش‌نیازها

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Docker](https://www.docker.com/) و Docker Compose

### ۱. کلون کردن مخزن

<div dir="ltr">

```bash
git clone https://github.com/MohammadHasanp/Microservice_Project.git
cd Microservice_Project
```

</div>

### ۲. پیکربندی

فایل‌های `appsettings.json` هر سرویس و متغیرهای محیطی موجود در `docker-compose.override.yml` را بررسی کنید. هرگونه اطلاعات محرمانه را با مقادیر خودتان جایگزین کنید.

### ۳. اجرا با Docker Compose (روش پیشنهادی)

<div dir="ltr">

```bash
docker compose up -d --build
```

</div>

این دستور پایگاه‌داده‌ها، RabbitMQ، ابزارهای مدیریتی و همه‌ی سرویس‌ها را راه‌اندازی می‌کند. پایگاه‌داده‌های Discount و Ordering به‌صورت خودکار هنگام راه‌اندازی migrate/seed می‌شوند.

پس از اجرا، هر API را از طریق Swagger بررسی کنید؛ برای مثال `http://localhost:8000/swagger` (کاتالوگ)، `http://localhost:8001/swagger` (سبد خرید)، و رابط مدیریت RabbitMQ در `http://localhost:15672`.

### ۴. اجرای یک سرویس به‌صورت محلی (اختیاری)

<div dir="ltr">

```bash
dotnet restore
dotnet run --project Services/Catalog/Catalog.Api/Catalog.Api.csproj
```

</div>

هنگام اجرا در خارج از Docker، تنظیمات اتصال را به میزبان‌های محلی پایگاه‌داده/broker (`localhost`) هدایت کنید.

### ۵. build کل solution

<div dir="ltr">

```bash
dotnet build Microservice_Project.slnx
```

</div>

---

## تست

این مخزن در حال حاضر شامل پروژه‌های تست خودکار نیست. مشارکت‌هایی که تست‌های واحد یا یکپارچه اضافه کنند مورد استقبال قرار می‌گیرد.

---

## مجوز

این پروژه تحت [مجوز MIT](LICENSE) منتشر شده است.

**مالک مخزن:** [@MohammadHasanp](https://github.com/MohammadHasanp)

</div>
