using Mss.WorkForce.Code.Models.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.Attributtes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MeasureAttributes: Attribute
    {
        public MeasuresType MeasuresType { get; }
        public MeasureAttributes(MeasuresType measuresType) { 
            this.MeasuresType = measuresType;
        }
    }
}
