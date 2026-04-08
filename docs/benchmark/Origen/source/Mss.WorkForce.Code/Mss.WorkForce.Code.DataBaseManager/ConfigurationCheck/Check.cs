using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.ConfigurationCheckLogs;

namespace Mss.WorkForce.Code.DataBaseManager.ConfigurationCheck
{
    public class Check
    {
        /// <summary>
        /// Checks if all the configuration is completed in order to have a sucessfull simulation 
        /// </summary>
        /// <param name="data">Data from the warehouse to check.</param>
        /// <returns>True if everuthing is ok, false if not.</returns>
        public bool Configuration(DataSimulatorTablaRequest? data, ref ConfigCheck configCheck)
        {
            if (!data.Layout.Any())
            {
                Console.WriteLine($"Warehouse {data.Warehouse.Name} has no layout. A layout must be created in Configuration -> Projects.");
                configCheck.Values.Add($"Warehouse {data.Warehouse.Name} has no layout. A layout must be created in Configuration -> Projects.");
                return false;
            }
            else
            {
                bool layoutCheck = new LayoutCheck(data).Check(ref configCheck);
                bool processes = new ProcessCheck(data).Check(ref configCheck);
                bool resources = new ResourcesCheck(data).Check(ref configCheck);

                if (layoutCheck && processes && resources)
                {
                    Console.WriteLine($"Configuration for warehouse {data.Warehouse.Code} OK.");
                    configCheck.Values.Add($"Configuration for warehouse {data.Warehouse.Code} OK.");
                    return true;
                }
                else return false;
            }
        }
    }
}
