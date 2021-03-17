using System.ComponentModel.DataAnnotations;

namespace JoyMoe.Common.Data
{
    public interface IIdentifier
    {
        [Key]
        long Id { get; set; }
    }
}
