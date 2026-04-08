using System.Globalization;
using Mss.WorkForce.Code.Models.Common;

namespace Mss.WorkForce.Code.Web.Model
{
    public class DataTotalStages
    {
        private UserFormatOptions _userFormat;
        public DataTotalStages(UserFormatOptions userFormat)
        {
            _userFormat = userFormat;
        }

        public double TS { get; set; }
        public double AU { get; set; }
        public double PU { get; set; }
        public double TC { get; set; }

        public string TotalSaturation => FormatPercentage(TS) ;
        public string ActualUtilization => FormatPercentage(AU);
        public string PlannedUtilization => FormatPercentage(PU);
        public string TotalCapacity => FormatPercentage(TC);

        private string FormatPercentage(double value) => value.ToUserNumber(_userFormat) + "%";

    }
}
