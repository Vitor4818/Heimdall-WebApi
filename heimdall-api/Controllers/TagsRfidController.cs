using Microsoft.AspNetCore.Mvc;
using HeimdallModel;
using HeimdallBusiness;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;
using HeimdallApi.SwaggerExamples.TagRfid;
using Swashbuckle.AspNetCore.Filters;

namespace HeimdallApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagRfidController : ControllerBase
    {
        private readonly TagRfidService tagService;
        private readonly LinkGenerator linkGenerator;

        public TagRfidController(TagRfidService tagService, LinkGenerator linkGenerator)
        {
            this.tagService = tagService;
            this.linkGenerator = linkGenerator;
        }

        private object GetTagRfidResource(TagRfidModel tag)
        {
            return new
            {
                tag.Id,
                tag.MotoId,
                tag.FaixaFrequencia,
                tag.Banda,
                tag.Aplicacao,
                links = new
                {
                    self = linkGenerator.GetPathByAction(HttpContext, nameof(GetById), "TagRfid", new { id = tag.Id }),
                    update = linkGenerator.GetPathByAction(HttpContext, nameof(Put), "TagRfid", new { id = tag.Id }),
                    delete = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), "TagRfid", new { id = tag.Id }),
                    all = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "TagRfid")
                }
            };
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(Summary = "Obtém todas as tags RFID", Description = "Retorna uma lista de todas as tags RFID cadastradas com paginação.")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TagRfidListResponseExample))]

    public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
{
    if (page <= 0) page = 1;
    if (pageSize <= 0) pageSize = 10;

    var query = tagService.ListarTags();

    var totalItems = await query.CountAsync();
    if (totalItems == 0)
        return NoContent();

    var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

    var tagsPage = await query
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    var tagLinks = tagsPage.Select(t => GetTagRfidResource(t)).ToList();

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
        Items = tagLinks
    };

    return Ok(result);
}


        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Obtém uma tag RFID pelo ID", Description = "Retorna uma tag RFID específica, caso exista.")]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(TagRfidGetByIdResponseExample))]

        public IActionResult GetById(int id)
        {
            var tag = tagService.ObterPorId(id);
            return tag == null ? NotFound() : Ok(GetTagRfidResource(tag));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Cadastra uma nova tag RFID", Description = "Cadastra uma nova tag RFID no sistema.")]
        [SwaggerRequestExample(typeof(TagRfidModel), typeof(TagRfidPostExample))]
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
            var resource = GetTagRfidResource(criada);

            return CreatedAtAction(nameof(GetById), new { id = criada.Id }, resource);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Atualiza uma tag RFID existente", Description = "Atualiza os dados de uma tag RFID já cadastrada.")]
        [SwaggerRequestExample(typeof(TagRfidModel), typeof(TagRfidPostExample))] // usa o mesmo exemplo do POST

        public IActionResult Put(int id, [FromBody] TagRfidModel tag)
        {
            if (tag == null || tag.Id != id)
                return BadRequest("Dados inconsistentes.");

            return tagService.AtualizarTag(tag) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Remove uma tag RFID", Description = "Remove uma tag RFID do sistema com base no ID.")]
        public IActionResult Delete(int id)
        {
            return tagService.RemoverTag(id) ? NoContent() : NotFound();
        }
    }
}
