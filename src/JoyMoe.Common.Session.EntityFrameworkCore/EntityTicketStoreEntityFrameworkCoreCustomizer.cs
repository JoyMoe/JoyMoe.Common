using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace JoyMoe.Common.Session.EntityFrameworkCore
{
    public class EntityTicketStoreEntityFrameworkCoreCustomizer<TUser, TSession> : RelationalModelCustomizer
        where TSession : EntityTicketStoreSession<TUser>
        where TUser : class
    {
        public EntityTicketStoreEntityFrameworkCoreCustomizer(ModelCustomizerDependencies dependencies)
            : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            if (modelBuilder is null)
            {
                throw new ArgumentNullException(nameof(modelBuilder));
            }

            if (context is null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            modelBuilder.ApplyConfiguration(new EntityTicketStoreConfiguration<TUser, TSession>());

            base.Customize(modelBuilder, context);
        }
    }
}
