# SmartQueue

> **Cloud-Native Digital Queue Management System with Real-Time Updates & Fault-Tolerant Messaging**

SmartQueue replaces physical waiting lines with digital tickets and a live queue dashboard. Visitors join virtual queues, receive real-time status updates, and staff manage arrivals, service counters, and check-ins.

---

## Implemented vs Planned

### Implemented

- **Authentication (Clerk):** ProfileApi and QueueApi validate Clerk JWTs. Frontend sign-in uses Clerk.
- **Profiles & organizations:** User profiles, organizations, and organization memberships.
- **Virtual queues:** Queues, counters, counter assignments, daily ticket numbers, ticket lifecycle.
- **Concurrency control:** Atomic ticket numbering and `FOR UPDATE SKIP LOCKED` for call-next.
- **Wait estimates:** Simple average based on recent completed tickets in the queue.
- **Transactional outbox:** Ticket changes write to `queue.outbox_events` in the same transaction; QueueApi publishes to the Azure Service Bus `ticket-events` topic (emulator locally).
- **NotificationWorker:** Consumes `ticket-events` / `notification-worker`, writes `processed_events` + `notifications` idempotently; pushes via Azure SignalR when `SignalR:ConnectionString` is set.
- **SignalR negotiate:** `POST /api/signalr/negotiate` on QueueApi (503 if SignalR is not configured).
- **Local stack:** Docker Compose for Postgres, Service Bus emulator, APIs, worker, and frontend.
- **CI:** GitHub Actions builds the three .NET services and the Next.js frontend.

### Planned

- **Frontend wired to real APIs** (UI still uses mock data for queues/tickets).
- **Browser SignalR client** for live ticket/queue updates.
- **Come Back Later** ticket reordering.
- **Cryptographic QR check-in** with secrets in Azure Key Vault.
- **Automatic call timeouts** via scheduled Service Bus messages.
- **Time slots & bookings.**
- **Terraform** Azure infrastructure and deploy workflows.
- **k6** load tests; Azure Monitor / Application Insights / OpenTelemetry.

---

## Tech Stack

| Category | Technologies |
|---|---|
| **Frontend** | Next.js, TypeScript |
| **Backend** | ASP.NET Core (.NET 10), .NET Worker Service |
| **Authentication** | Clerk |
| **Database** | PostgreSQL, Entity Framework Core |
| **Hosting** | Azure Container Apps (planned) |
| **Messaging** | Azure Service Bus Topics (emulator locally) |
| **Real-Time Communication** | Azure SignalR Service (negotiate + worker push; browser client planned) |
| **Secrets Management** | Azure Key Vault (planned) |
| **Infrastructure as Code** | Terraform (planned) |
| **Monitoring** | Azure Monitor, Application Insights, OpenTelemetry (planned) |
| **Load Testing** | k6 (planned) |
| **CI/CD** | GitHub Actions (CI build today) |
| **Local Development** | Docker Compose |

---

## Database Architecture

SmartQueue uses **one PostgreSQL database (`smartqueue`) with two schemas**.

| Schema | Service | Tables |
|---|---|---|
| `profile` | ProfileApi | `user_profiles`, `organizations`, `organization_memberships` |
| `queue` | QueueApi (& NotificationWorker) | **MVP today:** `queues`, `counters`, `counter_assignments`, `tickets`, `queue_daily_counters`, `outbox_events`, `processed_events`, `notifications`. **Later:** `time_slots`, `bookings`, `service_time_samples` |

Each API manages its own EF Core DbContext and migrations. QueueApi may read `profile.*` for display names and roles; there are no cross-schema foreign keys.

---

## Running SmartQueue Locally

### Prerequisites

- Docker Desktop
- Git
- .NET 10 SDK (for backend development)
- Node.js 22 (for frontend development)

### 1. Configure Environment Variables

From the project root:

```bash
cp .env.example .env
```

Update `.env` with PostgreSQL credentials, `SERVICEBUS_SQL_PASSWORD` (SQL Server complexity rules), Clerk keys for the frontend, and `CLERK_AUTHORITY` (JWT issuer URL for both APIs). Development API connection strings in `appsettings.Development.json` must match `.env` (default password `smartqueue_dev`).

### 2. Start All Services

```bash
docker compose --env-file .env -f infrastructure/docker-compose.yml up --build -d
```

This starts PostgreSQL, the Azure Service Bus emulator, QueueApi, ProfileApi, NotificationWorker, and the Next.js frontend. Both APIs apply their EF Core migrations on startup.

Docker Compose does not seed the database. After Postgres is up, load the sample organization, users, queues, counters, and tickets from the project root:

```bash
./backend/seed-dev.sh
```

The script uses the Development connection strings in `appsettings.Development.json`. It runs only in Development. Running it again resets the seeded Heilbronn University queues.

### 3. Check Container Status

```bash
docker compose --env-file .env -f infrastructure/docker-compose.yml ps
```

### 4. Access Services

| Service | Local Address |
|---|---|
| **Frontend** | `http://localhost:3000` |
| **QueueApi** | `http://localhost:5001` |
| **ProfileApi** | `http://localhost:5002` |
| **PostgreSQL** | `localhost:5433` |
| **Service Bus emulator** | `localhost:5672` (AMQP) |
| **NotificationWorker** | Background service |

Both APIs expose `GET /health`.

QueueApi writes ticket events to `queue.outbox_events` in the same transaction as the ticket change. A background publisher sends them to the `ticket-events` topic. NotificationWorker consumes the `notification-worker` subscription, records `processed_events`, and inserts `notifications`. If `ServiceBus:ConnectionString` is not set on QueueApi, events stay in the outbox. If `SignalR:ConnectionString` is not set, the worker still stores notifications and skips realtime push.

QueueApi and ProfileApi accept browser requests only from the origins in `Cors:AllowedOrigins`. Development allows `http://localhost:3000` through `appsettings.Development.json`. Other environments set it with environment variables such as `Cors__AllowedOrigins__0=https://<frontend-host>`.

### QueueApi auth (Bearer token)

| Access | Endpoints |
|---|---|
| Public | `GET /api/queues`, `GET /api/queues/{id}`, wait-estimate, `GET /api/tickets/{id}`, position, counters list, ticket statuses, `/health` |
| Any signed-in user with a ProfileApi profile | `POST /api/queues/{id}/tickets` (no `userId` body; profile comes from the Clerk `sub`) |
| Staff / OrganizationAdmin membership for that org, or global SuperAdmin | Queue/counter mutations, assignments, call-next, start, complete, cancel, no-show |
| Signed-in | `POST /api/signalr/negotiate?ticketId=&queueId=` (requires `SignalR:ConnectionString`) |

Manual API check after signing in on the frontend (`/dashboard` can log a JWT):

```bash
# Create profile once (ProfileApi)
curl -s -X POST http://localhost:5002/api/userprofiles/me \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{"firstName":"Alex","lastName":"Visitor","email":"alex@example.com"}'

# Join a queue (QueueApi) — Registrar seed id below after ./backend/seed-dev.sh
curl -s -X POST http://localhost:5001/api/queues/10000000-0000-0000-0000-000000000001/tickets \
  -H "Authorization: Bearer $TOKEN"

# Confirm the worker wrote a notification
docker compose --env-file .env -f infrastructure/docker-compose.yml exec postgres \
  psql -U smartqueue -d smartqueue -c 'SELECT "Type", "TicketId", "CreatedAt" FROM queue.notifications ORDER BY "CreatedAt" DESC LIMIT 5;'
```

Staff actions need a profile whose `organization_memberships.Role` is `Staff` or `OrganizationAdmin` for that queue’s organization (or global `SuperAdmin`). Seeded Sam/Dana UUIDs are not linked to Clerk until you set `ClerkUserId` or use a real login and promote the membership via ProfileApi.

### 5. View Logs

```bash
docker compose --env-file .env -f infrastructure/docker-compose.yml logs -f
```

### 6. Stop Services

```bash
docker compose --env-file .env -f infrastructure/docker-compose.yml down
```

This preserves the PostgreSQL data volume.

> **Note:** Avoid `docker compose down -v` unless you intentionally want to delete local database data.

---

## Development Commands

### Build Backend Services

Run from the project root:

```bash
dotnet build backend/QueueApi/QueueApi.csproj
dotnet build backend/ProfileApi/ProfileApi.csproj
dotnet build backend/NotificationWorker/NotificationWorker.csproj
```

### Run Frontend Separately

```bash
cd frontend
npm ci
npm run dev
```

Set `NEXT_PUBLIC_CLERK_PUBLISHABLE_KEY` and `CLERK_SECRET_KEY` in `frontend/.env.local` (or use the root `.env` when building via Compose).

---

## Continuous Integration

SmartQueue uses **GitHub Actions** for Continuous Integration (CI).

The workflow runs when a pull request targets `main` or `develop`, or when code is pushed to those branches.

### CI Checks

- Build QueueApi (.NET 10)
- Build ProfileApi (.NET 10)
- Build NotificationWorker (.NET 10)
- Build Next.js frontend

**Workflow location:** `.github/workflows/ci.yml`

Automated tests and deploy workflows will be added as development progresses.

---

## Contributors

- **Artian Rika**
- **Liburn Lumani**
- **Mehak Imtiaz**

---

**Project Status:** Under active development. See **Implemented vs Planned** above for what works today.
