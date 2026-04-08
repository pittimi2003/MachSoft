using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.Models;
using System.Globalization;
using Mss.WorkForce.Code.Models.Common;
namespace Mss.WorkForce.Code.Web
{
    public static  class PivotConverter
    {
  
        public static List<PivotTaskData> ConverterDataPlanning(IEnumerable<ItemPlanning> itemPlannings)
        { 
        List<PivotTaskData> resp = new List<PivotTaskData>();
            try
            {
                int cont = 0;
                PivotTaskData item = new PivotTaskData();
                foreach (ItemPlanning i in itemPlannings)
                {
                    item= new PivotTaskData();
                    item.PivotCount = cont ++;
                    item.Flow = i.IsOutbound ? "OUTPUT PROFILE" : "INPUT PROFILE";
                    item.title = i.WorkOrderPlanning.InputOrder?.OrderCode ?? string.Empty;
                    item.Priority = i.WorkOrderPlanning.Priority;
                    item.CommittedHour = i.WorkOrderPlanning.InputOrder?.AppointmentDate.ToString("h:mm:ss tt", CultureInfo.InvariantCulture)?? string.Empty;
                    item.OrderCommittedHour= i.WorkOrderPlanning?.AppointmentDate.ToString("h:mm:ss tt", CultureInfo.InvariantCulture) ?? string.Empty;
                    item.Status = i.WorkOrderPlanning.InputOrder?.Status??string.Empty;
                    item.OrderStatus = i.WorkOrderPlanning.Status;
                    item.ActivityTitle = i.Process.Type;
                    item.OrderStart = i.WorkOrderPlanning.InitDate.ToString("h:mm:ss tt", CultureInfo.InvariantCulture)?? string.Empty;
                    item.OrderEnd= i.WorkOrderPlanning.EndDate.ToString("h:mm:ss tt", CultureInfo.InvariantCulture);
                    item.OrderProgress = Convert.ToInt32(i.WorkOrderPlanning.Progress/100);
                    item.start =  i.InitDate.ToString("h:mm:ss tt", CultureInfo.InvariantCulture);
                    item.end =i.EndDate.ToString("h:mm:ss tt", CultureInfo.InvariantCulture);
                    item.Carrier = i.WorkOrderPlanning.InputOrder?.Carrier ?? string.Empty;
                    item.Customer = i.WorkOrderPlanning.InputOrder?.Account ?? string.Empty;
                    item.Dock = i.WorkOrderPlanning.InputOrder?.AssignedDock?.Zone.Name ?? string.Empty;
                    item.isBlock = i.WorkOrderPlanning.InputOrder?.IsBlocked ?? false;
                    item.EndBlockDate = i.WorkOrderPlanning.InputOrder?.EndBlockDate != null ? i.WorkOrderPlanning.InputOrder?.EndBlockDate.Value.ToString("h:mm:ss tt", CultureInfo.InvariantCulture) : string.Empty;
                    item.DelayedOrder = i.WorkOrderPlanning.IsOnTime ? "NOT DELAYED ORDER" : "DELAYED ORDER";
                    item.progress = Convert.ToInt32(i.Progress / 100);
                    item.Resource = i.Worker?.Name?? string.Empty;
                    item.Supplier = i.WorkOrderPlanning.InputOrder?.Supplier ?? string.Empty;
                    item.Trailer = i.WorkOrderPlanning.InputOrder?.Trailer ?? "No Trailer";
                    item.SumTimeOrderDelay = i.WorkOrderPlanning.OrderDelay != null ?i.WorkOrderPlanning.OrderDelay<0? 0: i.WorkOrderPlanning.OrderDelay:0;
                    item.SumActualWorkTime  = i.WorkOrderPlanning.WorkTime < 0?0: i.WorkOrderPlanning.WorkTime;
                    item.SLATargetTime = i.WorkOrderPlanning.SLATarget?.ToString("h:mm", CultureInfo.InvariantCulture);
                    item.SLAMet = i.WorkOrderPlanning.SLAMet.GetSLAMetString();
                    item.Shift = i.Shift?.Name??string.Empty;
                    resp.Add(item);
                }
            }
            catch  {
                throw;
            }

            return resp;
        }

    }
}
