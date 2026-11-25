"use client";

import React, { useState, Suspense } from "react";
import Link from "next/link";
import { usePathname, useRouter } from "next/navigation";
import { useUser, useClerk } from "@clerk/nextjs";
import { Menu, X, Ticket, User, LogOut, LayoutDashboard } from "lucide-react";

function NavbarContent() {
  const [mobileMenuOpen, setMobileMenuOpen] = useState(false);
  const pathname = usePathname();
  const router = useRouter();
  const { isLoaded, isSignedIn, user } = useUser();
  const { signOut } = useClerk();

  // Read role from Clerk publicMetadata (defaults to Customer)
  const role = (user?.publicMetadata?.role as string) || "Customer";

  const handleLogout = async () => {
    await signOut();
    router.push("/auth?mode=signin");
  };

  const isActive = (path: string) =>
    pathname === path
      ? "text-blue-600 font-semibold"
      : "text-gray-600 hover:text-gray-900";

  return (
    <nav className="bg-white border-b border-gray-200 sticky top-0 z-50">
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex justify-between h-16">
          {/* Brand Logo */}
          <div className="flex items-center">
            <Link href="/services" className="flex items-center gap-2">
              <div className="bg-blue-600 text-white p-1.5 rounded-lg">
                <Ticket className="h-6 w-6" />
              </div>
              <span className="text-xl font-bold text-gray-900 tracking-tight">
                SmartQueue
              </span>
            </Link>
          </div>

          {/* Desktop Navigation */}
          <div className="hidden md:flex md:items-center md:space-x-6">
            <Link href="/services" className={`text-sm ${isActive("/services")}`}>
              Live Queues
            </Link>

            {isLoaded && isSignedIn && user ? (
              <>
                {(role === "Staff" || role === "Admin") && (
                  <Link
                    href="/staff"
                    className={`flex items-center gap-1 text-sm ${isActive("/staff")}`}
                  >
                    <LayoutDashboard className="h-4 w-4" />
                    Staff Desk
                  </Link>
                )}

                <div className="h-5 w-px bg-gray-200" />

                <Link
                  href="/profile"
                  className={`flex items-center gap-2 text-sm ${isActive("/profile")}`}
                >
                  <div className="h-8 w-8 rounded-full bg-blue-100 text-blue-700 flex items-center justify-center font-semibold">
                    {(user.firstName || user.emailAddresses[0]?.emailAddress || "U")
                      .charAt(0)
                      .toUpperCase()}
                  </div>
                  <span>{user.firstName || "Profile"}</span>
                </Link>

                <button
                  onClick={handleLogout}
                  className="flex items-center gap-1 text-sm text-red-600 hover:text-red-700 font-medium cursor-pointer"
                >
                  <LogOut className="h-4 w-4" />
                  Sign out
                </button>
              </>
            ) : isLoaded ? (
              <div className="flex items-center gap-3">
                <Link
                  href="/auth?mode=signin"
                  className="text-sm font-medium text-gray-700 hover:text-blue-600 px-3 py-2"
                >
                  Sign in
                </Link>
                <Link
                  href="/auth?mode=signup"
                  className="text-sm font-medium text-white bg-blue-600 hover:bg-blue-700 px-4 py-2 rounded-lg transition"
                >
                  Sign up
                </Link>
              </div>
            ) : null}
          </div>

          {/* Mobile Hamburger Button */}
          <div className="flex items-center md:hidden">
            <button
              onClick={() => setMobileMenuOpen(!mobileMenuOpen)}
              className="text-gray-600 hover:text-gray-900 p-2 rounded-md"
              aria-label="Toggle Menu"
            >
              {mobileMenuOpen ? <X className="h-6 w-6" /> : <Menu className="h-6 w-6" />}
            </button>
          </div>
        </div>
      </div>

      {/* Mobile Dropdown */}
      {mobileMenuOpen && (
        <div className="md:hidden bg-white border-b border-gray-200 px-4 pt-2 pb-4 space-y-2">
          <Link
            href="/services"
            onClick={() => setMobileMenuOpen(false)}
            className="block py-2 text-base font-medium text-gray-700"
          >
            Live Queues
          </Link>

          {isLoaded && isSignedIn && user ? (
            <>
              {(role === "Staff" || role === "Admin") && (
                <Link
                  href="/staff"
                  onClick={() => setMobileMenuOpen(false)}
                  className="block py-2 text-base font-medium text-gray-700"
                >
                  Staff Desk
                </Link>
              )}
              <Link
                href="/profile"
                onClick={() => setMobileMenuOpen(false)}
                className="flex items-center gap-2 py-2 text-base font-medium text-gray-700"
              >
                <User className="h-4 w-4" /> Profile ({user.firstName || "Account"})
              </Link>
              <button
                onClick={() => {
                  setMobileMenuOpen(false);
                  handleLogout();
                }}
                className="w-full text-left py-2 text-base font-medium text-red-600"
              >
                Sign out
              </button>
            </>
          ) : (
            <div className="pt-2 flex flex-col gap-2">
              <Link
                href="/auth?mode=signin"
                onClick={() => setMobileMenuOpen(false)}
                className="w-full text-center py-2 border border-gray-300 rounded-lg text-gray-700 font-medium"
              >
                Sign in
              </Link>
              <Link
                href="/auth?mode=signup"
                onClick={() => setMobileMenuOpen(false)}
                className="w-full text-center py-2 bg-blue-600 text-white rounded-lg font-medium"
              >
                Sign up
              </Link>
            </div>
          )}
        </div>
      )}
    </nav>
  );
}

export default function Navbar() {
  return (
    <Suspense fallback={<div className="h-16 w-full bg-white border-b border-gray-200" />}>
      <NavbarContent />
    </Suspense>
  );
}