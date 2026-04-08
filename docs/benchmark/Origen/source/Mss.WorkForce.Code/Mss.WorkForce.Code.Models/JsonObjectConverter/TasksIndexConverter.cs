
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.ModelGantt;

namespace Mss.WorkForce.Code.Models.JsonObjectConverter
{
    public class TasksIndexConverter
    {
        public static List<TModel> AssignRootIndex<TModel>(List<TModel> dataTasks, EnumViewPlanning granularity = EnumViewPlanning.None) where TModel : GanttTaskBase
        {
            int parentStep = 1000;
            if (dataTasks != null)
            {
                dataTasks = SortData(dataTasks);

                var rootTask = dataTasks.Where(t => t.parentId == null).OrderBy(t => t.StartDate).ToList();

                int parentIndex = 1;
                foreach (var root in rootTask)
                {
                    root.index = parentIndex * parentStep;
                    parentIndex++;

                    AssignChildIndex(root, dataTasks, parentStep, 1, granularity);
                }
            }

            if (granularity == EnumViewPlanning.Priority)
            {
                foreach (var t in dataTasks)
                {
                    if (t.levelTask == 3)
                        t.Priority = null;

                }
            }
            return dataTasks;
        }


        public static List<TModel> AssignChildIndex<TModel>(TModel principalTask, List<TModel> data, int step, int level, EnumViewPlanning granularity = EnumViewPlanning.None) where TModel : GanttTaskBase
        {
            if (data != null)
            {
                var children = SortData(data.Where(t => t.parentId == principalTask.id).ToList(), level, granularity);
                int i = 1;
                foreach (var child in children)
                {
                    child.index = principalTask.index + i * (int)Math.Pow(10, 3 - level);
                    i++;

                    AssignChildIndex(child, data, step, level + 1, granularity);
                }
            }

            return data;

        }

        private static List<TModel> SortData<TModel>(List<TModel> dataTasks)
        {
            if (dataTasks != null)
            {
                var dataBase = dataTasks.Cast<GanttTaskBase>().ToList();               
                dataTasks = dataBase.OrderBy(item => item.StartDate).ThenBy(item => item.StartDate).Cast<TModel>().ToList();                
            }

            return dataTasks ?? new();
        }

        private static List<TModel> SortData<TModel>(List<TModel> childrenQuery, int level, EnumViewPlanning granularity) where TModel : GanttTaskBase
        {
            if (level == 1 && granularity == EnumViewPlanning.Priority)
            {
                return childrenQuery
                    .OrderBy(t => TryParsePriority(t.Priority))
                    .ThenBy(t => t.StartDate)
                    .ToList();

            }

            return SortData(childrenQuery);
        }

        private static EnumOrderPriority TryParsePriority(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return EnumOrderPriority.Normal;

            var cleaned = value.Replace(" ", "");

            if (Enum.TryParse(cleaned, ignoreCase: true, out EnumOrderPriority parsed))
                return parsed;

            return EnumOrderPriority.Normal;
        }

    }
}
