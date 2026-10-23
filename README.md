
# SmartQueue

> Cloud-Native Digital Queue Management System with Real-Time Updates & Fault-Tolerant Messaging

SmartQueue replaces physical waiting lines with digital tickets and a live queue dashboard. Visitors join virtual queues, receive real-time status updates without refreshing, and staff efficiently manage arrivals, call timeouts, and counter check-ins.


## Key Features & Technical Highlights

- **Live UI Synchronization:** Instant browser updates powered by Managed Azure SignalR Service.
- **Outbox Pattern:** Dual-writes tickets and events in a single database transaction to guarantee zero lost notifications.
- **Automatic Call Timeouts:** Scheduled messaging automatically marks unclaimed called tickets as missed.
- **Queue Flexibility:** Custom notification lead-time alerts and "Come Back Later" position reordering.
- **Safe Multi-Counter Processing:** Concurrency locks ensure two counters never receive the same ticket.
- **Cryptographic QR Check-in:** Signed Key Vault tokens verify authentic single-use ticket arrivals.


## Tech Stack

- **Frontend:** Next.js, TypeScript, Managed Azure SignalR Client
- **Backend:** ASP.NET Core Web API, .NET Worker Service (Docker Containers)
- **Compute & Ingress:** Azure Container Apps
- **Database:** Azure Database for PostgreSQL
- **Messaging & Event Streaming:** Azure Service Bus Topics, Dead-Letter Queues
- **Infrastructure & Monitoring:** Terraform, Azure Monitor, Application Insights, OpenTelemetry, k6

Contributors:
---
- Artian Rika
- Liburn Lumani
- Mehak Imtiaz
