using Microsoft.Extensions.DependencyInjection;
using System;

namespace GloballyPaid
{
    class Program
    {
        private static ServiceProvider _serviceProvider;

        private static ITokenService _tokenService;
        private static IChargeService _chargeService;
        private static ICaptureService _captureService;
        private static IRefundService _refundService;
        private static ICustomerService _customerService;
        private static IPaymentInstrumentService _paymentInstrumentService;

        static void Main(string[] args)
        {
            RegisterServices();
            GetServices();

            //authorization charge transaction
            var tokenizeRequest = new TokenizeRequest
            {
                PaymentInstrument = GetPaymentInstrumentRequest()
            };

            var tokenAuth = _tokenService.Tokenize(tokenizeRequest);
            var chargeAuth = _chargeService.Charge(new ChargeRequest
            {
                Source = tokenAuth.Id,
                Amount = 99,
                ClientCustomerId = "0000000",
                CofType = CofType.UNSCHEDULED_CARDHOLDER,
                CurrencyCode = CurrencyCode.USD,
                CountryCode = CountryCode.US,
                ClientTransactionId = "000000000",
                ClientTransactionDescription = "Description",
                ClientInvoiceId = "000000",
                Capture = false,
                CVV = "999"
            }, new RequestOptions(requestTimeoutSeconds: 50));

            var captureAuth = _captureService.Capture(new CaptureRequest
            {
                Amount = chargeAuth.Amount,
                Charge = chargeAuth.Id
            });

            var refundAuth = _refundService.Refund(new RefundRequest
            {
                Amount = chargeAuth.Amount,
                Charge = chargeAuth.Id
            });

            //sale charge transaction
            var tokenSale = _tokenService.Tokenize(tokenizeRequest);
            var chargeSale = _chargeService.Charge(new ChargeRequest
            {
                Source = tokenSale.Id,
                Amount = 99,
                ClientCustomerId = "0000000",
                CofType = CofType.UNSCHEDULED_CARDHOLDER,
                CurrencyCode = CurrencyCode.USD,
                CountryCode = CountryCode.US,
                ClientTransactionId = "000000000",
                ClientTransactionDescription = "ChargeWithToken new HMAC",
                ClientInvoiceId = "000000",
                AVS = false,
                CVV = "999"
            });
            var refundSale = _refundService.Refund(new RefundRequest
            {
                Amount = chargeSale.Amount,
                Charge = chargeSale.Id
            });

            //sale charge transaction, with saved payment instrument
            var tokenPaymentInstrument = _tokenService.Tokenize(tokenizeRequest);
            var chargePaymentInstrument = _chargeService.Charge(new ChargeRequest
            {
                Source = tokenPaymentInstrument.Id,
                Amount = 99,
                ClientCustomerId = "0000000",
                CofType = CofType.UNSCHEDULED_CARDHOLDER,
                CurrencyCode = CurrencyCode.USD,
                CountryCode = CountryCode.US,
                ClientTransactionId = "000000000",
                ClientTransactionDescription = "ChargeWithToken new HMAC",
                ClientInvoiceId = "000000",
                AVS = false,
                SavePaymentInstrument = true,
                CVV = "999"
            });

            var refundPaymentInstrument = _refundService.Refund(new RefundRequest
            {
                Amount = chargePaymentInstrument.Amount,
                Charge = chargePaymentInstrument.Id
            });

            //tokenize & sale charge transaction
            var charge = _chargeService.Charge(GetPaymentInstrumentRequest(), 99);
            var refundCharge = _refundService.Refund(new RefundRequest
            {
                Amount = charge.Amount,
                Charge = charge.Id
            });

            //customers CRUD
            var customers = _customerService.List();
            var customerSuffix = new Random().Next(100000, 999999);
            var customer = _customerService.Create(new Customer
            {
                ClientCustomerId = "0000000",
                FirstName = "Jane",
                LastName = "Doe",
                Address = new Address
                {
                    Line1 = "Address Line 1",
                    City = "CIty",
                    State = "State",
                    PostalCode = "00000",
                    Country = "Country"
                },
                Email = $"jane.doe-{customerSuffix}@example.com",
                Phone = "0000000000"
            });

            customer = _customerService.Get(customer.Id);

            customer.LastName = "Smith";
            _customerService.Update(customer);
            customer = _customerService.Get(customer.Id);

            _customerService.Delete(customer.Id);
            customers = _customerService.List();

            //payment instrument CRUD
            var customerSuffixForPI = new Random().Next(100000, 999999);
            var customerForPaymentInstrument = _customerService.Create(new Customer
            {
                ClientCustomerId = "0000000",
                FirstName = "Jane",
                LastName = "Doe",
                Address = new Address
                {
                    Line1 = "Address Line 1",
                    City = "CIty",
                    State = "State",
                    PostalCode = "00000",
                    Country = "Country"
                },
                Email = $"jane.doe-{customerSuffixForPI}@example.com",
                Phone = "0000000000"
            });

            var paymentInstrument = _paymentInstrumentService.Create("41111111111111", "999", new PaymentInstrument
            {
                Type = PaymentType.CreditCard,
                CustomerId = customerForPaymentInstrument.Id,
                ClientCustomerId = "0000000",
                Brand = "Visa",
                Expiration = "0725",
                LastFour = "1111",
                BillingContact = new Contact
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Address = new Address
                    {
                        Line1 = "Address Line 1",
                        City = "CIty",
                        State = "State",
                        PostalCode = "00000",
                        Country = "Country"
                    },
                    Email = "jane.doe@example.com",
                    Phone = "0000000000"
                }
            });

            var paymentInstruments = _paymentInstrumentService.List(customerForPaymentInstrument.Id);

            //first PaymentInstrument with alias
            var paymentInstrumentAliasId = paymentInstruments[0].Id;
            paymentInstrument = _paymentInstrumentService.Get(paymentInstrumentAliasId);

            paymentInstrument.BillingContact.LastName = "Smith";
            paymentInstrument = _paymentInstrumentService.Update(paymentInstrument);
            paymentInstrument = _paymentInstrumentService.Get(paymentInstrument.Id);

            _paymentInstrumentService.Delete(paymentInstrumentAliasId);
            paymentInstruments = _paymentInstrumentService.List(customerForPaymentInstrument.Id);

            DisposeServices();
        }

        private static PaymentInstrumentRequest GetPaymentInstrumentRequest()
        {
            return new PaymentInstrumentRequest
            {
                Type = PaymentType.CreditCard,
                CreditCard = new CreditCard
                {
                    Number = "4111111111111111",
                    Expiration = "1225",
                    Cvv = "999"
                },
                BillingContact = new Contact
                {
                    FirstName = "Test",
                    LastName = "Tester",
                    Address = new Address
                    {
                        Line1 = "Address Line 1",
                        City = "CIty",
                        State = "State",
                        PostalCode = "00000",
                        Country = "Country"
                    },
                    Phone = "000-000-0000",
                    Email = "test@test.com"
                }
            };
        }

        private static void RegisterServices()
        {
            var services = new ServiceCollection();

            services.AddGloballyPaidServices();
            _serviceProvider = services.BuildServiceProvider();
        }

        private static void GetServices()
        {
            _tokenService = _serviceProvider.GetService<ITokenService>();
            _chargeService = _serviceProvider.GetService<IChargeService>();
            _captureService = _serviceProvider.GetService<ICaptureService>();
            _refundService = _serviceProvider.GetService<IRefundService>();
            _customerService = _serviceProvider.GetService<ICustomerService>();
            _paymentInstrumentService = _serviceProvider.GetService<IPaymentInstrumentService>();
        }

        private static void DisposeServices()
        {
            if (_serviceProvider == null)
            {
                return;
            }
            if (_serviceProvider is IDisposable)
            {
                ((IDisposable)_serviceProvider).Dispose();
            }
        }
    }
}