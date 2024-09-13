using System;

namespace PetrotecRemotePurchaseTerminalIntegration.Lib.Models
{
    internal class Refund
    {
        public string TransactionId { get; set; }
        public string Amount { get; set; }
        public string OriginalPosIdentification { get; set; }
        public DateTime OriginalReceiptData { get; set; }
        public DateTime OriginalReceiptTime { get; set; }
    }
}
