namespace Mss.WorkForce.Code.Models.Attributtes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class MultiEditAttributes : Attribute
    {
        public bool IsMultiEditable {  get; }
        public MultiEditAttributes(bool isMultiEditable = true) 
        {
            IsMultiEditable = isMultiEditable;
        }
    }
}