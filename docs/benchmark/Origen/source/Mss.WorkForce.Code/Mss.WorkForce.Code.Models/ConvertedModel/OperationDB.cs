using Mss.WorkForce.Code.Models.ModelUpdate;
using System.Reflection;

namespace Mss.WorkForce.Code.Models.ConvertedModel
{
    public class OperationDB
    {

        #region Constructors

        public OperationDB()
        {
            New = new Dictionary<string, List<object>>();
            Update = new Dictionary<string, List<object>>();
            Delete = new Dictionary<string, List<object>>();
        }

        #endregion

        #region Properties

        public Dictionary<string, List<object>> Delete { get; private set; }
        public Dictionary<string, List<object>> New { get; private set; }
        public Dictionary<string, List<object>> Update { get; private set; }

        #endregion

        #region Methods

        public void AddDelete(string entityName, object entity)
        {

            if (!Delete.TryGetValue(entityName, out List<object>? entities))
            {
                Delete.Add(entityName, new List<object> { entity });
            }
            else
                entities.Add(entity);

        }

        public void AddNew(string entityName, object entity)
        {

            if (!New.TryGetValue(entityName, out List<object>? entities))
            {
                New.Add(entityName, new List<object> { entity });
            }
            else
                entities.Add(entity);
        }

        public void AddUpdate(string entityName, object entity)
        {
            if (!Update.TryGetValue(entityName, out List<object>? entities))
            {
                Update.Add(entityName, new List<object> { entity });
            }
            else
                entities.Add(entity);
        }

        public void Clear()
        {
            New.Clear();
            Update.Clear();
            Delete.Clear();
        }

        #endregion

    }
}
