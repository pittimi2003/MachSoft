namespace Mss.WorkForce.Code.Models.Models
{
    public class Transaction
    {
        public Guid Id { get; set; }
        public DateTime CreationDate { get; set; }
        public string WarehouseCode { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public MessageStatus Status { get; set; }
        public string? FailureMessage { get; set; }
        public string Sender { get; set; } = ConstStrings.UnknownSender;
        public string Recipient { get; set; } = string.Empty;
        public string UserName { get; set; } = ConstStrings.UnknownUser;
    }

    public enum MessageStatus
    {
        Success,
        Failure
    }

    public static class ConstStrings
    {
        public const string UnknownUser = "Unknown User";
        public const string UnknownSender = "Unknown Sender";
        public const string WFM = "WFM";
    }
}
