using System;

namespace PetrotecRemotePurchaseTerminalIntegration.Lib.Models
{
    public class PurchaseResult
    {
        public string TransactionId { get; set; }
        public decimal Amount { get; set; }
        public string OriginalPosIdentification { get; set; }
        public DateTime OriginalReceiptData { get; set; }
        public string ReceiptData { get; set; }
    }
}
