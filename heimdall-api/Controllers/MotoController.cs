using Microsoft.AspNetCore.Mvc; // Certifique-se de que você está referenciando o nome correto para o serviço
using HeimdallModel;   // E para o modelo
using HeimdallBusiness;

namespace MotosApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MotosController(MotoService motoService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get()
    {
        var motos = motoService.ListarTodas();
        return motos.Count == 0 ? NoContent() : Ok(motos);
    }

    [HttpGet("{id}")]
    public IActionResult Get(int id)
    {
        var moto = motoService.ObterPorId(id);
        return moto == null ? NotFound() : Ok(moto);
    }

    [HttpPost]
    public IActionResult Post([FromBody] MotoModel moto)
    {
        if (string.IsNullOrWhiteSpace(moto.tipoMoto) || string.IsNullOrWhiteSpace(moto.placa))
            return BadRequest("Tipo da moto e placa são obrigatórios.");

        var criada = motoService.Criar(moto);
        return CreatedAtAction(nameof(Get), new { id = criada.id }, criada);
    }

    [HttpPut("{id}")]
    public IActionResult Put(int id, [FromBody] MotoModel moto)
    {
        if (moto == null || moto.id != id)
            return BadRequest("Dados inconsistentes.");

        return motoService.Atualizar(moto) ? NoContent() : NotFound();
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        return motoService.Remover(id) ? NoContent() : NotFound();
    }
}
