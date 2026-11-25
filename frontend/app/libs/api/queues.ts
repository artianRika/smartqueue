import { fetchQueueApi } from "./client";
import type { ServiceQueue } from "../types";

export async function getServices(token?: string | null): Promise<ServiceQueue[]> {
  const res = await fetchQueueApi("/api/queues", undefined, token);
  if (!res.ok) throw new Error("Failed to fetch queues from backend.");
  const queues = await res.json();

  const servicesList = await Promise.all(
    queues.map(async (q: any) => {
      const ticketRes = await fetchQueueApi(
        `/api/queues/${q.id}/tickets?status=Waiting`,
        undefined,
        token
      );
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

export async function joinQueue(
  queueId: string,
  userId: string,
  token?: string | null
): Promise<{ ticketId: string }> {
  const res = await fetchQueueApi(
    `/api/queues/${queueId}/tickets`,
    {
      method: "POST",
      body: JSON.stringify({ userId }),
    },
    token
  );

  if (!res.ok) {
    const errorText = await res.text();
    throw new Error(`Failed to join queue: ${errorText || res.statusText}`);
  }

  const ticket = await res.json();
  return { ticketId: ticket.id };
}