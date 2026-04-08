using System.ComponentModel;

namespace Mss.WorkForce.Code.Models.Enums
{
    public enum EnumDateField
    {
        [Description("Start date")]
        StartDate,

        [Description("Committed date")]
        CommittedDate,

        [Description("Arrival date")]
        ArrivalDate,

        [Description("End date")]
        EndDate
    }
}
