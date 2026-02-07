using CoParenting.Application.Services;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Tests.Unit.Services;

public class ChangeRequestServiceTests : IDisposable
{
    private readonly CoParentingDbContext _context;
    private readonly ChangeRequestService _service;

    public ChangeRequestServiceTests()
    {
        var options = new DbContextOptionsBuilder<CoParentingDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new CoParentingDbContext(options);
        _service = new ChangeRequestService(_context);

        // Seed test users
        _context.Users.AddRange(
            new User { Id = 1, Email = "parent.a@test.com", Role = "ParentA", DisplayName = "Parent A", CreatedAt = DateTime.UtcNow },
            new User { Id = 2, Email = "parent.b@test.com", Role = "ParentB", DisplayName = "Parent B", CreatedAt = DateTime.UtcNow }
        );
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region CreateChangeRequestsAsync Tests

    [Fact]
    public async Task CreateChangeRequestsAsync_ShouldCreateRequests_WhenValidData()
    {
        // Arrange
        var dates = new List<DateOnly> { new DateOnly(2025, 3, 15), new DateOnly(2025, 3, 16) };
        var userId = 1;
        var requestedParent = "A";
        var comment = "Need to switch for work";

        // Act
        var result = await _service.CreateChangeRequestsAsync(userId, dates, requestedParent, comment);

        // Assert
        result.Should().HaveCount(2);
        result.All(cr => cr.RequestedByUserId == userId).Should().BeTrue();
        result.All(cr => cr.RequestedParent == requestedParent).Should().BeTrue();
        result.All(cr => cr.Status == "Pending").Should().BeTrue();
        result.All(cr => cr.Comment == comment).Should().BeTrue();
        result.Select(cr => cr.RequestedForDate).Should().BeEquivalentTo(dates);
    }

    [Fact]
    public async Task CreateChangeRequestsAsync_ShouldDetermineCurrentParent_FromParentARole()
    {
        // Arrange - Parent A requesting to swap to Parent A
        var dates = new List<DateOnly> { new DateOnly(2025, 3, 15) };
        var userId = 1; // ParentA role
        var requestedParent = "A";

        // Act
        var result = await _service.CreateChangeRequestsAsync(userId, dates, requestedParent, null);

        // Assert
        result.First().CurrentParent.Should().Be("B"); // If requesting A, current must be B
    }

    [Fact]
    public async Task CreateChangeRequestsAsync_ShouldDetermineCurrentParent_FromParentBRole()
    {
        // Arrange - Parent B requesting to swap to Parent B
        var dates = new List<DateOnly> { new DateOnly(2025, 3, 15) };
        var userId = 2; // ParentB role
        var requestedParent = "B";

        // Act
        var result = await _service.CreateChangeRequestsAsync(userId, dates, requestedParent, null);

        // Assert
        result.First().CurrentParent.Should().Be("A"); // If requesting B, current must be A
    }

    [Fact]
    public async Task CreateChangeRequestsAsync_ShouldThrow_WhenUserNotFound()
    {
        // Arrange
        var dates = new List<DateOnly> { new DateOnly(2025, 3, 15) };
        var userId = 999; // Non-existent user

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.CreateChangeRequestsAsync(userId, dates, "A", null));
    }

    [Fact]
    public async Task CreateChangeRequestsAsync_ShouldAllowNullComment()
    {
        // Arrange
        var dates = new List<DateOnly> { new DateOnly(2025, 3, 15) };
        var userId = 1;

        // Act
        var result = await _service.CreateChangeRequestsAsync(userId, dates, "A", null);

        // Assert
        result.First().Comment.Should().BeNull();
    }

    #endregion

    #region GetChangeRequestAsync Tests

    [Fact]
    public async Task GetChangeRequestAsync_ShouldReturnRequest_WhenExists()
    {
        // Arrange
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = 1,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _context.ChangeRequests.Add(changeRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetChangeRequestAsync(changeRequest.Id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(changeRequest.Id);
        result.RequestedByUser.Should().NotBeNull();
        result.RequestedByUser!.DisplayName.Should().Be("Parent A");
    }

    [Fact]
    public async Task GetChangeRequestAsync_ShouldReturnNull_WhenNotExists()
    {
        // Act
        var result = await _service.GetChangeRequestAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetChangeRequestAsync_ShouldIncludeReviewedByUser_WhenReviewed()
    {
        // Arrange
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = 1,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Approved",
            ReviewedByUserId = 2,
            ReviewedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.ChangeRequests.Add(changeRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetChangeRequestAsync(changeRequest.Id);

        // Assert
        result!.ReviewedByUser.Should().NotBeNull();
        result.ReviewedByUser!.DisplayName.Should().Be("Parent B");
    }

    #endregion

    #region GetPendingChangeRequestsAsync Tests

    [Fact]
    public async Task GetPendingChangeRequestsAsync_ShouldReturnOnlyPendingRequests()
    {
        // Arrange
        _context.ChangeRequests.AddRange(
            new ChangeRequest
            {
                RequestedByUserId = 1,
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            },
            new ChangeRequest
            {
                RequestedByUserId = 1,
                RequestedForDate = new DateOnly(2025, 3, 16),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Approved",
                ReviewedByUserId = 2,
                CreatedAt = DateTime.UtcNow
            },
            new ChangeRequest
            {
                RequestedByUserId = 2,
                RequestedForDate = new DateOnly(2025, 3, 17),
                CurrentParent = "A",
                RequestedParent = "B",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow.AddMinutes(1)
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPendingChangeRequestsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.All(cr => cr.Status == "Pending").Should().BeTrue();
    }

    [Fact]
    public async Task GetPendingChangeRequestsAsync_ShouldOrderByCreatedAt()
    {
        // Arrange
        _context.ChangeRequests.AddRange(
            new ChangeRequest
            {
                RequestedByUserId = 1,
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow.AddMinutes(2)
            },
            new ChangeRequest
            {
                RequestedByUserId = 2,
                RequestedForDate = new DateOnly(2025, 3, 16),
                CurrentParent = "A",
                RequestedParent = "B",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPendingChangeRequestsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.First().CreatedAt.Should().BeBefore(result.Last().CreatedAt);
    }

    [Fact]
    public async Task GetPendingChangeRequestsAsync_ShouldReturnEmpty_WhenNoPending()
    {
        // Arrange
        _context.ChangeRequests.Add(new ChangeRequest
        {
            RequestedByUserId = 1,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Approved",
            ReviewedByUserId = 2,
            CreatedAt = DateTime.UtcNow
        });
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetPendingChangeRequestsAsync();

        // Assert
        result.Should().BeEmpty();
    }

    #endregion

    #region GetMyChangeRequestsAsync Tests

    [Fact]
    public async Task GetMyChangeRequestsAsync_ShouldReturnRequestsCreatedByUser()
    {
        // Arrange
        _context.ChangeRequests.AddRange(
            new ChangeRequest
            {
                RequestedByUserId = 1,
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            },
            new ChangeRequest
            {
                RequestedByUserId = 2,
                RequestedForDate = new DateOnly(2025, 3, 16),
                CurrentParent = "B", // Changed from "A" to "B" so it doesn't affect Parent A
                RequestedParent = "B",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMyChangeRequestsAsync(1);

        // Assert
        result.Should().ContainSingle();
        result.First().RequestedByUserId.Should().Be(1);
    }

    [Fact]
    public async Task GetMyChangeRequestsAsync_ShouldReturnRequestsAffectingUsersDays()
    {
        // Arrange - Parent A (user 1) should see requests affecting their days (CurrentParent = "A")
        _context.ChangeRequests.AddRange(
            new ChangeRequest
            {
                RequestedByUserId = 2, // Created by Parent B
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "A", // Affects Parent A's day
                RequestedParent = "B",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            },
            new ChangeRequest
            {
                RequestedByUserId = 2, // Created by Parent B
                RequestedForDate = new DateOnly(2025, 3, 16),
                CurrentParent = "B", // Affects Parent B's day (not Parent A)
                RequestedParent = "B",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMyChangeRequestsAsync(1); // Parent A queries

        // Assert
        result.Should().ContainSingle();
        result.First().CurrentParent.Should().Be("A");
    }

    [Fact]
    public async Task GetMyChangeRequestsAsync_ShouldReturnBothCreatedAndAffecting()
    {
        // Arrange - Parent A creates a request AND has one affecting their day
        _context.ChangeRequests.AddRange(
            new ChangeRequest
            {
                RequestedByUserId = 1, // Created by Parent A
                RequestedForDate = new DateOnly(2025, 3, 15),
                CurrentParent = "B",
                RequestedParent = "A",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            },
            new ChangeRequest
            {
                RequestedByUserId = 2, // Created by Parent B
                RequestedForDate = new DateOnly(2025, 3, 16),
                CurrentParent = "A", // Affects Parent A's day
                RequestedParent = "B",
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            }
        );
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetMyChangeRequestsAsync(1); // Parent A queries

        // Assert
        result.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetMyChangeRequestsAsync_ShouldThrow_WhenUserNotFound()
    {
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.GetMyChangeRequestsAsync(999));
    }

    #endregion

    #region ReviewChangeRequestAsync Tests

    [Fact]
    public async Task ReviewChangeRequestAsync_ShouldApproveRequest_AndUpdateDayAssignment()
    {
        // Arrange
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = 1,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _context.ChangeRequests.Add(changeRequest);

        // Add existing day assignment
        var dayAssignment = new DayAssignment
        {
            Date = new DateOnly(2025, 3, 15),
            Parent = "B",
            IsVAB = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.DayAssignments.Add(dayAssignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ReviewChangeRequestAsync(changeRequest.Id, 2, true, "Approved!");

        // Assert
        result.Should().NotBeNull();
        result!.Status.Should().Be("Approved");
        result.ReviewedByUserId.Should().Be(2);
        result.ReviewedAt.Should().NotBeNull();
        result.Comment.Should().Be("Approved!");

        // Verify day assignment was updated
        var updatedDay = await _context.DayAssignments.FirstAsync(d => d.Date == new DateOnly(2025, 3, 15));
        updatedDay.Parent.Should().Be("A");
    }

    [Fact]
    public async Task ReviewChangeRequestAsync_ShouldCreateDayAssignment_WhenNotExists()
    {
        // Arrange
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = 1,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B", // Changed from null - must have a value
            RequestedParent = "A",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _context.ChangeRequests.Add(changeRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ReviewChangeRequestAsync(changeRequest.Id, 2, true, null);

        // Assert
        result!.Status.Should().Be("Approved");

        // Verify day assignment was created
        var newDay = await _context.DayAssignments.FirstOrDefaultAsync(d => d.Date == new DateOnly(2025, 3, 15));
        newDay.Should().NotBeNull();
        newDay!.Parent.Should().Be("A");
        newDay.IsVAB.Should().BeFalse();
        newDay.SpecialStatus.Should().BeNull();
    }

    [Fact]
    public async Task ReviewChangeRequestAsync_ShouldRejectRequest_WithoutUpdatingDayAssignment()
    {
        // Arrange
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = 1,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _context.ChangeRequests.Add(changeRequest);

        var dayAssignment = new DayAssignment
        {
            Date = new DateOnly(2025, 3, 15),
            Parent = "B",
            IsVAB = false,
            CreatedAt = DateTime.UtcNow
        };
        _context.DayAssignments.Add(dayAssignment);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ReviewChangeRequestAsync(changeRequest.Id, 2, false, "Sorry, can't do it");

        // Assert
        result!.Status.Should().Be("Rejected");
        result.ReviewedByUserId.Should().Be(2);
        result.Comment.Should().Be("Sorry, can't do it");

        // Verify day assignment was NOT updated
        var unchangedDay = await _context.DayAssignments.FirstAsync(d => d.Date == new DateOnly(2025, 3, 15));
        unchangedDay.Parent.Should().Be("B");
    }

    [Fact]
    public async Task ReviewChangeRequestAsync_ShouldReturnNull_WhenRequestNotFound()
    {
        // Act
        var result = await _service.ReviewChangeRequestAsync(999, 2, true, null);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task ReviewChangeRequestAsync_ShouldThrow_WhenAlreadyReviewed()
    {
        // Arrange
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = 1,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Approved", // Already reviewed
            ReviewedByUserId = 2,
            ReviewedAt = DateTime.UtcNow.AddHours(-1),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        };
        _context.ChangeRequests.Add(changeRequest);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.ReviewChangeRequestAsync(changeRequest.Id, 2, true, null));
    }

    [Fact]
    public async Task ReviewChangeRequestAsync_ShouldAllowNullComment()
    {
        // Arrange
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = 1,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _context.ChangeRequests.Add(changeRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.ReviewChangeRequestAsync(changeRequest.Id, 2, false, null);

        // Assert
        result!.Comment.Should().BeNull();
    }

    #endregion

    #region CancelChangeRequestAsync Tests

    [Fact]
    public async Task CancelChangeRequestAsync_ShouldCancelRequest_WhenCreatorMatches()
    {
        // Arrange
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = 1,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _context.ChangeRequests.Add(changeRequest);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.CancelChangeRequestAsync(changeRequest.Id, 1);

        // Assert
        result.Should().BeTrue();
        var cancelled = await _context.ChangeRequests.FindAsync(changeRequest.Id);
        cancelled!.Status.Should().Be("Cancelled");
    }

    [Fact]
    public async Task CancelChangeRequestAsync_ShouldReturnFalse_WhenRequestNotFound()
    {
        // Act
        var result = await _service.CancelChangeRequestAsync(999, 1);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task CancelChangeRequestAsync_ShouldThrow_WhenUserNotCreator()
    {
        // Arrange
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = 1,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };
        _context.ChangeRequests.Add(changeRequest);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.CancelChangeRequestAsync(changeRequest.Id, 2)); // Different user
    }

    [Fact]
    public async Task CancelChangeRequestAsync_ShouldThrow_WhenRequestNotPending()
    {
        // Arrange
        var changeRequest = new ChangeRequest
        {
            RequestedByUserId = 1,
            RequestedForDate = new DateOnly(2025, 3, 15),
            CurrentParent = "B",
            RequestedParent = "A",
            Status = "Approved", // Not pending
            ReviewedByUserId = 2,
            CreatedAt = DateTime.UtcNow
        };
        _context.ChangeRequests.Add(changeRequest);
        await _context.SaveChangesAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await _service.CancelChangeRequestAsync(changeRequest.Id, 1));
    }

    #endregion
}
