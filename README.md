# Gkash SoftPOS local web API (Offline payment)
This project is a local web API that allows merchants to call and make payments. The application has three main functionalities: login, request payment, and query payment status.

## Quick Start

To run this local web API, follow these steps:

1. Clone the repository to your local machine.
2. Install the [.NET Core 6 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/6.0).
3. Open CMD and navigate to the project folder then use the following command:
 ```
 dotnet publish -c Release --output ./Publish GkashSocketAPI.sln
 ```
 This command will create a folder name Publish in the project folder. 

4. Navigate to the "Publish" folder and use the following command to run the web API: 
```
dotnet GkashSocketAPI.dll
```

5. Place the pfx file in your local machine and you can start sending request to the web API already.

## Configuration - AppSettings.json

You can configure the settings in the AppSettings file.

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Settings": {
    "Urls": "http://localhost:5010",
    "CertPath": "C://t1clientcert/t1clientcert.pfx",
    "TestingEnv": false,
    "EnabledLogging": true,
    "LoggingPath": "C://GkashSocketAPILog/log.txt",
    "AllowedHosts": "*"
  }
}

```
- `Urls`: the hosting url used for this web API.
- `CertPath`: the path of pfx file used for authentication.
- `TestingEnv`: a boolean value indicating whether the API is running in a testing environment or not.
- `EnabledLogging`: a boolean value indicating whether the API should log its activities or not.
- `LoggingPath`: the path of text file used for logging.


## Host with a Service/IIS

- You may host this web API with IIS or run it in Windows service by using tools like [NSSM](https://nssm.cc/download).

## Login API - Method POST

The login API is used to authenticate the merchant. The content type of this API is application/json, the data transfer object used for this API is `LoginDto` and has the following properties:

```csharp
public class LoginDto
{
    public string Username { get; set; }
    public string Password { get; set; }
}
```

- `Username`: the login username of the Gkash SoftPOS APP.
- `Password`: the password of the Gkash SoftPOS APP.

The endpoint of this API: http://localhost:5010/api/Gkash/Login, you will receive status code 200 if the request is successful.

```json
{
    "message": "Login Successful"
}
```

## Request Payment API - Method POST

The request payment API is used to initiate a payment request. The content type of this API is application/json, the data transfer object used for this API is `PaymentRequestDto` and has the following properties:

```csharp
public class PaymentRequestDto
{
    public decimal Amount { get; set; }
    public string TerminalId { get; set; }
    public string Email { get; set; }
    public string MobileNo { get; set; }
    public string ReferenceNo { get; set; }
    public int PaymentType { get; set; }
    public bool PreAuth { get; set; }
    public string CallbackURL { get; set; }
}
```

- `Amount`: the amount to be paid.
- `TerminalId`: the terminal id that login to the SoftPOS by Gkash APP.
- `Email`: the email of the customer making the payment. (Optional)
- `MobileNo`: the mobile number of the customer making the payment. (Optional)
- `ReferenceNo`: a reference number for the payment request. (Optional)
- `PaymentType`: the type of payment to be made. Please refer the table below
- `PreAuth`: a boolean value indicating whether the payment should be a pre-authorization or not. (Optional)
- `CallbackURL`: the URL to which the response should be sent.

Note: If you are using DuitNow QR, we will append "-QR" to your reference number and generate the DuitNow QR.

| Payment Type | Value |
| --- | --- |
| eWallet Scan | 0 |
| Tap-to-Phone | 1 |
| Card Payment | 2 |
| Maybank QR | 3 |
| GrabPay QR | 4 |
| Touch 'n Go QR | 5 |
| Gkash QR | 6 |
| Boost QR | 7 |
| Wechat QR | 8 |
| ShopeePay QR | 9 |
| Alipay QR | 10 |
| Atome QR | 11 |
| MCash QR | 12 |
| DuitNow QR | 13 |

The endpoint of this API: http://localhost:5010/api/Gkash/RequestPayment, you will receive status code 200 if the request is successful.

## Cancel Transaction API - Method GET

The Cancel Transaction API is used to cancel current on-going transaction.
Note: This API cannot cancel the transaction that is already sent to host.

Example: http://localhost:5010/api/Gkash/CancelTransaction?TerminalId=M102-TD-9999, you will receive status code 200 if the request is successful.

- `TerminalId`: the Terminal Id that login to the SoftPOS by Gkash APP.

## Query Payment Status API - Method GET

The query payment status API is used to check the status of a previous payment request.

Example: http://localhost:5010/api/Gkash/QueryStatus?ReferenceNo=12345678912345, you will receive status code 200 and `TransactionStatus` if the request is successful.

If you are using DuitNow QR also, you can call this API to obtain both transaction status.\
Example: http://localhost:5010/api/Gkash/QueryCardAndDuitNowStatus?ReferenceNo=12345678912345, you will receive status code 200 and `List<TransactionStatus>` if the request is successful.

- `ReferenceNo`: the reference number of the payment request.

```
//Example response of QueryStatus API
{
    "applicationId": "A0000000031010",
    "authIDResponse": "      ",
    "responseOrderNumber": "333219000014",
    "cardNo": "VISA 4761 73** **** 0119",
    "cardType": "VISA CREDIT",
    "cartID": "12345678912345",
    "companyRemID": "M161-C-58175",
    "mid": "***********5284",
    "message": "00 - Approved",
    "method": "GMY.UMTECHMPOSMYR",
    "remID": "M161-PO-187445",
    "settlementBatchNumber": "000012",
    "signatureRequired": "0",
    "status": "88 - Transferred",
    "tid": "****3096",
    "tvr": "0000000000",
    "traceNo": "000014",
    "transferAmount": "3.00",
    "transferCurrency": "MYR",
    "transferDate": "2023-11-28 19:30:34",
    "txType": "SALE",
    "signature": null
}


//Example response of QueryCardAndDuitNowStatus API
[
    {
        "applicationId": "A0000000031010",
        "authIDResponse": "      ",
        "responseOrderNumber": "333219000014",
        "cardNo": "VISA 4761 73** **** 0119",
        "cardType": "VISA CREDIT",
        "cartID": "12345678912345",
        "companyRemID": "M161-C-58175",
        "mid": "***********5284",
        "message": "00 - Approved",
        "method": "GMY.UMTECHMPOSMYR",
        "remID": "M161-PO-187445",
        "settlementBatchNumber": "000012",
        "signatureRequired": "0",
        "status": "88 - Transferred",
        "tid": "****3096",
        "tvr": "0000000000",
        "traceNo": "000014",
        "transferAmount": "3.00",
        "transferCurrency": "MYR",
        "transferDate": "2023-11-28 19:30:34",
        "txType": "SALE",
        "signature": null
    },
    {
        "applicationId": null,
        "authIDResponse": null,
        "responseOrderNumber": null,
        "cardNo": "xxxx",
        "cardType": null,
        "cartID": "12345678912345-QR",
        "companyRemID": "M161-C-58320",
        "mid": "asd",
        "message": null,
        "method": "GMY.DNQRMYR",
        "remID": "M161-PO-187444",
        "settlementBatchNumber": null,
        "signatureRequired": null,
        "status": "66 - Failed",
        "tid": "asd",
        "tvr": null,
        "traceNo": null,
        "transferAmount": "3.00",
        "transferCurrency": "MYR",
        "transferDate": "2023-11-28 19:30:22",
        "txType": "SALE",
        "signature": null
    }
]
```


## Callback
The web API will send the transaction status to your CallbackURL after the payment is completed or query API is being called. The data transfer object used for this API is `TransactionStatus`.

## TransactionStatus

```csharp
public class TransactionStatus
{
    public string ApplicationId { get; set; }

    public string AuthIDResponse { get; set; }

    public string ResponseOrderNumber { get; set; }

    public string CardNo { get; set; }

    public string CardType { get; set; }

    public string CartID { get; set; }

    public string CompanyRemID { get; set; }

    public string MID { get; set; }

    public string Message { get; set; }

    public string Method { get; set; }

    public string RemID { get; set; }

    public string SettlementBatchNumber { get; set; }

    public string SignatureRequired { get; set; }

    public string Status { get; set; }

    public string TID { get; set; }

    public string TVR { get; set; }

    public string TraceNo { get; set; }

    public string TransferAmount { get; set; }

    public string TransferCurrency { get; set; }

    public string TransferDate { get; set; }

    public string TxType { get; set; }

    public string Signature { get; set; }
}
```

## Usage

To use the web API, follow these steps:

1. Call the login API to authenticate.
2. Call the request payment API to initiate a payment request.
3. The SDK will send the transaction status to your CallbackURL that you used to Login.
4. If you do not receive the callback from the SDK, call the query payment status API to check the status of the payment request.

## License

This project is licensed under the Apache License - see the LICENSE file for details.
