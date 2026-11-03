"use client";

import { useState } from "react";
import { verifyAndCheckInTicket } from "@/app/libs/api";

interface QRScannerModalProps {
  isOpen: boolean;
  onClose: () => void;
  onCheckInSuccess: (ticketId: string) => void;
}

export default function QRScannerModal({
  isOpen,
  onClose,
  onCheckInSuccess,
}: QRScannerModalProps) {
  const [manualCode, setManualCode] = useState("");
  const [verifying, setVerifying] = useState(false);
  const [errorMsg, setErrorMsg] = useState<string | null>(null);

  if (!isOpen) return null;

  async function handleVerify(codeToVerify: string) {
    if (!codeToVerify.trim()) return;
    setVerifying(true);
    setErrorMsg(null);

    try {
      const result = await verifyAndCheckInTicket(codeToVerify);
      if (result.success) {
        onCheckInSuccess(result.ticketId);
        onClose();
      } else {
        setErrorMsg(result.message || "Invalid or expired ticket code.");
      }
    } catch (err) {
      console.error("QR Verification Error:", err);
      setErrorMsg("Failed to verify signed QR payload.");
    } finally {
      setVerifying(false);
    }
  }

  function handleFormSubmit(e: React.FormEvent) {
    e.preventDefault();
    handleVerify(manualCode);
  }

  return (
    <div className="fixed inset-0 z-50 flex items-center justify-center bg-gray-900/50 backdrop-blur-xs p-4">
      <div className="w-full max-w-md bg-white rounded-xl shadow-xl border border-gray-200 p-6 flex flex-col gap-5">
        <div className="flex items-center justify-between border-b border-gray-100 pb-3">
          <h3 className="text-lg font-bold text-gray-900">Scan Visitor QR Ticket</h3>
          <button
            onClick={onClose}
            className="text-gray-400 hover:text-gray-600 font-bold text-xl cursor-pointer"
          >
            &times;
          </button>
        </div>

        {/* Camera Scanner Simulation Viewport */}
        <div className="w-full h-48 bg-gray-900 rounded-lg flex flex-col items-center justify-center text-white relative overflow-hidden border border-gray-800">
          <div className="w-36 h-36 border-2 border-dashed border-sky-400 rounded-lg flex items-center justify-center animate-pulse">
            <span className="text-xs text-sky-200">Align QR Code Here</span>
          </div>
          <span className="text-[11px] text-gray-400 absolute bottom-2">
            Camera Active • Scanning Signed Payload
          </span>
        </div>

        {errorMsg && (
          <div className="p-3 rounded-lg bg-red-50 border border-red-200 text-xs text-red-700 font-medium text-center">
            {errorMsg}
          </div>
        )}

        {/* Manual Fallback Entry */}
        <form onSubmit={handleFormSubmit} className="flex flex-col gap-3">
          <label htmlFor="manualCode" className="text-xs font-semibold text-gray-700">
            Or enter QR code payload manually:
          </label>
          <div className="flex gap-2">
            <input
              id="manualCode"
              type="text"
              value={manualCode}
              onChange={(e) => setManualCode(e.target.value)}
              placeholder="e.g. TICKET-9402-SIG"
              className="flex-1 px-3 py-2 text-sm rounded-lg border border-gray-300 bg-white text-gray-900 focus:outline-none focus:ring-2 focus:ring-sky-500"
            />
            <button
              type="submit"
              disabled={verifying || !manualCode.trim()}
              className="px-4 py-2 bg-sky-600 hover:bg-sky-700 disabled:bg-gray-300 text-white font-medium rounded-lg text-sm transition-colors cursor-pointer"
            >
              {verifying ? "Checking..." : "Verify"}
            </button>
          </div>
        </form>

        <button
          onClick={onClose}
          className="w-full py-2 bg-gray-100 hover:bg-gray-200 text-gray-700 font-medium rounded-lg text-sm transition-colors cursor-pointer mt-1"
        >
          Cancel
        </button>
      </div>
    </div>
  );
}