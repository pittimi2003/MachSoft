
namespace Mss.WorkForce.Code.Models.DTO
{
    public class CatalogEntity : IComparable<CatalogEntity>
    {
        #region Constructors

        public CatalogEntity(string catalogName)
        {
            CatalogName = catalogName;
        }

        #endregion

        #region Properties


        public Guid Id { get; set; }
        public string Name { get; set; }
        public string CatalogName { get; set; }

        #endregion

        #region Methods

        public override string ToString() => Name;

        public int CompareTo(CatalogEntity other)
        {
            if (other == null)
            {
                return 1; 
            }

            // Comparar por la propiedad Name
            return string.Compare(this.Name, other.Name, StringComparison.OrdinalIgnoreCase);
        }

        #endregion
    }
    }
