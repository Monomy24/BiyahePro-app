import React, { useState, useEffect } from 'react';
import LoginForm from '../../components/admin/LoginForm';
import AdminDashboard from '../../components/admin/AdminDashboard';
import { getStoredTokens, getStoredUser, clearSession } from '../../lib/api';

export default function AdminPage() {
  // On load, trust a previously stored session (from a prior login) until
  // an API call tells us otherwise via a 401/403 — see handleAuthError.
  const [session, setSession] = useState(() => {
    const tokens = getStoredTokens();
    const user = getStoredUser();
    return tokens?.accessToken && user ? user : null;
  });

  useEffect(() => {
    if (session) window.location.hash = '#admin';
  }, [session]);

  const handleLoginSuccess = (result) => {
    setSession({ role: result.role, userId: result.userId, fullName: result.fullName });
  };

  const handleLogout = () => {
    clearSession();
    setSession(null);
    window.location.hash = '';
  };

  // Passed down to admin components so an expired/invalid token (401/403
  // from apiFetch) bounces the user back to the login form instead of
  // silently failing or showing empty data.
  const handleAuthError = () => {
    clearSession();
    setSession(null);
  };

  return (
    <>
      {session ? (
        <AdminDashboard onLogout={handleLogout} onAuthError={handleAuthError} adminName={session.fullName} />
      ) : (
        <LoginForm onLoginSuccess={handleLoginSuccess} />
      )}
    </>
  );
}