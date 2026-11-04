# SmartQueue — Project Context

We are developing SmartQueue, a cloud-native digital queue management system for our university cloud computing semester project.

Use the following architecture and decisions as the source of truth when helping us write code, design databases, create diagrams, or configure infrastructure.

## 1. Project Idea

SmartQueue replaces physical waiting lines with virtual queues.

- Visitors join a queue and receive a numbered ticket.
- Visitors track their position and estimated waiting time.
- Staff manage service counters and call the next ticket.
- Visitors receive real-time updates.
- QR codes are used for secure check-in.
- The system supports missed tickets, notifications, and a “Come Back Later” option.

## 2. Technology Stack

- **Frontend:** Next.js + TypeScript
- **Backend:** .NET 10, ASP.NET Core Web API, .NET Worker Service
- **Database:** PostgreSQL + Entity Framework Core
- **Messaging:** Azure Service Bus Topics
- **Real-time:** Azure SignalR Service
- **Hosting:** Azure Container Apps
- **Security:** Azure Key Vault
- **Infrastructure:** Terraform
- **CI/CD:** GitHub Actions
- **Monitoring:** Azure Monitor, Application Insights, OpenTelemetry
- **Load testing:** k6

## 3. Backend Architecture

We have three independently deployable backend services:

### QueueApi

- Manages queues, tickets, and service counters.
- Handles ticket numbering and concurrent staff operations.
- Uses the transactional outbox pattern.
- Publishes events to Azure Service Bus.
- Handles SignalR connection setup.

### ProfileApi

- Manages user profiles and application roles.
- Uses an external identity provider for authentication.

### NotificationWorker

- Consumes events from Azure Service Bus.
- Processes events idempotently.
- Sends real-time notifications through Azure SignalR.

Do not combine these three services into one application.

## 4. Database Design

We use one PostgreSQL database with two schemas.

### `profile` schema — owned by ProfileApi

- `user_profiles`
- `organizations`

### `queue` schema — owned by QueueApi

- `queues`
- `queue_number_counters`
- `counters`
- `tickets`
- `time_slots`
- `bookings`
- `outbox_events`
- `processed_events`
- `notifications`
- `service_time_samples`
- `service_time_averages`

Each API has its own EF Core DbContext and migrations.

Do not create database foreign keys across service boundaries. Store user IDs as UUID references instead.

The NotificationWorker processes notifications using tables in the queue schema. This is a pragmatic semester-project decision, not strict independent database ownership.

**MVP priority:** Profiles, queues, counters, tickets, outbox, event processing, and notifications.

**Later features:** Appointment bookings, time slots, and advanced waiting-time estimation.

## 5. Important Technical Rules

- Ticket numbers must be unique within each queue.
- Concurrent requests must never assign duplicate ticket numbers.
- Two staff counters must never claim the same ticket.
- Ticket changes and outbox events must be saved in the same PostgreSQL transaction.
- An outbox publisher in QueueApi reads pending events and publishes them to Azure Service Bus.
- The NotificationWorker must handle duplicate events safely.
- Queue position should be calculated from the current queue ordering, not stored as a permanently fixed value.
- “Come Back Later” must support reordering independently of ticket numbers.
- QR tokens must be cryptographically signed, with signing secrets stored in Azure Key Vault.
- Automatic call timeouts using scheduled Service Bus messages are planned.

## 6. Repository Structure

```text
smartqueue/
├── frontend/
├── backend/
│   ├── SmartQueue.slnx
│   ├── QueueApi/
│   ├── ProfileApi/
│   └── NotificationWorker/
├── infrastructure/
│   ├── docker-compose.yml
│   └── terraform/
├── tests/
│   └── load/
└── .github/
    └── workflows/
```

We use one GitHub monorepo, but each service should be independently buildable and deployable.

## 7. Infrastructure and Deployment

- Terraform manages Azure infrastructure.
- GitHub Actions handles CI/CD, with separate workflows for the frontend, backend services, and Terraform.
- The backend services run as separate Azure Container Apps.

## 8. Instructions for the AI Assistant

When answering questions about SmartQueue:

- Follow the architecture above.
- Do not introduce additional microservices or databases without explaining why.
- Do not replace our chosen technologies.
- Keep solutions realistic for a university semester project.
- Prefer simple, practical implementations over unnecessary complexity.
- Clearly distinguish implemented features from planned features.
- When generating code, ensure it fits our existing project structure.
- Keep answers concise and direct.
- If a proposed change conflicts with these decisions, explain the conflict before suggesting it.

Our goal is to build a working cloud-native application demonstrating microservices, reliable messaging, concurrency control, real-time updates, Terraform, CI/CD, and cloud monitoring.
