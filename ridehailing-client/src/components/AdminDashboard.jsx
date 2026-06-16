import React, { useState, useEffect } from 'react';
import { Shield, Settings, CreditCard, LogOut, Car, Users } from 'lucide-react';
import LiveTrips from './LiveTrips';
import AuditLogs from './AuditLogs';

export default function AdminDashboard({ onLogout }) {
  const [activeTab, setActiveTab] = useState('general');
  const [loading, setLoading] = useState(true);
  const [settings, setSettings] = useState({});

  useEffect(() => {
    if (activeTab !== 'general' && activeTab !== 'fares') return;

    async function loadBackendSettings() {
      try {
        setLoading(true);
        const response = await fetch('http://localhost:5000/api/settings/public');
        if (response.ok) {
          const data = await response.json();
          const mappedSettings = {};
          data.forEach(item => { mappedSettings[item.key] = item.value; });
          setSettings(mappedSettings);
        }
      } catch (error) {
        console.error("Failed to reach backend API server:", error);
      } finally {
        setLoading(false);
      }
    }
    loadBackendSettings();
  }, [activeTab]);

  const handleSaveSettings = async (e) => {
    e.preventDefault();
    try {
      for (const [key, value] of Object.entries(settings)) {
        await fetch(`http://localhost:5000/api/settings/${key}`, {
          method: 'PATCH',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({ value: value })
        });
      }
      alert('Parameters successfully committed to PostgreSQL database! Audit logs created.');
    } catch (error) {
      alert('Error updating database settings.');
    }
  };

  const handleSettingChange = (key, value) => {
    setSettings(prev => ({ ...prev, [key]: value }));
  };

  return (
    <div className="fixed inset-0 bg-slate-950 flex text-slate-100 font-sans z-10">
      {/* Sidebar Navigation */}
      <div className="w-64 bg-slate-900 border-r border-slate-800 flex flex-col justify-between p-4">
        <div>
          <div className="flex items-center gap-3 px-2 py-4 border-b border-slate-800 mb-6">
            <Shield className="w-6 h-6 text-amber-500" />
            <span className="font-bold text-lg tracking-wide">BiyahePro Admin</span>
          </div>

          <nav className="space-y-1">
            <button
              type="button"
              onClick={() => setActiveTab('general')}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl font-medium text-sm transition ${
                activeTab === 'general' ? 'bg-amber-500 text-slate-950 shadow-lg shadow-amber-500/10' : 'text-slate-400 hover:bg-slate-800'
              }`}
            >
              <Settings className="w-4 h-4" /> System Control Variables
            </button>
            <button
              type="button"
              onClick={() => setActiveTab('fares')}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl font-medium text-sm transition ${
                activeTab === 'fares' ? 'bg-amber-500 text-slate-950 shadow-lg shadow-amber-500/10' : 'text-slate-400 hover:bg-slate-800'
              }`}
            >
              <CreditCard className="w-4 h-4" /> Fare Pricing Management
            </button>
            <button
              type="button"
              onClick={() => setActiveTab('trips')}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl font-medium text-sm transition ${
                activeTab === 'trips' ? 'bg-amber-500 text-slate-950 shadow-lg shadow-amber-500/10' : 'text-slate-400 hover:bg-slate-800'
              }`}
            >
              <Car className="w-4 h-4" /> Live System Trips
            </button>
            <button
              type="button"
              onClick={() => setActiveTab('audit')}
              className={`w-full flex items-center gap-3 px-3 py-2.5 rounded-xl font-medium text-sm transition ${
                activeTab === 'audit' ? 'bg-amber-500 text-slate-950 shadow-lg shadow-amber-500/10' : 'text-slate-400 hover:bg-slate-800'
              }`}
            >
              <Users className="w-4 h-4" /> Admin Activity Audit Logs
            </button>
          </nav>
        </div>

        <button
          type="button"
          onClick={onLogout}
          className="w-full flex items-center gap-3 px-3 py-2.5 rounded-xl text-sm font-medium text-red-400 hover:bg-red-950/20 transition border border-transparent hover:border-red-900/30"
        >
          <LogOut className="w-4 h-4" /> Terminate Session
        </button>
      </div>

      {/* Main Workspace Panel Container */}
      <div className="flex-1 overflow-y-auto p-8">
        {(activeTab === 'general' || activeTab === 'fares') && loading ? (
          <div className="text-amber-500 font-semibold text-sm">Syncing parameters data table...</div>
        ) : (
          <>
            {activeTab === 'general' && (
              <form onSubmit={handleSaveSettings} className="max-w-xl space-y-6">
                <header className="mb-8">
                  <h1 className="text-2xl font-bold tracking-tight">System Control Variables</h1>
                  <p className="text-slate-400 text-sm mt-1">Configure operational behaviors across the fleet</p>
                </header>
                <div className="bg-slate-900 p-6 rounded-2xl border border-slate-800 space-y-4">
                  <div>
                    <label className="block text-xs font-semibold uppercase text-slate-400 tracking-wider mb-2">
                      Driver Proximity Search Radius (KM)
                    </label>
                    <input
                      type="number"
                      value={settings['ops.driver_search_radius_km'] || '5'}
                      onChange={(e) => handleSettingChange('ops.driver_search_radius_km', e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-sm focus:border-amber-500 focus:outline-none transition"
                    />
                  </div>
                  <div>
                    <label className="block text-xs font-semibold uppercase text-slate-400 tracking-wider mb-2">
                      Surge Pricing Mode Toggle
                    </label>
                    <select
                      value={settings['surge.enabled'] || 'false'}
                      onChange={(e) => handleSettingChange('surge.enabled', e.target.value)}
                      className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-sm focus:border-amber-500 focus:outline-none transition"
                    >
                      <option value="false">Disabled (Standard Rates)</option>
                      <option value="true">Active (Apply Surge Multipliers)</option>
                    </select>
                  </div>
                </div>
                <button type="submit" className="bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold px-6 py-3 rounded-xl transition text-sm">
                  Commit Parameters Change
                </button>
              </form>
            )}

            {activeTab === 'fares' && (
              <form onSubmit={handleSaveSettings} className="max-w-xl space-y-6">
                <header className="mb-8">
                  <h1 className="text-2xl font-bold tracking-tight">Fare Pricing Management</h1>
                  <p className="text-slate-400 text-sm mt-1">Adjust active ride metrics formulas</p>
                </header>
                <div className="bg-slate-900 p-6 rounded-2xl border border-slate-800 space-y-4">
                  <div className="grid grid-cols-2 gap-4">
                    <div>
                      <label className="block text-xs font-semibold uppercase text-slate-400 tracking-wider mb-2">
                        Base Fare Flagfall (PHP)
                      </label>
                      <input
                        type="number"
                        step="0.01"
                        value={settings['fare.base_amount'] || '40.00'}
                        onChange={(e) => handleSettingChange('fare.base_amount', e.target.value)}
                        className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-sm focus:border-amber-500 focus:outline-none transition"
                      />
                    </div>
                    <div>
                      <label className="block text-xs font-semibold uppercase text-slate-400 tracking-wider mb-2">
                        Minimum Fare Cutoff (PHP)
                      </label>
                      <input
                        type="number"
                        step="0.01"
                        value={settings['fare.minimum'] || '80.00'}
                        onChange={(e) => handleSettingChange('fare.minimum', e.target.value)}
                        className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-sm focus:border-amber-500 focus:outline-none transition"
                      />
                    </div>
                  </div>
                </div>
                <button type="submit" className="bg-amber-500 hover:bg-amber-400 text-slate-950 font-bold px-6 py-3 rounded-xl transition text-sm">
                  Commit Parameters Change
                </button>
              </form>
            )}

            {activeTab === 'trips' && <LiveTrips />}
            {activeTab === 'audit' && <AuditLogs />}
          </>
        )}
      </div>
    </div>
  );
}
