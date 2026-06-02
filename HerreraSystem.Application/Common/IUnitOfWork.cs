using System;
using System.Collections.Generic;
using System.Text;

namespace HerreraSystem.Application.Common
{
    public interface IUnitOfWork
    {
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();

        Task SaveChangesAsync();
    }
}
