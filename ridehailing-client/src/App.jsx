import React, { useState, useEffect } from 'react';
import PinOverlay from './components/PinOverlay';
import AdminDashboard from './components/AdminDashboard';

export default function App() {
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [pin, setPin] = useState('');
  const [pinError, setPinError] = useState('');
  const [attempts, setAttempts] = useState(0);

  const MAX_ATTEMPTS = 5;
  const CORRECT_PIN = '1234';

  // --- Core Processing Logic ---
  const processPinInput = (digit, currentPin, currentAttempts) => {
    if (currentAttempts >= MAX_ATTEMPTS) return;
    
    if (currentPin.length < 4) {
      const updatedPin = currentPin + digit;
      setPin(updatedPin);
      setPinError('');

      // Auto-submit on 4th digit entry
      if (updatedPin.length === 4) {
        if (updatedPin === CORRECT_PIN) {
          setIsAuthenticated(true);
          setPinError('');
          window.location.hash = '#admin';
        } else {
          const newAttempts = currentAttempts + 1;
          setAttempts(newAttempts);
          setPin('');
          setPinError(newAttempts >= MAX_ATTEMPTS ? 'Locked.' : `Incorrect PIN. ${MAX_ATTEMPTS - newAttempts} left.`);
        }
      }
    }
  };

  const handleKeyPress = (num) => {
    processPinInput(num, pin, attempts);
  };

  const handleClear = () => {
    if (attempts >= MAX_ATTEMPTS) return;
    setPin('');
  };

  const handleLogout = () => {
    setIsAuthenticated(false);
    setAttempts(0);
    setPin('');
    window.location.hash = '';
  };

  // ── ⌨️ PHYSICAL KEYBOARD LISTENER MATRIX ──
  useEffect(() => {
    // Only listen for keys if the user isn't logged in yet
    if (isAuthenticated) return;

    const handleKeyDown = (event) => {
      // 1. Capture number key entries (0-9)
      if (/^[0-9]$/.test(event.key)) {
        event.preventDefault();
        processPinInput(event.key, pin, attempts);
      }
      
      // 2. Capture Backspace key to wipe the entry buffer clean
      if (event.key === 'Backspace') {
        event.preventDefault();
        handleClear();
      }
    };

    // Attach listener to window context
    window.addEventListener('keydown', handleKeyDown);

    // Clean up listener memory when the component destroys or user authenticates
    return () => {
      window.removeEventListener('keydown', handleKeyDown);
    };
  }, [pin, attempts, isAuthenticated]); // Re-bind whenever states mutate

  return (
    <>
      {isAuthenticated ? (
        <AdminDashboard onLogout={handleLogout} />
      ) : (
        <PinOverlay
          pin={pin}
          pinError={pinError}
          attempts={attempts}
          maxAttempts={MAX_ATTEMPTS}
          onKeyPress={handleKeyPress}
          onClear={handleClear}
        />
      )}
    </>
  );
}
