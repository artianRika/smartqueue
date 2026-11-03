import Navbar from "@/app/components/Navbar";
import ServicesList from "@/app/components/ServicesList";

export default function ServicesPage() {
  return (
    <div className="min-h-screen bg-gray-100 text-gray-900 flex flex-col">
      <Navbar />

      <main className="flex-1 max-w-5xl w-full mx-auto px-6 py-8">
        <div className="mb-8 text-left">
          <h1 className="text-3xl font-extrabold text-gray-900 tracking-tight">
            Available Queue Services
          </h1>
          <p className="text-sm text-gray-600 mt-1">
            Select a counter below to join the virtual line and track your ticket status live.
          </p>
        </div>

        <ServicesList />
      </main>
    </div>
  );
}