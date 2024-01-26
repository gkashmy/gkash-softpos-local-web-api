using ClientSocketConnection;
using ClientSocketConnection.model;
using GkashSocketAPI.Dto.LoginDto;

namespace GkashSocketAPI.Core
{
    public interface IGkashService
    {
        Task<string> LoginAsync(LoginDto dto);
        void SendCallback(TransResult.TransactionStatus result);
        ClientSocket GetGkashSDKInstance(string terminalId);
        void CancelTransaction(string terminalId);
        void RequestPayment(PaymentRequestDto dto);
        Task<TransResult.TransactionStatus> QueryTransactionStatusAsync(string referenceId);
        Task<List<TransResult.TransactionStatus>> QueryCardAndDuitNowStatusAsync(string referenceId);
        bool CancelRemoteTransaction(string terminalId);
        bool RequestRemotePayment(PaymentRequestDto dto);
    }
}
