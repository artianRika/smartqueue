import SignInForm from "./components/SignInForm";

export default function Home() {
  return (
    <main className="flex min-h-screen flex-col items-center justify-center p-6 bg-gray-100 text-gray-900">
      <div className="w-full max-w-md p-8 bg-white rounded-xl shadow-lg border border-gray-200">
        <h1 className="text-2xl font-bold mb-6 text-gray-900 text-center">
          SmartQueue Sign In
        </h1>
        <SignInForm />
      </div>
    </main>
  );
}