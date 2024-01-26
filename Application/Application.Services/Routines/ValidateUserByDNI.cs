

using Domain.Contracts.Interface.ValidationRoutines;
using System.ComponentModel.Composition;

namespace Application.Services.Routines
{
    [Export(typeof(IValidationRoutine))]
    [ExportMetadata("Name", "ValidateUserByDNI")]
    public class ValidateUserByDNI : IValidationRoutine
    {
        public bool ExecuteRoutine(string clientDocumentNumber)
        {
            return true;
        }
    }
}
