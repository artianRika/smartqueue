import { getServices } from "@/app/libs/api";
import ServiceCard from "@/app/components/ServiceCard";

export default async function ServicesPage() {
  const services = await getServices();

  return (
    <main className="min-h-screen p-8 bg-gray-100 text-gray-900">
      <div className="max-w-3xl mx-auto">
        <h1 className="text-3xl font-bold mb-2 text-center text-gray-900">
          Available Services
        </h1>
        <p className="text-gray-600 mb-8 text-center">
          Select a service counter below to join the virtual queue.
        </p>

        <div className="grid gap-4 md:grid-cols-2">
          {services.map((service) => (
            <ServiceCard key={service.id} service={service} />
          ))}
        </div>
      </div>
    </main>
  );
}