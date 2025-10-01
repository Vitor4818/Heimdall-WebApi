using Microsoft.AspNetCore.Mvc;
using HeimdallModel;
using HeimdallBusiness;
using Swashbuckle.AspNetCore.Annotations; 

namespace HeimdallApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagRfidController : ControllerBase
{
    private readonly TagRfidService tagService;

    public TagRfidController(TagRfidService tagService)
    {
        this.tagService = tagService;
    }

    /// <summary>
    /// Obtém todas as tags RFID.
    /// </summary>
    [HttpGet]
[ProducesResponseType(StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status204NoContent)]
[SwaggerOperation(Summary = "Obtém todas as tags RFID", Description = "Retorna uma lista de todas as tags RFID cadastradas com paginação.")]
public IActionResult Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
{
    if (page <= 0) page = 1;
    if (pageSize <= 0) pageSize = 10;

    var allTags = tagService.ListarTags();
    if (!allTags.Any()) return NoContent();

    var totalItems = allTags.Count;
    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    var tagsPage = allTags
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .Select(t => new
        {
            t.Id,
            t.MotoId,
            t.FaixaFrequencia,
            t.Banda,
            t.Aplicacao,
            links = new
            {
                self = Url.Action(nameof(Get), new { id = t.Id }),
                update = Url.Action(nameof(Put), new { id = t.Id }),
                delete = Url.Action(nameof(Delete), new { id = t.Id }),
                all = Url.Action(nameof(Get))
            }
        })
        .Cast<object>() // forçando para object
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
        Items = tagsPage
    };

    return Ok(result);
}

    /// <summary>
    /// Obtém uma tag RFID específica pelo ID.
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Obtém uma tag RFID pelo ID", Description = "Retorna uma tag RFID específica, caso exista.")]
    public IActionResult Get(int id)
    {
        var tag = tagService.ObterPorId(id);
        if (tag == null) return NotFound();

        var resultado = new {
            tag.Id,
            tag.MotoId,
            tag.FaixaFrequencia,
            tag.Banda,
            tag.Aplicacao,
            links = new {
                self = Url.Action(nameof(Get), new { id = tag.Id }),
                update = Url.Action(nameof(Put), new { id = tag.Id }),
                delete = Url.Action(nameof(Delete), new { id = tag.Id }),
                all = Url.Action(nameof(Get))
            }
        };

        return Ok(resultado);
    }

    /// <summary>
    /// Cadastra uma nova tag RFID.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [SwaggerOperation(Summary = "Cadastra uma nova tag RFID", Description = "Cadastra uma nova tag RFID no sistema.")]
    public IActionResult Post([FromBody] TagRfidModel tag)
    {
        if (string.IsNullOrWhiteSpace(tag.FaixaFrequencia) ||
            string.IsNullOrWhiteSpace(tag.Banda) ||
            string.IsNullOrWhiteSpace(tag.Aplicacao) ||
            tag.MotoId == 0)
        {
            return BadRequest("Todos os campos da tag são obrigatórios.");
        }

        var criada = tagService.CadastrarTag(tag);

        var resultado = new {
            criada.Id,
            criada.MotoId,
            criada.FaixaFrequencia,
            criada.Banda,
            criada.Aplicacao,
            links = new {
                self = Url.Action(nameof(Get), new { id = criada.Id }),
                update = Url.Action(nameof(Put), new { id = criada.Id }),
                delete = Url.Action(nameof(Delete), new { id = criada.Id }),
                all = Url.Action(nameof(Get))
            }
        };

        return CreatedAtAction(nameof(Get), new { id = criada.Id }, resultado);
    }

    /// <summary>
    /// Atualiza os dados de uma tag RFID existente.
    /// </summary>
    [HttpPut("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Atualiza uma tag RFID existente", Description = "Atualiza os dados de uma tag RFID já cadastrada.")]
    public IActionResult Put(int id, [FromBody] TagRfidModel tag)
    {
        if (tag == null || tag.Id != id)
            return BadRequest("Dados inconsistentes.");

        return tagService.AtualizarTag(tag) ? NoContent() : NotFound();
    }

    /// <summary>
    /// Remove uma tag RFID do sistema.
    /// </summary>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Remove uma tag RFID", Description = "Remove uma tag RFID do sistema com base no ID.")]
    public IActionResult Delete(int id)
    {
        return tagService.RemoverTag(id) ? NoContent() : NotFound();
    }
}
