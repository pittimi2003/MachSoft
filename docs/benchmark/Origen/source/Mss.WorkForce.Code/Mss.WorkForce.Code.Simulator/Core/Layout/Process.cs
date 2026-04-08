using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Simulator.Core.Layout
{
    /// <summary>
    /// Defines a warehouse process.
    /// </summary>
    public class Process
    {
        #region Variables
        public Guid Id { get; set; }
        public string Code { get; set; }
        public Area Area { get; set; }
        public string ProcessType { get; set; }
        public bool IsInitProcess { get; set; }
        public bool IsWarehouseProcess { get; set; }
        public Process? PreviousProcess { get; set; }
        public int Containers { get; set; }
        #endregion

        #region Constructor
        public Process(Guid id, string code, Area area, string processType, bool isInitProcess, bool isWarehouseProcess, int containers)
        {
            this.Id = id;
            this.Code = code;
            this.ProcessType = processType;
            this.Area = area;
            this.IsInitProcess = isInitProcess;
            this.IsWarehouseProcess = isWarehouseProcess;
            this.PreviousProcess = null;
            this.Containers = containers;
        }
        #endregion
    }
}
