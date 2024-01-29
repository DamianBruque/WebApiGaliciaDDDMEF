using Domain.Contracts;
using Domain.Contracts.Interface.Repositories;
using Domain.Contracts.Interface.ValidationRoutines;
using System.ComponentModel.Composition;

namespace Application.Services.Routines;

[Export(typeof(IValidationRoutine))]
[Export(typeof(Interface1))]
[ExportMetadata("Name", "ValidateDNI")]
public class ValidateDNI : IValidationRoutine, Interface1
{
    public bool ExecuteRoutine(string clientDocumentNumber)
    {
        return true;
    }

    public bool ExecuteRoutine(string clientDocumentNumber, string email)
    {
        return false;
    }
}
