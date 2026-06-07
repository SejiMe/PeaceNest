export type FamilyMemberRole = 'Owner' | 'ParentAdmin' | 'AdultMember' | 'ChildMember' | 'Viewer';
export type FamilyMemberStatus = 'Active' | 'Removed';

export type CurrentUserResponse = {
  id: string;
  supabaseUserId: string;
  email: string;
  displayName: string;
  avatarUrl?: string | null;
  timezone?: string | null;
  lastSeenAt?: string | null;
};

export type FamilyMembershipResponse = {
  familyId: string;
  familyName: string;
  role: FamilyMemberRole;
  status: FamilyMemberStatus;
};

export type GetMeResponse = {
  user: CurrentUserResponse;
  familyMemberships: FamilyMembershipResponse[];
};
