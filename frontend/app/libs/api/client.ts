const QUEUE_API_URL = process.env.NEXT_PUBLIC_QUEUE_API_URL || "http://localhost:5087";
const PROFILE_API_URL = process.env.NEXT_PUBLIC_PROFILE_API_URL || "http://localhost:5104";

export async function fetchQueueApi(
  endpoint: string,
  options?: RequestInit,
  token?: string | null
) {
  const res = await fetch(`${QUEUE_API_URL}${endpoint}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options?.headers || {}),
    },
  });
  return res;
}

export async function fetchProfileApi(
  endpoint: string,
  options?: RequestInit,
  token?: string | null
) {
  const res = await fetch(`${PROFILE_API_URL}${endpoint}`, {
    ...options,
    headers: {
      "Content-Type": "application/json",
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
      ...(options?.headers || {}),
    },
  });
  return res;
}