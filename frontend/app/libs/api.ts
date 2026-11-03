import type { ServiceQueue, Ticket, StaffQueueData, QRCheckInResult } from "./types";

// Mock Services State
let services: ServiceQueue[] = [
  { id: "registrar", name: "Student Registrar", status: "open", waiting: 12, estimatedWaitMin: 25 },
  { id: "financial-aid", name: "Financial Aid", status: "open", waiting: 4, estimatedWaitMin: 10 },
  { id: "it-help", name: "IT Help Desk", status: "paused", waiting: 0, estimatedWaitMin: 0 },
];

// Mock Active Tickets Storage
let mockTickets: Record<string, Ticket> = {
  "mock-registrar-042": {
    id: "mock-registrar-042",
    number: "A-042",
    serviceId: "registrar",
    serviceName: "Student Registrar",
    status: "waiting",
    positionAhead: 3,
    estimatedWaitMin: 15,
    signedQrPayload: "SQ-SIG-REG-042-VERIFIED",
    notificationLeadTime: 3,
    createdAt: new Date().toISOString(),
  },
};

// 1. Get all public services
export async function getServices(): Promise<ServiceQueue[]> {
  return services;
}

// 2. Join a virtual queue
export async function joinQueue(queueId: string): Promise<{ ticketId: string }> {
  const service = services.find((s) => s.id === queueId);
  const ticketId = `mock-${queueId}-${Math.floor(100 + Math.random() * 900)}`;
  
  mockTickets[ticketId] = {
    id: ticketId,
    number: `A-${Math.floor(100 + Math.random() * 900)}`,
    serviceId: queueId,
    serviceName: service ? service.name : "Service",
    status: "waiting",
    positionAhead: service ? service.waiting : 0,
    estimatedWaitMin: service ? service.estimatedWaitMin : 10,
    signedQrPayload: `SQ-SIG-${ticketId.toUpperCase()}`,
    notificationLeadTime: 3,
    createdAt: new Date().toISOString(),
  };

  if (service) {
    service.waiting += 1;
  }

  return { ticketId };
}

// 3. Get single ticket details
export async function getTicket(ticketId: string): Promise<Ticket> {
  const ticket = mockTickets[ticketId];
  if (!ticket) {
    throw new Error("Ticket not found");
  }
  return ticket;
}

// 4. Move ticket back in line ("Come back later")
export async function moveBackTicket(ticketId: string): Promise<Ticket> {
  const ticket = mockTickets[ticketId];
  if (ticket) {
    ticket.positionAhead = (ticket.positionAhead || 0) + 3;
    ticket.estimatedWaitMin = (ticket.estimatedWaitMin || 0) + 6;
  }
  return ticket;
}

// 5. Cancel / Leave queue
export async function cancelTicket(ticketId: string): Promise<{ success: boolean }> {
  delete mockTickets[ticketId];
  return { success: true };
}

// 6. Update notification lead time preference
export async function updateNotificationLeadTime(
  ticketId: string,
  leadTime: number
): Promise<Ticket> {
  const ticket = mockTickets[ticketId];
  if (ticket) {
    ticket.notificationLeadTime = leadTime;
  }
  return ticket;
}

// 7. Get staff dashboard data for a counter
export async function getStaffQueue(serviceId: string): Promise<StaffQueueData> {
  const queue = services.find((s) => s.id === serviceId) || services[0];
  const currentTicket = Object.values(mockTickets).find(
    (t) => t.serviceId === serviceId && t.status === "called"
  );
  return { queue, currentTicket: currentTicket || null };
}

// 8. Safe Call Next ticket (Staff action)
export async function callNextTicket(serviceId: string): Promise<{ ticket: Ticket }> {
  const waitingTicket = Object.values(mockTickets).find(
    (t) => t.serviceId === serviceId && t.status === "waiting"
  );

  if (!waitingTicket) {
    throw new Error("No waiting tickets in this queue.");
  }

  waitingTicket.status = "called";
  waitingTicket.counterId = "Counter 1";
  return { ticket: waitingTicket };
}

// 9. Mark ticket as served
export async function markTicketServed(ticketId: string): Promise<{ success: boolean }> {
  if (mockTickets[ticketId]) {
    mockTickets[ticketId].status = "served";
  }
  return { success: true };
}

// 10. Mark ticket as missed
export async function markTicketMissed(ticketId: string): Promise<{ success: boolean }> {
  if (mockTickets[ticketId]) {
    mockTickets[ticketId].status = "missed";
  }
  return { success: true };
}

// 11. Toggle pause/open queue status
export async function toggleQueueStatus(
  queueId: string,
  status: "open" | "paused"
): Promise<ServiceQueue> {
  const service = services.find((s) => s.id === queueId);
  if (service) {
    service.status = status;
    return { ...service };
  }
  throw new Error("Service not found");
}

// 12. Verify signed QR code payload at counter check-in
export async function verifyAndCheckInTicket(
  qrPayload: string
): Promise<QRCheckInResult> {
  const matchedTicket = Object.values(mockTickets).find(
    (t) => t.signedQrPayload === qrPayload || t.id === qrPayload
  );

  if (matchedTicket) {
    matchedTicket.status = "checked_in";
    return {
      success: true,
      ticketId: matchedTicket.id,
    };
  }

  return {
    success: false,
    ticketId: "",
    message: "Invalid or expired signed QR signature.",
  };
}