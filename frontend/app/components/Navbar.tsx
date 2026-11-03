"use client";

import Link from "next/link";
import { useRouter } from "next/navigation";

export default function Navbar() {
  const router = useRouter();

  return (
    <header className="w-full bg-white border-b border-gray-200 sticky top-0 z-50 shadow-xs">
      <div className="max-w-5xl mx-auto px-6 h-16 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link href="/services" className="text-xl font-bold text-gray-900 tracking-tight">
            Smart<span className="text-sky-600">Queue</span>
          </Link>
        </div>

        <nav className="flex items-center gap-4">
          <Link
            href="/services"
            className="text-sm font-medium text-gray-600 hover:text-sky-600 transition-colors"
          >
            Services
          </Link>
          <button
            onClick={() => router.push("/")}
            className="text-sm font-medium text-red-600 hover:text-red-700 transition-colors cursor-pointer"
          >
            Sign out
          </button>
        </nav>
      </div>
    </header>
  );
}