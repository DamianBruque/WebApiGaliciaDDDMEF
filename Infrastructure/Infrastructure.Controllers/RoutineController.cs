using Application.DataTransferObjects.Request;
using Application.DataTransferObjects.Response;
using Domain.Contracts.Interface.ValidationRoutines;
using Infrastructure.DatabaseConnection.DatabaseAccess;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;

namespace Infrastructure.WebApi.Controllers;

[Route("Routines")]
[ApiController]
public class RoutineController : ControllerBase
{
    private readonly ProjectContext _context;
    public RoutineController(ProjectContext context)
    {
        _context = context;
    }




    [ImportMany(typeof(IValidationRoutine))]
    public List<Lazy<IValidationRoutine, IValidationRoutineMetadata>> ValidationRoutines { get; set; }

    [HttpGet("Validation")]
    public IActionResult ExecuteRoutine([FromBody] ValidationRoutineRequest parameters)
    {
        ActionResult result;
        string message = string.Empty;
        // probamos la conexion a la base de datos a travez de la clase ProjectContext
        Console.WriteLine(_context.Database.CanConnect());
        try
        {
            var catalog = new DirectoryCatalog(@"..\..\Application\Application.Services\obj\Debug\net8.0");
            var container = new CompositionContainer(catalog);
            container.ComposeParts(this);

            var routine = ValidationRoutines.FirstOrDefault(x => x.Metadata.Name == parameters.RoutineName);
            if (routine == null && ValidationRoutines.Count != 0)
            {
                throw new Exception($"La rutina no existe. Rutinas existentes: {ValidationRoutines.Count}");
            }
            else if (ValidationRoutines.Count == 0)
            {
                throw new Exception("No hay rutinas disponibles.");
            }
            else
            {
                bool routineResult = routine.Value.ExecuteRoutine(parameters.ClientDNI);
                result = routineResult ? Ok(new ValidationRoutineResponse { Message = "El cliente puede ser asegurado.", Result = routineResult }) : Ok(new ValidationRoutineResponse { Message = "El cliente no puede ser asegurado.", Result = routineResult });
            }
        }
        catch (Exception ex)
        {
            result = StatusCode(500, $"Error capturado en controller: {ex.Message}");
        }
        return result;
    }
}
