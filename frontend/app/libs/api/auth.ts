
import { fetchProfileApi } from "./client";

type UserProfile = {
  id: string;
  fullName: string;
  email: string;
  role: string | number;
};

async function getErrorMessage(res: Response): Promise<string> {
  const body = await res.text();
  return body || `${res.status} ${res.statusText}`;
}

export async function signUpUser(
  fullName: string,
  email: string,
  token?: string | null
) {
  const userId = crypto.randomUUID();

  const res = await fetchProfileApi(
    "/api/userprofiles",
    {
      method: "POST",
      body: JSON.stringify({
        id: userId,
        fullName,
        email,
        role: 0,
      }),
    },
    token
  );

  if (!res.ok) {
    throw new Error(
      `Failed to create user profile: ${await getErrorMessage(res)}`
    );
  }

  const user: UserProfile = await res.json();

  localStorage.setItem("smartQueue_userId", user.id || userId);

  return user;
}

export async function signInUser(
  email: string,
  token?: string | null
) {
  const res = await fetchProfileApi(
    "/api/userprofiles",
    {},
    token
  );

  if (!res.ok) {
    throw new Error(
      `Failed to fetch user profiles: ${await getErrorMessage(res)}`
    );
  }

  const users: UserProfile[] = await res.json();

  const user = users.find(
    (u) => u.email?.toLowerCase() === email.toLowerCase()
  );

  if (!user) {
    throw new Error("User profile not found. Please sign up first.");
  }

  localStorage.setItem("smartQueue_userId", user.id);

  return user;
}

export async function getUserProfile(
  userId: string,
  token?: string | null
) {
  const res = await fetchProfileApi(
    `/api/userprofiles/${userId}`,
    {},
    token
  );

  if (!res.ok) {
    throw new Error(
      `Failed to fetch user profile: ${await getErrorMessage(res)}`
    );
  }

  return await res.json();
}

export async function updateUserProfile(
  userId: string,
  data: {
    fullName?: string;
    email?: string;
    role?: string | number;
  },
  token?: string | null
) {
  const res = await fetchProfileApi(
    `/api/userprofiles/${userId}`,
    {
      method: "PUT",
      body: JSON.stringify(data),
    },
    token
  );

  if (!res.ok) {
    throw new Error(
      `Failed to update user profile: ${await getErrorMessage(res)}`
    );
  }

  return await res.json();
}

export async function getOrganizations(token?: string | null) {
  const res = await fetchProfileApi(
    "/api/organizations",
    {},
    token
  );

  if (!res.ok) {
    throw new Error(
      `Failed to fetch organizations: ${await getErrorMessage(res)}`
    );
  }

  return await res.json();
}

export async function getOrCreateProfileUser(
  fullName: string,
  email: string,
  token: string | null
) {
  if (!token) {
    throw new Error("Clerk authentication token is missing. Please sign in again.");
  }

  const res = await fetchProfileApi(
    "/api/userprofiles",
    {},
    token
  );

  if (!res.ok) {
    throw new Error(
      `Failed to fetch user profiles: ${await getErrorMessage(res)}`
    );
  }

  const users: UserProfile[] = await res.json();

  let profile = users.find(
    (u) => u.email?.toLowerCase() === email.toLowerCase()
  );

  if (!profile) {
    const createRes = await fetchProfileApi(
      "/api/userprofiles",
      {
        method: "POST",
        body: JSON.stringify({
          id: crypto.randomUUID(),
          fullName,
          email,
          role: 0,
        }),
      },
      token
    );

    if (!createRes.ok) {
      throw new Error(
        `Failed to create user profile: ${await getErrorMessage(createRes)}`
      );
    }

    profile = await createRes.json();
  }

  if (!profile?.id) {
    throw new Error("Profile API did not return a valid user ID.");
  }

  localStorage.setItem("smartQueue_userId", profile.id);

  return profile;
}
