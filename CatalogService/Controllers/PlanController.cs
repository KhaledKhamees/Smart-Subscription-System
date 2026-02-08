using CatalogService.Data.Interfaces;
using CatalogService.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanController : ControllerBase
    {
        private readonly IPlanRepository _planRepository;
        public PlanController(IPlanRepository planRepository)
        {
            _planRepository = planRepository;
        }

        [HttpGet("{Id:guid}")]
        public async Task<IActionResult> GetPlans(Guid Id)
        {
            var plan = await _planRepository.GetByIdAsync(Id);
            if (plan == null)
            {
                return NotFound();
            }
            return Ok(plan);
        }
        [HttpGet("product/{productId:guid}")]
        public async Task<IActionResult> GetbyProductId(Guid productId)
        {
            var palns = await _planRepository.GetByProductIdAsync(productId);
            return Ok(palns);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreatePlan([FromBody] SubscriptionPlanDTO plan)
        {
            await _planRepository.AddAsync(plan);
            return Ok();
        }
         
    }
}
