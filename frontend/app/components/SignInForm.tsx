"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import Link from "next/link";

export default function SignInForm() {
  const router = useRouter();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");

  function handleSubmit(e: React.FormEvent) {
    e.preventDefault();
    router.push("/services");
  }

  return (
    <form onSubmit={handleSubmit} className="flex flex-col gap-5">
      <div className="flex flex-col gap-1.5 text-left">
        <label htmlFor="email" className="text-sm font-semibold text-gray-700">
          Email
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
          placeholder="Password"
          onChange={(e) => setPassword(e.target.value)}
          className="w-full px-3.5 py-2.5 rounded-lg border border-gray-300 bg-white text-gray-900 placeholder-gray-400 focus:outline-none focus:ring-2 focus:ring-sky-500 focus:border-transparent transition-all"
        />
      </div>

      <button
        type="submit"
        className="w-full py-2.5 px-4 mt-2 bg-sky-600 hover:bg-sky-700 text-white font-medium rounded-lg shadow-sm transition-colors cursor-pointer"
      >
        Sign in
      </button>

      <div className="text-sm text-gray-600 text-center mt-2">
        New here?{" "}
        <Link href="/signup" className="text-sky-600 hover:underline font-semibold">
          Create an account
        </Link>
      </div>
    </form>
  );
}