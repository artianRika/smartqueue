"use client";

import { useState, useEffect } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";

interface UserProfile {
  id: string;
  fullName: string;
  email: string;
  role: number; // 0: Visitor, 1: Staff, 2: Admin
}

export default function Navbar() {
  const router = useRouter();
  const [user, setUser] = useState<UserProfile | null>(null);

  useEffect(() => {
    const userId = localStorage.getItem("smartQueue_userId");
    if (!userId) return;

    // Fetch user profile to determine role dynamically
    fetch(`http://localhost:5104/api/userProfiles`)
      .then((res) => res.json())
      .then((profiles: UserProfile[]) => {
        const currentUser = profiles.find((p) => p.id === userId);
        if (currentUser) {
          setUser(currentUser);
        }
      })
      .catch((err) => console.error("Failed to load user role for navbar", err));
  }, []);

  // Define menu items dynamically based on role
  const getNavLinks = () => {
    const links = [{ label: "Services", href: "/services" }];

    if (user?.role === 1) {
      links.push({ label: "Staff Counter", href: "/staff" });
    } else if (user?.role === 2) {
      links.push({ label: "Admin Dashboard", href: "/admin" });
    }

    return links;
  };

  const handleSignOut = () => {
    localStorage.removeItem("smartQueue_userId");
    localStorage.removeItem("smartQueue_activeTickets");
    router.push("/");
  };

  return (
    <header className="w-full bg-white border-b border-gray-200 sticky top-0 z-50 shadow-xs">
      <div className="max-w-5xl mx-auto px-6 h-16 flex items-center justify-between">
        <div className="flex items-center gap-3">
          <Link href="/services" className="text-xl font-bold text-gray-900 tracking-tight">
            Smart<span className="text-sky-600">Queue</span>
          </Link>
          {user && (
            <span className="text-xs px-2 py-0.5 bg-gray-100 text-gray-600 rounded-full font-medium">
              {user.role === 2 ? "Admin" : user.role === 1 ? "Staff" : "Visitor"}
            </span>
          )}
        </div>

        <nav className="flex items-center gap-6">
          {getNavLinks().map((link) => (
            <Link
              key={link.href}
              href={link.href}
              className="text-sm font-medium text-gray-600 hover:text-sky-600 transition-colors"
            >
              {link.label}
            </Link>
          ))}
          <button
            onClick={handleSignOut}
            className="text-sm font-medium text-red-600 hover:text-red-700 transition-colors cursor-pointer"
          >
            Sign out
          </button>
        </nav>
      </div>
    </header>
  );
}