namespace JoyMoe.Common.Workflow.Models
{
    public interface IStateful
    {
        public string State { get; set; }

        public string Note { get; set; }

        public long LastUpdatedById { get; set; }

        public string LastUpdatedBy { get; set; }
    }
}
