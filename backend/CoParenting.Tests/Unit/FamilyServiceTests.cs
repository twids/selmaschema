using CoParenting.Application.DTOs;
using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;

namespace CoParenting.Tests.Unit;

public sealed class FamilyServiceTests
{
    [Fact]
    public async Task Create_family_uses_generic_empty_defaults()
    {
        await using var db = Database();
        var account = AddAccount(db, "owner@example.test");
        await db.SaveChangesAsync();
        var result = await Service(db).CreateFamilyAsync(account.Id, new CreateFamilyRequest("Familjen Exempel"));
        result.Name.Should().Be("Familjen Exempel");
        result.SideALabel.Should().Be("Hem A");
        result.SideBLabel.Should().Be("Hem B");
        result.TimeZoneId.Should().Be("Europe/Stockholm");
        result.ExchangeDetailLevel.Should().Be(ExchangeDetailLevel.Day);
        result.Calendars.Should().ContainSingle(x => x.Name == "Boendeschema");
        result.ActiveChildren.Should().Be(0);
        result.HasActiveSchedule.Should().BeFalse();
        (await db.FamilyMembers.SingleAsync()).Permission.Should().Be(FamilyPermission.Owner);
    }

    [Fact]
    public async Task Invitation_code_and_link_refer_to_same_single_use_invitation()
    {
        await using var db = Database();
        var owner = AddAccount(db, "owner@example.test");
        var invitee = AddAccount(db, "invitee@example.test");
        await db.SaveChangesAsync();
        var service = Service(db);
        var family = await service.CreateFamilyAsync(owner.Id, new CreateFamilyRequest("Test"));
        var created = await service.CreateInvitationAsync(owner.Id, family.Id,
            new CreateInvitationRequest(FamilyPermission.Editor, ScheduleSide.B, "hint@example.test", 7), "https://selma.test");
        created.Should().NotBeNull();
        created!.Code.Should().MatchRegex("^[A-Z2-9]{4}-[A-Z2-9]{4}$");
        created.Link.Should().StartWith("https://selma.test/join/");
        var token = Uri.UnescapeDataString(created.Link.Split('/').Last());
        (await service.GetJoinPreviewByTokenAsync(invitee.Id, token))!.FamilyId.Should().Be(family.Id);
        (await service.GetJoinPreviewByCodeAsync(invitee.Id, created.Code))!.EmailHint.Should().Be("hint@example.test");
        (await db.FamilyInvitations.SingleAsync()).ConsumedAt.Should().BeNull();
        var redeemed = await service.RedeemCodeAsync(invitee.Id, created.Code);
        redeemed!.Joined.Should().BeTrue();
        (await service.RedeemTokenAsync(AddAccount(db, "third@example.test").Id, token)).Should().BeNull();
        var membership = await db.FamilyMembers.SingleAsync(x => x.AccountId == invitee.Id);
        membership.Permission.Should().Be(FamilyPermission.Editor);
        membership.Side.Should().Be(ScheduleSide.B);
    }

    [Fact]
    public async Task Expired_invitation_and_owner_permission_are_rejected()
    {
        await using var db = Database();
        var owner = AddAccount(db, "owner-expiry@example.test");
        var invitee = AddAccount(db, "invitee-expiry@example.test");
        await db.SaveChangesAsync();
        var service = Service(db);
        var family = await service.CreateFamilyAsync(owner.Id, new CreateFamilyRequest("Test"));
        (await service.CreateInvitationAsync(owner.Id, family.Id,
            new CreateInvitationRequest(FamilyPermission.Owner, null, null, null), "https://selma.test")).Should().BeNull();
        var invitation = (await service.CreateInvitationAsync(owner.Id, family.Id,
            new CreateInvitationRequest(FamilyPermission.Viewer, null, null, null), "https://selma.test"))!;
        (await db.FamilyInvitations.SingleAsync(x => x.Id == invitation.Invitation.Id)).ExpiresAt = DateTime.UtcNow.AddMinutes(-1);
        await db.SaveChangesAsync();
        (await service.GetJoinPreviewByCodeAsync(invitee.Id, invitation.Code)).Should().BeNull();
        (await service.RedeemCodeAsync(invitee.Id, invitation.Code)).Should().BeNull();
    }

    [Fact]
    public async Task Ownership_is_transferred_atomically_and_cannot_be_granted_as_member_edit()
    {
        await using var db = Database();
        var owner = AddAccount(db, "owner-transfer@example.test");
        var editor = AddAccount(db, "editor-transfer@example.test");
        await db.SaveChangesAsync();
        var service = Service(db);
        var family = await service.CreateFamilyAsync(owner.Id, new CreateFamilyRequest("Test"));
        var editorMember = new FamilyMember { FamilyId = family.Id, AccountId = editor.Id, Permission = FamilyPermission.Editor, JoinedAt = DateTime.UtcNow };
        db.FamilyMembers.Add(editorMember);
        await db.SaveChangesAsync();

        (await service.UpdateMemberAsync(owner.Id, family.Id, editorMember.Id,
            new UpdateMemberRequest(FamilyPermission.Owner, ScheduleSide.B, "Fel väg"))).Should().BeNull();
        (await service.TransferOwnershipAsync(owner.Id, family.Id,
            new TransferOwnershipRequest(editorMember.Id, "Ny familjeägare"))).Should().BeTrue();
        (await db.FamilyMembers.CountAsync(x => x.FamilyId == family.Id && x.Permission == FamilyPermission.Owner)).Should().Be(1);
        (await db.FamilyMembers.SingleAsync(x => x.Id == editorMember.Id)).Permission.Should().Be(FamilyPermission.Owner);
    }

    [Fact]
    public async Task Existing_member_cannot_change_permission_with_invitation()
    {
        await using var db = Database();
        var owner = AddAccount(db, "owner@example.test");
        var viewer = AddAccount(db, "viewer@example.test");
        await db.SaveChangesAsync();
        var service = Service(db);
        var family = await service.CreateFamilyAsync(owner.Id, new CreateFamilyRequest("Test"));
        db.FamilyMembers.Add(new FamilyMember
        {
            FamilyId = family.Id, AccountId = viewer.Id, Permission = FamilyPermission.Viewer,
            JoinedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        var invitation = (await service.CreateInvitationAsync(owner.Id, family.Id,
            new CreateInvitationRequest(FamilyPermission.Editor, ScheduleSide.A, null, null), "https://selma.test"))!;
        var result = await service.RedeemCodeAsync(viewer.Id, invitation.Code);
        result!.Joined.Should().BeFalse();
        (await db.FamilyMembers.SingleAsync(x => x.AccountId == viewer.Id)).Permission.Should().Be(FamilyPermission.Viewer);
        (await db.FamilyInvitations.SingleAsync(x => x.Id == invitation.Invitation.Id)).ConsumedAt.Should().BeNull();
    }

    [Fact]
    public async Task Account_can_belong_to_multiple_families_but_other_tenant_is_neutral()
    {
        await using var db = Database();
        var first = AddAccount(db, "first@example.test");
        var second = AddAccount(db, "second@example.test");
        await db.SaveChangesAsync();
        var service = Service(db);
        var familyA = await service.CreateFamilyAsync(first.Id, new CreateFamilyRequest("A"));
        var familyB = await service.CreateFamilyAsync(second.Id, new CreateFamilyRequest("B"));
        (await service.GetFamilyAsync(first.Id, familyB.Id)).Should().BeNull();
        var invitation = (await service.CreateInvitationAsync(second.Id, familyB.Id,
            new CreateInvitationRequest(FamilyPermission.Viewer, null, null, null), "https://selma.test"))!;
        (await service.RedeemCodeAsync(first.Id, invitation.Code))!.Joined.Should().BeTrue();
        (await service.GetFamiliesAsync(first.Id)).Select(x => x.Id).Should().BeEquivalentTo([familyA.Id, familyB.Id]);
    }

    private static CoParentingDbContext Database() => new(new DbContextOptionsBuilder<CoParentingDbContext>()
        .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
    private static FamilyService Service(CoParentingDbContext db) => new(db, NullLogger<FamilyService>.Instance);
    private static Account AddAccount(CoParentingDbContext db, string email)
    {
        var account = new Account { Email = email, NormalizedEmail = email.ToUpperInvariant(), DisplayName = email, CreatedAt = DateTime.UtcNow };
        db.Accounts.Add(account);
        return account;
    }
}
