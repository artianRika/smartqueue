"use client";

import { useState } from "react";
import { useRouter } from "next/navigation";
import { joinQueue } from "@/app/libs/api";
import type { ServiceQueue } from "@/app/libs/types";

export default function ServiceCard({ service }: { service: ServiceQueue }) {
  const router = useRouter();
  const [busy, setBusy] = useState(false);
  const paused = service.status === "paused";

  async function handleJoin() {
    setBusy(true);
    const { ticketId } = await joinQueue(service.id);
    router.push(`/ticket/${ticketId}`);
  }

  return (
    <article className="flex flex-col justify-between p-6 bg-white rounded-xl shadow-md border border-gray-200 hover:shadow-lg transition-shadow">
      <div>
        <div className="flex items-center justify-between mb-3">
          <h2 className="text-lg font-bold text-gray-900">{service.name}</h2>
          <span
            className={`px-3 py-1 text-xs font-semibold rounded-full ${
              paused
                ? "bg-amber-100 text-amber-800 border border-amber-200"
                : "bg-emerald-100 text-emerald-800 border border-emerald-200"
            }`}
          >
            {paused ? "Paused" : "Open"}
          </span>
        </div>
        
        <p className="text-sm text-gray-600 mb-6">
          {paused
            ? "Queue is currently paused by staff."
            : `${service.waiting} people waiting • ~${service.estimatedWaitMin} min wait`}
        </p>
      </div>

      <button
        disabled={paused || busy}
        onClick={handleJoin}
        className={`w-full py-2.5 px-4 font-medium rounded-lg shadow-sm transition-colors cursor-pointer ${
          paused
            ? "bg-gray-200 text-gray-400 cursor-not-allowed"
            : busy
            ? "bg-sky-400 text-white cursor-wait"
            : "bg-sky-600 hover:bg-sky-700 text-white"
        }`}
      >
        {paused ? "Unavailable" : busy ? "Joining..." : "Join queue"}
      </button>
    </article>
  );
}