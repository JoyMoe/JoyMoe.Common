namespace JoyMoe.Common.Workflow.Models
{
    public interface IStateful
    {
        string? Stateful { get; set; }

        string? State { get; set; }

        string? Note { get; set; }

        long? LastUpdatedById { get; set; }

        string? LastUpdatedBy { get; set; }
    }
}
