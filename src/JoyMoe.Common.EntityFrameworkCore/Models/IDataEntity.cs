using System;

namespace JoyMoe.Common.EntityFrameworkCore.Models
{
    public interface IDataEntity : IIdentifier
    {
        DateTimeOffset CreatedAt { get; set; }

        DateTimeOffset UpdatedAt { get; set; }
    }
}
