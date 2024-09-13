using System.ComponentModel;

namespace PetrotecRemotePurchaseTerminalIntegration.Lib
{
    public class Enums
    {
        public enum TerminalCommandOptions
        {
            [Description("Send terminal open period request")]
            SendTerminalOpenPeriod = 1,
            [Description("Send terminal close period request")]
            SendTerminalClosePeriod = 2,
            [Description("Send terminal purchase request")]
            SendProcessPaymentRequest = 3,
            [Description("Send terminal refund request")]
            SendProcessRefundRequest = 4,
            [Description("Show list of commands")]
            ShowListOfCommands = 9998,
            [Description("Stop listening")]
            StopTheServer = 9999
        }
    }
}
