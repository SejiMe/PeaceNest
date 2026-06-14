import { SafeAreaView } from 'react-native-safe-area-context';
import { ScrollView, View, type ViewProps } from 'react-native';
import { cn } from '@/lib/utils';

type ScreenProps = ViewProps & {
  scroll?: boolean;
};

export function Screen({ children, className, scroll = false, ...props }: ScreenProps) {
  const content = (
    <View className={cn('flex-1 gap-5 px-5 py-6', className)} {...props}>
      {children}
    </View>
  );

  return (
    <SafeAreaView className="flex-1 bg-peacenest-background">
      {scroll ? <ScrollView contentContainerClassName="grow">{content}</ScrollView> : content}
    </SafeAreaView>
  );
}
