using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class Contact
    {
        [DisplayAttributes(21, "Contact name", true, isVisibleDefault: true)]
        //[ValidationAttributes(ValidationType.OnlyLetters)]
        public string? Name { get; set; }
        
        [DisplayAttributes(22, "Telephone", true, ComponentType.TextBox, textAlignment : true)]
		public string? Telephone { get; set; }
        
        [DisplayAttributes(23, "Extension", false, ComponentType.TextBox, textAlignment: true)]
        public string? Extension{ get; set; }
        
        [DisplayAttributes(24, "Telephone 2", false, ComponentType.TextBox, textAlignment: true)]
		public string? TelephoneOther { get; set; }
        
        [DisplayAttributes(31, "Fax", false, ComponentType.TextBox, textAlignment: true)]
		public string? Fax { get; set; }
        
        [DisplayAttributes(32, "Email", true)]
        [ValidationAttributes(ValidationType.EmailMask)]
        public string? Email { get; set; }
        
        [DisplayAttributes(33, "Contact comment", false)]
        //[ValidationAttributes(ValidationType.OnlyLetters)]
        public string? Comment { get; set; }

        public static Contact NewDto()
        {
            return new Contact
            {
                Name = string.Empty,
                Telephone = string.Empty,
                Extension = string.Empty,
                TelephoneOther = string.Empty,
                Fax = string.Empty,
                Email = string.Empty,
                Comment = string.Empty,
            };
        }
    }

    
}


