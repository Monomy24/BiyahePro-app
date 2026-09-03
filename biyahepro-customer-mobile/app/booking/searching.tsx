import { useEffect, useState } from 'react';
import { ActivityIndicator, Pressable, StyleSheet, Text, View } from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { useAuth } from '@/src/context/AuthContext';
import { api } from '@/src/lib/api';
import { colors } from '@/src/theme/colors';

export default function SearchingForDriverScreen() {
  const { session } = useAuth();
  const { tripId } = useLocalSearchParams<{ tripId?: string }>();
  const [status, setStatus] = useState('requested');
  const [checking, setChecking] = useState(false);

  useEffect(() => {
    if (!session) return;
    let active = true;
    const check = async () => {
      setChecking(true);
      try {
        const result = await api.getTripHistory(session.accessToken, 1, 20);
        const trip = result.items.find((item) => item.id === tripId);
        if (active && trip) setStatus(trip.status);
      } finally {
        if (active) setChecking(false);
      }
    };
    check();
    const timer = setInterval(check, 5000);
    return () => { active = false; clearInterval(timer); };
  }, [session, tripId]);

  const assigned = ['accepted', 'en_route', 'arrived', 'in_progress'].includes(status);

  return (
    <View style={styles.page}>
      <View style={styles.circle}><ActivityIndicator size="large" color={colors.brand} /></View>
      <Text style={styles.title}>{assigned ? 'Driver found' : 'Finding your driver'}</Text>
      <Text style={styles.subtitle}>{assigned ? 'Your ride has been accepted. Your driver is on the way.' : 'We sent your booking to available drivers nearby.'}</Text>

      <View style={styles.statusCard}>
        <View style={styles.statusDot} />
        <View style={styles.statusText}><Text style={styles.statusTitle}>{status.replace('_', ' ').toUpperCase()}</Text><Text style={styles.statusCopy}>{checking ? 'Updating ride status…' : assigned ? 'Ride accepted' : 'Waiting for a driver to accept your booking'}</Text></View>
      </View>

      <View style={styles.actions}>
        <Pressable style={styles.secondaryButton} onPress={() => router.replace('/(tabs)/bookings')}><Text style={styles.secondaryText}>View my bookings</Text></Pressable>
        <Pressable style={styles.linkButton} onPress={() => router.replace('/(tabs)')}><Text style={styles.linkText}>Back to home</Text></Pressable>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: colors.background, padding: 24, alignItems: 'center', justifyContent: 'center' },
  circle: { width: 104, height: 104, borderRadius: 52, backgroundColor: colors.brandSoft, alignItems: 'center', justifyContent: 'center', marginBottom: 26 },
  title: { fontSize: 28, fontWeight: '900', color: colors.text, textAlign: 'center' },
  subtitle: { marginTop: 8, color: colors.muted, textAlign: 'center', lineHeight: 21, maxWidth: 330 },
  statusCard: { width: '100%', marginTop: 26, flexDirection: 'row', gap: 12, alignItems: 'center', backgroundColor: colors.surface, borderWidth: 1, borderColor: colors.border, borderRadius: 18, padding: 16 },
  statusDot: { width: 13, height: 13, borderRadius: 7, backgroundColor: colors.brand },
  statusText: { flex: 1 },
  statusTitle: { color: colors.brandDark, fontWeight: '900', fontSize: 11, letterSpacing: 1 },
  statusCopy: { color: colors.muted, marginTop: 4, lineHeight: 18 },
  actions: { width: '100%', marginTop: 22, gap: 10 },
  secondaryButton: { backgroundColor: colors.brand, borderRadius: 14, paddingVertical: 14, alignItems: 'center' },
  secondaryText: { color: '#fff', fontWeight: '900' },
  linkButton: { paddingVertical: 10, alignItems: 'center' },
  linkText: { color: colors.brandDark, fontWeight: '800' },
});
