import React from 'react';
import { Shield, KeyRound } from 'lucide-react';

export default function PinOverlay({ pin, pinError, attempts, maxAttempts, onKeyPress, onClear }) {
  // Pad the tracking display so it maintains constant sizing blocks
  const displayValue = pin.padEnd(4, '•');

  return (
    <div className="fixed inset-0 bg-slate-950 flex flex-col items-center justify-center text-white font-sans z-50">
      <div className="bg-slate-900 p-8 rounded-2xl shadow-2xl border border-slate-800 w-full max-w-sm text-center">
        <div className="bg-amber-500/10 p-4 rounded-full w-16 h-16 flex items-center justify-center mx-auto mb-4 border border-amber-500/20">
          <Shield className="w-8 h-8 text-amber-500" />
        </div>
        <h2 className="text-xl font-bold mb-1">BiyahePro Terminal</h2>
        <p className="text-slate-400 text-sm mb-6">Enter secure pin to access system parameters</p>

        {/* 📟 NEW: TEXT DIGITAL DISPLAY WINDOW */}
        <div className="bg-slate-950 tracking-[1em] pl-[1em] text-2xl font-mono py-4 rounded-xl border border-slate-800 text-amber-500 font-bold mb-6 select-none shadow-inner">
          {pin.length > 0 ? '*'.repeat(pin.length).padEnd(4, '-') : '----'}
        </div>

        {/* Error Feedback */}
        {pinError && (
          <p className={`text-xs font-semibold mb-6 ${attempts >= maxAttempts ? 'text-red-500' : 'text-amber-400'}`}>
            {pinError}
          </p>
        )}

        {/* Keypad Grid Matrix */}
        <div className="grid grid-cols-3 gap-3 max-w-[260px] mx-auto">
          {['1', '2', '3', '4', '5', '6', '7', '8', '9'].map((num) => (
            <button
              key={num}
              disabled={attempts >= maxAttempts}
              onClick={() => onKeyPress(num)}
              className="bg-slate-800 hover:bg-slate-700 active:scale-95 disabled:opacity-40 text-lg font-semibold py-4 rounded-xl transition border border-slate-700/50"
            >
              {num}
            </button>
          ))}
          <button
            disabled={attempts >= maxAttempts}
            onClick={onClear}
            className="bg-slate-800/50 hover:bg-red-950/30 text-red-400 font-medium py-4 rounded-xl transition text-sm border border-red-900/20"
          >
            Clear
          </button>
          <button
            disabled={attempts >= maxAttempts}
            onClick={() => onKeyPress('0')}
            className="bg-slate-800 hover:bg-slate-700 active:scale-95 disabled:opacity-40 text-lg font-semibold py-4 rounded-xl transition border border-slate-700/50"
          >
            0
          </button>
          <div className="flex items-center justify-center opacity-20 text-slate-400">
            <KeyRound className="w-5 h-5" />
          </div>
        </div>
      </div>
    </div>
  );
}
