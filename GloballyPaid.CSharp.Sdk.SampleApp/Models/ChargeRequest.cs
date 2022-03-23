namespace GloballyPaid.CSharp.Sdk.SampleApp.Models
{
    public class SampleChargeRequest
    {
        public string Source { get; set; }

        public int Amount { get; set; }

        public string CVV { get; set; }
    }
}
