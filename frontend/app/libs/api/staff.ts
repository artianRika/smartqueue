import { fetchQueueApi } from "./client";
import { getTicket } from "./tickets";
import type { StaffQueueData, Ticket, QRCheckInResult } from "../types";

export async function getStaffQueue(counterId: string): Promise<StaffQueueData> {
  const counterRes = await fetchQueueApi(`/api/counters/${counterId}`);
  if (!counterRes.ok) throw new Error("Counter not found.");
  const counter = await counterRes.json();

  const queueRes = await fetchQueueApi(`/api/queues/${counter.queueId}`);
  const queueData = queueRes.ok ? await queueRes.json() : { id: counter.queueId, name: "Queue", isOpen: true, description: "" };

  const waitingRes = await fetchQueueApi(`/api/queues/${counter.queueId}/tickets?status=Waiting`);
  const waitingTickets = waitingRes.ok ? await waitingRes.json() : [];

  const ticketRes = await fetchQueueApi(`/api/queues/${counter.queueId}/tickets?status=Called`);
  const calledTickets = ticketRes.ok ? await ticketRes.json() : [];
  const currentTicketData = calledTickets.length > 0 ? calledTickets[0] : null;

  return {
    queue: {
      id: queueData.id,
      name: queueData.name,
      description: queueData.description || "",
      status: queueData.isOpen ? "open" : "paused",
      waiting: waitingTickets.length,
      estimatedWaitMin: waitingTickets.length * 3,
      ticketPrefix: queueData.ticketPrefix || "A",
    },
    currentTicket: currentTicketData
      ? {
          id: currentTicketData.id,
          number: currentTicketData.ticketNumber,
          serviceId: counter.queueId,
          serviceName: queueData.name,
          status: "Called",
          positionAhead: 0,
          estimatedWaitMin: 0,
          counterId: counter.name,
          signedQrPayload: currentTicketData.signedPayload || `SQ-SIG-${currentTicketData.id.toUpperCase()}`,
          notificationLeadTime: 3,
          createdAt: currentTicketData.createdAt || new Date().toISOString(),
        }
      : null,
  };
}

export async function callNextTicket(counterId: string): Promise<{ ticket: Ticket }> {
  const res = await fetchQueueApi(`/api/counters/${counterId}/tickets/call-next`, {
    method: "POST",
  });
  if (!res.ok) {
    const err = await res.text();
    throw new Error(err || "No waiting tickets available in this queue.");
  }
  const ticket = await res.json();
  return {
    ticket: {
      id: ticket.id,
      number: ticket.ticketNumber,
      serviceId: ticket.queueId,
      serviceName: "Queue",
      status: "Called",
      positionAhead: 0,
      estimatedWaitMin: 0,
      counterId,
      signedQrPayload: ticket.signedPayload || `SQ-SIG-${ticket.id.toUpperCase()}`,
      notificationLeadTime: 3,
      createdAt: ticket.createdAt || new Date().toISOString(),
    },
  };
}

export async function markTicketServed(ticketId: string): Promise<{ success: boolean }> {
  const startRes = await fetchQueueApi(`/api/tickets/${ticketId}/start`, { method: "PATCH" });
  if (!startRes.ok) throw new Error("Failed to start service.");

  const completeRes = await fetchQueueApi(`/api/tickets/${ticketId}/complete`, { method: "PATCH" });
  if (!completeRes.ok) throw new Error("Failed to complete service.");

  return { success: true };
}

export async function markTicketMissed(ticketId: string): Promise<{ success: boolean }> {
  const res = await fetchQueueApi(`/api/tickets/${ticketId}/no-show`, { method: "PATCH" });
  if (!res.ok) throw new Error("Failed to mark ticket as missed.");
  return { success: true };
}

export async function verifyAndCheckInTicket(qrPayload: string): Promise<QRCheckInResult> {
  try {
    const ticket = await getTicket(qrPayload);
    if (ticket) {
      return { success: true, ticketId: ticket.id };
    }
  } catch {
    // Fallback
  }

  return {
    success: false,
    ticketId: "",
    message: "Invalid or expired signed QR signature.",
  };
}