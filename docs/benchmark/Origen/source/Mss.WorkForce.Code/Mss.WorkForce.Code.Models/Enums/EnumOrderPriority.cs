
using System.ComponentModel;

namespace Mss.WorkForce.Code.Models.Enums
{
    public enum EnumOrderPriority
    {
        [Description("Urgent")]
        Urgent,
        [Description("High")]
        High,
        [Description("Normal")]
        Normal,
        [Description("Low")]
        Low,
        [Description("Very Low")]
        VeryLow
    }

    public enum EnumOrderPriorityIcon
    {
        [Description("mlx-ico-Priority-1-urgent")]
        Urgent,
        [Description("mlx-ico-Priority-2-high")]
        High,
        [Description("mlx-ico-Priority-3-normal")]
        Normal,
        [Description("mlx-ico-Priority-4-low")]
        Low,
        [Description("mlx-ico-Priority-5-very-low")]
        VeryLow,
    }

    public enum EnumSeverityIcon
    {
        [Description("mlx-ico-warning-bg-solid")]
        Warning,
        [Description("mlx-ico-info-bg-solid")]
        Information,
        [Description("mlx-ico-error-bg-solid")]
        Error,
    }


    public enum EnumMessageStatus
    {
        [Description("mlx-ico-check")]
        Success,
        [Description("mlx-ico-close")]
        Failure,
    }

    public enum EnumAlertOperatorIcon
    {
        [Description("mlx-ico-greater-than")]
        GreaterThan,
        [Description("mlx-ico-less-than")]
        LessThan,
        [Description("mlx-ico-equal")]
        Equal,
        [Description("mlx-ico-distinct")]
        NotEqual,
        [Description("mlx-ico-greater-than-or-equal-to")]
        GreaterOrEqual,
        [Description("mlx-ico-less-than-or-equal-to")]
        LessOrEqual
    }

    public enum EnumSeverityIconDisabled
    {
        [Description("mlx-ico-warning")]
        Warning,
        [Description("mlx-ico-info")]
        Information,
        [Description("mlx-ico-error")]
        Error,
    }
}
