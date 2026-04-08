namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class DependenciesGantt
    {
        public Guid id { get; set; }
        public Guid predecessorId { get; set; }
        public Guid successorId { get; set; }
        public int type { get; set; } = 1;       // tipo de relación - se envia uno para que se dibuje de lado izquierdo de las tareas
        // 0 - Finist to Start
        // 1 - Start to Start
        // 2 - Finis to finish
        // 3 - Start to Finish
    }
}
