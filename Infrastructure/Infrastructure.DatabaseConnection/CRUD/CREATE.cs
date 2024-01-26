

using Domain.Contracts.Interface.Repositories;
using System.ComponentModel.Composition;

namespace Infrastructure.DatabaseConnection.CRUD;

[Export(typeof(IRepositoryMethod<>))]
public class CREATE<T> : IRepositoryMethod<T> where T : class
{
    
    public Task<T> ExecuteMethod(T entity)
    {
        throw new NotImplementedException();
    }
}
