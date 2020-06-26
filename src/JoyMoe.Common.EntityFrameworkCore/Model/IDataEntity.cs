using System;

namespace JoyMoe.Common.EntityFrameworkCore.Model
{
    public interface IDataEntity : IIdentifier
    {
        DateTimeOffset CreatedAt { get; set; }

        DateTimeOffset UpdatedAt { get; set; }
    }
}
