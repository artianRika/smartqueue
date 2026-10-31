# SmartQueue

> **Cloud-Native Digital Queue Management System with Real-Time Updates & Fault-Tolerant Messaging**

SmartQueue replaces physical waiting lines with digital tickets and a live queue dashboard. Visitors join virtual queues, receive real-time status updates, and staff manage arrivals, service counters, and check-ins.

---

## 🚀 Key Features & Technical Highlights

- **Authentication & Authorization:** Supabase Auth with role-based access for users and administrators.
- **Virtual Queues:** Digital tickets, live queue positions, and estimated waiting times.
- **Time Slots & Bookings:** Reserve available service slots.
- **Live UI Synchronization:** Real-time updates using Azure SignalR Service.
- **Transactional Outbox:** Reliably publishes events through Azure Service Bus.
- **Automatic Call Timeouts:** Scheduled messages mark unclaimed tickets as missed.
- **Queue Flexibility:** Custom notification lead times and "Come Back Later" position reordering.
- **Safe Multi-Counter Processing:** Concurrency control prevents duplicate ticket assignment.
- **Cryptographic QR Check-in:** Signed QR tokens verify ticket arrivals.

---

## 🛠️ Tech Stack

| Category | Technologies |
|---|---|
| **Frontend** | Next.js, TypeScript |
| **Backend** | ASP.NET Core (.NET 10), .NET Worker Service |
| **Authentication** | Supabase Auth |
| **Database** | PostgreSQL, Entity Framework Core |
| **Hosting** | Azure Container Apps |
| **Messaging** | Azure Service Bus Topics, Dead-Letter Queues |
| **Real-Time Communication** | Azure SignalR Service |
| **Secrets Management** | Azure Key Vault |
| **Infrastructure as Code** | Terraform |
| **Monitoring** | Azure Monitor, Application Insights, OpenTelemetry |
| **Load Testing** | k6 |
| **CI/CD** | GitHub Actions |
| **Local Development** | Docker Compose |

---

## 🗄️ Database Architecture

SmartQueue uses **one PostgreSQL database (`smartqueue`) with two schemas**.

| Schema | Service | Tables |
|---|---|---|
| `profile` | ProfileApi | `user_profiles`, `organizations` |
| `queue` | QueueApi & NotificationWorker | `queues`, `counters`, `counter_assignments`, `tickets`, `queue_daily_counters`, `time_slots`, `bookings`, `outbox_events`, `processed_events`, `notifications`, `service_time_samples` |

Each API manages its own EF Core DbContext and migrations.

Both schemas share the same database connection but remain logically separated.

---

## 🐳 Running SmartQueue Locally

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

Update `.env` with your local PostgreSQL credentials.

### 2. Start All Services

```bash
docker compose --env-file .env -f infrastructure/docker-compose.yml up --build -d
```

This starts PostgreSQL, QueueApi, ProfileApi, NotificationWorker, and the Next.js frontend.

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
| **NotificationWorker** | Background service |

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

## 💻 Development Commands

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

---

## 🔄 Continuous Integration

SmartQueue uses **GitHub Actions** for Continuous Integration (CI).

The workflow automatically runs when:

- A Pull Request targets `main`.
- Code is pushed to `main`.

### CI Checks

- Build QueueApi (.NET 10)
- Build ProfileApi (.NET 10)
- Build NotificationWorker (.NET 10)
- Build Next.js frontend

**Workflow location:** `.github/workflows/ci.yml`

Automated tests will be added as development progresses.

---

## 👥 Contributors

- **Artian Rika**
- **Liburn Lumani**
- **Mehak Imtiaz**

---

**Project Status:** Under active development. Some features and Azure integrations described above are planned and not yet implemented.
