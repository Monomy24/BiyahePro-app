import { useState } from 'react';
import { Pressable, StyleSheet, Text, TextInput, View, type TextInputProps } from 'react-native';
import { colors } from '@/src/theme/colors';

type Props = TextInputProps & { label: string };

export function AppInput({ label, secureTextEntry, ...props }: Props) {
  // Only password-style fields get the toggle — everything else behaves
  // exactly as before. Local state starts hidden (matches the original
  // secureTextEntry default) and the user can reveal it per-field.
  const [visible, setVisible] = useState(false);
  const isPasswordField = !!secureTextEntry;

  return (
    <View style={styles.group}>
      <Text style={styles.label}>{label}</Text>
      <View style={styles.row}>
        <TextInput
          placeholderTextColor="#8B9790"
          style={[styles.input, isPasswordField && styles.inputWithToggle]}
          secureTextEntry={isPasswordField && !visible}
          {...props}
        />
        {isPasswordField && (
          <Pressable
            onPress={() => setVisible((v) => !v)}
            hitSlop={10}
            style={styles.toggle}
            accessibilityRole="button"
            accessibilityLabel={visible ? 'Hide password' : 'Show password'}
          >
            <Text style={styles.toggleText}>{visible ? 'Hide' : 'Show'}</Text>
          </Pressable>
        )}
      </View>
    </View>
  );
}

const styles = StyleSheet.create({
  group: { gap: 7 },
  label: { fontSize: 13, fontWeight: '700', color: colors.text },
  row: { position: 'relative', justifyContent: 'center' },
  input: { minHeight: 50, backgroundColor: colors.surface, borderWidth: 1, borderColor: colors.border, borderRadius: 13, paddingHorizontal: 14, color: colors.text, fontSize: 15 },
  inputWithToggle: { paddingRight: 60 },
  toggle: { position: 'absolute', right: 14 },
  toggleText: { color: colors.brandDark, fontWeight: '800', fontSize: 13 },
});