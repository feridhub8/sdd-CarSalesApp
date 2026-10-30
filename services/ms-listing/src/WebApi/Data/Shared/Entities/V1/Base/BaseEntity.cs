namespace WebApi.Data.Shared.Entities.V1.Base;

public abstract class BaseEntity
{
    // Version 7 ids are time-ordered, so inserts stay index-friendly and ids reveal no counts.
    public Guid Id { get; set; } = Guid.CreateVersion7();

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; }

    public DateTime? DeletedAt { get; set; }
}
