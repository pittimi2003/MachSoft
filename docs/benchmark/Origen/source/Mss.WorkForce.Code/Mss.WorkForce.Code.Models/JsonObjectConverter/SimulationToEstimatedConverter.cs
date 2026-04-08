using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Models
{
    public static class SimulationToEstimatedConverter
    {
        public static PlanningPreview ConvertPlanningToEstimation(List<ItemPlanning> planning)
        {
            if (planning == null)
            {
                return new PlanningPreview();
            }
            List<ItemPlanningPreview> Inputs = new List<ItemPlanningPreview>();
            List<ItemPlanningPreview> Outputs = new List<ItemPlanningPreview>();
            DateTime? StartDateInput = null;
            DateTime? EndDateInput = null;
            DateTime? StartDateOutput = null;
            DateTime? EndDateOutput = null;

            foreach (var item in planning)
            {
                if (item.IsOutbound)
                {
                    Outputs.Add(new ItemPlanningPreview()
                    {
                        Inicio = item.InitDate,
                        Fin = item.EndDate,
                        Type = item.Process.Name,
                    });
                    StartDateOutput = StartDateOutput == null || StartDateOutput.Value > item.InitDate ? item.InitDate : StartDateOutput;
                    EndDateOutput = EndDateOutput == null || EndDateOutput.Value < item.EndDate ? item.EndDate : EndDateOutput;
                    
                }
                else
                {
                    Inputs.Add(new ItemPlanningPreview()
                    {
                        Inicio = item.InitDate,
                        Fin = item.EndDate,
                        Type = item.Process.Name,
                    });
                    StartDateInput = StartDateInput == null || StartDateInput.Value > item.InitDate ? item.InitDate : StartDateInput;
                    EndDateInput = EndDateInput == null || EndDateInput.Value < item.EndDate ? item.EndDate : EndDateInput;
                   
                }
            }
            OutputPlanningPreview planningPreviewOutput = new OutputPlanningPreview()
            {
                Inicio = StartDateOutput ?? DateTime.UtcNow,
                Fin = EndDateOutput ?? DateTime.UtcNow,
                ListaDeElementos = Outputs
            };
            InputPlanningPreview planningPreviewInput = new InputPlanningPreview()
            {
                Inicio = StartDateInput ?? DateTime.UtcNow,
                Fin = EndDateInput ?? DateTime.UtcNow,
                ListaDeElementos = Inputs
            };

            return new PlanningPreview()
            {
                Entradas = planningPreviewInput,
                Salidas = planningPreviewOutput
            };


        }
    }
}
