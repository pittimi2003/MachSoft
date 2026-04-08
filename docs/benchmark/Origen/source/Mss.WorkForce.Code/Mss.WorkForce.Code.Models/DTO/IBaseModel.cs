using Mss.WorkForce.Code.Models.Attributtes;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;

namespace Mss.WorkForce.Code.Models.DTO
{
    public interface IBaseModel
    {
        #region Properties

        OperationType DataOperationType { get; set; }
        Guid Id { get; set; }

        string Name { get; set; }
        bool IsDependencies { get; set; }
        #endregion

        #region Methods

        object GetProperty(string propertyName);

        object Clone();
        bool Equals(object? obj);

        void SetDependencies(bool val);
        bool GetDependencies();

        #endregion
    }

    public abstract class PenelEditorOperations: IBaseModel
    {
        public virtual OperationType DataOperationType { get; set; }
        public virtual Guid WarehouseId { get; set; }

        [DisplayAttributes(index: 1, caption: "Name", isVisible: true, required: true, fieldType: ComponentType.TextBox)]
        [ValidationAttributes(Enums.ValidationType.None), UniqueAttributes(true)]
        public virtual string Name { get; set; }

        public virtual Guid Id { get; set; }
        public virtual bool IsDependencies { get; set; }

        public virtual object Clone()
        {
            return MemberwiseClone();
        }
        
        public virtual bool GetDependencies() => IsDependencies;

        public virtual void SetDependencies(bool val) => IsDependencies = val;


        public object GetProperty(string propertyName)
        {
            var propertyInfo = GetType().GetProperty(propertyName);
            return propertyInfo?.GetValue(this);
        }
    }
}
