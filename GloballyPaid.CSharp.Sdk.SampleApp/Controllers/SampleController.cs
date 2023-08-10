using System.Diagnostics;
using DeepStack.Core;
using DeepStack.Entities.Common;
using DeepStack.Enums;
using DeepStack.Requests;
using DeepStack.Services.v1.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GloballyPaid.CSharp.Sdk.SampleApp.Models;

namespace GloballyPaid.CSharp.Sdk.SampleApp.Controllers
{
    public class SampleController : Controller
    {
        private readonly ILogger<SampleController> _logger;
        private readonly IChargeService _chargeService;
        private readonly ITokenService _tokenService;
        private readonly IRefundService _refundService;
        private readonly ICaptureService _captureService;
        private readonly IPaymentInstrumentService _paymentInstrumentService;
        
        public SampleController(IChargeService chargeService, ITokenService tokenService, ICaptureService captureService, IRefundService refundService, IPaymentInstrumentService paymentInstrumentService, ILogger<SampleController> logger)
        {
            _captureService = captureService;
            _refundService = refundService;
            _tokenService = tokenService;
            _chargeService = chargeService;
            _paymentInstrumentService = paymentInstrumentService;
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Charge([FromBody] BaseTokenResponse chargeRequest)
        {

            var request = new ChargeRequest
            {
                Source = new PaymentSourceCardOnFile()
                {
                    Type = PaymentSourceType.CARD_ON_FILE,
                    CardOnFile = new CardOnFile()
                    {
                        Id = chargeRequest.ID,
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
                    SavePaymentInstrument = false
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
            catch (DeepStackException ex)
            {
                return BadRequest(ex.ErrorMessage);
            }
            catch (System.Exception ex)
            {
                var exx = ex;
                // throw;
                return BadRequest(ex.Message);
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
