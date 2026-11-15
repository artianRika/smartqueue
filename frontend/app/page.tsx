"use client";

import { useEffect } from "react";
import { useRouter } from "next/navigation";
import {
    SignInButton,
    SignUpButton,
    useAuth,
} from "@clerk/nextjs";

export default function Home() {
    const router = useRouter();
    const { isLoaded, isSignedIn } = useAuth();

    useEffect(() => {
        if (isLoaded && isSignedIn) {
            router.replace("/services");
        }
    }, [isLoaded, isSignedIn, router]);

    if (!isLoaded || isSignedIn) {
        return (
            <main className="flex min-h-screen items-center justify-center bg-gray-100">
                <p className="text-gray-600">Loading...</p>
            </main>
        );
    }

    return (
        <main className="flex min-h-screen flex-col items-center justify-center bg-gray-100 p-6 text-gray-900">
            <div className="w-full max-w-md rounded-xl border border-gray-200 bg-white p-8 shadow-lg">
                <h1 className="mb-6 text-center text-2xl font-bold text-gray-900">
                    SmartQueue Sign In
                </h1>

                <div className="flex flex-col gap-4">
                    <SignInButton mode="modal">
                        <button className="w-full rounded-lg bg-blue-600 px-4 py-3 font-medium text-white hover:bg-blue-700">
                            Sign In
                        </button>
                    </SignInButton>

                    <SignUpButton mode="modal">
                        <button className="w-full rounded-lg bg-green-600 px-4 py-3 font-medium text-white hover:bg-green-700">
                            Create Account
                        </button>
                    </SignUpButton>
                </div>
            </div>
        </main>
    );
}