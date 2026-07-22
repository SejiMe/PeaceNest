using PeaceNest.Api.Common.Auth;
using PeaceNest.Api.Common.Database.Entities;

namespace PeaceNest.Api.Tests.Unit.Common.Auth;

public sealed class FamilyRolePermissionsTests
{
    [Theory]
    [InlineData(FamilyMemberRole.Owner, FamilyMemberRole.ParentAdmin, true)]
    [InlineData(FamilyMemberRole.ParentAdmin, FamilyMemberRole.ParentAdmin, false)]
    [InlineData(FamilyMemberRole.Owner, FamilyMemberRole.Owner, false)]
    [InlineData(FamilyMemberRole.ParentAdmin, FamilyMemberRole.AdultMember, true)]
    [InlineData(FamilyMemberRole.AdultMember, FamilyMemberRole.Viewer, false)]
    public void CanAssignRoleFromJoinRequest_EnforcesPrivilegeBoundary(
        FamilyMemberRole reviewerRole,
        FamilyMemberRole assignedRole,
        bool expected)
    {
        Assert.Equal(expected, FamilyRolePermissions.CanAssignRoleFromJoinRequest(reviewerRole, assignedRole));
    }

    [Theory]
    [InlineData(FamilyMemberRole.Owner, true)]
    [InlineData(FamilyMemberRole.ParentAdmin, true)]
    [InlineData(FamilyMemberRole.AdultMember, false)]
    [InlineData(FamilyMemberRole.ChildMember, false)]
    [InlineData(FamilyMemberRole.Viewer, false)]
    public void CanManageFamilySettings_OnlyOwnerAndParentAdminCanChangePreferences(FamilyMemberRole role, bool expected)
    {
        Assert.Equal(expected, FamilyRolePermissions.CanManageFamilySettings(role));
    }

    [Theory]
    [InlineData(FamilyMemberRole.Owner, true)]
    [InlineData(FamilyMemberRole.ParentAdmin, true)]
    [InlineData(FamilyMemberRole.AdultMember, true)]
    [InlineData(FamilyMemberRole.ChildMember, true)]
    [InlineData(FamilyMemberRole.Viewer, true)]
    public void CanViewFamily_AllDefinedRolesCanViewTheirFamily(FamilyMemberRole role, bool expected)
    {
        Assert.Equal(expected, FamilyRolePermissions.CanViewFamily(role));
    }

    [Theory]
    [InlineData(FamilyMemberRole.Owner, true)]
    [InlineData(FamilyMemberRole.ParentAdmin, true)]
    [InlineData(FamilyMemberRole.AdultMember, false)]
    [InlineData(FamilyMemberRole.ChildMember, false)]
    [InlineData(FamilyMemberRole.Viewer, false)]
    public void CanInviteFamilyMembers_OnlyOwnerAndParentAdminCanInvite(FamilyMemberRole role, bool expected)
    {
        Assert.Equal(expected, FamilyRolePermissions.CanInviteFamilyMembers(role));
    }

    [Theory]
    [InlineData(FamilyMemberRole.Owner, true)]
    [InlineData(FamilyMemberRole.ParentAdmin, true)]
    [InlineData(FamilyMemberRole.AdultMember, true)]
    [InlineData(FamilyMemberRole.ChildMember, false)]
    [InlineData(FamilyMemberRole.Viewer, false)]
    public void CanCreateFamilyPlans_ExcludesChildAndViewerRoles(FamilyMemberRole role, bool expected)
    {
        Assert.Equal(expected, FamilyRolePermissions.CanCreateFamilyPlans(role));
    }

    [Theory]
    [InlineData(FamilyMemberRole.Owner, true)]
    [InlineData(FamilyMemberRole.ParentAdmin, true)]
    [InlineData(FamilyMemberRole.AdultMember, true)]
    [InlineData(FamilyMemberRole.ChildMember, false)]
    [InlineData(FamilyMemberRole.Viewer, false)]
    public void CanUpdateFamilyPlans_ExcludesChildAndViewerRoles(FamilyMemberRole role, bool expected)
    {
        Assert.Equal(expected, FamilyRolePermissions.CanUpdateFamilyPlans(role));
    }

    [Theory]
    [InlineData(FamilyMemberRole.Owner, true)]
    [InlineData(FamilyMemberRole.ParentAdmin, true)]
    [InlineData(FamilyMemberRole.AdultMember, true)]
    [InlineData(FamilyMemberRole.ChildMember, true)]
    [InlineData(FamilyMemberRole.Viewer, false)]
    public void CanUpdateMilestoneSteps_ExcludesViewerRole(FamilyMemberRole role, bool expected)
    {
        Assert.Equal(expected, FamilyRolePermissions.CanUpdateMilestoneSteps(role));
    }

    [Theory]
    [InlineData(FamilyMemberRole.Owner, true)]
    [InlineData(FamilyMemberRole.ParentAdmin, true)]
    [InlineData(FamilyMemberRole.AdultMember, true)]
    [InlineData(FamilyMemberRole.ChildMember, true)]
    [InlineData(FamilyMemberRole.Viewer, false)]
    public void CanAddPlanNotes_ExcludesViewerRole(FamilyMemberRole role, bool expected)
    {
        Assert.Equal(expected, FamilyRolePermissions.CanAddPlanNotes(role));
    }

    [Theory]
    [InlineData(FamilyMemberRole.Owner, true)]
    [InlineData(FamilyMemberRole.ParentAdmin, true)]
    [InlineData(FamilyMemberRole.AdultMember, true)]
    [InlineData(FamilyMemberRole.ChildMember, true)]
    [InlineData(FamilyMemberRole.Viewer, false)]
    public void CanCastPlanVotes_ExcludesViewerRole(FamilyMemberRole role, bool expected)
    {
        Assert.Equal(expected, FamilyRolePermissions.CanCastPlanVotes(role));
    }

    [Theory]
    [InlineData(FamilyMemberRole.Owner, true)]
    [InlineData(FamilyMemberRole.ParentAdmin, true)]
    [InlineData(FamilyMemberRole.AdultMember, false)]
    [InlineData(FamilyMemberRole.ChildMember, false)]
    [InlineData(FamilyMemberRole.Viewer, false)]
    public void CanModeratePlanNotes_OnlyOwnerAndParentAdminCanModerate(FamilyMemberRole role, bool expected)
    {
        Assert.Equal(expected, FamilyRolePermissions.CanModeratePlanNotes(role));
    }
}
