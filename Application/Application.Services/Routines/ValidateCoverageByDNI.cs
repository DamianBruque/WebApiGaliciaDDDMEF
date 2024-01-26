using Domain.Contracts.Interface.ValidationRoutines;
using System.ComponentModel.Composition;

namespace Application.Services.Routines;

[Export(typeof(IValidationRoutine))]
[ExportMetadata("Name", "ValidateCoverageByDNI")]
public class ValidateCoverageByDNI : IValidationRoutine
{
    public bool ExecuteRoutine(string clientDocumentNumber)
    {
        return false;
    }
}
