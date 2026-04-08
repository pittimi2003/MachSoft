using Mss.WorkForce.Code.Models.DTO.Enums;

namespace Mss.WorkForce.Code.Web.Code
{
    public static class MaskControllers
    {
        public static string GetMaskForProperty(ValidationType value)
        {
            switch (value)
            {
                case ValidationType.None: return @".*";
                case ValidationType.OnlyLetters: return "[A-Za-z ]*";
                case ValidationType.AllExceptWhiteSpace: return @"[^ ]*";
                case ValidationType.OnlyNumbers: return @"\d*";
                case ValidationType.NoSpecialCharacters: return @"[A-Za-z0-9]*";
                case ValidationType.EmailMask: return @"";
                case ValidationType.ZipCode: return @"[A-Za-z0-9 ]*";
                default: return "[A-Za-z0-9 ]*";
            };
        }
    }
}
