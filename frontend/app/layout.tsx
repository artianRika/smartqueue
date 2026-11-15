import type { Metadata } from "next";
import { Inter } from "next/font/google";
import "./globals.css";
import {ClerkProvider} from "@clerk/nextjs";

const inter = Inter({ subsets: ["latin"], variable: "--font-inter" });

export const metadata: Metadata = {
  title: "SmartQueue",
  description: "Join a virtual queue and track your turn live.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
      <ClerkProvider>
        <html lang="en" className={inter.variable}>
        <body>{children}</body>
        </html>
      </ClerkProvider>
  );
}
