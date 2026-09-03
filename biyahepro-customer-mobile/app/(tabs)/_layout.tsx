import { Redirect, Tabs } from 'expo-router';
import { Text } from 'react-native';
import { useAuth } from '@/src/context/AuthContext';
import { colors } from '@/src/theme/colors';

const icon = (symbol: string, color: string) => <Text style={{ color, fontSize: 18 }}>{symbol}</Text>;

export default function TabsLayout() {
  const { session, isLoading } = useAuth();
  if (!isLoading && !session) return <Redirect href="/(auth)/login" />;
  return (
    <Tabs screenOptions={{ tabBarActiveTintColor: colors.brand, tabBarInactiveTintColor: colors.muted, headerShadowVisible: false, tabBarStyle: { height: 64, paddingBottom: 8 } }}>
      <Tabs.Screen name="index" options={{ title: 'Home', tabBarIcon: ({ color }) => icon('⌂', color) }} />
      <Tabs.Screen name="bookings" options={{ title: 'Bookings', tabBarIcon: ({ color }) => icon('▤', color) }} />
      <Tabs.Screen name="account" options={{ title: 'Account', tabBarIcon: ({ color }) => icon('●', color) }} />
    </Tabs>
  );
}
