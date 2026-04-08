using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.DTO.Enums
{

    public enum ValidationType
    {
        None, //todas las entradas 
        OnlyLetters, //solo letras con espacios
        OnlyNumbers, //solo numeros sin espacios
        EmailMask,//tipo email
        NoSpecialCharacters,//Letras y numeros sin espacios ni caracteres especiales
        AllExceptWhiteSpace,//Todo sin espacios en blanco
        ZipCode,// codigo postal, acepta letras, numeros y espacios
    }
}
