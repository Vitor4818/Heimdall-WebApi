using Microsoft.AspNetCore.Mvc; 
using HeimdallModel;   
using HeimdallBusiness;
using Swashbuckle.AspNetCore.Annotations;  
namespace MotosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotosController : ControllerBase
{
    private readonly MotoService motoService;

    public MotosController(MotoService motoService)
    {
        this.motoService = motoService;
    }

   [HttpGet]
[SwaggerOperation(
    Summary = "Lista todas as motos",
    Description = "Retorna uma lista paginada de todas as motos cadastradas. Caso não haja motos, retorna 204 No Content.",
    OperationId = "GetAllMotos",
    Tags = new[] { "Moto" }
)]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public IActionResult Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
{
    if (page <= 0) page = 1;
    if (pageSize <= 0) pageSize = 10;

    var allMotos = motoService.ListarTodas();
    if (!allMotos.Any()) return NoContent();

    var totalItems = allMotos.Count;
    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    var motosPage = allMotos
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(m => new
        {
            m.id,
            m.tipoMoto,
            m.placa,
            m.numChassi,
            links = new
            {
                self = Url.Action(nameof(Get), new { id = m.id }),
                update = Url.Action(nameof(Put), new { id = m.id }),
                delete = Url.Action(nameof(Delete), new { id = m.id }),
                all = Url.Action(nameof(Get))
            }
        })
        .Cast<object>() // força para object, compatível com PagedResultDto<object>
        .ToList();

    var result = new PagedResultDto<object>
    {
        Page = page,
        PageSize = pageSize,
        TotalPages = totalPages,
        TotalItems = totalItems,
        Links = new
        {
            self = Url.Action(nameof(Get), new { page, pageSize }),
            next = page < totalPages ? Url.Action(nameof(Get), new { page = page + 1, pageSize }) : null,
            prev = page > 1 ? Url.Action(nameof(Get), new { page = page - 1, pageSize }) : null
        },
        Items = motosPage
    };

    return Ok(result);
}

    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Obtém uma moto por ID",
        Description = "Retorna uma moto com base no ID fornecido. Se não encontrar a moto, retorna 404 Not Found.",
        OperationId = "GetMotoById",
        Tags = new[] { "Moto" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Get(int id)
    {
        var moto = motoService.ObterPorId(id);
        if (moto == null) return NotFound();

        var resultado = new {
            moto.id,
            moto.tipoMoto,
            moto.placa,
            moto.numChassi,
            links = new {
                all = Url.Action(nameof(Get)),
                self = Url.Action(nameof(Get), new { id = moto.id }),
                update = Url.Action(nameof(Put), new { id = moto.id }),
                delete = Url.Action(nameof(Delete), new { id = moto.id })
            }
        };

        return Ok(resultado);
    }

    [HttpGet("tipo")]
    [SwaggerOperation(
        Summary = "Obtém motos por tipo",
        Description = "Retorna uma lista de motos filtradas por tipo. Se não encontrar motos desse tipo, retorna 404 Not Found.",
        OperationId = "GetMotosByType",
        Tags = new[] { "Moto" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
public IActionResult GetPorTipo([FromQuery] string tipo)
{
    var moto = motoService.ObterPorTipo(tipo);
    if (moto == null) return NotFound();

    var resultado = new {
        moto.id,
        moto.tipoMoto,
        moto.placa,
        moto.numChassi,
        links = new {
            self = Url.Action(nameof(Get), new { id = moto.id }),
            update = Url.Action(nameof(Put), new { id = moto.id }),
            delete = Url.Action(nameof(Delete), new { id = moto.id })
        }
    };

    return Ok(resultado);
}

    [HttpPost]
    [SwaggerOperation(
        Summary = "Cadastra uma nova moto",
        Description = "Recebe um objeto de moto e cadastra uma nova moto. Retorna o objeto da moto criada com status 201 Created.",
        OperationId = "CreateMoto",
        Tags = new[] { "Moto" }
    )]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult Post([FromBody] MotoModel moto)
    {
        if (string.IsNullOrWhiteSpace(moto.tipoMoto) || string.IsNullOrWhiteSpace(moto.placa))
            return BadRequest("Tipo da moto e placa são obrigatórios.");

        var criada = motoService.CadastrarMoto(moto);

        var resultado = new {
            criada.id,
            criada.tipoMoto,
            criada.placa,
            criada.numChassi,
            links = new {
                self = Url.Action(nameof(Get), new { id = criada.id }),
                update = Url.Action(nameof(Put), new { id = criada.id }),
                delete = Url.Action(nameof(Delete), new { id = criada.id }),
                all = Url.Action(nameof(Get))
            }
        };

        return CreatedAtAction(nameof(Get), new { id = criada.id }, resultado);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualiza uma moto existente",
        Description = "Atualiza os dados de uma moto existente. Retorna 204 No Content se a atualização for bem-sucedida. Caso contrário, retorna 404 Not Found.",
        OperationId = "UpdateMoto",
        Tags = new[] { "Moto" }
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Put(int id, [FromBody] MotoModel moto)
    {
        if (moto == null || moto.id != id)
            return BadRequest("Dados inconsistentes.");

        return motoService.Atualizar(moto) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remove uma moto",
        Description = "Remove uma moto com base no ID fornecido. Retorna 204 No Content se a remoção for bem-sucedida. Caso contrário, retorna 404 Not Found.",
        OperationId = "DeleteMoto",
        Tags = new[] { "Moto" }
    )]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        return motoService.Remover(id) ? NoContent() : NotFound();
    }
}
