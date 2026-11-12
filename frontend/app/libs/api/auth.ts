import { fetchProfileApi } from "./client";

export async function signUpUser(fullName: string, email: string) {
  const userId = crypto.randomUUID(); // Generate a client-side GUID if required by the DTO
  const res = await fetchProfileApi("/api/userprofiles", {
    method: "POST",
    body: JSON.stringify({ id: userId, fullName, email, role: 0 }),
  });
  if (!res.ok) throw new Error("Failed to sign up user profile.");
  const user = await res.json();
  localStorage.setItem("smartQueue_userId", user.id || userId);
  return user;
}

export async function signInUser(email: string) {
  const res = await fetchProfileApi("/api/userprofiles");
  if (!res.ok) throw new Error("Failed to fetch user profiles.");
  const users = await res.json();
  const user = users.find((u: any) => u.email.toLowerCase() === email.toLowerCase());
  if (!user) throw new Error("User profile not found. Please sign up first.");
  localStorage.setItem("smartQueue_userId", user.id);
  return user;
}

export async function getUserProfile(userId: string) {
  const res = await fetchProfileApi(`/api/userprofiles/${userId}`);
  if (!res.ok) throw new Error("Failed to fetch user profile.");
  return await res.json();
}

export async function updateUserProfile(userId: string, data: { fullName?: string; email?: string; role?: string | number }) {
  const res = await fetchProfileApi(`/api/userprofiles/${userId}`, {
    method: "PUT",
    body: JSON.stringify(data),
  });
  if (!res.ok) throw new Error("Failed to update user profile.");
  return await res.json();
}

export async function getOrganizations() {
  const res = await fetchProfileApi("/api/organizations");
  if (!res.ok) throw new Error("Failed to fetch organizations.");
  return await res.json();
}