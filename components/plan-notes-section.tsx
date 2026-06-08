import { useState } from 'react';
import { Alert, View } from 'react-native';
import { Button } from '@/components/ui/button';
import { Card } from '@/components/ui/card';
import { Input } from '@/components/ui/input';
import { LoadingState } from '@/components/ui/state';
import { Text } from '@/components/ui/text';
import { useAddPlanNote, useDeletePlanNote, usePlanNotes } from '@/hooks/use-plan-notes';
import { FamilyMemberRole, type FamilyMemberRole as FamilyMemberRoleValue } from '@/lib/api/contracts';

type PlanNotesSectionProps = {
  familyId: string;
  planId: string;
  currentUserId: string;
  currentUserRole: FamilyMemberRoleValue;
};

export function PlanNotesSection({
  currentUserId,
  currentUserRole,
  familyId,
  planId,
}: PlanNotesSectionProps) {
  const notes = usePlanNotes(familyId, planId);
  const addNote = useAddPlanNote(familyId, planId);
  const deleteNote = useDeletePlanNote(familyId, planId);
  const [body, setBody] = useState('');
  const canModerate = currentUserRole === FamilyMemberRole.Owner || currentUserRole === FamilyMemberRole.ParentAdmin;

  async function handleAddNote() {
    try {
      await addNote.mutateAsync({ body });
      setBody('');
    } catch (error) {
      Alert.alert('Note could not be added', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  async function handleDeleteNote(noteId: string) {
    try {
      await deleteNote.mutateAsync(noteId);
    } catch (error) {
      Alert.alert('Note could not be removed', error instanceof Error ? error.message : 'Please try again.');
    }
  }

  return (
    <Card className="gap-4">
      <View className="gap-1">
        <Text variant="section">Family notes</Text>
        <Text variant="caption">Simple plan-level notes for this MVP.</Text>
      </View>

      <View className="gap-2">
        <Input
          className="min-h-24 py-3"
          maxLength={4000}
          multiline
          onChangeText={setBody}
          placeholder="Add a calm note for the family."
          textAlignVertical="top"
          value={body}
        />
        <Button
          disabled={!body.trim() || addNote.isPending}
          label={addNote.isPending ? 'Adding note' : 'Add note'}
          onPress={handleAddNote}
        />
      </View>

      {notes.isLoading ? <LoadingState title="Gathering notes" /> : null}

      {notes.isError ? (
        <View className="gap-3 rounded-lg border border-peacenest-danger bg-peacenest-surface p-3">
          <Text className="font-semibold">Notes could not open</Text>
          <Text variant="caption">{notes.error.message}</Text>
          <Button label="Try again" onPress={() => notes.refetch()} variant="secondary" />
        </View>
      ) : null}

      {!notes.isLoading && (notes.data?.notes.length ?? 0) === 0 ? (
        <View className="gap-1 rounded-lg bg-peacenest-blush p-3">
          <Text className="font-semibold">No notes yet</Text>
          <Text variant="caption">Start with one thought, question, or family reminder.</Text>
        </View>
      ) : null}

      <View className="gap-3">
        {notes.data?.notes.map((note) => {
          const canDelete = canModerate || note.authorUserId === currentUserId;

          return (
            <View key={note.id} className="gap-2 rounded-lg bg-peacenest-blush p-3">
              <View className="flex-row items-start justify-between gap-3">
                <View className="flex-1 gap-1">
                  <Text className="font-semibold">{note.authorDisplayName}</Text>
                  <Text variant="caption">{formatDateTime(note.createdAt)}</Text>
                </View>
                {canDelete ? (
                  <Button
                    className="min-h-9 px-3"
                    disabled={deleteNote.isPending}
                    label="Remove"
                    onPress={() => handleDeleteNote(note.id)}
                    variant="ghost"
                  />
                ) : null}
              </View>
              <Text>{note.body}</Text>
            </View>
          );
        })}
      </View>
    </Card>
  );
}

function formatDateTime(value: string) {
  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat(undefined, {
    month: 'short',
    day: 'numeric',
    hour: 'numeric',
    minute: '2-digit',
  }).format(date);
}
