using System;
using System.Net.Sockets;
using PetrotecRemotePurchaseTerminalIntegration.Lib;
using PetrotecRemotePurchaseTerminalIntegration.Lib.Models;
using static PetrotecRemotePurchaseTerminalIntegration.Lib.Enums;

namespace PetrotecRemotePurchaseTerminalIntegration.Console
{
    internal static class Program
    {
        #region "Constants"

        private const string _MessageTheFollowingCommandsAreAvailable = "The following commands are available:";
        private const string _MessageInvalidInput = "Invalid input";

        #endregion

        #region "Members"

        #region "Home"

        private static readonly string terminalAddress = "http://192.168.40.167:45000";
        private static readonly string localSystemAddress = "http://192.168.40.126:45000";

        #endregion

        #region "Office"

        //private static readonly string terminalAddress = "http://192.168.40.167:45000";
        //private static readonly string localSystemAddress = "http://192.168.40.126:45000";

        #endregion

        private static readonly PetrotecRemote petrotecRemote = new PetrotecRemote(terminalAddress, localSystemAddress);

        #endregion

        static void Main()
        {
            try
            {
                ListenForUserInput();
            }
            catch (Exception e) when (e is ArgumentNullException || e is SocketException)
            {
                System.Console.WriteLine($"{e.GetType().Name}: {e.Message}");
            }
            System.Console.WriteLine("Press any key to exit...");
            System.Console.ReadKey();
        }        

        #region "Private Methods"

        /// <summary>
        /// Listens for user input and sends the input to the WebSocket server.
        /// </summary>
        /// <returns>The task object representing the asynchronous operation.</returns>
        private static void ListenForUserInput()
        {
            var serverIsRunning = true;

            while (serverIsRunning)
            {
                ShowListOfCommands();
                var input = System.Console.ReadLine()?.ToLower();

                if (int.TryParse(input, out int commandValue) && Enum.IsDefined(typeof(TerminalCommandOptions), commandValue))
                {
                    Result result = null;
                    var command = (TerminalCommandOptions)commandValue;

                    switch (command)
                    {
                        case TerminalCommandOptions.SendTerminalOpenPeriod:
                            result = petrotecRemote.OpenPeriod();
                            break;
                        case TerminalCommandOptions.SendTerminalClosePeriod:
                            result = petrotecRemote.ClosePeriod();
                            break;
                        case TerminalCommandOptions.SendProcessPaymentRequest:
                            result = petrotecRemote.Purchase("0.09", false);
                            break;
                        case TerminalCommandOptions.SendProcessRefundRequest:
                            result = petrotecRemote.Refund("0.09", "00069085", DateTime.Now, DateTime.Now);
                            break;
                        case TerminalCommandOptions.ShowListOfCommands:
                            ShowListOfCommands();
                            break;
                        case TerminalCommandOptions.StopTheServer:
                            serverIsRunning = false;
                            break;
                    }

                    System.Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result));
                }
                else
                {
                    System.Console.WriteLine(_MessageInvalidInput);
                    ShowListOfCommands();
                }
            }
        }

        /// <summary>
        /// Shows the list of commands.
        /// </summary>
        private static void ShowListOfCommands()
        {
            System.Console.WriteLine($"\n\n{_MessageTheFollowingCommandsAreAvailable}");
            foreach (TerminalCommandOptions command in Enum.GetValues(typeof(TerminalCommandOptions)))
            {
                System.Console.WriteLine($"   {(int)command} - {Utilities.GetEnumDescription(command)}");
            }
            System.Console.WriteLine();
        }

        #endregion
    }
}