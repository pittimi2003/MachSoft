
namespace Mss.WorkForce.Code.Models.DTO
{
    public class Multiselect: IComparable<Multiselect>
    {
        private string _name = string.Empty;
        private Dictionary<Guid, string> _items = new Dictionary<Guid, string>();

        public string Name
        {
            get => _name;
            set
            {
                _name = value;
                foreach(var item in _items.Keys.ToList())
                    _items[item] = _name;
            }
        }

        public Dictionary<Guid, string> Items
        {
            get => _items;
            set
            {
                _items = value.ToDictionary(v => v.Key, v => Name ?? v.Value);
            }
        }

        public int CompareTo(Multiselect other)
        {
            if (other == null)
            {
                return 1;  
            }

            // Comparar por la propiedad Name
            return string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }
    }
}
