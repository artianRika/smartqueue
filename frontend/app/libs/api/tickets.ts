import { fetchQueueApi } from "./client";
import type { Ticket } from "../types";

export async function getTicket(ticketId: string): Promise<Ticket> {
  const ticketRes = await fetchQueueApi(`/api/tickets/${ticketId}`);
  if (!ticketRes.ok) throw new Error("Ticket not found.");
  const ticket = await ticketRes.json();

  const posRes = await fetchQueueApi(`/api/tickets/${ticketId}/position`);
  const posData = posRes.ok ? await posRes.json() : { position: 0 };

  const queueRes = await fetchQueueApi(`/api/queues/${ticket.queueId}`);
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

export async function moveBackTicket(ticketId: string): Promise<Ticket> {
  const ticket = await getTicket(ticketId);
  ticket.positionAhead = (ticket.positionAhead || 0) + 3;
  ticket.estimatedWaitMin = (ticket.estimatedWaitMin || 0) + 6;
  return ticket;
}

export async function cancelTicket(ticketId: string): Promise<{ success: boolean }> {
  const res = await fetchQueueApi(`/api/tickets/${ticketId}/cancel`, {
    method: "PATCH",
  });
  if (!res.ok) throw new Error("Failed to cancel ticket.");
  return { success: true };
}

export async function updateNotificationLeadTime(
  ticketId: string,
  leadTime: number
): Promise<Ticket> {
  const ticket = await getTicket(ticketId);
  ticket.notificationLeadTime = leadTime;
  return ticket;
}
