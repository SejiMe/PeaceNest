export type RecoveryCodeHandoff = {
  familyId: string;
  recoveryCode: string;
  recoveryExpiresAt: string;
};

let pendingRecoveryCode: RecoveryCodeHandoff | null = null;

export function setRecoveryCodeHandoff(value: RecoveryCodeHandoff) {
  pendingRecoveryCode = value;
}

export function takeRecoveryCodeHandoff() {
  const value = pendingRecoveryCode;
  pendingRecoveryCode = null;
  return value;
}
