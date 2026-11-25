import { fetchQueueApi } from "./client";
import type { Ticket } from "../types";

export async function getTicket(ticketId: string, token?: string | null): Promise<Ticket> {
  const ticketRes = await fetchQueueApi(`/api/tickets/${ticketId}`, undefined, token);
  if (!ticketRes.ok) throw new Error("Ticket not found.");
  const ticket = await ticketRes.json();

  const posRes = await fetchQueueApi(`/api/tickets/${ticketId}/position`, undefined, token);
  const posData = posRes.ok ? await posRes.json() : { position: 0 };

  const queueRes = await fetchQueueApi(`/api/queues/${ticket.queueId}`, undefined, token);
  const queueData = queueRes.ok ? await queueRes.json() : { name: "Service Queue" };

  return {
    id: ticket.id,
    number: ticket.ticketNumber || "A-001",
    serviceId: ticket.queueId,
    serviceName: queueData.name,
    status: ticket.status ? ticket.status.toLowerCase() : "waiting",
    positionAhead: posData.position ?? 0,
    estimatedWaitMin: (posData.position ?? 0) * 3,
    signedQrPayload: ticket.signedPayload || `SQ-SIG-${ticket.id.toUpperCase()}`,
    notificationLeadTime: 3,
    createdAt: ticket.createdAt || new Date().toISOString(),
  };
}

export async function moveBackTicket(ticketId: string, token?: string | null): Promise<Ticket> {
  const res = await fetchQueueApi(
    `/api/tickets/${ticketId}/postpone`,
    { method: "PATCH" },
    token
  );
  if (!res.ok) throw new Error("Failed to move ticket back in queue.");
  return await getTicket(ticketId, token);
}

export async function cancelTicket(
  ticketId: string,
  token?: string | null
): Promise<{ success: boolean }> {
  const res = await fetchQueueApi(
    `/api/tickets/${ticketId}/cancel`,
    { method: "PATCH" },
    token
  );
  if (!res.ok) throw new Error("Failed to cancel ticket.");
  return { success: true };
}