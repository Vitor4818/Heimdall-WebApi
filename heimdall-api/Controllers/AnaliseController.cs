using Microsoft.AspNetCore.Mvc;
using HeimdallModel;
using HeimdallBusiness;
using Microsoft.AspNetCore.Authorization;
using HeimdallModel.DTOs; 
using Swashbuckle.AspNetCore.Annotations;

namespace HeimdallApi.Controllers
{
    [ApiController]
    [ApiVersion("1.0")] 
    [Route("api/v{version:apiVersion}/analise")] 
    [Authorize] 
    public class AnaliseController : ControllerBase
    {
        private readonly PredictionService _predictionService;

        public AnaliseController(PredictionService predictionService)
        {
            _predictionService = predictionService;
        }

        [HttpPost("prever-revisao")]
        [ProducesResponseType(typeof(MotoRevisaoPrediction), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [SwaggerOperation(Summary = "(ML.NET) Prevê a necessidade de revisão da moto")]
        public IActionResult PreverRevisao([FromBody] AnaliseRevisaoDto dto)
        {
            if (dto.KmRodados < 0)
            {
                return BadRequest("KmRodados não pode ser negativo.");
            }

            var previsao = _predictionService.PreverRevisao(dto.KmRodados);

            return Ok(previsao);
        }
    }
}

