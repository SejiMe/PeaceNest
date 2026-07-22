using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Common.Auth;

public static class FamilyRolePermissions
{
    public static bool CanViewFamily(FamilyMemberRole role) =>
        role is FamilyMemberRole.Owner
            or FamilyMemberRole.ParentAdmin
            or FamilyMemberRole.AdultMember
            or FamilyMemberRole.ChildMember
            or FamilyMemberRole.Viewer;

    public static bool CanCreateFamilyPlans(FamilyMemberRole role) =>
        role is FamilyMemberRole.Owner
            or FamilyMemberRole.ParentAdmin
            or FamilyMemberRole.AdultMember;

    public static bool CanUpdateFamilyPlans(FamilyMemberRole role) =>
        role is FamilyMemberRole.Owner
            or FamilyMemberRole.ParentAdmin
            or FamilyMemberRole.AdultMember;

    public static bool CanUpdateMilestoneSteps(FamilyMemberRole role) =>
        role is FamilyMemberRole.Owner
            or FamilyMemberRole.ParentAdmin
            or FamilyMemberRole.AdultMember
            or FamilyMemberRole.ChildMember;

    public static bool CanAddPlanNotes(FamilyMemberRole role) =>
        role is FamilyMemberRole.Owner
            or FamilyMemberRole.ParentAdmin
            or FamilyMemberRole.AdultMember
            or FamilyMemberRole.ChildMember;

    public static bool CanCastPlanVotes(FamilyMemberRole role) =>
        role is FamilyMemberRole.Owner
            or FamilyMemberRole.ParentAdmin
            or FamilyMemberRole.AdultMember
            or FamilyMemberRole.ChildMember;

    public static bool CanModeratePlanNotes(FamilyMemberRole role) =>
        role is FamilyMemberRole.Owner
            or FamilyMemberRole.ParentAdmin;

    public static bool CanInviteFamilyMembers(FamilyMemberRole role) =>
        role is FamilyMemberRole.Owner
            or FamilyMemberRole.ParentAdmin;

    public static bool CanManageFamilyMembers(FamilyMemberRole role) =>
        role is FamilyMemberRole.Owner
            or FamilyMemberRole.ParentAdmin;

    public static bool CanManageFamilySettings(FamilyMemberRole role) =>
        role is FamilyMemberRole.Owner
            or FamilyMemberRole.ParentAdmin;

    public static bool CanReviewFamilyJoinRequests(FamilyMemberRole role) =>
        role is FamilyMemberRole.Owner
            or FamilyMemberRole.ParentAdmin;

    public static bool CanAssignRoleFromJoinRequest(
        FamilyMemberRole reviewerRole,
        FamilyMemberRole assignedRole) =>
        assignedRole switch
        {
            FamilyMemberRole.Owner => false,
            FamilyMemberRole.ParentAdmin => reviewerRole == FamilyMemberRole.Owner,
            FamilyMemberRole.AdultMember or FamilyMemberRole.ChildMember or FamilyMemberRole.Viewer =>
                CanReviewFamilyJoinRequests(reviewerRole),
            _ => false
        };
}
