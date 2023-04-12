using ClientSocketConnection.model;

namespace GkashSocketAPI.Dto.PaymentDto
{
    public class PaymentRequestDto
    {
        public string Amount { get; set; }

        public string Email { get; set; }

        public string MobileNo { get; set; }

        public string ReferenceNo { get; set; }

        public SocketStatus.PaymentType PaymentType { get; set; }

        public bool PreAuth { get; set; }

        public string CallbackURL { get; set; }
    }
}
