"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";
import { signUpUser } from "@/app/libs/api/auth";

export default function SignUpForm() {
  const router = useRouter();
  const [fullName, setFullName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [confirmPassword, setConfirmPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  async function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    if (password !== confirmPassword) {
      setErrorMsg("Passwords do not match");
      return;
    }

    setLoading(true);
    setErrorMsg(null);

    try {
      await signUpUser(fullName, email);
      router.push("/services");
    } catch (err: any) {
      console.error("Sign up failed:", err);
      setErrorMsg(err.message || "Failed to create account.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-4">
      {errorMsg && (
        <div className="p-3 rounded-lg bg-red-50 border border-red-200 text-xs text-red-700 font-medium text-center">
          {errorMsg}
        </div>
      )}

      <div className="flex flex-col gap-1.5 text-left">
        <label htmlFor="fullName" className="text-sm font-semibold text-gray-700">
          Full Name
        </label>
        <input
          id="fullName"
          type="text"
          required
          value={fullName}
          placeholder="John Doe"
          onChange={(e) => setFullName(e.target.value)}
          className="w-full px-3.5 py-2.5 rounded-lg border border-gray-300 bg-white text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-transparent transition-all"
        />
      </div>

      <div className="flex flex-col gap-1.5 text-left">
        <label htmlFor="email" className="text-sm font-semibold text-gray-700">
          Email address
        </label>
        <input
          id="email"
          type="email"
          required
          value={email}
          placeholder="you@university.edu"
          onChange={(e) => setEmail(e.target.value)}
          className="w-full px-3.5 py-2.5 rounded-lg border border-gray-300 bg-white text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-transparent transition-all"
        />
      </div>

      <div className="flex flex-col gap-1.5 text-left">
        <label htmlFor="password" className="text-sm font-semibold text-gray-700">
          Password
        </label>
        <input
          id="password"
          type="password"
          required
          value={password}
          placeholder="Create password"
          onChange={(e) => setPassword(e.target.value)}
          className="w-full px-3.5 py-2.5 rounded-lg border border-gray-300 bg-white text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-transparent transition-all"
        />
      </div>

      <div className="flex flex-col gap-1.5 text-left">
        <label htmlFor="confirmPassword" className="text-sm font-semibold text-gray-700">
          Confirm Password
        </label>
        <input
          id="confirmPassword"
          type="password"
          required
          value={confirmPassword}
          placeholder="Confirm password"
          onChange={(e) => setConfirmPassword(e.target.value)}
          className="w-full px-3.5 py-2.5 rounded-lg border border-gray-300 bg-white text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-transparent transition-all"
        />
      </div>

      <button
        type="submit"
        disabled={loading}
        className="w-full py-2.5 px-4 mt-2 bg-sky-600 hover:bg-sky-700 disabled:bg-sky-400 text-white font-medium rounded-lg shadow-sm transition-colors cursor-pointer"
      >
        {loading ? "Creating Account..." : "Create Account"}
      </button>

      <div className="text-sm text-gray-600 text-center mt-2">
        Already have an account?{" "}
        <Link href="/" className="text-sky-600 hover:underline font-semibold">
          Sign in
        </Link>
      </div>
    </form>
  );
}