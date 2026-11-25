export type QueueStatus = "open" | "paused";
export type UserRole = "Customer" | "Staff" | "Admin";

export interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  phoneNumber?: string;
  role: UserRole;
  notificationLeadTime: number; // e.g., notify when 3 people ahead or 10 mins left
  createdAt: string;
}

export interface AuthResponse {
  token: string;
  user: UserProfile;
}

export type TicketStatus =
  | "Waiting"
  | "Called"
  | "Serving"
  | "Completed"
  | "Cancelled"
  | "NoShow";

export interface ServiceQueue {
  id: string;
  name: string;
  description?: string;
  status?: string;
  isOpen?: boolean;
  ticketPrefix: string;
  waiting?: number;          // Add this
  estimatedWaitMin?: number; // Add this
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