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
