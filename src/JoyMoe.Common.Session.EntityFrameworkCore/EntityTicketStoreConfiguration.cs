using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JoyMoe.Common.Session.EntityFrameworkCore
{
    public class EntityTicketStoreConfiguration<TUser, TSession> : IEntityTypeConfiguration<TSession>
       where TSession : EntityTicketStoreSession<TUser>
       where TUser : class
    {
        public void Configure(EntityTypeBuilder<TSession> builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.HasKey(session => session.Id);

            builder.HasOne(session => session.User)
                   .WithMany();

            builder.Property(session => session.Id)
                   .ValueGeneratedOnAdd();

            builder.ToTable("EntityTicketStoreSessions");
        }
    }
}
