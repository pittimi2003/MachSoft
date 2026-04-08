namespace Mss.WorkForce.Code.Simulator.Simulation.Resources
{
    /// <summary>
    /// Defines a class for the breaks in order to add the dates for the selected init and end break periods
    /// </summary>
    public class CustomBreak
    {
        #region Variables
        public double InitBreak { get; set; }
        public double EndBreak { get; set; }
        public Guid Id { get; set; }
        #endregion

        #region Constructor
        public CustomBreak() { }

        public CustomBreak(Guid id, double initBreak, double endBreak) 
        {
            this.Id = id;
            this.InitBreak = initBreak;
            this.EndBreak = endBreak;
        }
        #endregion
    }
}
