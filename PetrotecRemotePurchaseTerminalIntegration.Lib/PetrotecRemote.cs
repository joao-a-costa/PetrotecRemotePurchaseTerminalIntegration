using System;
using System.Globalization;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
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

        private const string _OperationDateFormat = "yyyyMMdd";
        private const string _OperationTimeFormat = "HHmmss";


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
                        success = serviceResponseEventReceivedResponse.ErrorCode == 0;
                        message = serviceResponseEventReceivedResponse.ToString();
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
                        success = serviceResponseEventReceivedResponse.ErrorCode == 0;
                        message = serviceResponseEventReceivedResponse.ToString();
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
                        success = cardServiceResponseEventReceivedResponse.ErrorCode == 0;
                        message = cardServiceResponseEventReceivedResponse.ToString();
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
        /// The refund.
        /// </summary>
        /// <param name="amount">The amount.</param>
        public Result Refund(string amount, string posId, DateTime operationDate, DateTime operationTime)
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
                        Amount = Convert.ToDecimal(amount, CultureInfo.InvariantCulture),
                        PosId = posId,
                        OperationDate = operationDate.ToString(_OperationDateFormat),
                        OperationTime = operationTime.ToString(_OperationTimeFormat)
                    }, 120);

                    if (requestStartInfo.ErrorCode != 0)
                    {
                        success = false;
                        message = $"ErrorCode: {requestStartInfo.ErrorCode}. ErrorMessage: {requestStartInfo.ErrorMessage}. RequestID: {requestStartInfo.RequestId}.";
                    }
                    else
                    {
                        WaitForEvent(cardServiceResponseEventReceived);
                        success = cardServiceResponseEventReceivedResponse.ErrorCode == 0;
                        message = cardServiceResponseEventReceivedResponse.ToString();
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
        private void WaitForEvent(ManualResetEvent eventHandle)
        {
            eventHandle.WaitOne();
            eventHandle.Reset();
        }
    }
}