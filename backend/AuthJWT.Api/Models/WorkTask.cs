namespace AuthJWT.Api.Models;

public class WorkTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = WorkTaskStatus.Pending;
    public int? AssignedUserId { get; set; }
    public User? AssignedUser { get; set; }
    public DateTime? Deadline { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public bool IsClosed =>
        Status == WorkTaskStatus.Completed || Status == WorkTaskStatus.Cancelled;

    public bool CanTransitionTo(string newStatus) =>
        (Status, newStatus) switch
        {
            _ when Status == newStatus                                          => false,
            (WorkTaskStatus.Completed, _)                                       => false,
            (WorkTaskStatus.Cancelled,  _)                                      => false,
            (WorkTaskStatus.Pending,    WorkTaskStatus.InProgress)              => true,
            (WorkTaskStatus.Pending,    WorkTaskStatus.Cancelled)               => true,
            (WorkTaskStatus.InProgress, WorkTaskStatus.Review)                  => true,
            (WorkTaskStatus.InProgress, WorkTaskStatus.Pending)                 => true,
            (WorkTaskStatus.InProgress, WorkTaskStatus.Cancelled)               => true,
            (WorkTaskStatus.Review,     WorkTaskStatus.Completed)               => true,
            (WorkTaskStatus.Review,     WorkTaskStatus.InProgress)              => true,
            (WorkTaskStatus.Review,     WorkTaskStatus.Cancelled)               => true,
            _                                                                   => false
        };
}
