namespace JoyMoe.Common.Workflow.Models
{
    public interface IEventData
    {
        public string Note { get; set; }

        public long JockeyId { get; set; }

        public string Jockey { get; set; }
    }
}
