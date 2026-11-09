using System;
using System.Web.Mvc;
using StockFlow.LegacyMvc5.Data;
using StockFlow.LegacyMvc5.Models;

namespace StockFlow.LegacyMvc5.Controllers
{
    /// <summary>
    /// Legacy ASP.NET MVC 5 Controller serving Razor views on .NET Framework 4.8.
    /// Uses traditional ViewData/ViewBag, ModelState validation, and Synchronous ActionResults.
    /// </summary>
    public class StockController : Controller
    {
        private readonly ILegacyStockRepository _repository;

        public StockController() : this(new LegacyAdoNetStockRepository())
        {
        }

        public StockController(ILegacyStockRepository repository)
        {
            _repository = repository;
        }

        // GET: /Stock
        [HttpGet]
        public ActionResult Index()
        {
            ViewBag.FrameworkVersion = ".NET Framework 4.8 / ASP.NET MVC 5";
            ViewBag.ServerTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

            var products = _repository.GetAll();
            return View(products);
        }

        // POST: /Stock/Adjust
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Adjust(LegacyStockAdjustmentRequest request)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Validation failed: Please enter a valid product and delta.";
                return View("Index", _repository.GetAll());
            }

            int newQty;
            string errorMessage;
            bool success = _repository.AdjustStock(request.ProductId, request.Delta, out newQty, out errorMessage);

            if (!success)
            {
                ViewBag.ErrorMessage = errorMessage;
                return View("Index", _repository.GetAll());
            }

            TempData["SuccessMessage"] = string.Format("Product stock adjusted successfully to {0}.", newQty);
            return RedirectToAction("Index");
        }
    }
}
