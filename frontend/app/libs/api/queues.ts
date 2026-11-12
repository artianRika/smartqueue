import { fetchQueueApi } from "./client";
import type { ServiceQueue } from "../types";

export async function getServices(): Promise<ServiceQueue[]> {
  const res = await fetchQueueApi("/api/queues");
  if (!res.ok) throw new Error("Failed to fetch queues from backend.");
  const queues = await res.json();

  const servicesList = await Promise.all(
    queues.map(async (q: any) => {
      const ticketRes = await fetchQueueApi(`/api/queues/${q.id}/tickets?status=Waiting`);
      const waitingTickets = ticketRes.ok ? await ticketRes.json() : [];

      return {
        id: q.id,
        name: q.name,
        description: q.description || "General Queue",
        status: q.isOpen ? ("open" as const) : ("paused" as const),
        waiting: waitingTickets.length,
        estimatedWaitMin: waitingTickets.length * 3,
      };
    })
  );

  return servicesList;
}

export async function joinQueue(queueId: string): Promise<{ ticketId: string }> {
  const userId = localStorage.getItem("smartQueue_userId") || "00000000-0000-0000-0000-000000000000";

  const res = await fetchQueueApi(`/api/queues/${queueId}/tickets`, {
    method: "POST",
    body: JSON.stringify({ userId }),
  });

  if (!res.ok) {
    const errorText = await res.text();
    throw new Error(`Failed to join queue: ${errorText || res.statusText}`);
  }

  const ticket = await res.json();
  return { ticketId: ticket.id };
}

export async function toggleQueueStatus(
  queueId: string,
  status: "open" | "paused"
): Promise<ServiceQueue> {
  const isOpen = status === "open";
  const res = await fetchQueueApi(`/api/queues/${queueId}/open`, {
    method: "PATCH",
    body: JSON.stringify({ isOpen }),
  });

  if (!res.ok) throw new Error("Failed to update queue status.");
  const q = await res.json();

  return {
    id: q.id,
    name: q.name,
    description: q.description || "",
    status: q.isOpen ? "open" : "paused",
    waiting: 0,
    estimatedWaitMin: 0,
  };
}