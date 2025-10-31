using Microsoft.AspNetCore.Mvc;
using HeimdallModel;
using HeimdallBusiness;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.EntityFrameworkCore;


namespace HeimdallApi.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class VagaController : ControllerBase
    {
        private readonly VagaService vagaService;

        private readonly LinkGenerator linkGenerator;

        public VagaController(VagaService vagaService, LinkGenerator linkGenerator)
        {
            this.vagaService = vagaService;
            this.linkGenerator = linkGenerator;
        }

        private object GetVagaResources(VagaModel vaga)
        {
            return new
            {
                vaga.Id,
                vaga.Codigo,
                vaga.Ocupada,
                vaga.ZonaId,
                Zona = vaga.Zona != null ? new
                {
                    vaga.Zona.Id,
                    vaga.Zona.Nome,
                    vaga.Zona.Tipo
                } : null,
                Moto = vaga.Moto != null ? new
                {
                    vaga.Moto.id,
                    vaga.Moto.tipoMoto,
                    vaga.Moto.placa,
                    vaga.Moto.numChassi
                } : null,
                links = new
                {
                    self = linkGenerator.GetPathByAction(HttpContext, nameof(GetById), "Vaga", new { id = vaga.Id }),
                    update = linkGenerator.GetPathByAction(HttpContext, nameof(Put), "Vaga", new { id = vaga.Id }),
                    delete = linkGenerator.GetPathByAction(HttpContext, nameof(Delete), "Vaga", new { id = vaga.Id }),
                    all = linkGenerator.GetPathByAction(HttpContext, nameof(Get), "Vaga")
                }
            };
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [SwaggerOperation(Summary = "Obtém todas as vagas", Description = "Retorna uma lista de todas as vagas cadastradas com paginação")]
        // reservado para example
        public async Task<IActionResult> Get([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;

            var query = vagaService.ListarVagas();
            var totalItems = await query.CountAsync();
            if (totalItems == 0)
                return NoContent();

            var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            var vagasPage = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

            var vagaLinks = vagasPage.Select(v => GetVagaResources(v)).ToList();
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
                Items = vagaLinks
            };

            return Ok(result);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Obtém uma Vaga por id", Description = "Retorna uma vaga específica, caso exista.")]
        // reservado para example
        public IActionResult GetById(int id)
        {
            var vaga = vagaService.ObterPorId(id);
            return vaga == null ? NotFound() : Ok(GetVagaResources(vaga));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "Cadastra uma nova vaga", Description = "Cadastra uma nova vaga no sistema.")]
        //Reservado para example 
        public IActionResult Post([FromBody] VagaModel vaga)
        {
            if (string.IsNullOrWhiteSpace(vaga.Codigo))
            {
                return BadRequest("Todos os campos da zona são obrigatórios.");
            }
            var criada = vagaService.CadastrarVaga(vaga);
            var resource = GetVagaResources(criada);
            return CreatedAtAction(nameof(GetById), new { id = criada.Id }, resource);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Atualiza uma vaga existente", Description = "Atualiza os dados de uma vaga já cadastrada.")]
        //Reservado para example
        public IActionResult Put(int id, [FromBody] VagaModel vaga)
        {
            if (vaga == null || vaga.Id != id)
            {
                return BadRequest("Dados inconsistentes.");
            }

            return vagaService.AtualizarVaga(vaga) ? NoContent() : NotFound();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [SwaggerOperation(Summary = "Remove uma vaga", Description = "Remove uma vaga do sistema com base no ID.")]
        public IActionResult Delete(int id)
        {
            return vagaService.RemoverVaga(id) ? NoContent() : NotFound();
        }
    }
}