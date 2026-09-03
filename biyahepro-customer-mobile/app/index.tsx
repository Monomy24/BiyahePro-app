import { ActivityIndicator, StyleSheet, View } from 'react-native';
import { Redirect } from 'expo-router';
import { useAuth } from '@/src/context/AuthContext';
import { colors } from '@/src/theme/colors';

export default function Index() {
  const { session, isLoading } = useAuth();
  if (isLoading) return <View style={styles.center}><ActivityIndicator size="large" color={colors.brand} /></View>;
  return <Redirect href={session ? '/(tabs)' : '/(auth)/login'} />;
}

const styles = StyleSheet.create({ center: { flex: 1, alignItems: 'center', justifyContent: 'center', backgroundColor: colors.background } });
