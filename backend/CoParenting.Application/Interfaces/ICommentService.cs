using CoParenting.Core.Entities;

namespace CoParenting.Application.Interfaces;

public interface ICommentService
{
    Task<Comment> AddCommentAsync(int dayAssignmentId, string parent, string commentText);
    Task<List<Comment>> GetCommentsForDayAsync(int dayAssignmentId);
    Task<Comment?> UpdateCommentAsync(int commentId, string commentText);
    Task<bool> DeleteCommentAsync(int commentId);
}
