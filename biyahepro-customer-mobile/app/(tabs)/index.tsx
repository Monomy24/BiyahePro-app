import { router } from 'expo-router';
import { ScrollView, StyleSheet, Text, View } from 'react-native';
import { AppButton } from '@/src/components/AppButton';
import { useAuth } from '@/src/context/AuthContext';
import { colors } from '@/src/theme/colors';

export default function HomeScreen() {
  const { session } = useAuth();
  return (
    <ScrollView style={styles.page} contentContainerStyle={styles.content}>
      <Text style={styles.eyebrow}>GOOD DAY</Text>
      <Text style={styles.title}>Where to, {session?.fullName?.split(' ')[0] || 'rider'}?</Text>
      <Text style={styles.subtitle}>Request a motorcycle or motorcab ride through BiyahePro.</Text>

      <View style={styles.heroCard}>
        <Text style={styles.heroTitle}>Ready for your next ride?</Text>
        <Text style={styles.heroCopy}>Enter your pickup and destination, see the estimated fare, then confirm your booking.</Text>
        <AppButton title="Book a ride" onPress={() => router.push('/booking/new')} />
      </View>

      <Text style={styles.sectionTitle}>Booking flow</Text>
      <View style={styles.step}><Text style={styles.number}>1</Text><View><Text style={styles.stepTitle}>Choose locations</Text><Text style={styles.stepText}>Pickup and destination details.</Text></View></View>
      <View style={styles.step}><Text style={styles.number}>2</Text><View><Text style={styles.stepTitle}>Review fare</Text><Text style={styles.stepText}>Get the server-calculated estimate.</Text></View></View>
      <View style={styles.step}><Text style={styles.number}>3</Text><View><Text style={styles.stepTitle}>Confirm booking</Text><Text style={styles.stepText}>Create the trip and wait for a driver.</Text></View></View>
    </ScrollView>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: colors.background }, content: { padding: 20, gap: 10 },
  eyebrow: { marginTop: 8, fontSize: 12, fontWeight: '800', letterSpacing: 1.5, color: colors.brand },
  title: { fontSize: 30, fontWeight: '900', color: colors.text }, subtitle: { fontSize: 15, color: colors.muted, marginBottom: 12 },
  heroCard: { backgroundColor: colors.surface, borderRadius: 22, padding: 20, gap: 12, borderWidth: 1, borderColor: colors.border },
  heroTitle: { fontSize: 21, fontWeight: '800', color: colors.text }, heroCopy: { color: colors.muted, lineHeight: 21 },
  sectionTitle: { marginTop: 14, fontSize: 18, fontWeight: '800', color: colors.text },
  step: { flexDirection: 'row', gap: 12, alignItems: 'center', backgroundColor: colors.surface, borderRadius: 16, padding: 14 },
  number: { width: 34, height: 34, lineHeight: 34, borderRadius: 17, textAlign: 'center', backgroundColor: colors.brandSoft, color: colors.brandDark, fontWeight: '900' },
  stepTitle: { fontWeight: '800', color: colors.text }, stepText: { color: colors.muted, marginTop: 2 },
});
