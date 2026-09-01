// ridehailing-client/src/components/admin/LoginForm.jsx
import React, { useState, useRef, useEffect } from 'react';
import { Shield, LogIn } from 'lucide-react';
import { login, saveSession } from '../../lib/api';

export default function LoginForm({ onLoginSuccess }) {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [error, setError] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const emailRef = useRef(null);

  // Auto-focus email on mount so an admin can start typing immediately —
  // no click needed to begin the login flow at all.
  useEffect(() => {
    emailRef.current?.focus();
  }, []);

  const handleSubmit = async (e) => {
    e.preventDefault();
    setError('');
    setSubmitting(true);

    try {
      const result = await login(email, password);

      if (result.role !== 'admin') {
        setError('This account does not have admin access.');
        return;
      }

      saveSession(result);
      onLoginSuccess(result);
    } catch (err) {
      setError(err.message || 'Login failed. Please try again.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <div className="fixed inset-0 bg-slate-950 flex flex-col items-center justify-center text-white font-sans z-50">
      <form
        onSubmit={handleSubmit}
        className="bg-slate-900 p-8 rounded-2xl shadow-2xl border border-slate-800 w-full max-w-sm"
      >
        <div className="bg-amber-500/10 p-4 rounded-full w-16 h-16 flex items-center justify-center mx-auto mb-4 border border-amber-500/20">
          <Shield className="w-8 h-8 text-amber-500" />
        </div>
        <h2 className="text-xl font-bold mb-1 text-center">BiyahePro Admin</h2>
        <p className="text-slate-400 text-sm mb-6 text-center">Sign in with your admin account</p>

        <div className="space-y-4">
          <div>
            <label className="block text-xs font-semibold uppercase text-slate-400 tracking-wider mb-2">
              Email
            </label>
            <input
              ref={emailRef}
              type="email"
              autoComplete="username"
              required
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-sm focus:border-amber-500 focus:outline-none transition"
              placeholder="admin@biyahepro.local"
            />
          </div>
          <div>
            <label className="block text-xs font-semibold uppercase text-slate-400 tracking-wider mb-2">
              Password
            </label>
            <input
              type="password"
              autoComplete="current-password"
              required
              value={password}
              onChange={(e) => setPassword(e.target.value)}
              className="w-full bg-slate-950 border border-slate-800 rounded-xl px-4 py-3 text-sm focus:border-amber-500 focus:outline-none transition"
              placeholder="••••••••"
            />
            {/* Pressing Enter here submits the form natively (type="submit"
                button below + a single <form onSubmit>) — no extra keydown
                handler needed, and none added, so we don't fight the
                browser's built-in behavior or double-submit. */}
          </div>
        </div>

        {error && (
          <p className="text-xs font-semibold mt-4 text-red-400" role="alert">{error}</p>
        )}

        <button
          type="submit"
          disabled={submitting}
          className="w-full mt-6 flex items-center justify-center gap-2 bg-amber-500 hover:bg-amber-400 disabled:opacity-50 text-slate-950 font-bold px-6 py-3 rounded-xl transition text-sm"
        >
          <LogIn className="w-4 h-4" />
          {submitting ? 'Signing in…' : 'Sign In'}
        </button>
        <p className="text-center text-xs text-slate-500 mt-3">Press Enter to sign in</p>
      </form>
    </div>
  );
}