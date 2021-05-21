using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GloballyPaid.CSharp.Sdk.SampleApp.Models;

namespace GloballyPaid.CSharp.Sdk.SampleApp.Controllers
{
    public class SampleController : Controller
    {
        private readonly ILogger<SampleController> _logger;
        private readonly IChargeService _chargeService;

        public SampleController(IChargeService chargeService, ILogger<SampleController> logger)
        {
            _chargeService = chargeService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Charge([FromBody] SampleChargeRequest chargeRequest)
        {
            var request = new ChargeRequest
            {
                Source = chargeRequest.Source, //this can be the token or payment instrument identifier
                Amount = chargeRequest.Amount,
                Capture = true, //sale charge
                ClientCustomerId = "12345", //set your customer id
                ClientInvoiceId = "IX213", //set your invoice id
                ClientTransactionDescription = "Tuition for CS",
                CVV = chargeRequest.CVV,
                CofType = CofType.UNSCHEDULED_CARDHOLDER
            };

            try
            {
                var charge = _chargeService.Charge(request);
                return Ok(charge);

            }
            catch (System.Exception ex)
            {
                var exx = ex;
                throw;
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
