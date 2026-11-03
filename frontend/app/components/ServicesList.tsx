"use client";

import { useEffect, useState } from "react";
import ServiceCard from "@/app/components/ServiceCard";
import type { ServiceQueue } from "@/app/libs/types";
import { getServices } from "@/app/libs/api";

export default function ServicesList() {
  const [services, setServices] = useState<ServiceQueue[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    async function loadServices() {
      try {
        const data = await getServices();
        setServices(data);
      } catch (error) {
        console.error("Failed to load services", error);
      } finally {
        setLoading(false);
      }
    }
    loadServices();
  }, []);

  if (loading) {
    return (
      <div className="flex justify-center items-center py-16">
        <p className="text-sm font-medium text-gray-500 animate-pulse">Loading available queues...</p>
      </div>
    );
  }

  if (services.length === 0) {
    return (
      <div className="text-center py-12 bg-white rounded-xl border border-gray-200 p-8 shadow-xs">
        <p className="text-gray-600">No service queues are currently available.</p>
      </div>
    );
  }

  return (
    <div className="grid grid-cols-1 md:grid-cols-2 gap-6 w-full">
      {services.map((service) => (
        <ServiceCard key={service.id} service={service} />
      ))}
    </div>
  );
}