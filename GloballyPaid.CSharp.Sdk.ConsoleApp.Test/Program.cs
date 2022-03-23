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
                Source = new PaymentSourceCardOnFile()
                {
                    Type = PaymentSourceType.CARD_ON_FILE,
                    CardOnFile = new  CardOnFile()
                    {
                        Id =  tokenAuth.Id,
                        CVV = "" // should provide when user attended
                    }
                },
                Params = new TransactionParameters()
                {
                    Amount = 99,
                    Capture = false,
                    CofType = CofType.UNSCHEDULED_CARDHOLDER,
                    CurrencyCode = CurrencyCode.USD,
                    CountryCode = ISO3166CountryCode.USA,
                },
                Meta = new TransactionMeta()
                {
                    ClientCustomerID = "0000000",
                    ClientTransactionID = "000000000",
                    ClientTransactionDescription = "Description",
                    ClientInvoiceID = "000000",
                }

            }, new RequestOptions(requestTimeoutSeconds: 50));

            var captureAuth = _captureService.Capture(new CaptureRequest
            {
                Amount = chargeAuth.Amount,
                Charge = chargeAuth.ID
            });

            var refundAuth = _refundService.Refund(new RefundRequest
            {
                Amount = chargeAuth.Amount,
                Charge = chargeAuth.ID
            });

            //sale charge transaction
            var tokenSale = _tokenService.Tokenize(tokenizeRequest);
            var chargeSale = _chargeService.Charge(new ChargeRequest
            {
                Source = new PaymentSourceCardOnFile()
                {
                    Type = PaymentSourceType.CARD_ON_FILE,
                    CardOnFile = new  CardOnFile()
                    {
                        Id =  tokenSale.Id,
                        CVV = "999" // should provide when user attended
                    }
                },
                Params = new TransactionParameters()
                {
                    Amount = 99,
                    Capture = true,
                    CofType = CofType.UNSCHEDULED_CARDHOLDER,
                    CurrencyCode = CurrencyCode.USD,
                    CountryCode = ISO3166CountryCode.USA,
                },
                Meta = new TransactionMeta()
                {
                    ClientCustomerID = "0000000",
                    ClientTransactionID = "000000000",
                    ClientTransactionDescription = "Description",
                    ClientInvoiceID = "000000",
                }

            });
            
            
            var refundSale = _refundService.Refund(new RefundRequest
            {
                Amount = chargeSale.Amount,
                Charge = chargeSale.ID
            });

            //sale charge transaction, with saved payment instrument
            var tokenPaymentInstrument = _tokenService.Tokenize(tokenizeRequest);
            var chargePaymentInstrument = _chargeService.Charge(new ChargeRequest
            {
                Source = new PaymentSourceCardOnFile()
                {
                    Type = PaymentSourceType.CARD_ON_FILE,
                    CardOnFile = new CardOnFile()
                    {
                        Id = tokenPaymentInstrument.Id,
                        CVV = "999"
                    }
                } ,
                Params = new TransactionParameters()
                {
                    Amount = 99,    
                    CofType = CofType.UNSCHEDULED_CARDHOLDER,
                    CurrencyCode = CurrencyCode.USD,
                    CountryCode = ISO3166CountryCode.USA,
                    SavePaymentInstrument = true // a permanent PaymentInstrument will be returned in the response
                },
                Meta = new TransactionMeta()
                {
                    ClientCustomerID = "0000000",
                    ClientTransactionID = "000000000",
                    ClientTransactionDescription = "ChargeWithToken new HMAC",
                    ClientInvoiceID = "000000",    
                }
            });

            var refundPaymentInstrument = _refundService.Refund(new RefundRequest
            {
                Amount = chargePaymentInstrument.Amount,
                Charge = chargePaymentInstrument.ID
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
                    CountryCode = ISO3166CountryCode.USA 
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
                    CountryCode = ISO3166CountryCode.USA
                },
                Email = $"jane.doe-{customerSuffixForPI}@example.com",
                Phone = "0000000000"
            });

            var paymentInstrument = _paymentInstrumentService.Create(new PaymentInstrumentRequest()
            {
                Type = PaymentType.CreditCard,
                CreditCard = new CreditCard()
                {
                    Number = "4111111111111111",
                    Expiration = "0725"
                    
                },
                CustomerId = customerForPaymentInstrument.Id,
                ClientCustomerId = "0000000",
                BillingContact = new BillingContact()
                {
                    FirstName = "Jane",
                    LastName = "Doe",
                    Address = new Address
                    {
                        Line1 = "Address Line 1",
                        City = "CIty",
                        State = "State",
                        PostalCode = "00000",
                        CountryCode = ISO3166CountryCode.USA
                    },
                    Email = "jane.doe@example.com",
                    Phone = "0000000000"
                }
            });

            var paymentInstruments = _paymentInstrumentService.List(customerForPaymentInstrument.Id);

            //first PaymentInstrument with alias
            var paymentInstrumentAliasId = paymentInstruments[0].Id;
            paymentInstrument = _paymentInstrumentService.Get(paymentInstrumentAliasId, customerForPaymentInstrument.Id);

            paymentInstrument.BillingContact.LastName = "Smith";
            paymentInstrument = _paymentInstrumentService.Update(paymentInstrument);
            paymentInstrument = _paymentInstrumentService.Get(paymentInstrument.Id, customerForPaymentInstrument.Id);

            _paymentInstrumentService.Delete(paymentInstrumentAliasId, customerForPaymentInstrument.Id);
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
                    CVV = "999"
                },
                BillingContact = new Contact
                {
                    FirstName = "Test",
                    LastName = "Tester",
                    Address = new Address
                    {
                        Line1 = "Address Line 1",
                        City = "Los Angeles",
                        State = "CA",
                        PostalCode = "00000",
                        CountryCode = ISO3166CountryCode.USA
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