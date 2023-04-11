using ClientSocketConnection;
using ClientSocketConnection.model;
using GkashSocketAPI.Dto.LoginDto;

namespace GkashSocketAPI.Core
{
    public interface IGkashRepository
    {
        Task LoginAsync(LoginDto dto, ILogger logger);
        Task UpdateGkashSDKInstanceAsync(ClientSocket socket);
        void SendCallback(TransResult.TransactionStatus result);
        Task<ClientSocket> GetGkashSDKInstanceAsync();
        Task<ILogger> GetLoggerAsync();
    }
}
