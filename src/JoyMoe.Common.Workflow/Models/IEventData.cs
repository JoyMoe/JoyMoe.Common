namespace JoyMoe.Common.Workflow.Models
{
    public interface IEventData
    {
        string Note { get; set; }

        long JockeyId { get; set; }

        string Jockey { get; set; }
    }
}
