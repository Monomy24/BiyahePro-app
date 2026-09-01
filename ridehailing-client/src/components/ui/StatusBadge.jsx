import React from 'react';

// Shared trip-status pill. Lives in ui/ (not components/admin/) because
// customer and driver trip views will need the exact same status styling
// once they exist — this isn't admin-specific.
const STATUS_STYLES = {
  requested: 'bg-blue-500/10 text-blue-400 border-blue-500/20',
  accepted: 'bg-purple-500/10 text-purple-400 border-purple-500/20',
  en_route: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
  arrived: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
  in_progress: 'bg-amber-500/10 text-amber-400 border-amber-500/20',
  completed: 'bg-green-500/10 text-green-400 border-green-500/20',
  cancelled: 'bg-red-500/10 text-red-400 border-red-500/20',
};

export default function StatusBadge({ status }) {
  const style = STATUS_STYLES[status] || 'bg-slate-500/10 text-slate-400 border-slate-500/20';
  return (
    <span className={`px-2.5 py-1 rounded-full text-xs font-semibold border ${style}`}>
      {status.replace('_', ' ')}
    </span>
  );
}