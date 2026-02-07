using CoParenting.Core.Entities;

namespace CoParenting.Application.Interfaces;

public interface IChangeRequestService
{
    Task<List<ChangeRequest>> CreateChangeRequestsAsync(int requestedByUserId, List<DateOnly> dates, string requestedParent, string? comment);
    Task<ChangeRequest?> GetChangeRequestAsync(int id);
    Task<List<ChangeRequest>> GetPendingChangeRequestsAsync();
    Task<List<ChangeRequest>> GetMyChangeRequestsAsync(int userId);
    Task<ChangeRequest?> ReviewChangeRequestAsync(int id, int reviewedByUserId, bool approved, string? comment);
    Task<bool> CancelChangeRequestAsync(int id, int userId);
}
