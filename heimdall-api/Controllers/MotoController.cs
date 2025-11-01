using Microsoft.AspNetCore.Mvc;
using HeimdallModel;
using HeimdallBusiness;
using Swashbuckle.AspNetCore.Annotations;
namespace MotosApi.Controllers; 
using Microsoft.EntityFrameworkCore;
using MotosApi.SwaggerExamples; 
using Swashbuckle.AspNetCore.Filters;
using System.Threading.Tasks; 
using System.Linq; 

[ApiController]
[Route("api/[controller]")]
public class MotosController : ControllerBase
{
    private readonly MotoService motoService;
    private readonly LinkGenerator linkGenerator;

    public MotosController(MotoService motoService, LinkGenerator linkGenerator)
    {
        this.motoService = motoService;
        this.linkGenerator = linkGenerator;
    }


    private object GetMotoResource(MotoModel moto)
    {
        return new
        {
            moto.id,
            moto.tipoMoto,
            moto.placa,
            moto.numChassi,
            moto.VagaId,
            vaga = moto.Vaga != null
                ? new
                {
                    moto.Vaga.Id,
                    moto.Vaga.Codigo,
                    moto.Vaga.Ocupada,
                    moto.Vaga.ZonaId,
                    links = new
                    {
                        self = linkGenerator.GetPathByAction(HttpContext, "GetById", "Vaga", new { id = moto.Vaga.Id })
                    }
                }
                : null,
            tagRfid = moto.TagRfid != null
                ? new
                {
                    moto.TagRfid.Id,
                    moto.TagRfid.MotoId,
                    moto.TagRfid.FaixaFrequencia,
                    moto.TagRfid.Banda,
                    moto.TagRfid.Aplicacao
                }
                : null,
            links = new
            {
                self = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Motos", new { id = moto.id }),
                update = linkGenerator.GetPathByAction(HttpContext, nameof(Put), "Motos", new { id = moto.id }),
                delete = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), "Motos", new { id = moto.id }),
                all = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Motos")
            }
        };
    }


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [SwaggerOperation(Summary = "Obtém todas as motos", Description = "Retorna uma lista de todas as motos cadastradas com paginação.")]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(MotoListResponseExample))]
    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
    {
        if (page <= 0) page = 1;
        if (pageSize <= 0) pageSize = 10;

        var query = motoService.ListarTodas();

        var totalItems = await query.CountAsync();
        if (totalItems == 0)
            return NoContent();

        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

        var motosPage = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var motosComLinks = motosPage.Select(m => GetMotoResource(m)).ToList();

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
            Items = motosComLinks
        };

        return Ok(result);
    }

    [HttpGet("{id}")]
    [SwaggerOperation(Summary = "Obtém uma moto por ID")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(MotoGetByIdResponseExample))]
    public IActionResult Get(int id)
    {
        var moto = motoService.ObterPorId(id);
        if (moto == null) return NotFound();

        var resultado = GetMotoResource(moto);
        return Ok(resultado);
    }

    [HttpGet("tipo")]
    [SwaggerOperation(Summary = "Obtém motos por tipo")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetPorTipo([FromQuery] string tipo)
    {
        // NOTA: arrumar o service ObterPorTipo pois está retornando APENAS UMA moto.
        var moto = motoService.ObterPorTipo(tipo);
        if (moto == null) return NotFound();

        var resultado = GetMotoResource(moto);
        return Ok(resultado);
    }

    [HttpPost]
    [SwaggerOperation(Summary = "Cadastra uma nova moto")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerRequestExample(typeof(MotoModel), typeof(MotoExample))]
    public IActionResult Post([FromBody] MotoModel moto)
    {
        if (string.IsNullOrWhiteSpace(moto.tipoMoto) || string.IsNullOrWhiteSpace(moto.placa))
            return BadRequest("Tipo da moto e placa são obrigatórios.");

        var criada = motoService.CadastrarMoto(moto);

        if (criada == null)
        {
            return BadRequest("A VagaId fornecida é inválida ou já está ocupada.");
        }

        var resultado = GetMotoResource(criada);
        return CreatedAtAction(nameof(Get), new { id = criada.id }, resultado);
    }

    [HttpPut("{id}")]
    [SwaggerOperation(Summary = "Atualiza uma moto existente")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerRequestExample(typeof(MotoModel), typeof(MotoExample))]
    public IActionResult Put(int id, [FromBody] MotoModel moto)
    {
        if (moto == null || moto.id != id)
            return BadRequest("Dados inconsistentes.");
    
        var motoExistente = motoService.ObterPorId(id);
        if (motoExistente == null)
        {
            return NotFound(); 
        }

       
        var sucesso = motoService.Atualizar(moto);

        if (!sucesso)
        {
            return BadRequest("A vaga de destino é inválida ou já está ocupada.");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    [SwaggerOperation(Summary = "Remove uma moto")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult Delete(int id)
    {
        return motoService.Remover(id) ? NoContent() : NotFound();
    }
}

