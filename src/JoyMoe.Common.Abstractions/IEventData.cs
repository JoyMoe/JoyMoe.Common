namespace JoyMoe.Common.Abstractions
{
    public interface IEventData
    {
        string Note { get; set; }

        long JockeyId { get; set; }

        string Jockey { get; set; }
    }
}
