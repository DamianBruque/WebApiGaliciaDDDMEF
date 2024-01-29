using Domain.Contracts.Interface.ValidationRoutines;
using Domain.Entities;
using System.ComponentModel.Composition;

namespace Application.Services.Routines;

[Export(typeof(IValidationRoutine))]
[ExportMetadata("Name", "ValidateAccumulationByDNI")]
public class ValidateAccumulationByDNI : IValidationRoutine
{
    public bool ExecuteRoutine(string clientDocumentNumber)
    {
        bool result = Calculate(clientDocumentNumber);
        /*
        public bool Calculate(string dniParameter)
        {
        bool result = false;
        Client client = GetClient(dniParameter);
        if (client != null)
        {
            PolicyCollection policies = GetPolicies(client);
            int maxCumulo = GetMaxCum(dniParameter);
            int cumulo = 0;
            int index = 0;
            while (index < policies.Count && cumulo < maxCumulo)
            {
                cumulo += GetCumulo(policies[index]);
                index++;
            }
            result = cumulo < maxCumulo;
        }
        else
        {
            throw new Exception("El cliente no existe");
        }
        return result;
        }

        private Client? GetClient(string dniParameter)
        {
        Client result = new Client
        {
            IsVT1 = ExistInVT1(dniParameter),
            IsVT7 = ExistInVT7(dniParameter)
        };
        return result.IsVT1 || result.IsVT7 ? result : null;
        }

        private bool ExistInVT7(string dniParameter)
        {
        throw new NotImplementedException();
        }

        private bool ExistInVT1(string dniParameter)
        {
        throw new NotImplementedException();
        }

        private PolicyCollection GetPolicies(Client client)
        {
        PolicyCollection result = new PolicyCollection();
        if (client.IsVT1)
        {
            result.AddRange(GetPoliciesVT1(client.ClientCode));
        }
        if (client.IsVT7)
        {
            result.AddRange(GetPoliciesVT7(client.ClientCode));
        }
        return result;
        }

        private IEnumerable<Policy> GetPoliciesVT7(string clientCode)
        {
        throw new NotImplementedException();
        }

        private IEnumerable<Policy> GetPoliciesVT1(string clientCode)
        {
        throw new NotImplementedException();
        }

        private int GetCumulo(Policy policy)
        {
        throw new NotImplementedException();
        }

        private int GetMaxCum(string dniParameter)
        {
        //query
        throw new NotImplementedException();
        }
         
         
        */
        // Aca va la logica de la validacion en vez de un simple "return true"
        //Console.WriteLine("Ejecutando rutina de validacion de acumulacion por DNI");
        
        return result;
    }
    public bool Calculate(string dniParameter)
    {
        bool result = false;
        Client client = GetClient(dniParameter);
        if (client != null)
        {
            PolicyCollection policies = GetPolicies(client);
            int maxCumulo = GetMaxCum(dniParameter);
            int cumulo = 0;
            int index = 0;
            while (index < policies.Count && cumulo < maxCumulo)
            {
                cumulo += GetCumulo(policies[index]);
                index++;
            }
            result = cumulo < maxCumulo;
        }
        else
        {
            throw new Exception("El cliente no existe");
        }
        return result;
    }

    private Client? GetClient(string dniParameter)
    {
        Client result = new Client
        {
            IsVT1 = ExistInVT1(dniParameter),
            IsVT7 = ExistInVT7(dniParameter),
            ClientDNI = dniParameter
        };
        return result.IsVT1 || result.IsVT7 ? result : null;
    }

    private bool ExistInVT7(string dniParameter)
    {
        return true;
    }

    private bool ExistInVT1(string dniParameter)
    {
        return false;
    }

    private PolicyCollection GetPolicies(Client client)
    {
        PolicyCollection result = new PolicyCollection();
        if (client.IsVT1)
        {
            result.AddRange(GetPoliciesVT1(client.ClientCode));
        }
        if (client.IsVT7)
        {
            result.AddRange(GetPoliciesVT7(client.ClientDNI));
        }
        return result;
    }

    private IEnumerable<Policy> GetPoliciesVT7(string clientCode)
    {
        throw new NotImplementedException();
    }

    private IEnumerable<Policy> GetPoliciesVT1(string clientCode)
    {
        throw new NotImplementedException();
    }

    private int GetCumulo(Policy policy)
    {
        throw new NotImplementedException();
    }

    private int GetMaxCum(string dniParameter)
    {
        //query
        throw new NotImplementedException();
    }
}
