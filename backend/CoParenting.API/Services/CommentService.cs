using CoParenting.Core.Entities;
using CoParenting.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CoParenting.API.Services;

/// <summary>
/// Service for managing comments on day assignments
/// </summary>
public class CommentService
{
    private readonly CoParentingDbContext _context;

    public CommentService(CoParentingDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Adds a comment to a specific day
    /// </summary>
    public async Task<Comment> AddCommentAsync(int dayAssignmentId, string parent, string commentText)
    {
        var comment = new Comment
        {
            DayAssignmentId = dayAssignmentId,
            Parent = parent,
            CommentText = commentText,
            CreatedAt = DateTime.UtcNow
        };

        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        return comment;
    }

    /// <summary>
    /// Gets all comments for a specific day assignment
    /// </summary>
    public async Task<List<Comment>> GetCommentsForDayAsync(int dayAssignmentId)
    {
        return await _context.Comments
            .Where(c => c.DayAssignmentId == dayAssignmentId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
    }

    /// <summary>
    /// Updates a comment
    /// </summary>
    public async Task<Comment?> UpdateCommentAsync(int commentId, string commentText)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null) return null;

        comment.CommentText = commentText;
        comment.ModifiedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return comment;
    }

    /// <summary>
    /// Deletes a comment
    /// </summary>
    public async Task<bool> DeleteCommentAsync(int commentId)
    {
        var comment = await _context.Comments.FindAsync(commentId);
        if (comment == null) return false;

        _context.Comments.Remove(comment);
        await _context.SaveChangesAsync();
        return true;
    }
}
