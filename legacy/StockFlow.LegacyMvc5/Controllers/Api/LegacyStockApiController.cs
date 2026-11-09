using System;
using System.Net;
using System.Web.Http;
using StockFlow.LegacyMvc5.Data;
using StockFlow.LegacyMvc5.Models;

namespace StockFlow.LegacyMvc5.Controllers.Api
{
    /// <summary>
    /// Legacy ASP.NET Web API 2 Controller on .NET Framework 4.8.
    /// Demonstrates traditional IHttpActionResult, ApiController lifecycle, and HTTP status code mappings.
    /// </summary>
    [RoutePrefix("api/legacystock")]
    public class LegacyStockApiController : ApiController
    {
        private readonly ILegacyStockRepository _repository;

        public LegacyStockApiController() : this(new LegacyAdoNetStockRepository())
        {
        }

        public LegacyStockApiController(ILegacyStockRepository repository)
        {
            _repository = repository;
        }

        // GET: api/legacystock/{id}
        [HttpGet]
        [Route("{id:guid}")]
        public IHttpActionResult Get(Guid id)
        {
            var product = _repository.GetById(id);
            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        // POST: api/legacystock/adjust
        [HttpPost]
        [Route("adjust")]
        public IHttpActionResult Adjust([FromBody] LegacyStockAdjustmentRequest request)
        {
            if (request == null || !ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (request.Delta == 0)
            {
                return BadRequest("Adjustment delta cannot be zero.");
            }

            int newQuantity;
            string errorMessage;
            bool success = _repository.AdjustStock(request.ProductId, request.Delta, out newQuantity, out errorMessage);

            if (!success)
            {
                if (errorMessage == "product not found")
                {
                    return NotFound();
                }

                if (errorMessage == "stock would go negative")
                {
                    return Content(HttpStatusCode.Conflict, new LegacyStockAdjustmentResponse
                    {
                        ProductId = request.ProductId,
                        NewQuantity = newQuantity,
                        Delta = request.Delta,
                        Message = "stock would go negative"
                    });
                }

                return Content(HttpStatusCode.InternalServerError, new { message = errorMessage });
            }

            var response = new LegacyStockAdjustmentResponse
            {
                ProductId = request.ProductId,
                NewQuantity = newQuantity,
                Delta = request.Delta,
                Message = "Stock adjusted successfully."
            };

            return Ok(response);
        }
    }
}
