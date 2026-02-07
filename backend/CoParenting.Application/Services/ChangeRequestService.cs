using CoParenting.Application.Interfaces;
using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.Application.Services;

/// <summary>
/// Service for managing change requests between parents for day assignments
/// </summary>
public class ChangeRequestService : IChangeRequestService
{
    private readonly CoParentingDbContext _context;

    public ChangeRequestService(CoParentingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates change requests for multiple dates
    /// </summary>
    public async Task<List<ChangeRequest>> CreateChangeRequestsAsync(
        int requestedByUserId,
        List<DateOnly> dates,
        string requestedParent,
        string? comment)
    {
        var user = await _context.Users.FindAsync(requestedByUserId);
        if (user == null)
            throw new InvalidOperationException("User not found");

        // Determine currentParent based on requester's role (opposite of requested)
        var currentParent = user.Role == "ParentA" ? "A" : "B";

        var changeRequests = new List<ChangeRequest>();

        foreach (var date in dates)
        {
            var changeRequest = new ChangeRequest
            {
                RequestedByUserId = requestedByUserId,
                RequestedForDate = date,
                CurrentParent = currentParent,
                RequestedParent = requestedParent,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow,
                Comment = comment
            };

            _context.ChangeRequests.Add(changeRequest);
            changeRequests.Add(changeRequest);
        }

        await _context.SaveChangesAsync();
        return changeRequests;
    }

    /// <summary>
    /// Gets a specific change request with user details
    /// </summary>
    public async Task<ChangeRequest?> GetChangeRequestAsync(int id)
    {
        return await _context.ChangeRequests
            .Include(cr => cr.RequestedByUser)
            .Include(cr => cr.ReviewedByUser)
            .FirstOrDefaultAsync(cr => cr.Id == id);
    }

    /// <summary>
    /// Gets all pending change requests
    /// </summary>
    public async Task<List<ChangeRequest>> GetPendingChangeRequestsAsync()
    {
        return await _context.ChangeRequests
            .Include(cr => cr.RequestedByUser)
            .Include(cr => cr.ReviewedByUser)
            .Where(cr => cr.Status == "Pending")
            .OrderBy(cr => cr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Gets all change requests created by or affecting a specific user
    /// </summary>
    public async Task<List<ChangeRequest>> GetMyChangeRequestsAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return new List<ChangeRequest>();

        // Get requests created by this user OR affecting their days
        var userParent = user.Role == "ParentA" ? "A" : "B";

        return await _context.ChangeRequests
            .Include(cr => cr.RequestedByUser)
            .Include(cr => cr.ReviewedByUser)
            .Where(cr => cr.RequestedByUserId == userId || cr.CurrentParent == userParent)
            .OrderByDescending(cr => cr.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Reviews a change request (approve or reject)
    /// If approved, updates the day assignment
    /// </summary>
    public async Task<ChangeRequest?> ReviewChangeRequestAsync(
        int id,
        int reviewedByUserId,
        bool approved,
        string? comment)
    {
        var changeRequest = await GetChangeRequestAsync(id);
        if (changeRequest == null)
            return null;

        if (changeRequest.Status != "Pending")
            throw new InvalidOperationException("Change request has already been reviewed");

        var reviewer = await _context.Users.FindAsync(reviewedByUserId);
        if (reviewer == null)
            throw new InvalidOperationException("Reviewer not found");

        // Update change request
        changeRequest.Status = approved ? "Approved" : "Rejected";
        changeRequest.ReviewedByUserId = reviewedByUserId;
        changeRequest.ReviewedAt = DateTime.UtcNow;
        if (comment != null)
            changeRequest.Comment = comment;

        // If approved, update the day assignment
        if (approved)
        {
            var existingDay = await _context.DayAssignments
                .FirstOrDefaultAsync(d => d.Date == changeRequest.RequestedForDate);

            if (existingDay != null)
            {
                existingDay.Parent = changeRequest.RequestedParent;
                existingDay.ModifiedAt = DateTime.UtcNow;
            }
            else
            {
                // Create new day assignment
                var newDay = new DayAssignment
                {
                    Date = changeRequest.RequestedForDate,
                    Parent = changeRequest.RequestedParent,
                    IsVAB = false,
                    SpecialStatus = null,
                    CreatedAt = DateTime.UtcNow
                };
                _context.DayAssignments.Add(newDay);
            }
        }

        await _context.SaveChangesAsync();
        return changeRequest;
    }

    /// <summary>
    /// Cancels a pending change request (only by creator)
    /// </summary>
    public async Task<bool> CancelChangeRequestAsync(int id, int userId)
    {
        var changeRequest = await GetChangeRequestAsync(id);
        if (changeRequest == null)
            return false;

        if (changeRequest.RequestedByUserId != userId)
            throw new InvalidOperationException("Only the creator can cancel a change request");

        if (changeRequest.Status != "Pending")
            throw new InvalidOperationException("Only pending change requests can be cancelled");

        changeRequest.Status = "Cancelled";
        changeRequest.ReviewedAt = DateTime.UtcNow;
        changeRequest.ReviewedByUserId = userId;

        await _context.SaveChangesAsync();
        return true;
    }
}
