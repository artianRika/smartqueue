"use client";

import { use, Suspense, useEffect, useState } from "react";
import Navbar from "@/app/components/Navbar";
import ActiveTicketView from "@/app/components/ActiveTicketView";
import type { Ticket } from "@/app/libs/types";
import { getTicket } from "@/app/libs/api";

function TicketContent({ params }: { params: Promise<{ id: string }> }) {
  // Unwrapping params INSIDE the Suspense boundary
  const { id } = use(params);
  
  const [ticket, setTicket] = useState<Ticket | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    async function loadTicket() {
      try {
        const data = await getTicket(id);
        setTicket(data);
      } catch (err) {
        console.error("Failed to load ticket:", err);
        setError("Ticket not found or has expired.");
      } finally {
        setLoading(false);
      }
    }
    loadTicket();
  }, [id]);

  if (loading) {
    return (
      <div className="py-20 text-center text-sm font-medium text-gray-500 animate-pulse">
        Loading active ticket...
      </div>
    );
  }

  if (error) {
    return (
      <div className="py-12 bg-white rounded-xl border border-red-200 p-8 shadow-xs max-w-md w-full text-center">
        <p className="text-red-600 font-semibold mb-2">{error}</p>
        <a href="/services" className="text-sm text-sky-600 hover:underline font-medium">
          Back to Services
        </a>
      </div>
    );
  }

  return ticket ? <ActiveTicketView initialTicket={ticket} /> : null;
}

export default function TicketPage({ params }: { params: Promise<{ id: string }> }) {
  return (
    <div className="min-h-screen bg-gray-100 text-gray-900 flex flex-col">
      <Navbar />
      <main className="flex-1 max-w-5xl w-full mx-auto px-6 py-8 flex flex-col items-center">
        <Suspense fallback={<div className="py-20 text-center text-sm text-gray-500">Loading active ticket...</div>}>
          <TicketContent params={params} />
        </Suspense>
      </main>
    </div>
  );
}