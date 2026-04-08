
using System.ComponentModel.DataAnnotations;

namespace Mss.WorkForce.Code.Models
{
    public interface ICustomValidation
    {
        ValidationResult CustomValidation();
    }
}
