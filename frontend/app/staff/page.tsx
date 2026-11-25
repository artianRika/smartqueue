"use client";

import { useState } from "react";
import Navbar from "@/app/components/Navbar";
import StaffDashboard from "@/app/components/StaffDashboard";
import QRScannerModal from "@/app/components/QRScannerModal";

export default function StaffPage() {
  const [isScannerOpen, setIsScannerOpen] = useState(false);
  const [checkedInTicketId, setCheckedInTicketId] = useState<string | null>(null);

  function handleCheckInSuccess(ticketId: string) {
    setCheckedInTicketId(ticketId);
    setTimeout(() => setCheckedInTicketId(null), 5000);
  }

  return (
    <div className="min-h-screen bg-gray-100 text-gray-900 flex flex-col">
      {/* <Navbar /> */}

      <main className="flex-1 max-w-5xl w-full mx-auto px-6 py-8">
        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 mb-8">
          <div>
            <h1 className="text-3xl font-extrabold text-gray-900 tracking-tight">
              Staff Counter Management
            </h1>
            <p className="text-sm text-gray-600 mt-1">
              Manage queue operations, call next visitors, and verify signed QR codes at the desk.
            </p>
          </div>

          <button
            onClick={() => setIsScannerOpen(true)}
            className="py-2.5 px-4 bg-sky-600 hover:bg-sky-700 text-white font-medium rounded-lg shadow-sm transition-colors cursor-pointer text-sm flex items-center gap-2"
          >
            Scan QR Code
          </button>
        </div>

        {checkedInTicketId && (
          <div className="mb-6 p-4 rounded-xl bg-emerald-50 border border-emerald-200 text-emerald-800 text-sm font-semibold text-center">
            Ticket <span className="font-mono font-bold">{checkedInTicketId}</span> successfully verified and checked in!
          </div>
        )}

        {/* Pass your primary counter ID directly (e.g., "1" or your seeded counter GUID) */}
        <StaffDashboard serviceId="1" />

        <QRScannerModal
          isOpen={isScannerOpen}
          onClose={() => setIsScannerOpen(false)}
          onCheckInSuccess={handleCheckInSuccess}
        />
      </main>
    </div>
  );
}