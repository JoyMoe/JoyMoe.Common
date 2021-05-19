namespace JoyMoe.Common.Workflow.Models
{
    public interface IStateful
    {
        string? State { get; set; }

        string? Note { get; set; }

        long? LastUpdatedById { get; set; }

        string? LastUpdatedBy { get; set; }
    }
}
