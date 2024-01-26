

using System.Data.Common;

namespace Domain.Contracts.Interface.Repositories;

public interface IRepositoryMethod<T> where T : class
{
    Task<T> ExecuteMethod(T entity);
}
