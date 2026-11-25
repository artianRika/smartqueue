
"use client";

import { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth, useUser } from "@clerk/nextjs";
import { getOrCreateProfileUser } from "@/app/libs/api/auth";
import { joinQueue } from "../libs/api/queues";
import type { ServiceQueue } from "@/app/libs/types";

export default function ServiceCard({ service }: { service: ServiceQueue }) {
  const router = useRouter();
  const { getToken } = useAuth();
  const { user } = useUser();
  const [isJoining, setIsJoining] = useState(false);
  const [existingTicketId, setExistingTicketId] = useState<string | null>(null);

  const isServiceOpen =
    service.isOpen === true ||
    service.status?.toLowerCase() === "open" ||
    service.status?.toLowerCase() === "active";

  useEffect(() => {
    let isMounted = true;

    const syncTicketState = async () => {
      setIsJoining(false);
      const activeTickets = JSON.parse(
        localStorage.getItem("smartQueue_activeTickets") || "{}"
      );
      const storedTicketId = activeTickets[service.id];

      if (!storedTicketId) {
        if (isMounted) setExistingTicketId(null);
        return;
      }

      try {
        const token = await getToken();
        const res = await fetch(
          `http://localhost:5001/api/tickets/${storedTicketId}`,
          {
            headers: {
              ...(token ? { Authorization: `Bearer ${token}` } : {}),
            },
          }
        );

        if (res.ok) {
          const ticket = await res.json();
          const isActive =
            ticket.status === "Waiting" || ticket.status === "Called";

          if (isActive) {
            if (isMounted) setExistingTicketId(storedTicketId);
            return;
          }
        }

        delete activeTickets[service.id];
        localStorage.setItem(
          "smartQueue_activeTickets",
          JSON.stringify(activeTickets)
        );
        if (isMounted) setExistingTicketId(null);
      } catch {
        if (isMounted) setExistingTicketId(storedTicketId);
      }
    };

    syncTicketState();
    window.addEventListener("pageshow", syncTicketState);
    return () => {
      isMounted = false;
      window.removeEventListener("pageshow", syncTicketState);
    };
  }, [service.id, getToken]);

  const handleAction = async () => {
    if (existingTicketId) {
      router.push(`/ticket/${existingTicketId}`);
      return;
    }

    try {
      setIsJoining(true);

      const token = await getToken();

      if (!token) {
        throw new Error("Clerk authentication token is missing. Please sign in again.");
      }

      const email = user?.primaryEmailAddress?.emailAddress;

      if (!email) {
        throw new Error("Please sign in again. User email not found.");
      }

      const fullName =
        user.fullName ||
        [user.firstName, user.lastName].filter(Boolean).join(" ") ||
        email.split("@")[0];

      const profile = await getOrCreateProfileUser(
        fullName,
        email,
        token
      );

      const ticket = await joinQueue(service.id, profile.id, token);
      const assignedTicketId =
        ticket.ticketId || (ticket as { id?: string }).id;

      if (!assignedTicketId) {
        throw new Error("No ticket ID returned from server.");
      }

      const activeTickets = JSON.parse(
        localStorage.getItem("smartQueue_activeTickets") || "{}"
      );

      activeTickets[service.id] = assignedTicketId;

      localStorage.setItem(
        "smartQueue_activeTickets",
        JSON.stringify(activeTickets)
      );

      setExistingTicketId(assignedTicketId);
      router.push(`/ticket/${assignedTicketId}`);
    } catch (err) {
      console.error("Join queue failed:", err);
    } finally {
      setIsJoining(false);
    }
  };

  const getButtonStyle = () => {
    if (isJoining) {
      return "bg-sky-600 opacity-70 text-white cursor-wait";
    }
    if (existingTicketId) {
      return "bg-white text-sky-600 border-2 border-sky-600 hover:bg-sky-50";
    }
    if (isServiceOpen) {
      return "bg-sky-600 hover:opacity-90 text-white";
    }
    return "bg-gray-100 text-gray-400 cursor-not-allowed";
  };

  const getButtonLabel = () => {
    if (isJoining) return "Joining...";
    if (existingTicketId) return "View My Ticket";
    if (isServiceOpen) return "Join queue";
    return "Unavailable";
  };

  return (
    <div className="border border-gray-200 p-5 rounded-xl bg-white shadow-sm flex flex-col justify-between">
      <div>
        <div className="flex justify-between items-start mb-2">
          <h3 className="font-semibold text-lg text-gray-900">{service.name}</h3>
          <div className="flex items-center gap-2">
            {existingTicketId && (
              <span className="px-2.5 py-0.5 text-xs font-medium rounded-full bg-blue-100 text-blue-800">
                Ticket Active
              </span>
            )}
            <span
              className={`px-2.5 py-0.5 text-xs font-medium rounded-full ${
                isServiceOpen
                  ? "bg-green-100 text-green-800"
                  : "bg-amber-100 text-amber-800"
              }`}
            >
              {isServiceOpen ? "Open" : "Paused"}
            </span>
          </div>
        </div>
        <p className="text-sm text-gray-500 mb-4">
          {service.description ||
            "Select to join the virtual line and track your ticket status live."}
        </p>
      </div>

      <button
        onClick={handleAction}
        disabled={isJoining || (!isServiceOpen && !existingTicketId)}
        className={`w-full py-2.5 px-4 rounded-lg font-medium transition-colors ${getButtonStyle()}`}
      >
        {getButtonLabel()}
      </button>
    </div>
  );
}
