using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Transactions;
using Petrotec.Lib.PetrotecEps;
using Petrotec.Lib.PetrotecEps.COM_Classes;
using Petrotec.Lib.PetrotecEps.COM_Enums;
using Petrotec.Lib.PetrotecEps.COM_Interfaces;
using PetrotecRemotePurchaseTerminalIntegration.Lib.Models;

namespace PetrotecRemotePurchaseTerminalIntegration.Lib
{
    public class PetrotecRemote
    {
        #region "Constants"

        private const int _okStatus = 0;

        private const string _OperationDateFormat = "yyyyMMdd";
        private const string _OperationTimeFormat = "HHmmss";

        private const string _patternIdentTpa = @"Ident\. TPA:\s*(\d+)\s*(\d{2}-\d{2}-\d{2})\s*(\d{2}:\d{2}:\d{2})";
        private const string _dateTimeFormat = "yy-MM-dd HH:mm:ss";

        #endregion

        #region "Members"

        private readonly string terminalAddress;
        private readonly string localSystemAddress;
        private readonly ManualResetEvent serviceResponseEventReceived = new ManualResetEvent(false);
        private readonly ManualResetEvent cardServiceResponseEventReceived = new ManualResetEvent(false);
        private IServiceResponse serviceResponseEventReceivedResponse;
        private ICardServiceResponseEventArgs cardServiceResponseEventReceivedResponse;

        #endregion

        #region "Constructors"

        public PetrotecRemote(string terminalAddress, string localSystemAddress)
        {
            this.terminalAddress = terminalAddress;
            this.localSystemAddress = localSystemAddress;
        }

        #endregion

        /// <summary>
        /// Terminal status.
        /// </summary>
        public Result TerminalStatus()
        {
            var success = true;
            var message = string.Empty;

            try
            {
                using (var _clientEPS = new EpsClient())
                {
                    _clientEPS.OnServiceResponse += _clientEPS_OnServiceResponse;

                    _clientEPS.Initialize(new ConnectionConfiguration
                    {
                        TerminalAddress = terminalAddress,
                        LocalSystemAddress = localSystemAddress,
                    });

                    var requestStartInfo = _clientEPS.ExecuteServiceRequest(new ServiceRequest { RequestType = ServiceRequestType.SIBSCheckPeriodState }, 120);

                    if (requestStartInfo.ErrorCode != 0)
                    {
                        success = false;
                        message = $"ErrorCode: {requestStartInfo.ErrorCode}. ErrorMessage: {requestStartInfo.ErrorMessage}. RequestID: {requestStartInfo.RequestId}.";
                    }
                    else
                    {
                        WaitForEvent(serviceResponseEventReceived);
                        success = serviceResponseEventReceivedResponse.ErrorCode == _okStatus;
                        message = serviceResponseEventReceivedResponse.ErrorMessage;
                    }

                    _clientEPS.Terminate();
                }
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            return new Result { Success = success, Message = message };
        }

        /// <summary>
        /// Opens the period.
        /// </summary>
        public Result OpenPeriod()
        {
            var success = true;
            var message = string.Empty;

            try
            {
                using (var _clientEPS = new EpsClient())
                {
                    _clientEPS.OnServiceResponse += _clientEPS_OnServiceResponse;

                    _clientEPS.Initialize(new ConnectionConfiguration
                    {
                        TerminalAddress = terminalAddress,
                        LocalSystemAddress = localSystemAddress,
                    });

                    var requestStartInfo = _clientEPS.ExecuteServiceRequest(new ServiceRequest { RequestType = ServiceRequestType.SIBSOpenAccountingPeriod }, 120);

                    if (requestStartInfo.ErrorCode != 0)
                    {
                        success = false;
                        message = $"ErrorCode: {requestStartInfo.ErrorCode}. ErrorMessage: {requestStartInfo.ErrorMessage}. RequestID: {requestStartInfo.RequestId}.";
                    }
                    else
                    {
                        WaitForEvent(serviceResponseEventReceived);
                        success = serviceResponseEventReceivedResponse.ErrorCode == _okStatus;
                        message = serviceResponseEventReceivedResponse.ErrorMessage;
                    }

                    _clientEPS.Terminate();
                }
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            return new Result { Success = success, Message = message };
        }

        /// <summary>
        /// Closes the period.
        /// </summary>
        public Result ClosePeriod()
        {
            var success = true;
            var message = string.Empty;

            try
            {
                using (var _clientEPS = new EpsClient())
                {
                    _clientEPS.OnServiceResponse += _clientEPS_OnServiceResponse;

                    _clientEPS.Initialize(new ConnectionConfiguration
                    {
                        TerminalAddress = terminalAddress,
                        LocalSystemAddress = localSystemAddress,
                    });

                    var requestStartInfo = _clientEPS.ExecuteServiceRequest(new ServiceRequest { RequestType = ServiceRequestType.SIBSCloseAccountingPeriod }, 120);

                    if (requestStartInfo.ErrorCode != 0)
                    {
                        success = false;
                        message = $"ErrorCode: {requestStartInfo.ErrorCode}. ErrorMessage: {requestStartInfo.ErrorMessage}. RequestID: {requestStartInfo.RequestId}.";
                    }
                    else
                    {
                        WaitForEvent(serviceResponseEventReceived);
                        success = serviceResponseEventReceivedResponse.ErrorCode == _okStatus;
                        message = serviceResponseEventReceivedResponse.ErrorMessage;
                    }

                    _clientEPS.Terminate();
                }
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            return new Result { Success = success, Message = message };
        }

        /// <summary>
        /// Purchases the specified transaction identifier and amount.
        /// </summary>
        /// <param name="amount">The amount.</param>
        /// <param name="printLocal">Print local.</param>
        public Result Purchase(string amount, bool printLocal)
        {
            var success = true;
            var message = string.Empty;
            var purchaseResult = new PurchaseResult();

            try
            {
                using (var _clientEPS = new EpsClient())
                {
                    _clientEPS.OnCardServiceResponse += _clientEPS_OnCardServiceResponse;

                    _clientEPS.Initialize(new ConnectionConfiguration
                    {
                        TerminalAddress = terminalAddress,
                        LocalSystemAddress = localSystemAddress,
                    });

                    var requestStartInfo = _clientEPS.CardServiceRequestSIBSPurchase(new PurchaseData
                    {
                        PrintLocal = printLocal,
                        TotalAmount = new TotalAmount
                        {
                            Value = Convert.ToDecimal(amount, CultureInfo.InvariantCulture)
                        }
                    }, 120);

                    if (requestStartInfo.ErrorCode != 0)
                    {
                        success = false;
                        message = $"ErrorCode: {requestStartInfo.ErrorCode}. ErrorMessage: {requestStartInfo.ErrorMessage}. RequestID: {requestStartInfo.RequestId}.";
                    }
                    else
                    {
                        WaitForEvent(cardServiceResponseEventReceived);

                        if (cardServiceResponseEventReceivedResponse.ErrorCode == _okStatus)
                        {
                            purchaseResult.TransactionId = cardServiceResponseEventReceivedResponse.FepTransactionData.TerminalId;
                            purchaseResult.Amount = Convert.ToDecimal(amount, CultureInfo.InvariantCulture);

                            // Match Ident. TPA for terminal ID, date, and time:
                            var matchIdentTpa = Regex.Match(cardServiceResponseEventReceivedResponse.FepTransactionData.TextForClientReceipt, _patternIdentTpa);
                            if (matchIdentTpa.Success)
                            {
                                purchaseResult.OriginalPosIdentification = matchIdentTpa.Groups[1].Value;

                                DateTime.TryParseExact(
                                    matchIdentTpa.Groups[2].Value + " " + matchIdentTpa.Groups[3].Value,
                                    _dateTimeFormat,
                                    CultureInfo.InvariantCulture,
                                    DateTimeStyles.None,
                                    out DateTime originalReceiptDataParsed
                                );

                                purchaseResult.OriginalReceiptData = originalReceiptDataParsed;
                                purchaseResult.ReceiptData = cardServiceResponseEventReceivedResponse.FepTransactionData.TextForClientReceipt.Substring(29);
                            }
                            else
                            {
                                // Receipt is being printed on the terminal
                                //purchaseResult.OriginalPosIdentification = originalPosIdentification;
                                //purchaseResult.OriginalReceiptData = originalReceiptData;
                            }
                        }

                        success = cardServiceResponseEventReceivedResponse.ErrorCode == _okStatus;
                        message = cardServiceResponseEventReceivedResponse.ErrorMessage;
                    }

                    _clientEPS.Terminate();
                }
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            return new Result { Success = success, Message = message, ExtraData = purchaseResult };
        }

        /// <summary>
        /// The refund.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public Result Refund(PurchaseResult purchaseResult)
        {
            var success = true;
            var message = string.Empty;

            try
            {
                using (var _clientEPS = new EpsClient())
                {
                    _clientEPS.OnCardServiceResponse += _clientEPS_OnCardServiceResponse;

                    _clientEPS.Initialize(new ConnectionConfiguration
                    {
                        TerminalAddress = terminalAddress,
                        LocalSystemAddress = localSystemAddress,
                    });

                    var requestStartInfo = _clientEPS.CardServiceRequestSIBSRefund(new RefundData
                    {
                        Amount = Convert.ToDecimal(purchaseResult.Amount, CultureInfo.InvariantCulture),
                        PosId = purchaseResult.OriginalPosIdentification,
                        OperationDate = purchaseResult.OriginalReceiptData.ToString(_OperationDateFormat),
                        OperationTime = purchaseResult.OriginalReceiptData.ToString(_OperationTimeFormat)
                    }, 120);

                    if (requestStartInfo.ErrorCode != 0)
                    {
                        success = false;
                        message = $"ErrorCode: {requestStartInfo.ErrorCode}. ErrorMessage: {requestStartInfo.ErrorMessage}. RequestID: {requestStartInfo.RequestId}.";
                    }
                    else
                    {
                        WaitForEvent(cardServiceResponseEventReceived);

                        success = cardServiceResponseEventReceivedResponse.ErrorCode == _okStatus;
                        message = cardServiceResponseEventReceivedResponse.ErrorMessage;
                    }

                    _clientEPS.Terminate();
                }
            }
            catch (Exception ex)
            {
                success = false;
                message = ex.Message;
            }

            return new Result { Success = success, Message = message };
        }

        private void _clientEPS_OnServiceResponse(IServiceResponse args)
        {
            serviceResponseEventReceivedResponse = args;
            serviceResponseEventReceived.Set();
        }

        private void _clientEPS_OnCardServiceResponse(ICardServiceResponseEventArgs args)
        {
            cardServiceResponseEventReceivedResponse = args;

            cardServiceResponseEventReceived.Set();
        }

        /// <summary>
        /// Event wait handler
        /// </summary>
        /// <param name="eventHandle">The event handle</param>
        private static void WaitForEvent(ManualResetEvent eventHandle)
        {
            eventHandle.WaitOne();
            eventHandle.Reset();
        }
    }
}