namespace Mss.WorkForce.Code.Web.Services
{
    public interface IFieldLabelService
    {
        string GetLabel(string entityName, string fieldName);
        void Load();
    }
}
