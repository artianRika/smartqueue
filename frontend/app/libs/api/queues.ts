import { fetchQueueApi } from "./client";
import type { ServiceQueue } from "../types";

/**
 * Helper to automatically retrieve the active Clerk token on the client side.
 */
async function getClerkToken(): Promise<string | null> {
  if (typeof window !== "undefined" && (window as any).Clerk?.session) {
    return await (window as any).Clerk.session.getToken();
  }
  return null;
}

export async function getServices(explicitToken?: string | null): Promise<ServiceQueue[]> {
  const token = explicitToken !== undefined ? explicitToken : await getClerkToken();
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
        ticketPrefix: q.ticketPrefix || q.name.substring(0, 3).toUpperCase(),
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
  explicitToken?: string | null
): Promise<{ ticketId: string }> {
  const token = explicitToken !== undefined ? explicitToken : await getClerkToken();
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

export async function toggleQueueStatus(
  queueId: string,
  status: "open" | "paused",
  explicitToken?: string | null
): Promise<ServiceQueue> {
  const token = explicitToken !== undefined ? explicitToken : await getClerkToken();
  const isOpen = status === "open";

  const res = await fetchQueueApi(
    `/api/queues/${queueId}`,
    {
      method: "PATCH",
      body: JSON.stringify({ isOpen }),
    },
    token
  );

  if (!res.ok) {
    const errorText = await res.text();
    throw new Error(`Failed to update queue status: ${errorText || res.statusText}`);
  }

  const q = await res.json();

  // Fetch current waiting count to return a fully updated ServiceQueue object
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
    ticketPrefix: q.ticketPrefix || q.name.substring(0, 3).toUpperCase(),
    status: q.isOpen ? ("open" as const) : ("paused" as const),
    waiting: waitingTickets.length,
    estimatedWaitMin: waitingTickets.length * 3,
  };
}