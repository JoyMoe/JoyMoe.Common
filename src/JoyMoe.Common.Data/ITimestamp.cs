using System;

namespace JoyMoe.Common.Data
{
    public interface ITimestamp
    {
        DateTime? CreationDate { get; set; }

        DateTime? ModificationDate { get; set; }
    }
}
