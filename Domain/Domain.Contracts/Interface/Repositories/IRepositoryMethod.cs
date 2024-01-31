

using System.Data.Common;

namespace Domain.Contracts.Interface.Repositories;

public interface IRepositoryMethod
{
    Task<object> ExecuteMethod(object entity);
}
