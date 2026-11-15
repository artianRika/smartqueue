"use client";

import { useAuth, useUser, UserButton } from "@clerk/nextjs";
import { useRouter } from "next/navigation";
import { useEffect } from "react";

export default function Dashboard() {
    const router = useRouter();
    const { isLoaded, isSignedIn, getToken } = useAuth();
    const { user } = useUser();

    useEffect(() => {
        if (isLoaded && !isSignedIn) {
            router.replace("/");
        }
    }, [isLoaded, isSignedIn, router]);

    async function handleGetToken() {
        try {
            const token = await getToken();

            if (!token) {
                console.error("No JWT available.");
                return;
            }

            console.log("Clerk JWT:", token);
        } catch (error) {
            console.error("Failed to retrieve JWT:", error);
        }
    }

    if (!isLoaded || !isSignedIn) {
        return <main className="p-8">Loading...</main>;
    }

    return (
        <main className="min-h-screen bg-gray-100 p-8 text-gray-900">
            <div className="mx-auto max-w-3xl rounded-xl bg-white p-8 shadow-lg">
                <div className="mb-8 flex items-center justify-between">
                    <h1 className="text-3xl font-bold">
                        SmartQueue Dashboard
                    </h1>
                    <UserButton />
                </div>

                <p className="mb-2 text-lg">
                    Welcome, {user?.firstName ?? "User"}!
                </p>

                <p className="mb-6 text-gray-600">
                    {user?.primaryEmailAddress?.emailAddress}
                </p>

                <button
                    onClick={handleGetToken}
                    className="rounded-lg bg-blue-600 px-5 py-3 font-medium text-white hover:bg-blue-700"
                >
                    Get JWT Token
                </button>
            </div>
        </main>
    );
}