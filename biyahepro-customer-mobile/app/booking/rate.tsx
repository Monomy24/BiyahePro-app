import { useState } from 'react';
import { Pressable, StyleSheet, Text, TextInput, View } from 'react-native';
import { router, useLocalSearchParams } from 'expo-router';
import { useAuth } from '@/src/context/AuthContext';
import { api } from '@/src/lib/api';
import { AppButton } from '@/src/components/AppButton';
import { colors } from '@/src/theme/colors';

const STARS = [1, 2, 3, 4, 5];

export default function RateTripScreen() {
  const { session } = useAuth();
  const { tripId } = useLocalSearchParams<{ tripId?: string }>();
  const [score, setScore] = useState(0);
  const [comment, setComment] = useState('');
  const [submitting, setSubmitting] = useState(false);
  const [error, setError] = useState('');

  const finish = () => router.replace('/(tabs)/bookings');

  const submit = async () => {
    if (!session || !tripId) return;
    if (score < 1) { setError('Tap a star to rate your ride.'); return; }
    setSubmitting(true);
    setError('');
    try {
      await api.rateTrip(tripId, { score, comment: comment.trim() || undefined }, session.accessToken);
      finish();
    } catch (e) {
      setError(e instanceof Error ? e.message : 'Unable to submit rating.');
    } finally {
      setSubmitting(false);
    }
  };

  return (
    <View style={styles.page}>
      <Text style={styles.title}>Trip completed</Text>
      <Text style={styles.subtitle}>How was your ride? Your feedback helps keep BiyahePro drivers accountable.</Text>

      <View style={styles.stars}>
        {STARS.map((value) => (
          <Pressable key={value} onPress={() => setScore(value)} hitSlop={8}>
            <Text style={[styles.star, value <= score && styles.starFilled]}>★</Text>
          </Pressable>
        ))}
      </View>

      <TextInput
        style={styles.input}
        placeholder="Leave a comment (optional)"
        placeholderTextColor={colors.muted}
        value={comment}
        onChangeText={setComment}
        multiline
        numberOfLines={4}
      />

      {!!error && <Text style={styles.error}>{error}</Text>}

      <View style={styles.actions}>
        <AppButton title="Submit rating" onPress={submit} loading={submitting} disabled={submitting} />
        <Pressable style={styles.skipButton} onPress={finish}><Text style={styles.skipText}>Skip for now</Text></Pressable>
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  page: { flex: 1, backgroundColor: colors.background, padding: 24, justifyContent: 'center' },
  title: { fontSize: 28, fontWeight: '900', color: colors.text, textAlign: 'center' },
  subtitle: { marginTop: 8, color: colors.muted, textAlign: 'center', lineHeight: 21 },
  stars: { flexDirection: 'row', justifyContent: 'center', gap: 10, marginTop: 30 },
  star: { fontSize: 44, color: colors.border },
  starFilled: { color: colors.brand },
  input: { marginTop: 26, backgroundColor: colors.surface, borderWidth: 1, borderColor: colors.border, borderRadius: 14, padding: 14, minHeight: 96, textAlignVertical: 'top', color: colors.text },
  error: { color: colors.danger, marginTop: 14, textAlign: 'center' },
  actions: { marginTop: 26, gap: 10 },
  skipButton: { paddingVertical: 10, alignItems: 'center' },
  skipText: { color: colors.brandDark, fontWeight: '800' },
});