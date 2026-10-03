namespace Ordering.Common.Domain;

public abstract class Entity
{
    public int Id { get; private set; }
    public string? CreatedBy { get; private set; } = null;
    public DateTime CreationData { get; set; }
    public string? LastModifiedBy { get; set; } = null;
    public DateTime? ModifiedDate { get; set; } = null;
}