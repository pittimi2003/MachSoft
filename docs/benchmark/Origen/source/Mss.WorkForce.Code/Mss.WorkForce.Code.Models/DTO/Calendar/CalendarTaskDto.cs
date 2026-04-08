namespace Mss.WorkForce.Code.Models.DTO.Calendar
{
    public class CalendarTaskDto
    {
        public Guid Id { get; set; }
        public DateTime StartDate { get; set; }  // Campo de inicio
        public DateTime EndDate { get; set; }    // Campo de fin
        public string Caption { get; set; }      // Título o descripción
        public bool IsActive { get; set; }       // Indica si la configuración está activa
        public bool IsConflicting { get; set; }  // Indica si hay conflicto en la fecha
    }

}
