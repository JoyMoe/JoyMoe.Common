namespace JoyMoe.Common.Abstractions;

public interface IStateful
{
    string? State { get; set; }

    long? LastUpdatedById { get; set; }

    string? LastUpdatedBy { get; set; }
}
