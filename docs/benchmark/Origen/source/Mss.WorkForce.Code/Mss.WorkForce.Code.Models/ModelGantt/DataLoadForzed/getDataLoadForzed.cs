namespace Mss.WorkForce.Code.Models.ModelGantt
{
    public class getDataLoadForzed
    {
        public static GanttDataConvertDto<TaskData> GenerateData()
        {
            GanttDataConvertDto<TaskData> propertiesGantt = new GanttDataConvertDto<TaskData>();
            try
            {
                propertiesGantt.TaskGantt = new List<TaskData>();
                propertiesGantt.DependenciesGantt = new List<DependenciesGantt>();

                List<TaskData> tasksPlanning = GenerateDataForGantt(1, "Planning", "Input", 50, 1);
                propertiesGantt.TaskGantt = AddElementsToList(propertiesGantt.TaskGantt, tasksPlanning);

                List<TaskData> tasksWarehouseProcess = GenerateDataForGantt(2, "WarehouseProcess", "Out", 50, 2);
                propertiesGantt.TaskGantt = AddElementsToList(propertiesGantt.TaskGantt, tasksWarehouseProcess);

                List<TaskData> tasksPreview = GenerateDataForGantt(3, "Preview", "Out", 100, 2);
                propertiesGantt.TaskGantt = AddElementsToList(propertiesGantt.TaskGantt, tasksPreview);

                List<DependenciesGantt> dependenciesPlanning = dataDependenciesForGantt(tasksPlanning, 10);
                propertiesGantt.DependenciesGantt = AddElementsToList(propertiesGantt.DependenciesGantt, dependenciesPlanning);

                List<DependenciesGantt> dependenciesWarehouseProcess = dataDependenciesForGantt(tasksWarehouseProcess, 18);
                propertiesGantt.DependenciesGantt = AddElementsToList(propertiesGantt.DependenciesGantt, dependenciesWarehouseProcess);

                List<DependenciesGantt> dependenciesPreview = dataDependenciesForGantt(tasksPreview, 20);
                propertiesGantt.DependenciesGantt = AddElementsToList(propertiesGantt.DependenciesGantt, dependenciesPreview);

                propertiesGantt.ResourcesGantt = dataResourcesForGantt(80);
                propertiesGantt.ResourcesAssignmentsGantt = dataAsignmentsGantt(propertiesGantt.TaskGantt, propertiesGantt.ResourcesGantt, 100);

                return propertiesGantt;
            }
            catch (Exception ex)
            {
                return propertiesGantt;
            }

        }

        private static List<TaskData> AddElementsToList(List<TaskData> taskGantt, List<TaskData> listToElements)
        {
            foreach (var element in listToElements)
            {
                bool isExist = taskGantt.Any(r => r.id == element.id);
                if (!isExist)
                    taskGantt.Add(element);
            }
            return taskGantt;
        }

        private static List<DependenciesGantt> AddElementsToList(List<DependenciesGantt> dependenciesGantt, List<DependenciesGantt> listToElements)
        {
            foreach (var element in listToElements)
            {
                bool isExist = dependenciesGantt.Any(r => r.id == element.id);
                if (!isExist)
                    dependenciesGantt.Add(element);
            }
            return dependenciesGantt;
        }


        public static GanttDataConvertDto<TaskData> GenerateDataForFirtsGantt()
        {
            GanttDataConvertDto<TaskData> propertiesGantt = new GanttDataConvertDto<TaskData>();
            propertiesGantt.TaskGantt = GenerateDataForGantt(1, "Planning", "Input", 50, 1);
            propertiesGantt.DependenciesGantt = dataDependenciesForGantt(propertiesGantt.TaskGantt, 32);
            propertiesGantt.ResourcesGantt = dataResourcesForGantt(20);
            propertiesGantt.ResourcesAssignmentsGantt = dataAsignmentsGantt(propertiesGantt.TaskGantt, propertiesGantt.ResourcesGantt, 40);
            return propertiesGantt;
        }

        public static GanttDataConvertDto<TaskData> GenerateDataForSecondGantt()
        {
            GanttDataConvertDto<TaskData> propertiesGantt = new GanttDataConvertDto<TaskData>();
            propertiesGantt.TaskGantt = GenerateDataForGantt(2, "WarehouseProcess", "Out", 50, 2);
            propertiesGantt.DependenciesGantt = dataDependenciesForGantt(propertiesGantt.TaskGantt, 25);
            propertiesGantt.ResourcesGantt = dataResourcesForGantt(20);
            propertiesGantt.ResourcesAssignmentsGantt = dataAsignmentsGantt(propertiesGantt.TaskGantt, propertiesGantt.ResourcesGantt, 32);
            return propertiesGantt;
        }

        public static GanttDataConvertDto<TaskData> GenerateDataForThirdGantt()
        {
            GanttDataConvertDto<TaskData> propertiesGantt = new GanttDataConvertDto<TaskData>();
            propertiesGantt.TaskGantt = GenerateDataForGantt(3, "Preview", "Out", 100, 2);
            propertiesGantt.DependenciesGantt = dataDependenciesForGantt(propertiesGantt.TaskGantt, 68);
            propertiesGantt.ResourcesGantt = dataResourcesForGantt(10);
            propertiesGantt.ResourcesAssignmentsGantt = dataAsignmentsGantt(propertiesGantt.TaskGantt, propertiesGantt.ResourcesGantt, 65);
            return propertiesGantt;
        }

        private static List<DependenciesGantt> dataDependenciesForGantt(List<TaskData> taskGantts, int numDependencies)
        {
            Random random = new Random();
            List<DependenciesGantt> dependencies = new List<DependenciesGantt>();
            for (int i = 0; i < numDependencies; i++)
            {
                DependenciesGantt dependenciesGantt = new DependenciesGantt
                {
                    id = Guid.NewGuid(),
                    predecessorId = taskGantts[random.Next(taskGantts.Count)].id,
                    successorId = taskGantts[random.Next(taskGantts.Count)].id,
                };
                bool isExistDependencie = dependencies.Any(r => r.id == dependenciesGantt.id);
                if (!isExistDependencie)
                    dependencies.Add(dependenciesGantt);

            }
            return dependencies;
        }
        private static List<ResourcesGantt> dataResourcesForGantt(int numTotal)
        {
            List<ResourcesGantt> resourcesGantts = new List<ResourcesGantt>();
            try
            {
                Random random = new Random();
                string[] resuorces = { "Management", "Project Manager", "Analyst", "Developer", "Testers", "Trainers", "Technical Communicators", "Deployment Team" };
                for (int i = 0; i < resuorces.Length; i++)
                {
                    var res = new ResourcesGantt
                    {
                        id = Guid.NewGuid(),
                        text = resuorces[i]
                    };

                    bool isExist = resourcesGantts.Any(r => r.id == res.id);
                    if (!isExist)
                        resourcesGantts.Add(res);
                }
                return resourcesGantts;
            }
            catch (Exception ex)
            {
                return resourcesGantts;
            }

        }
        private static List<ResourcesAssignmentsGantt> dataAsignmentsGantt(List<TaskData> listTask, List<ResourcesGantt> listResources, int numTotal)
        {
            Random random = new Random();
            List<ResourcesAssignmentsGantt> resourcesAssignmentsGantts = new List<ResourcesAssignmentsGantt>();
            for (int i = 0; i < listResources.Count; i++)
            {
                ResourcesAssignmentsGantt resourcesAssignments = new ResourcesAssignmentsGantt
                {
                    id = Guid.NewGuid(),
                    taskId = listTask[random.Next(listTask.Count)].id,
                    resourceId = listResources[i].id
                };

                resourcesAssignmentsGantts.Add(resourcesAssignments);
                var task = listTask.FirstOrDefault(x => x.id == resourcesAssignments.taskId);
                if (task != null)
                    task.Resource = string.Join(", ", listResources.Where(r => r.id == resourcesAssignments.resourceId).Select(r => r.text));

            }

            return resourcesAssignmentsGantts;
        }

        public static List<TaskData> GenerateDataForGantt(int idCode, string taskTitleParent, string activity, int numData, int activityType)
        {
            List<TaskData> propertiesTask = new List<TaskData>();
            TaskData taskPrincipal = GenerateDataPrincipalForGantt(idCode, taskTitleParent, activity, activityType);
            propertiesTask.Add(taskPrincipal);

            List<TaskData> listTaskParent = GenerateParentForGantt(taskPrincipal, 10);
            foreach (TaskData task in listTaskParent)
            {
                bool isExist = propertiesTask.Any(r => r.id == task.id);
                if (!isExist)
                    propertiesTask.Add(task);
            }

            List<TaskData> listTaskChildren = GenerateChildrenForGantt(listTaskParent, numData);
            foreach (TaskData task in listTaskChildren)
            {
                bool isExist = propertiesTask.Any(r => r.id == task.id);
                if (!isExist)
                    propertiesTask.Add(task);
            }

            return propertiesTask;

        }


        private static TaskData GenerateDataPrincipalForGantt(int idCode, string title, string activity, int activityType)
        {
            return new TaskData
            {
                id = Guid.NewGuid(),
                IDCode = idCode.ToString(),
                ActivityTitle = activity,
                title = title,
                CommintedDate = DateTime.Parse("7:00 PM"),
            };
        }

        private static List<TaskData> GenerateParentForGantt(TaskData taskPrincipal, int numParents)
        {
            Random random = new Random();
            List<TaskData> propertiesTask = new List<TaskData>();
            string[] activities = { "Input", "Out", "Reception", "" };

            string period = random.Next(0, 2) == 0 ? "AM" : "PM";
            for (int i = 0; i < numParents; i++)
            {
                var task = new TaskData
                {
                    id = Guid.NewGuid(),
                    isBlock = i % 2 == 0,
                    parentId = taskPrincipal.id,
                    ActivityTitle = activities[i % activities.Length],
                    CommintedDate = DateTime.Parse($"{random.Next(6, 12)}:00 PM"),
                    Customer = "Customer name",
                    DockName = "01",
                    Trailer = "1234 GHG",
                };

                bool isExist = propertiesTask.Any(r => r.id == task.id);
                if (!isExist)
                    propertiesTask.Add(task);
            }

            return propertiesTask;

        }

        private static List<TaskData> GenerateChildrenForGantt(List<TaskData> listParent, int numData)
        {
            Random random = new Random();
            string[] activities = { "Download", "Reception", "Location", "Picking", "Shipping", "Upload", "" };
            string[] titles = { "", "S5871", "Repo01", "S1235", "E1111" };
            List<TaskData> propertiesTask = new List<TaskData>();
            int day = 18; 
            DateTime lastEnd = new DateTime(2024, 8, day, random.Next(3, 5), random.Next(0, 40), 0);
            for (int i = 0; i < numData; i++)
            {
                var taskStart = lastEnd.AddMinutes(random.Next(5, 20));
                var taskEnd = taskStart.AddHours(random.Next(1, 2));
                var task = new TaskData
                {
                    id = Guid.NewGuid(),
                    parentId = listParent[random.Next(listParent.Count)].id,
                    title = titles[i % titles.Length],
                    ActivityTitle = activities[i % activities.Length],
                    StartDate = DateTimeOffset.Now,
                    EndDate = DateTimeOffset.Now,
                    progress = random.Next(0, 101),
                };

                bool isExist = propertiesTask.Any(r => r.id == task.id);
                if (!isExist)
                {
                    propertiesTask.Add(task);
                    lastEnd = taskEnd;
                }
            }
            return propertiesTask;
        }
    }
}
