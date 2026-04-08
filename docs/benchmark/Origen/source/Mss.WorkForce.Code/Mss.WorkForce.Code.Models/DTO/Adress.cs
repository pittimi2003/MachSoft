using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web;
using Mss.WorkForce.Code.Web.Model.Enums;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class Adress
    {
        [DisplayAttributes(10,"Address", true, isVisibleDefault: true)]
        //[ValidationAttributes(ValidationType.OnlyLetters)]
        public string? CompleteAdress { get; set; }

        [DisplayAttributes(2, "Address line", false)]
        //[ValidationAttributes(ValidationType.OnlyLetters)]
        public string? AdressLine { get; set; }

        [DisplayAttributes(13, "Zip code", true, textAlignment: true)]
        [ValidationAttributes(ValidationType.ZipCode)]
		public string ZipCode { get; set; }

        [DisplayAttributes(14, "City", true)]
        //[ValidationAttributes(ValidationType.OnlyLetters)]
        public string? City { get; set; }

        [DisplayAttributes(11, "State", true)]
        //[ValidationAttributes(ValidationType.OnlyLetters)]
        public string State { get; set; }

        [DisplayAttributes(12, "Country", true, ComponentType.DropDown)]
        public Country? Country { get; set; }

        [DisplayAttributes(15, "Address comment", false)]
        //[ValidationAttributes(ValidationType.OnlyLetters)]
        public string? Comment{ get; set; }

        public static Adress NewDto()
        {
            return new Adress
            {
                CompleteAdress= string.Empty,
                AdressLine = string.Empty,
                ZipCode = string.Empty,
                City = string.Empty, 
                State = string.Empty,
                Country = new Country(),
                Comment = string.Empty
            };
        }

    }
}
