"use client";

import React, { useState, useEffect, Suspense } from "react";
import { useSearchParams } from "next/navigation";
import SignInForm from "@/app/components/SignInForm";
import SignUpForm from "@/app/components/SignUpForm";
import { Ticket } from "lucide-react";

function AuthContent() {
  const searchParams = useSearchParams();
  const [mode, setMode] = useState<"signin" | "signup">(
    searchParams.get("mode") === "signup" ? "signup" : "signin"
  );

  useEffect(() => {
    const queryMode = searchParams.get("mode");
    if (queryMode === "signup" || queryMode === "signin") {
      setMode(queryMode);
    }
  }, [searchParams]);

  return (
    <div className="max-w-md w-full bg-white rounded-2xl shadow-sm border border-gray-200 p-8">
      <div className="text-center mb-6">
        <div className="inline-flex items-center justify-center p-3 bg-blue-50 text-blue-600 rounded-xl mb-3">
          <Ticket className="h-7 w-7" />
        </div>
        <h1 className="text-2xl font-bold text-gray-900">SmartQueue</h1>
        <p className="text-sm text-gray-500 mt-1">
          {mode === "signin"
            ? "Sign in to manage your tickets"
            : "Create an account to join queues remotely"}
        </p>
      </div>

      {/* Mode Switcher */}
      <div className="grid grid-cols-2 bg-gray-100 p-1 rounded-lg mb-6">
        <button
          type="button"
          onClick={() => setMode("signin")}
          className={`py-2 text-sm font-medium rounded-md transition cursor-pointer ${
            mode === "signin"
              ? "bg-white text-gray-900 shadow-sm"
              : "text-gray-600 hover:text-gray-900"
          }`}
        >
          Sign In
        </button>
        <button
          type="button"
          onClick={() => setMode("signup")}
          className={`py-2 text-sm font-medium rounded-md transition cursor-pointer ${
            mode === "signup"
              ? "bg-white text-gray-900 shadow-sm"
              : "text-gray-600 hover:text-gray-900"
          }`}
        >
          Sign Up
        </button>
      </div>

      {mode === "signin" ? <SignInForm /> : <SignUpForm />}
    </div>
  );
}

export default function AuthPage() {
  return (
    <div className="min-h-[calc(100vh-4rem)] flex items-center justify-center bg-gray-50 px-4 py-12">
      <Suspense fallback={<div className="text-sm text-gray-500">Loading...</div>}>
        <AuthContent />
      </Suspense>
    </div>
  );
}