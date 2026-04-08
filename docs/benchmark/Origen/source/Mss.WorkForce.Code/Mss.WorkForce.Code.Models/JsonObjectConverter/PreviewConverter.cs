using Mss.WorkForce.Code.Models.ModelInsert;

namespace Mss.WorkForce.Code.Models
{
    public static class PreviewConverter
    {

        public static PlanningPreview ConvertPlanningToPreview(List<WorkOrderPlanningReturn> planning, List<WarehouseProcessPlanningReturn> whProcess)
        {
            if (planning == null)
            {
                return new PlanningPreview();
            }
            List<ItemPlanningPreview> Inputs = new List<ItemPlanningPreview>();
            List<ItemPlanningPreview> Outputs = new List<ItemPlanningPreview>();
            List<ItemPlanningPreview> WhProcess = new List<ItemPlanningPreview>();
            DateTime? StartDateInput = null;
            DateTime? EndDateInput = null;
            DateTime? StartDateOutput = null;
            DateTime? EndDateOutput = null;
            DateTime? StartDateWh = null;
            DateTime? EndDateWh = null;

            foreach (var order in planning) 
            {
                if (order.IsOutbound) {
                    foreach (var item in order.ItemPlanning)
                    {
                        Outputs.Add(new ItemPlanningPreview()
                        {
                            Inicio = item.InitDate,
                            Fin = item.EndDate,
                            Type = item.Process.Name,
                        });
                        StartDateOutput = StartDateOutput == null || StartDateOutput.Value > item.InitDate ?  item.InitDate : StartDateOutput;
                        EndDateOutput = EndDateOutput == null || EndDateOutput.Value < item.EndDate ? item.EndDate : EndDateOutput;
                    }
                }
                else
                {
                    foreach (var item in order.ItemPlanning)
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
            }

            foreach (var order in whProcess)
            {
                WhProcess.Add(new ItemPlanningPreview()
                    {
                        Inicio = order.InitDate,
                        Fin = order.EndDate,
                        Type = order.Process.Name,
                    });
                StartDateWh = StartDateWh == null || StartDateWh.Value > order.InitDate ? order.InitDate : StartDateWh;
                EndDateWh = EndDateWh == null || EndDateWh.Value < order.EndDate ? order.EndDate : EndDateWh;
                    
                
            }

            OutputPlanningPreview planningPreviewOutput = new OutputPlanningPreview()
            {
                Inicio = StartDateOutput ?? DateTime.UtcNow,
                Fin = EndDateOutput?? DateTime.UtcNow,
                ListaDeElementos = Outputs
            };
            InputPlanningPreview planningPreviewInput = new InputPlanningPreview()
            {
                Inicio = StartDateInput ?? DateTime.UtcNow,
                Fin = EndDateInput ?? DateTime.UtcNow,
                ListaDeElementos = Inputs
            };
            WhProcessPlanningPreview planningPreviewWh = new WhProcessPlanningPreview()
            {
                Inicio = StartDateInput ?? DateTime.UtcNow,
                Fin = EndDateInput ?? DateTime.UtcNow,
                ListaDeElementos = WhProcess
            };


            return  new PlanningPreview() 
            {
                Entradas = planningPreviewInput,
                Salidas = planningPreviewOutput,
                WarehouseProcess = planningPreviewWh
            };

             
        }
    }
}
