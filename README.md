SmartQueue

Cloud-Native Digital Queue Management System with Real-Time Updates & Fault-Tolerant Messaging

SmartQueue replaces physical waiting lines with digital tickets and a live queue dashboard. Visitors join virtual queues, receive real-time status updates, and staff manage arrivals, service counters, and check-ins.

Key Features & Technical Highlights

* Authentication & Authorization: Supabase Auth with role-based access for users and administrators.
* Virtual Queues: Digital tickets, live queue positions, and estimated waiting times.
* Time Slots & Bookings: Reserve available service slots.
* Live UI Synchronization: Real-time updates using Azure SignalR Service.
* Transactional Outbox: Reliably publishes events through Azure Service Bus.
* Automatic Call Timeouts: Scheduled messages mark unclaimed tickets as missed.
* Queue Flexibility: Custom notification lead times and “Come Back Later” position reordering.
* Safe Multi-Counter Processing: Concurrency control prevents duplicate ticket assignment.
* Cryptographic QR Check-in: Signed QR tokens verify ticket arrivals.

Tech Stack

* Frontend: Next.js, TypeScript
* Backend: ASP.NET Core (.NET 10), .NET Worker Service
* Authentication: Supabase Auth
* Database: PostgreSQL, Entity Framework Core
* Hosting: Azure Container Apps
* Messaging: Azure Service Bus Topics, Dead-Letter Queues
* Real-Time Communication: Azure SignalR Service
* Secrets Management: Azure Key Vault
* Infrastructure as Code: Terraform
* Monitoring: Azure Monitor, Application Insights, OpenTelemetry
* Load Testing: k6
* CI/CD: GitHub Actions
* Local Development: Docker Compose

Database Architecture

SmartQueue uses one PostgreSQL database (smartqueue) with two schemas.

Schema	Service	Tables
profile	ProfileApi	user_profiles, organizations
queue	QueueApi & NotificationWorker	queues, counters, tickets, time_slots, bookings, outbox_events, processed_events, notifications, service_time_samples

Each API manages its own EF Core DbContext and migrations.

Both schemas share the same database connection but remain logically separated.

Running SmartQueue Locally

Prerequisites

* Docker Desktop
* Git
* .NET 10 SDK (for backend development)
* Node.js 22 (for frontend development)

1. Configure Environment Variables

From the project root:

cp .env.example .env

Update .env with your local PostgreSQL credentials.

2. Start All Services

docker compose --env-file .env -f infrastructure/docker-compose.yml up --build -d

3. Check Container Status

docker compose --env-file .env -f infrastructure/docker-compose.yml ps

4. Access Services

Service	Address
Frontend	http://localhost:3000
QueueApi	http://localhost:5001
ProfileApi	http://localhost:5002
PostgreSQL	localhost:5433
NotificationWorker	Background service

5. View Logs

docker compose --env-file .env -f infrastructure/docker-compose.yml logs -f

6. Stop Services

docker compose --env-file .env -f infrastructure/docker-compose.yml down

This preserves the PostgreSQL data volume.

Build Backend Services

dotnet build backend/QueueApi/QueueApi.csproj
dotnet build backend/ProfileApi/ProfileApi.csproj
dotnet build backend/NotificationWorker/NotificationWorker.csproj

Run Frontend Separately

cd frontend
npm ci
npm run dev

Continuous Integration

GitHub Actions automatically builds the three .NET services and Next.js frontend when a Pull Request targets main or code is pushed to main.

Workflow: .github/workflows/ci.yml

Automated tests will be added as development progresses.

Contributors

* Artian Rika
* Liburn Lumani
* Mehak Imtiaz