"use client";

import { useState, useEffect } from "react";
import type { ServiceQueue, Ticket } from "@/app/libs/types";
import { callNextTicket, markTicketServed, markTicketMissed, toggleQueueStatus, getStaffQueue } from "@/app/libs/api";

export default function StaffDashboard({ serviceId }: { serviceId: string }) {
  const [queue, setQueue] = useState<ServiceQueue | null>(null);
  const [currentTicket, setCurrentTicket] = useState<Ticket | null>(null);
  const [busy, setBusy] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadData() {
      try {
        const data = await getStaffQueue(serviceId);
        setQueue(data.queue);
        setCurrentTicket(data.currentTicket || null);
      } catch (error) {
        console.error("Failed to load staff queue data", error);
      } finally {
        setLoading(false);
      }
    }
    loadData();
  }, [serviceId]);

  async function handleCallNext() {
    setBusy(true);
    try {
      const result = await callNextTicket(serviceId);
      setCurrentTicket(result.ticket);
      if (queue) {
        setQueue({ ...queue, waiting: Math.max(0, queue.waiting - 1) });
      }
    } catch (error) {
      console.error("Failed to call next ticket", error);
    } finally {
      setBusy(false);
    }
  }

  async function handleServed() {
    if (!currentTicket) return;
    setBusy(true);
    try {
      await markTicketServed(currentTicket.id);
      setCurrentTicket(null);
    } catch (error) {
      console.error("Failed to mark ticket served", error);
    } finally {
      setBusy(false);
    }
  }

  async function handleMissed() {
    if (!currentTicket) return;
    setBusy(true);
    try {
      await markTicketMissed(currentTicket.id);
      setCurrentTicket(null);
    } catch (error) {
      console.error("Failed to mark ticket missed", error);
    } finally {
      setBusy(false);
    }
  }

  async function handleTogglePause() {
    if (!queue) return;
    const newStatus = queue.status === "paused" ? "open" : "paused";
    try {
      const updated = await toggleQueueStatus(queue.id, newStatus);
      setQueue(updated);
    } catch (error) {
      console.error("Failed to update queue status", error);
    }
  }

  if (loading) {
    return (
      <div className="flex justify-center items-center py-16">
        <p className="text-sm font-medium text-gray-500 animate-pulse">Loading staff dashboard...</p>
      </div>
    );
  }

  const isPaused = queue?.status === "paused";

  return (
    <div className="w-full max-w-2xl mx-auto flex flex-col gap-6">
      <div className="bg-white rounded-xl shadow-md border border-gray-200 p-6 flex items-center justify-between">
        <div>
          <span className="text-xs font-semibold uppercase tracking-wider text-gray-500">Counter Control Panel</span>
          <h1 className="text-2xl font-bold text-gray-900 mt-1">{queue?.name || "Service Queue"}</h1>
          <p className="text-sm text-gray-600 mt-1">
            {queue?.waiting || 0} visitors waiting • Est. wait ~{queue?.estimatedWaitMin || 0} min
          </p>
        </div>

        <button
          onClick={handleTogglePause}
          className={`px-4 py-2 text-sm font-medium rounded-lg border transition-colors cursor-pointer ${
            isPaused
              ? "bg-emerald-50 border-emerald-300 text-emerald-800 hover:bg-emerald-100"
              : "bg-amber-50 border-amber-300 text-amber-800 hover:bg-amber-100"
          }`}
        >
          {isPaused ? "Resume Queue" : "Pause Queue"}
        </button>
      </div>

      <div className="bg-white rounded-xl shadow-md border border-gray-200 p-6 text-center">
        <h2 className="text-sm font-semibold uppercase tracking-wider text-gray-500 mb-2">
          Currently Serving at Counter
        </h2>

        {currentTicket ? (
          <div className="flex flex-col items-center gap-4 my-4">
            <span className="text-6xl font-extrabold text-sky-600">{currentTicket.number}</span>
            <p className="text-sm text-gray-600">Visitor ID: {currentTicket.id.slice(0, 8)}...</p>

            <div className="grid grid-cols-2 gap-3 w-full max-w-md mt-2">
              <button
                disabled={busy}
                onClick={handleServed}
                className="py-2.5 px-4 bg-emerald-600 hover:bg-emerald-700 text-white font-medium rounded-lg shadow-sm transition-colors cursor-pointer text-sm"
              >
                Mark Served
              </button>
              <button
                disabled={busy}
                onClick={handleMissed}
                className="py-2.5 px-4 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium rounded-lg border border-gray-300 transition-colors cursor-pointer text-sm"
              >
                Mark Missed
              </button>
            </div>
          </div>
        ) : (
          <div className="py-8 flex flex-col items-center gap-4">
            <p className="text-gray-500 text-sm">No ticket currently called at this counter.</p>
            <button
              disabled={busy || isPaused}
              onClick={handleCallNext}
              className={`py-3 px-8 font-semibold rounded-lg shadow-sm transition-colors cursor-pointer text-base ${
                isPaused
                  ? "bg-gray-200 text-gray-400 cursor-not-allowed"
                  : "bg-sky-600 hover:bg-sky-700 text-white"
              }`}
            >
              {busy ? "Calling..." : "Call Next Ticket"}
            </button>
          </div>
        )}
      </div>
    </div>
  );
}