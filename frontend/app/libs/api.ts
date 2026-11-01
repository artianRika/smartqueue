import type { ServiceQueue } from "./types";

// MOCK DATA. Later, replace the bodies with fetch() calls to the Queue API.
// Only this file should change when the real backend is ready.
const services: ServiceQueue[] = [
  { id: "registrar", name: "Student Registrar", status: "open", waiting: 12, estimatedWaitMin: 25 },
  { id: "financial-aid", name: "Financial Aid", status: "open", waiting: 4, estimatedWaitMin: 10 },
  { id: "it-help", name: "IT Help Desk", status: "paused", waiting: 0, estimatedWaitMin: 0 },
];

export async function getServices(): Promise<ServiceQueue[]> {
  return services;
}

export async function joinQueue(queueId: string): Promise<{ ticketId: string }> {
  return { ticketId: `mock-${queueId}-042` };
}
