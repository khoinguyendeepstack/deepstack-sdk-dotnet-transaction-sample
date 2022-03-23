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
                Source = new PaymentSourceCardOnFile()
                {
                    Type = PaymentSourceType.CARD_ON_FILE,
                    CardOnFile = new CardOnFile()
                    {
                        Id = chargeRequest.Source,
                        CVV = chargeRequest.CVV // should be provided when the transaction is user attended
                    }
                },
                Params = new TransactionParameters()
                {
                    Amount = chargeRequest.Amount,
                    Capture = true, //sale charge
                    CofType = CofType.UNSCHEDULED_CARDHOLDER,
                    CurrencyCode = CurrencyCode.USD,
                    CountryCode = ISO3166CountryCode.USA,
                    SavePaymentInstrument = true
                },
                Meta = new TransactionMeta(){
                    ClientCustomerID = "12345", //set your customer id
                    ClientInvoiceID = "IX213", //set your invoice id
                    ClientTransactionDescription = "E-comm order", // any useful description
                    ClientTransactionID = "000111222333"
                }
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
