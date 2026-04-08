
using System.ComponentModel;

namespace Mss.WorkForce.Code.Models.Enums
{
    public enum EnumViewPlanning
    {
        None,
        [Description("Order")]
        Order,
        [Description("Priority")]
        Priority,
        [Description("Trailer")]
        Trailer,
    }
}
