using Microsoft.AspNetCore.Mvc;
using HeimdallModel;
using HeimdallBusiness;
using Swashbuckle.AspNetCore.Annotations;
namespace MotosApi.Controllers; // Presumi que este namespace estava correto no seu original
using Microsoft.EntityFrameworkCore;
using MotosApi.SwaggerExamples; // Presumi que este namespace estava correto
using Swashbuckle.AspNetCore.Filters;

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

    /// <summary>
    /// Método helper privado para formatar a resposta da Moto com links HATEOAS.
    /// </summary>
    private object GetMotoResource(MotoModel moto)
    {
                return new
        {
            // --- Propriedades Principais da Moto ---
            moto.id,
            moto.tipoMoto,
            moto.placa,
            moto.numChassi,
            moto.VagaId, // O ID simples

            // --- Objeto Vaga Aninhado (SUMÁRIO) ---
            // CORREÇÃO: Formatamos a vaga manualmente para quebrar o ciclo.
            vaga = moto.Vaga != null
                ? new // Criamos um sumário da vaga
                {
                    moto.Vaga.Id,
                    moto.Vaga.Codigo,
                    moto.Vaga.Ocupada,
                    moto.Vaga.ZonaId,
                    // Note: Não incluímos moto.Vaga.Moto para quebrar o ciclo.
                    links = new
                    {
                        // Adicionamos um link para o recurso completo da Vaga
                        self = linkGenerator.GetPathByAction(HttpContext, "GetById", "Vaga", new { id = moto.Vaga.Id })
                    }
                }
                : null, // Se VagaId for null, o objeto 'vaga' será null

            // --- Objeto TagRfid Aninhado (SUMÁRIO) ---
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

            // --- Links HATEOAS da Moto ---
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

        // Paginação direta no banco
        var motosPage = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Gera os links HATEOAS usando GetMotoResource
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
    [SwaggerOperation(
        Summary = "Obtém uma moto por ID",
        Description = "Retorna uma moto com base no ID fornecido. Se não encontrar a moto, retorna 404 Not Found.",
        OperationId = "GetMotoById",
        Tags = new[] { "Moto" }
    )]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(MotoGetByIdResponseExample))]
    public IActionResult Get(int id)
    {
        var moto = motoService.ObterPorId(id);
        if (moto == null) return NotFound();

        // --- CORREÇÃO: Usar o método helper padronizado ---
        var resultado = GetMotoResource(moto);

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
        // NOTA: ObterPorTipo parece retornar apenas UMA moto. 
        // Se deveria retornar uma LISTA, este método precisa ser ajustado.
        var moto = motoService.ObterPorTipo(tipo); 
        if (moto == null) return NotFound();

        // --- CORREÇÃO: Usar o método helper padronizado ---
        var resultado = GetMotoResource(moto);

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
    [SwaggerRequestExample(typeof(MotoModel), typeof(MotoExample))]
    public IActionResult Post([FromBody] MotoModel moto)
    {
        if (string.IsNullOrWhiteSpace(moto.tipoMoto) || string.IsNullOrWhiteSpace(moto.placa))
            return BadRequest("Tipo da moto e placa são obrigatórios.");

        var criada = motoService.CadastrarMoto(moto);

        // --- CORREÇÃO: Usar o método helper padronizado ---
        var resultado = GetMotoResource(criada);

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
    [SwaggerRequestExample(typeof(MotoModel), typeof(MotoExample))]
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

