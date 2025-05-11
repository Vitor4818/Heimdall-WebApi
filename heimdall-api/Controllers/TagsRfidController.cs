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
    /// <returns>Uma lista de tags RFID.</returns>
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [SwaggerOperation(Summary = "Obtém todas as tags RFID", Description = "Retorna uma lista de todas as tags RFID cadastradas.")]
    public IActionResult Get()
    {
        var tags = tagService.ListarTags();
        return tags.Count == 0 ? NoContent() : Ok(tags);
    }

    /// <summary>
    /// Obtém uma tag RFID específica pelo ID.
    /// </summary>
    /// <param name="id">ID da tag RFID</param>
    /// <returns>Tag RFID com o ID fornecido.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Obtém uma tag RFID pelo ID", Description = "Retorna uma tag RFID específica, caso exista.")]
    public IActionResult Get(int id)
    {
        var tag = tagService.ObterPorId(id);
        return tag == null ? NotFound() : Ok(tag);
    }

    /// <summary>
    /// Cadastra uma nova tag RFID.
    /// </summary>
    /// <param name="tag">Dados da tag RFID a ser cadastrada.</param>
    /// <returns>Detalhes da tag criada.</returns>
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
        return CreatedAtAction(nameof(Get), new { id = criada.Id }, criada);
    }

    /// <summary>
    /// Atualiza os dados de uma tag RFID existente.
    /// </summary>
    /// <param name="id">ID da tag RFID a ser atualizada.</param>
    /// <param name="tag">Dados atualizados da tag RFID.</param>
    /// <returns>Resposta de sucesso ou falha.</returns>
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
    /// <param name="id">ID da tag RFID a ser removida.</param>
    /// <returns>Resposta de sucesso ou falha.</returns>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [SwaggerOperation(Summary = "Remove uma tag RFID", Description = "Remove uma tag RFID do sistema com base no ID.")]
    public IActionResult Delete(int id)
    {
        return tagService.RemoverTag(id) ? NoContent() : NotFound();
    }
}
