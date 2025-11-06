using Microsoft.AspNetCore.Mvc;
using HeimdallModel;
using HeimdallBusiness;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;



namespace HeimdallApi.Controllers
{

    [ApiController]
    [ApiVersion("1.0")] 
    [Route("api/v{version:apiVersion}/[controller]")] 
    [Authorize]

    public class ZonaController : ControllerBase
    {

        private readonly ZonaService zonaService;

        private readonly LinkGenerator linkGenerator;

        public ZonaController(ZonaService zonaService, LinkGenerator linkGenerator)
        {
            this.zonaService = zonaService;
            this.linkGenerator = linkGenerator;
        }

        private object GetZonaResources(ZonaModel zona)
        {
            return new
            {
                zona.Id,
                zona.Nome,
                zona.Tipo,
                zona.Vagas,
                links = new
                {
                    self = linkGenerator.GetPathByAction(HttpContext, nameof(GetById), "Zona", new { version = "1.0", id = zona.Id }),
                    update = linkGenerator.GetPathByAction(HttpContext, nameof(Put), "Zona", new { version = "1.0", id = zona.Id }),
                    delete = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), "Zona", new { version = "1.0", id = zona.Id }),
                    all = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Zona", new {  version = "1.0"})
                }

            };
        }


        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(Summary = "Obtém todas as zonas", Description = "Retorna uma lista de todas as zonas cadastradas com paginação")]
        // reservado para example
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = zonaService.ListarZonas();
            var totalItems = await query.CountAsync();
            if (totalItems == 0)
                return NoContent();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);

            var zonasPage = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            var zonaLinks = zonasPage.Select(z => GetZonaResources(z)).ToList();

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
                Items = zonaLinks
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Obtém uma zona por id", Description = "Retorna uma zona específica, caso exista.")]
        // reservado para example
        public IActionResult GetById(int id)
        {
            var zona = zonaService.ObterPorId(id);
            return zona == null ? NotFound() : Ok(GetZonaResources(zona));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Cadastra uma nova Zona", Description = "Cadastra uma nova Zona no sistema.")]
        //Reservado para example 
        public IActionResult Post([FromBody] ZonaModel zona)
        {
            if (string.IsNullOrWhiteSpace(zona.Nome) ||
                 string.IsNullOrWhiteSpace(zona.Tipo)
            )
            {
                return BadRequest("Todos os campos da zona são obrigatórios.");
            }
            var criada = zonaService.CadastrarZona(zona);
            var resource = GetZonaResources(criada);
            return CreatedAtAction(nameof(GetById), new { id = criada.Id }, resource);
        }
        
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Atualiza uma zona existente", Description = "Atualiza os dados de uma zona já cadastrada.")]
        //Reservado para example
        public IActionResult Put(int id, [FromBody] ZonaModel zona)
        {
            if (zona == null || zona.Id != id)
            {
                return BadRequest("Dados inconsistentes.");
            }
            return zonaService.AtualizarZona(zona) ? NoContent() : NotFound();
        }
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Remove uma zona", Description = "Remove uma zona do sistema com base no ID.")]
        public IActionResult Delete(int id)
        {
            return zonaService.RemoverZona(id) ? NoContent() : NotFound();
        }


    }


}