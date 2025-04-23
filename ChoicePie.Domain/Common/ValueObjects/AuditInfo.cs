namespace ChoicePie.Domain.Common.ValueObjects;

public class AuditInfo : ValueObject
{
    public DateTime CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public Guid UpdatedBy { get; private set; }

    private AuditInfo()
    {
    }

    public AuditInfo(DateTime createdAt, Guid createdBy, DateTime updatedAt, Guid updatedBy)
    {
        CreatedAt = createdAt;
        CreatedBy = createdBy;
        UpdatedAt = updatedAt;
        UpdatedBy = updatedBy;
    }

    public AuditInfo Touch()
    {
        return new AuditInfo(CreatedAt, CreatedBy, DateTime.UtcNow, UpdatedBy);
    }

    public AuditInfo Touch(Guid updateBy)
    {
        return new AuditInfo(CreatedAt, CreatedBy, DateTime.UtcNow, updateBy);
    }

    protected override IEnumerable<object> GetEqualityComponents()
    {
        yield return CreatedAt;
        yield return CreatedBy;
        yield return UpdatedAt;
        yield return UpdatedBy;
    }
}