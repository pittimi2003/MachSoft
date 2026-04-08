using System.ComponentModel;

namespace Mss.WorkForce.Code.Models.Enums
{
    public enum MeasuresType
    {
        [Description("%")]
        Percent,
        [Description("mts")]
        Meters,
        [Description("kg")]
        KiloGrams,
    }
}
