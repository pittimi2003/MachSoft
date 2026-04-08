using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mss.WorkForce.Code.Models.Attributtes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class UniqueAttributes : Attribute
    {
        public bool IsUnique { get; }
        public UniqueAttributes(bool isUnique = true) 
        {
            IsUnique = isUnique; 
        }
    }
}
