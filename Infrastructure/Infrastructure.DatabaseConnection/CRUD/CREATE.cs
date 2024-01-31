

using Domain.Contracts.Interface.Repositories;
using Infrastructure.DatabaseConnection.DatabaseAccess;
using System.ComponentModel.Composition;

namespace Infrastructure.DatabaseConnection.CRUD;

[Export(typeof(IRepositoryMethod))]
public class CREATE : IRepositoryMethod
{
    private readonly ProjectContext context;
    public CREATE(ProjectContext projectContext)
    {
        context = projectContext;
    }
    public Task<object> ExecuteMethod(object entity)
    {
        throw new NotImplementedException();
    }
}
