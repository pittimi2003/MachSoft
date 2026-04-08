namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class SimulationLogCheck
    {
        public string SimulationResult { get; set; }
        public string Log { get; set; }
    }

    public enum SimulationResults
    {
        Processes,
        Workers,
        Roles,
        Equipments,
        ContainersFull,
        ContainersBlock,
        ContainersInsufficient,
        ResultOk,
        ResultNotOk,
        General,
        WorkLoad,
    }
}
