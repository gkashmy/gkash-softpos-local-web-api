using ClientSocketConnection;
using ClientSocketConnection.model;
using GkashSocketAPI.Core;
using GkashSocketAPI.Dto.LoginDto;
using GkashSocketAPI.Dto.Settings;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace GkashSocketAPI.Service.Impl
{
    public class GkashService : IGkashService, IDataCallback
    {
        private readonly HttpClient _client;
        private string _callbackURL;
        private string _cartId;
        private readonly ILogger _logger;
        private readonly SettingsDto _settingsDto;
        private readonly MultiClientSocket _multiClientSocket = new();
        private string _defaultTerminalId;
        private readonly string tag = $"{nameof(GkashService)}";

        public GkashService(ILogger<GkashService> logger,
                            IOptions<SettingsDto> settingsDto,
                            IHttpClientFactory httpClientFactory)
        {

            _logger = logger;
            _settingsDto = settingsDto.Value;
            _client = httpClientFactory.CreateClient();
        }

        public ClientSocket GetGkashSDKInstance(string terminalId)
        {
            ClientSocket clientSocket = _multiClientSocket.GetClientSocket(terminalId);
            if (clientSocket == null) _logger?.LogInformation($"{tag} clientSocket instance empty: {terminalId}");
            return clientSocket;
        }

        private ClientSocket GetGkashSDKInstance()
        {
            return GetGkashSDKInstance(_defaultTerminalId);
        }

        public async Task<string> LoginAsync(LoginDto dto)
        {
            _logger?.LogInformation($"{tag} LoginAsync: " + dto.Username);

            ClientSocket clientSocket = GetGkashSDKInstance(dto.Username);

            if (clientSocket == null)
            {
                _logger?.LogInformation($"{tag} LoginAsync create new clientSocket instance: " + dto.Username);
                clientSocket = new(this, _settingsDto.CertPath, !_settingsDto.TestingEnv, _logger, _client);

                _defaultTerminalId ??= dto.Username;

                _multiClientSocket.AddClient(dto.Username, clientSocket);
            }

            return await clientSocket.LoginAsync(dto.Username, dto.Password);
        }

        public void TransactionResult(TransResult.TransactionStatus result)
        {
            _logger?.LogInformation(result.Status);
            SendCallback(result);
        }

        public void QueryTransactionResult(TransResult.TransactionStatus result)
        {
            _logger?.LogInformation(result.Status);
        }

        public void TransactionEventCallback(SocketStatus.TransactionEventCallback transactionEventCallback)
        {
            Console.WriteLine(transactionEventCallback.ToString());
            if (transactionEventCallback == SocketStatus.TransactionEventCallback.CANCEL_PAYMENT ||
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
            _logger?.LogInformation(socketConnectivityCallback.ToString());
        }

        public void QueryTransMessage(string message)
        {
            _logger?.LogInformation(message);
        }

        public void CurrentTransactionCartId(string cartId)
        {
            _logger?.LogInformation($"{tag} CurrentTransactionCartId: " + cartId);
            _cartId = cartId;
        }

        public void SendCallback(TransResult.TransactionStatus result)
        {
            //Send Callback
            try
            {
                var json = JsonSerializer.Serialize(result);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                _logger?.LogInformation($"{tag} Sending callback to : " + _callbackURL + ", content: " + json);
                var response = _client.PostAsync(_callbackURL, content).Result;
                _logger?.LogInformation($"{tag} Sent callback: " + result.CartID + ", StatusCode: " + response.StatusCode);
            }
            catch (Exception ex)
            {
                _logger?.LogError($"{tag} SendCallback Exception: " + ex);
            }
        }

        public void RequestPayment(PaymentRequestDto dto)
        {
            ClientSocket clientSocket = GetGkashSDKInstance(dto.TerminalId) ?? throw new Exception(dto.TerminalId + " ClientSocket instance not found, forgot to login?");
            SetCallbackURL(dto.CallbackURL);
            clientSocket.RequestPayment(dto);
        }

        public void CancelTransaction(string terminalId)
        {
            ClientSocket clientSocket = GetGkashSDKInstance(terminalId) ?? throw new Exception(terminalId + " ClientSocket instance not found, forgot to login?");

            clientSocket.CancelPayment();
        }

        public async Task<TransResult.TransactionStatus> QueryTransactionStatusAsync(string referenceId)
        {
            ClientSocket clientSocket = GetGkashSDKInstance() ?? throw new Exception("ClientSocket instance not found, forgot to login?");

            return await clientSocket.QueryStatusAsync(referenceId);
        }

        public async Task<List<TransResult.TransactionStatus>> QueryCardAndDuitNowStatusAsync(string referenceId)
        {
            ClientSocket clientSocket = GetGkashSDKInstance() ?? throw new Exception("ClientSocket instance not found, forgot to login?");

            return await clientSocket.QueryCardAndDuitNowStatusAsync(referenceId);
        }

        private void SetCallbackURL(string callbackURL)
        {
            _callbackURL = callbackURL;
        }
    }
}
