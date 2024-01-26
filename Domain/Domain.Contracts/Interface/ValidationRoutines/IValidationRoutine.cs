

namespace Domain.Contracts.Interface.ValidationRoutines;

public interface IValidationRoutine
{
    bool ExecuteRoutine(string clientDocumentNumber);
}
