namespace JoyMoe.Common.Abstractions;

public interface INamedEntity : IDataEntity
{
    string CanonicalName { get; set; }
}
