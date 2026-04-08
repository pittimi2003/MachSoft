namespace Mss.WorkForce.Code.Simulator.Core.Layout
{
    /// <summary>
    /// Defines a warehouse route.
    /// </summary>
    public class Route
    {
        #region Variables
        public Guid DepartureAreaId { get; set; }
        public Guid ArrivalAreaId { get; set; }
        public int Time { get; set; }
        #endregion

        #region Constructor
        public Route(Guid departureAreaId, Guid arrivalAreaId, int time)
        {
            this.DepartureAreaId = departureAreaId;
            this.ArrivalAreaId = arrivalAreaId;
            this.Time = time;
        }
        #endregion
    }
}
