// ridehailing-client/src/App.jsx
import React from 'react';
import AdminPage from './pages/admin/AdminPage';

// Entry point. Currently routes straight to the admin panel since that's
// the only client surface built so far. Once customer/driver UIs exist
// (pages/customer/CustomerPage.jsx, pages/driver/DriverPage.jsx), this is
// the place to add real routing (e.g. react-router) between them.
export default function App() {
  return <AdminPage />;
}