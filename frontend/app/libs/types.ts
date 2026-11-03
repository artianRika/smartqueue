export type QueueStatus = "open" | "paused";

export type TicketStatus =
  | "waiting"
  | "called"
  | "checked_in"
  | "served"
  | "missed";

export interface ServiceQueue {
  id: string;
  name: string;
  status: QueueStatus;
  waiting: number;
  estimatedWaitMin: number;
}

export interface Ticket {
  id: string;
  number: string;
  serviceId: string;
  serviceName: string;
  status: TicketStatus;
  positionAhead: number;
  estimatedWaitMin: number;
  counterId?: string;
  signedQrPayload: string;
  notificationLeadTime: number;
  createdAt: string;
}

export interface StaffQueueData {
  queue: ServiceQueue;
  currentTicket?: Ticket | null;
}

export interface QRCheckInResult {
  success: boolean;
  ticketId: string;
  message?: string;
}