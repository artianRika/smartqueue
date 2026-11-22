"use client";

import { useAuth, useUser } from "@clerk/nextjs";
import { useEffect, useRef } from "react";

const API_URL = process.env.NEXT_PUBLIC_PROFILE_API_URL;

export default function ProfileSync() {
    const { isLoaded, isSignedIn, getToken } = useAuth();
    const { user, isLoaded: userLoaded } = useUser();
    const lastSyncedUserId = useRef<string | null>(null);

    useEffect(() => {
        if (!isLoaded || !userLoaded || !isSignedIn || !user) {
            return;
        }

        if (lastSyncedUserId.current === user.id) {
            return;
        }

        const currentUser = user;
        let cancelled = false;

        async function syncProfile() {
            try {
                if (!API_URL) {
                    throw new Error("ProfileApi URL is not configured.");
                }

                const token = await getToken();

                if (!token) {
                    throw new Error("Clerk JWT not available.");
                }

                const response = await fetch(
                    `${API_URL}/api/userprofiles/me`,
                    {
                        headers: {
                            Authorization: `Bearer ${token}`,
                        },
                    }
                );

                if (!response.ok && response.status !== 404) {
                    throw new Error(
                        `Profile lookup failed: ${response.status}`
                    );
                }

                if (response.status === 404) {
                    const email =
                        currentUser.primaryEmailAddress?.emailAddress;

                    if (!email) {
                        throw new Error("User email is missing.");
                    }

                    const createResponse = await fetch(
                        `${API_URL}/api/userprofiles/me`,
                        {
                            method: "POST",
                            headers: {
                                "Content-Type": "application/json",
                                Authorization: `Bearer ${token}`,
                            },
                            body: JSON.stringify({
                                firstName: currentUser.firstName || "Unknown",
                                lastName: currentUser.lastName || "User",
                                email,
                            }),
                        }
                    );

                    if (!createResponse.ok &&
                        createResponse.status !== 409) {
                        throw new Error(
                            `Profile creation failed: ${createResponse.status}`
                        );
                    }
                }

                if (!cancelled) {
                    lastSyncedUserId.current = currentUser.id;
                    console.log("Profile synchronized successfully.");
                }
            } catch (error) {
                if (!cancelled) {
                    console.error("Profile synchronization failed:", error);
                }
            }
        }

        void syncProfile();

        return () => {
            cancelled = true;
        };
    }, [isLoaded, userLoaded, isSignedIn, user, getToken]);

    return null;
}