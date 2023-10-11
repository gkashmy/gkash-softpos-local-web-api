using ClientSocketConnection;
using ClientSocketConnection.model;
using GkashSocketAPI.Core;
using GkashSocketAPI.Dto.LoginDto;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Text;

namespace GkashSocketAPI.Repository
{
    public class GkashRepository : IGkashRepository, IDataCallback
    {
        private ClientSocket _clientSocket;
        private readonly HttpClient _client = new();
        private string _callbackURL;
        private string _cartId;
        private ILogger _logger;
        private readonly IConfiguration _configuration;

        public GkashRepository(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task UpdateGkashSDKInstanceAsync(ClientSocket socket)
        {
            await Task.FromResult(_clientSocket = socket);
        }

        public async Task<ClientSocket> GetGkashSDKInstanceAsync()
        {
            return await Task.FromResult(_clientSocket);
        }

        public async Task LoginAsync(LoginDto dto, ILogger logger)
        {
            _logger = logger;
            bool isProdEnvironment = false;
            bool testingEnv = _configuration.GetValue<bool>("TestingEnv");
            string certPath = _configuration.GetValue<string>("CertPath");
            if (!testingEnv)
            {
                isProdEnvironment = true;
            }

            ClientSocket socketConnection = new(dto.Username, dto.Password, this, certPath, isProdEnvironment, logger);
            await UpdateGkashSDKInstanceAsync(socketConnection);
            _logger.LogInformation("Login UpdateGkashSDKInstanceAsync: " + dto.Username);
        }

        public void TransactionResult(TransResult.TransactionStatus result)
        {
            _logger.LogInformation(result.Status);
            SendCallback(result);
        }

        public void QueryTransactionResult(TransResult.TransactionStatus result)
        {
            _logger.LogInformation(result.Status);
          //  SendCallback(result);
        }

        public void TransactionEventCallback(SocketStatus.TransactionEventCallback transactionEventCallback)
        {
            Console.WriteLine(transactionEventCallback.ToString());
            if(transactionEventCallback == SocketStatus.TransactionEventCallback.CANCEL_PAYMENT ||
               transactionEventCallback == SocketStatus.TransactionEventCallback.GET_KEY_FAIL ||
               transactionEventCallback == SocketStatus.TransactionEventCallback.INVALID_AMOUNT ||
               transactionEventCallback == SocketStatus.TransactionEventCallback.INVALID_METHOD ||
               transactionEventCallback == SocketStatus.TransactionEventCallback.INVALID_SIGNATURE ||
               transactionEventCallback == SocketStatus.TransactionEventCallback.INVALID_PAYMENT_TYPE ||
               transactionEventCallback == SocketStatus.TransactionEventCallback.DEVICE_OFFLINE ||
               transactionEventCallback == SocketStatus.TransactionEventCallback.NO_CARD_DETECTED_TIMEOUT ||
               transactionEventCallback == SocketStatus.TransactionEventCallback.NO_PIN_DETECTED_TIMEOUT)
            {
                TransResult.TransactionStatus result = new()
                {
                    Status = transactionEventCallback.ToString(),
                    CartID = _cartId
                };
                SendCallback(result);
            }
        }

        public void SocketStatusCallback(SocketStatus.SocketConnectivityCallback socketConnectivityCallback)
        {
            _logger.LogInformation(socketConnectivityCallback.ToString());
        }

        public void QueryTransMessage(string message)
        {
            _logger.LogInformation(message);
        }

        public void CurrentTransactionCartId(string cartId)
        {
            _logger.LogInformation("CurrentTransactionCartId: " + cartId);
            _cartId = cartId;
        }

        public void SendCallback(TransResult.TransactionStatus result)
        {
            //Send Callback
            try
            {
                var json = JsonConvert.SerializeObject(result);

                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger.LogInformation("Sending callback to : " + _callbackURL + ", content: " + json);
                var response = _client.PostAsync(_callbackURL, content).Result;
                _logger.LogInformation(response.StatusCode.ToString());

                _logger.LogInformation("Sent callback: " + result.CartID + ", StatusCode: " + response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger.LogError("SendCallback Exception: " + ex);
            }
        
        }

        public async Task<ILogger> GetLoggerAsync()
        {
            return await Task.FromResult(_logger);
        }

        public async Task SetCallbackURL(string callbackURL)
        {
            await Task.FromResult(_callbackURL = callbackURL);
        }
    }
}
