# Order Microservice — Serverless on AWS Lambda

A serverless order microservice built with **Vertical Slice Architecture**, **CQRS**, and **DDD**, deployed as individual AWS Lambda functions per endpoint using **.NET 9**.

## Architecture

Each HTTP endpoint maps to its own Lambda function. Every function owns its full slice — from the HTTP handler down to the domain logic and data access.

```
API Gateway
    │
    ├── POST   /orders              → CreateOrderFunction
    ├── GET    /orders              → GetAllOrdersFunction
    ├── GET    /orders/{id}         → GetOrderFunction
    ├── GET    /orders/customer/{customerId} → GetOrdersByCustomerFunction
    ├── PUT    /orders/{id}/status  → UpdateOrderStatusFunction
    ├── POST   /orders/{id}/cancel  → CancelOrderFunction
    ├── POST   /orders/{id}/items   → AddOrderItemFunction
    └── DELETE /orders/{id}         → DeleteOrderFunction
                                           │
                                    [Static DI container]
                                           │
                               ┌───────────┴───────────┐
                          MediatR Handler         DynamoDB / SNS
```

Each Lambda cold-starts with a **static `ServiceProvider`** that registers only the dependencies that slice needs — no shared container, no cross-slice coupling.

## Project Structure

```
src/
└── Order.Api/                        # Single project — all layers merged
    ├── Features/                     # Vertical slices (one folder per operation)
    │   ├── CreateOrder/
    │   │   ├── CreateOrderCommand.cs
    │   │   ├── CreateOrderHandler.cs
    │   │   ├── CreateOrderValidator.cs
    │   │   └── CreateOrderFunction.cs
    │   ├── GetOrder/
    │   ├── GetAllOrders/
    │   ├── GetOrdersByCustomer/
    │   ├── UpdateOrderStatus/
    │   ├── CancelOrder/
    │   ├── AddOrderItem/
    │   └── DeleteOrder/
    │
    ├── Domain/                       # DDD model
    │   ├── Entities/                 # OrderAggregate, OrderItem, DomainException
    │   ├── ValueObjects/             # Money, Address
    │   ├── Events/                   # OrderCreated, OrderCancelled, ...
    │   ├── Enums/                    # OrderStatus
    │   ├── Interfaces/               # IEventBus
    │   └── Repositories/             # IOrderRepository
    │
    ├── Infrastructure/               # AWS adapters
    │   ├── Repositories/             # DynamoDbOrderRepository
    │   └── EventBus/                 # SnsEventBus, InMemoryEventBus
    │
    ├── Shared/                       # ApiResponse<T>, OrderDto, OrderMapper
    └── Helpers/                      # ApiResponseHelper

tests/
├── Order.UnitTests/
└── Order.IntegrationTests/           # Testcontainers (DynamoDB local)

deploy/
└── aws/
    ├── template.yaml                 # AWS SAM — one function resource per endpoint
    ├── parameters.dev.json
    └── parameters.prod.json
```

## Key Design Decisions

**Vertical Slice Architecture** — code is organized by feature, not by technical layer. Adding a new operation means adding one folder under `Features/`, self-contained from Lambda handler to domain logic.

**Static DI per Lambda** — each `XxxFunction` holds a `private static readonly ServiceProvider` built once at cold start. Only that slice's dependencies are registered, keeping the container minimal.

**MediatR with direct registration** — handlers are registered via `services.AddTransient<IRequestHandler<TReq, TRes>, THandler>()` rather than assembly scanning, so only the relevant handler is loaded per function.

**Lambda Powertools** — structured JSON logging (`[Logging]`), X-Ray tracing (`[Tracing]`), and CloudWatch metrics (`[Metrics]`) via attributes on each handler method.

**Rich domain model** — `OrderAggregate` is a DDD aggregate root with value objects (`Money`, `Address`), factory methods, and domain events. Events are published to SNS via `IEventBus` after each mutation.

## Order Status Flow

```
Pending → Confirmed → Processing → Shipped → Delivered
   │           │            │
   └───────────┴────────────┴──────────────→ Cancelled
```

## API Endpoints

| Method | Path | Description |
|--------|------|-------------|
| `POST` | `/orders` | Create a new order |
| `GET` | `/orders` | List all orders |
| `GET` | `/orders/{id}` | Get order by ID |
| `GET` | `/orders/customer/{customerId}` | Get orders by customer |
| `PUT` | `/orders/{id}/status` | Update order status |
| `POST` | `/orders/{id}/cancel` | Cancel an order |
| `POST` | `/orders/{id}/items` | Add item to order |
| `DELETE` | `/orders/{id}` | Delete an order |

## Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [AWS CLI](https://aws.amazon.com/cli/)
- [AWS SAM CLI](https://docs.aws.amazon.com/serverless-application-model/latest/developerguide/install-sam-cli.html)
- [Docker](https://www.docker.com/) (for integration tests)

## Getting Started

```bash
# Build
dotnet build

# Unit tests
dotnet test tests/Order.UnitTests

# Integration tests (requires Docker for DynamoDB local)
dotnet test tests/Order.IntegrationTests
```

### Local API

```bash
cd deploy/aws
sam build
sam local start-api --parameter-overrides Environment=dev
```

## Deployment

```bash
cd deploy/aws
sam build

# First deploy (interactive)
sam deploy --guided

# Subsequent deploys
sam deploy --parameter-overrides Environment=prod --config-file parameters.prod.json
```

## Sample Requests

### Create Order

```bash
curl -X POST https://{api-id}.execute-api.{region}.amazonaws.com/dev/orders \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": "cust-123",
    "items": [
      { "productId": "prod-001", "productName": "Widget A", "quantity": 2, "unitPrice": 29.99 }
    ],
    "shippingAddress": {
      "street": "123 Main St", "city": "Seattle", "state": "WA", "zipCode": "98101", "country": "USA"
    }
  }'
```

### Update Order Status

```bash
curl -X PUT https://{api-id}.execute-api.{region}.amazonaws.com/dev/orders/{id}/status \
  -H "Content-Type: application/json" \
  -d '{"status": 1}'
```

### Add Item

```bash
curl -X POST https://{api-id}.execute-api.{region}.amazonaws.com/dev/orders/{id}/items \
  -H "Content-Type: application/json" \
  -d '{ "productId": "prod-002", "productName": "Widget B", "quantity": 1, "unitPrice": 49.99 }'
```

## License

MIT
