"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import type { Ticket } from "@/app/libs/types";
import { moveBackTicket, cancelTicket } from "@/app/libs/api";

interface ActiveTicketViewProps {
  initialTicket: Ticket;
}

export default function ActiveTicketView({ initialTicket }: ActiveTicketViewProps) {
  const router = useRouter();
  const [ticket, setTicket] = useState<Ticket>(initialTicket);
  const [busy, setBusy] = useState(false);

  async function handleMoveBack() {
    setBusy(true);
    try {
      const updated = await moveBackTicket(ticket.id);
      setTicket(updated);
    } catch (error) {
      console.error("Failed to move back in queue", error);
    } finally {
      setBusy(false);
    }
  }

  async function handleLeaveQueue() {
    setBusy(true);
    try {
      await cancelTicket(ticket.id);
      router.push("/services");
    } catch (error) {
      console.error("Failed to leave queue", error);
      setBusy(false);
    }
  }

  const isCalled = ticket.status === "called";
  const isServed = ticket.status === "served";
  const isMissed = ticket.status === "missed";

  // Calculate mock current serving ticket number (e.g., 3 spots behind current ticket)
  const currentNum = parseInt(ticket.number.replace(/\D/g, ""), 10) || 42;
  const servingTicketNum = `A-0${Math.max(1, currentNum - ticket.positionAhead)}`;
  const totalWaitingInQueue = ticket.positionAhead + 5; // Example total waiting sum

  return (
    <div className="w-full max-w-md mx-auto flex flex-col gap-5">
      {/* Top Banner Status */}
      {isCalled && (
        <div className="p-4 rounded-xl bg-amber-50 border border-amber-200 text-amber-900 text-center animate-pulse">
          <p className="font-bold text-lg">It's your turn!</p>
          <p className="text-sm">Please proceed to Counter {ticket.counterId || "1"} and show your QR code.</p>
        </div>
      )}

      {/* Main Ticket Container */}
      <div className="bg-white rounded-xl shadow-md border border-gray-200 p-6 text-center">
        <span className="text-xs font-semibold uppercase tracking-wider text-gray-500">
          {ticket.serviceName}
        </span>
        
        {/* Ticket Big Number */}
        <h1 className="text-5xl font-extrabold text-sky-600 mt-2 mb-4">{ticket.number}</h1>

        {!isCalled && !isServed && !isMissed && (
          <>
            {/* Visual Queue Nodes Tracker */}
            <div className="flex items-center justify-center gap-2 my-4">
              <span className="text-[11px] font-semibold text-gray-400 font-mono">{servingTicketNum}</span>
              <div className="flex items-center gap-1.5 px-2">
                <span className="w-2.5 h-2.5 rounded-full bg-emerald-500" title="Now Serving"></span>
                <span className="w-2 h-2 rounded-full bg-gray-300"></span>
                <span className="w-2 h-2 rounded-full bg-gray-300"></span>
                <span className="w-3.5 h-3.5 rounded-full bg-sky-600 border-2 border-sky-200 animate-pulse" title="Your Spot"></span>
              </div>
              <span className="text-[11px] font-semibold text-sky-600 font-mono">{ticket.number}</span>
            </div>

            {/* 3-Column Metrics Grid */}
            <div className="grid grid-cols-3 gap-2 py-3 px-2 bg-gray-50 rounded-xl border border-gray-200 my-4 text-center">
                <div>
                    <p className="text-[10px] font-bold uppercase tracking-wider text-gray-400">Serving</p>
                    <p className="text-base font-extrabold text-gray-900 mt-0.5">{servingTicketNum}</p>
                </div>
                <div className="border-x border-gray-200">
                    <p className="text-[10px] font-bold uppercase tracking-wider text-gray-400">Position</p>
                    <p className="text-base font-extrabold text-sky-600 mt-0.5">
                    {ticket.positionAhead === 0 ? "Next" : `${ticket.positionAhead} ahead`}
                    </p>
                </div>
                <div>
                    <p className="text-[10px] font-bold uppercase tracking-wider text-gray-400">Est. Wait</p>
                    <p className="text-base font-extrabold text-gray-900 mt-0.5">~{ticket.estimatedWaitMin} min</p>
                </div>
                </div>
          </>
        )}

        {/* QR Code Verification Section */}
        <div className="mt-5 flex flex-col items-center">
          <p className="text-xs text-gray-500 mb-2">Scan at counter to check in</p>
          <div className="p-3 bg-gray-50 border border-gray-200 rounded-xl shadow-2xs">
            <img
              src={`https://api.qrserver.com/v1/create-qr-code/?size=150x150&data=${encodeURIComponent(
                ticket.signedQrPayload || ticket.id
              )}`}
              alt="Signed QR Code Ticket"
              className="w-36 h-36"
            />
          </div>
          <span className="text-[10px] text-gray-400 mt-2 font-mono">
            Signed Verification Code Active
          </span>
        </div>
      </div>

      {/* Action Controls */}
      {!isCalled && !isServed && !isMissed && (
        <div className="flex flex-col gap-2.5">
          <button
            disabled={busy}
            onClick={handleMoveBack}
            className="w-full py-2.5 px-4 bg-white hover:bg-gray-50 text-gray-800 font-medium rounded-xl border border-gray-300 shadow-2xs transition-colors cursor-pointer text-sm"
          >
            {busy ? "Updating position..." : "Come back later (move 3 spots back)"}
          </button>
          
          <button
            disabled={busy}
            onClick={handleLeaveQueue}
            className="w-full py-2.5 px-4 bg-white hover:bg-red-50 text-red-600 font-medium rounded-xl border border-red-200 shadow-2xs transition-colors cursor-pointer text-sm"
          >
            Leave queue
          </button>
        </div>
      )}
    </div>
  );
}