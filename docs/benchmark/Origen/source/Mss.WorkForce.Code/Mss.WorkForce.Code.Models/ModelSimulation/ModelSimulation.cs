namespace Mss.WorkForce.Code.Models
{
    public class PlanningPreview
    {
        public InputPlanningPreview Entradas { get; set; }
        public OutputPlanningPreview Salidas { get; set; }

        public WhProcessPlanningPreview WarehouseProcess { get; set; }
    }

    public class InputPlanningPreview
    {
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public List<ItemPlanningPreview> ListaDeElementos { get; set; } = new List<ItemPlanningPreview>();
    }

    public class OutputPlanningPreview
    {
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public List<ItemPlanningPreview> ListaDeElementos { get; set; } = new List<ItemPlanningPreview>();
    }

    public class WhProcessPlanningPreview
    {
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public List<ItemPlanningPreview> ListaDeElementos { get; set; } = new List<ItemPlanningPreview>();
    }

    public class ItemPlanningPreview
    {
        public DateTime Inicio { get; set; }
        public DateTime Fin { get; set; }
        public string Type { get; set; }
        public int Cantidad {  get; set; }
    }

    public class OperatorsByProcess
    {
        public string? Process { get; set; }
        public int Quantity { get; set; }
    }
}
