import { ActivityIndicator, Pressable, StyleSheet, Text } from 'react-native';
import { colors } from '@/src/theme/colors';

type Props = {
  title: string;
  onPress: () => void;
  loading?: boolean;
  disabled?: boolean;
  variant?: 'primary' | 'secondary';
};

export function AppButton({ title, onPress, loading, disabled, variant = 'primary' }: Props) {
  const secondary = variant === 'secondary';
  return (
    <Pressable
      accessibilityRole="button"
      onPress={onPress}
      disabled={disabled || loading}
      style={({ pressed }) => [
        styles.button,
        secondary ? styles.secondary : styles.primary,
        pressed && styles.pressed,
        (disabled || loading) && styles.disabled,
      ]}
    >
      {loading ? <ActivityIndicator color={secondary ? colors.brand : '#fff'} /> : (
        <Text style={[styles.text, secondary && styles.secondaryText]}>{title}</Text>
      )}
    </Pressable>
  );
}

const styles = StyleSheet.create({
  button: { minHeight: 52, borderRadius: 14, alignItems: 'center', justifyContent: 'center', paddingHorizontal: 18 },
  primary: { backgroundColor: colors.brand },
  secondary: { backgroundColor: colors.brandSoft, borderWidth: 1, borderColor: '#CBE4D4' },
  text: { color: '#fff', fontSize: 16, fontWeight: '700' },
  secondaryText: { color: colors.brandDark },
  pressed: { opacity: 0.82 },
  disabled: { opacity: 0.5 },
});
