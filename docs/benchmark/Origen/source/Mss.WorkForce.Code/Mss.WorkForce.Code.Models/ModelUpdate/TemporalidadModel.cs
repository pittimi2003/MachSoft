using System.Text.Json.Serialization;

namespace Mss.WorkForce.Code.Models.ModelUpdate
{
    public class TemporalidadModel
    {
        private string _dateFormat;
        public TemporalidadModel(string dateFormat)
        {
            _dateFormat = string.IsNullOrWhiteSpace(dateFormat) ? "dd/MM/yyyy" : dateFormat;
        }
        [JsonConstructor]
        public TemporalidadModel(){}

        public Guid Id;
        public string Nombre { get; set; }
        public DateTime FechaDesde { get; set; } = DateTime.Now.Date;
        public DateTime FechaHasta { get; set; } = DateTime.Now.Date;
        public Guid WarehouseId { get; set; }
        public string Data { get; set; } = "{}";

        public string DisplayText => $"{Nombre} ({FechaDesde.ToString(_dateFormat)} - {FechaHasta.ToString(_dateFormat)})";
    }
}
