using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;
using System.ComponentModel.DataAnnotations;


namespace Mss.WorkForce.Code.Models.DTO
{
    public class TransactionDto
    {
        [DisplayAttributes(required: true,isVisible: false, isVisibleDefault:false)]
        [Key]
        public Guid Id { get; set; }


        private DateTime creationDate;
        [DisplayAttributes(1, "CreationDate", true, ComponentType.DateTime, "", GroupTypes.None, false, isVisibleDefault: true)]
        public DateTime CreationDate
        {
            get => creationDate;
            set => creationDate = DateTime.SpecifyKind(value, DateTimeKind.Utc);
        }

        [DisplayAttributes(2, "User", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        [UniqueAttributes]
        public string UserName { get; set; }

        [DisplayAttributes(3, "Warehouse", true,ComponentType.TextBox,"",GroupTypes.None, false, isVisibleDefault: true)]
        [UniqueAttributes]
        public string WarehouseCode { get; set; }

        [DisplayAttributes(4, "Sender", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        [UniqueAttributes]
        public string Sender { get; set; }

        [DisplayAttributes(5, "Recipient", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        [UniqueAttributes]
        public string Recipient { get; set; }

        [DisplayAttributes(6, "Type", true, ComponentType.TextBox, "", GroupTypes.None, false, isVisibleDefault: true)]
        [UniqueAttributes]
        public string Type { get; set; }

        [DisplayAttributes(7, "Status", true, ComponentType.ItemIcon, "Status", GroupTypes.None, false, isVisibleDefault: false)]
        [UniqueAttributes]
        public string Status { get; set; }

        [DisplayAttributes(8, "FailureMessage", false, ComponentType.TextBox, "Status", GroupTypes.None, false, isVisibleDefault: true)]
        [UniqueAttributes]
        public string? FailureMessage { get; set; }

        public string Content { get; set; }

        [DisplayAttributes(9, "Content", true, ComponentType.ReadOnlyText, "Message", GroupTypes.None, false, isVisibleDefault: false)]
        [UniqueAttributes]
        public string ContentDisplay { get; set; } = string.Empty;

        public static TransactionDto NewDto()
        {
            return new TransactionDto
            {
                Id = Guid.NewGuid(),
                CreationDate = default,
                WarehouseCode = string.Empty,
                Type = string.Empty,
                Content = string.Empty,
                ContentDisplay = string.Empty,
                Status = string.Empty,
                FailureMessage = null,
                Sender = string.Empty,
                Recipient = string.Empty,
                UserName = string.Empty,
            };
        }
    }
}


