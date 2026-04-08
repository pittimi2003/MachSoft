
namespace Mss.WorkForce.Code.Models.DTO
{
    public interface IDataOperation
    {
        /// <summary>
        /// Propiedad que define la operación a ejecutar en un modelo 
        /// </summary>
        OperationType DataOperationType { get; set; }
    }
}
