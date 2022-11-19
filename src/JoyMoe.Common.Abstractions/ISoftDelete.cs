namespace JoyMoe.Common.Abstractions;

public interface ISoftDelete
{
    DateTime? DeletionDate { get; set; }
}
