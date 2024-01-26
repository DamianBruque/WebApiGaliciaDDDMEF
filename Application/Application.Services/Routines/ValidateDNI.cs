using Domain.Contracts;
using Domain.Contracts.Interface.Repositories;
using Domain.Contracts.Interface.ValidationRoutines;
using System.ComponentModel.Composition;

namespace Application.Services.Routines;

[Export(typeof(IValidationRoutine))]
[ExportMetadata("Name", "ValidateDNI")]
public class ValidateDNI : IValidationRoutine
{
    public bool ExecuteRoutine(string clientDocumentNumber)
    {
        return true;
    }
}
