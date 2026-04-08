namespace Mss.WorkForce.Code.Models.DTO
{
    public class ResourceDto: ICloneable
    {
        public string resource { get; set; }
        public string value { get; set; }

        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
