using System;

namespace JoyMoe.Common.Data
{
    public interface ISoftDelete
    {
        DateTime? DeletionDate { get; set; }
    }
}
