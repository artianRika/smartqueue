"use client";

import React, { useState } from "react";
import { UserProfile } from "@/app/libs/types";
import { Bell, Shield, CheckCircle2, User, Mail, Phone } from "lucide-react";

// Example initial state (replace with fetched user data / context)
const MOCK_USER: UserProfile = {
  id: "usr_101",
  fullName: "Alex Morgan",
  email: "alex.morgan@example.com",
  phoneNumber: "+49 151 23456789",
  role: "Customer",
  notificationLeadTime: 10,
  createdAt: "2026-10-12T10:00:00Z",
};

export default function UserProfilePage() {
  const [profile, setProfile] = useState<UserProfile>(MOCK_USER);
  const [saving, setSaving] = useState(false);
  const [savedMessage, setSavedMessage] = useState(false);

  const handleSave = async (e: React.FormEvent) => {
    e.preventDefault();
    setSaving(true);
    setSavedMessage(false);

    try {
      // Replace with PUT /api/users/me
      await new Promise((resolve) => setTimeout(resolve, 500));
      setSavedMessage(true);
      setTimeout(() => setSavedMessage(false), 3000);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="max-w-3xl mx-auto px-4 py-10">
      <div className="mb-8">
        <h1 className="text-2xl font-bold text-gray-900">Account Settings</h1>
        <p className="text-sm text-gray-500 mt-1">
          Manage your profile details and queue notification preferences
        </p>
      </div>

      {/* Header Card */}
      <div className="bg-white rounded-2xl border border-gray-200 p-6 mb-6 flex flex-col sm:flex-row items-center gap-5">
        <div className="h-16 w-16 rounded-full bg-blue-600 text-white flex items-center justify-center text-2xl font-bold shrink-0">
          {profile.fullName.charAt(0).toUpperCase()}
        </div>
        <div className="text-center sm:text-left flex-1">
          <div className="flex flex-col sm:flex-row sm:items-center gap-2">
            <h2 className="text-lg font-bold text-gray-900">{profile.fullName}</h2>
            <span className="inline-flex items-center gap-1 px-2.5 py-0.5 rounded-full text-xs font-medium bg-blue-50 text-blue-700 w-fit mx-auto sm:mx-0">
              <Shield className="h-3 w-3" />
              {profile.role}
            </span>
          </div>
          <p className="text-sm text-gray-500">{profile.email}</p>
        </div>
      </div>

      {/* Profile & Preferences Form */}
      <form onSubmit={handleSave} className="space-y-6">
        <div className="bg-white rounded-2xl border border-gray-200 p-6 space-y-5">
          <h3 className="text-base font-semibold text-gray-900 border-b border-gray-100 pb-3">
            Personal Information
          </h3>

          <div className="grid grid-cols-1 sm:grid-cols-2 gap-5">
            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Full Name
              </label>
              <div className="relative">
                <User className="h-4 w-4 text-gray-400 absolute left-3 top-3" />
                <input
                  type="text"
                  required
                  value={profile.fullName}
                  onChange={(e) =>
                    setProfile({ ...profile, fullName: e.target.value })
                  }
                  className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>

            <div>
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Phone Number
              </label>
              <div className="relative">
                <Phone className="h-4 w-4 text-gray-400 absolute left-3 top-3" />
                <input
                  type="tel"
                  value={profile.phoneNumber || ""}
                  onChange={(e) =>
                    setProfile({ ...profile, phoneNumber: e.target.value })
                  }
                  className="w-full pl-9 pr-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-blue-500"
                />
              </div>
            </div>

            <div className="sm:col-span-2">
              <label className="block text-sm font-medium text-gray-700 mb-1">
                Email Address
              </label>
              <div className="relative">
                <Mail className="h-4 w-4 text-gray-400 absolute left-3 top-3" />
                <input
                  type="email"
                  disabled
                  value={profile.email}
                  className="w-full pl-9 pr-3 py-2 border border-gray-200 bg-gray-50 text-gray-500 rounded-lg text-sm cursor-not-allowed"
                />
              </div>
            </div>
          </div>
        </div>

        {/* Queue Notification Preferences */}
        <div className="bg-white rounded-2xl border border-gray-200 p-6 space-y-4">
          <div className="flex items-center gap-2 border-b border-gray-100 pb-3">
            <Bell className="h-5 w-5 text-blue-600" />
            <h3 className="text-base font-semibold text-gray-900">
              Queue Alert Preferences
            </h3>
          </div>

          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1">
              Default Notification Lead Time
            </label>
            <p className="text-xs text-gray-500 mb-3">
              Choose how many minutes before your estimated service time you want to be notified via SignalR / push alert.
            </p>
            <select
              value={profile.notificationLeadTime}
              onChange={(e) =>
                setProfile({
                  ...profile,
                  notificationLeadTime: Number(e.target.value),
                })
              }
              className="w-full sm:w-64 px-3 py-2 border border-gray-300 rounded-lg text-sm bg-white focus:outline-none focus:ring-2 focus:ring-blue-500"
            >
              <option value={5}>5 minutes before</option>
              <option value={10}>10 minutes before</option>
              <option value={15}>15 minutes before</option>
              <option value={20}>20 minutes before</option>
            </select>
          </div>
        </div>

        {/* Save Bar */}
        <div className="flex items-center justify-end gap-4">
          {savedMessage && (
            <span className="flex items-center gap-1.5 text-sm text-green-600 font-medium">
              <CheckCircle2 className="h-4 w-4" />
              Changes saved
            </span>
          )}
          <button
            type="submit"
            disabled={saving}
            className="px-6 py-2.5 bg-blue-600 hover:bg-blue-700 disabled:bg-blue-400 text-white font-medium rounded-lg text-sm transition cursor-pointer"
          >
            {saving ? "Saving..." : "Save Changes"}
          </button>
        </div>
      </form>
    </div>
  );
}