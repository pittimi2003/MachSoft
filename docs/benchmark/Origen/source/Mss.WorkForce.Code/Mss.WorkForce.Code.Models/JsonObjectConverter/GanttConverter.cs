using Mss.WorkForce.Code.Models;
using Mss.WorkForce.Code.Models.Common;
using Mss.WorkForce.Code.Models.DataAccess;
using Mss.WorkForce.Code.Models.DTO.Enums;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.JsonObjectConverter;
using Mss.WorkForce.Code.Models.ModelGantt;
using Mss.WorkForce.Code.Models.ModelInsert;
using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Web
{
    public class GanttConverter
    {
        private readonly DataAccess _dataAccess;

        #region AddResources

        private static ResourcesAssignmentsGantt loadResourcesAssignmentsForGantt(ItemPlanning taskChild)
        {
            var resourceAssigments = new ResourcesAssignmentsGantt
            {
                id = Guid.NewGuid(),
                taskId = taskChild.Id,
                resourceId = taskChild.ProcessId,
            };
            return resourceAssigments;
        }

        private static ResourcesAssignmentsGantt loadWarehouseResourcesAssignmentsForGantt(WarehouseProcessPlanning taskChild)
        {
            var resourceAssigments = new ResourcesAssignmentsGantt
            {
                id = Guid.NewGuid(),
                taskId = taskChild.Id,
                resourceId = taskChild.ProcessId,
            };
            return resourceAssigments;
        }

        private static ResourcesGantt loadResourceForGantt(ItemPlanning itemPlanning)
        {
            if (itemPlanning != null)
            {
                var resource = new ResourcesGantt
                {
                    id = itemPlanning.WorkerId,
                    text = itemPlanning.Worker?.Name ?? string.Empty,
                };
                return resource;
            }
            else
                return null;
        }

        private static ResourcesGantt loadWarehouseResourceForGantt(WarehouseProcessPlanning itemPlanning)
        {
            if (itemPlanning != null)
            {
                var resource = new ResourcesGantt
                {
                    id = itemPlanning.ProcessId,
                    text = itemPlanning.Process.Type,
                };
                return resource;
            }
            else
                return null;
        }

        #endregion

        #region AddTask

        private static TaskData getTaskLevelWorkFlow(IEnumerable<ItemPlanning> itemsPlanning, EnumViewPlanning granularity, WorkOrderPlanning workOrderPlanning, TaskData taskPlanning, TaskData taskWProcess, TaskData taskEstimated, ref Dictionary<string, Guid> levelFlow, UserFormatOptions userFormat)
        {
            string data;
            Guid flowId;
            var progress = 0.0;

            switch (granularity)
            {
                case EnumViewPlanning.Order:

                    data = taskPlanning.FillWorkFlow(workOrderPlanning.IsOutbound);

                    // Obtener o crear el Guid para el perfil
                    if (!levelFlow.TryGetValue(data, out flowId))
                    {
                        flowId = Guid.NewGuid();
                        levelFlow[data] = flowId;
                    }
                    else
                    {
                        return null;
                    }

                    return new TaskData(userFormat)
                    {
                        id = flowId,
                        parentId = workOrderPlanning.IsEstimated ? taskEstimated.id : taskPlanning.id,
                        IsOutbound = workOrderPlanning.IsOutbound,
                        TooltipType = GanttTooltipType.PlanningFlow,
                        progress = Convert.ToInt32(progress),
                        isParentData = true,
                        FillProgress = true,
                        levelTask = 2,

                    };
                case EnumViewPlanning.Priority:
                    data = workOrderPlanning.Priority ?? string.Empty;

                    // Obtener o crear el Guid para el perfil
                    if (!levelFlow.TryGetValue(data, out flowId))
                    {
                        flowId = Guid.NewGuid();
                        levelFlow[data] = flowId;                        
                    }
                    else
                    {
                        return null;
                    }

                    return new TaskData(userFormat)
                    {
                        id = flowId,
                        parentId = workOrderPlanning.IsEstimated ? taskEstimated.id : taskPlanning.id,
                        Priority = data,
                        IsOutbound = workOrderPlanning.IsOutbound,
                        TooltipType = GanttTooltipType.PlanningPriority,
                        progress = Convert.ToInt32(progress),
                        isParentData = true,
                        levelTask = 2,
                    };
                case EnumViewPlanning.Trailer:
                    data = workOrderPlanning.InputOrder?.VehicleCode ?? "No trailer";

                    // Obtener o crear el Guid para el perfil
                    if (!levelFlow.TryGetValue(data, out flowId))
                    {
                        flowId = Guid.NewGuid();
                        levelFlow[data] = flowId;
                    }
                    else
                    {
                        return null;
                    }

                    return new TaskData(userFormat)
                    {
                        id = flowId,
                        parentId = workOrderPlanning.IsEstimated ? taskEstimated.id : taskPlanning.id,
                        IsOutbound = workOrderPlanning.IsOutbound,
                        Trailer = data,
                        TooltipType = GanttTooltipType.PlanningTrailer,
                        progress = Convert.ToInt32(progress),
                        isParentData = true,
                        levelTask = 2,
                    };
                default:
                    data = taskPlanning.FillWorkFlow(workOrderPlanning.IsOutbound);

                    // Obtener o crear el Guid para el perfil
                    if (!levelFlow.TryGetValue(data, out flowId))
                    {
                        flowId = Guid.NewGuid();
                        levelFlow[data] = flowId;
                    }
                    else
                    {
                        return null;
                    }

                    return new TaskData(userFormat)
                    {
                        id = flowId,
                        parentId = workOrderPlanning.IsEstimated ? taskEstimated.id : taskPlanning.id,
                        TooltipType = GanttTooltipType.PlanningFlow,
                        progress = Convert.ToInt32(progress),
                        isParentData = true,
                        levelTask = 2,
                    };
            }

        }

        private static TaskData getTaskParent(EnumViewPlanning granularity, WorkOrderPlanning workOrderPlanning, TaskData taskPlanning, TaskData taskWProcess, TaskData taskEstimated, Dictionary<string, Guid> levelFlow, UserFormatOptions userFormat)
        {
            Guid parent;
            switch (granularity)
            {
                case EnumViewPlanning.Priority:
                    levelFlow.TryGetValue(workOrderPlanning.Priority, out parent);
                    break;
                case EnumViewPlanning.Trailer:
                    var trailerKey = workOrderPlanning.InputOrder.VehicleCode ?? "No trailer";
                    levelFlow.TryGetValue(trailerKey, out parent);
                    break;
                case EnumViewPlanning.Order:
                default:
                    levelFlow.TryGetValue(taskPlanning.FillWorkFlow(workOrderPlanning.IsOutbound), out parent);
                    break;
            }

            TaskData InformationGantt = new(userFormat);

            InformationGantt.id = workOrderPlanning.Id;
            InformationGantt.parentId = parent;
            InformationGantt.IsOutbound = workOrderPlanning.IsOutbound;
            InformationGantt.progress = Convert.ToInt32(workOrderPlanning.Progress);
            InformationGantt.DockName = workOrderPlanning.AssignedDock == null ? string.Empty : workOrderPlanning.AssignedDock.Zone.Name ?? string.Empty;
            InformationGantt.isEstimated = workOrderPlanning.IsEstimated;
            InformationGantt.isOnTime = workOrderPlanning.IsOnTime;
            InformationGantt.StartDate = workOrderPlanning.InitDate;
            InformationGantt.EndDate = workOrderPlanning.EndDate;
            InformationGantt.TooltipType = GanttTooltipType.PlanningOrder;
            InformationGantt.IsChildTask = false;
            InformationGantt.WorkFlow = granularity != EnumViewPlanning.Priority ? string.Empty : InformationGantt.WorkFlow;
            InformationGantt.InputOrderId = workOrderPlanning.InputOrderId;

            InformationGantt.TimeOrderDelay = workOrderPlanning.OrderDelay == null || workOrderPlanning.OrderDelay <= 0 ? string.Empty : TimeSpan.FromMinutes(workOrderPlanning.OrderDelay.Value / 60).ToString(@"hh\:mm"); ;
            InformationGantt.ActualWorkTime = workOrderPlanning.WorkTime == null ? "" : TimeSpan.FromMinutes(workOrderPlanning.WorkTime / 60).ToString(@"hh\:mm");
            InformationGantt.SLAMet = workOrderPlanning.SLAMet.GetSLAMetString();
            InformationGantt.SLATargetDateTime = workOrderPlanning.SLATarget ?? DateTimeOffset.MinValue;

            InformationGantt.levelTask = 3;

			InformationGantt.FillDataOrder(workOrderPlanning.InputOrder, granularity);
            return InformationGantt;
        }

        private static TaskData getTaskChild(ItemPlanning itemPlanning, UserFormatOptions userFormat)
        {
            TaskData taskForGantt = new(userFormat);

            taskForGantt.id = itemPlanning.Id;
            taskForGantt.Resource = itemPlanning.Worker?.Name ?? string.Empty;
            taskForGantt.parentId = itemPlanning.WorkOrderPlanningId;
            taskForGantt.ActivityTitle = itemPlanning.Process.Name;
            taskForGantt.progress = Convert.ToInt32(itemPlanning.Progress);
            taskForGantt.EquipmentGroup = itemPlanning.EquipmentGroup?.Name ?? string.Empty;
            taskForGantt.EquipmentType = itemPlanning.EquipmentGroup?.TypeEquipment.Name ?? string.Empty;
            taskForGantt.WorkTime = TimeFormatter.GetFormattedWorkTime(itemPlanning.WorkTime);
            taskForGantt.StartDate = itemPlanning.InitDate;
            taskForGantt.EndDate = itemPlanning.EndDate;
            taskForGantt.CommintedDate = itemPlanning.LimitDate;
            taskForGantt.DelayedOrder = itemPlanning.WorkOrderPlanning.IsOnTime ? "NOT DELAYED ORDER" : "DELAYED ORDER";
            taskForGantt.DockName = itemPlanning.WorkOrderPlanning.AssignedDock == null ? string.Empty : itemPlanning.WorkOrderPlanning.AssignedDock.Zone.Name ?? string.Empty;
            taskForGantt.IsOutbound = itemPlanning.WorkOrderPlanning.IsOutbound;
            taskForGantt.TooltipType = GanttTooltipType.PlanningActivity;
            taskForGantt.IsChildTask = true;
            taskForGantt.Shift = itemPlanning.Shift?.Name ?? string.Empty;
            taskForGantt.levelTask = 4;
            taskForGantt.FillDataActivity(itemPlanning.WorkOrderPlanning.InputOrder);
            return taskForGantt;

        }

        private static TaskData getTaskChildProcessPlanning(WarehouseProcessPlanning itemPlanning, Guid parent, UserFormatOptions userFormat)
        {
            var taskForGantt = new TaskData(userFormat)
            {
                id = itemPlanning.Id,
                isBlock = itemPlanning.IsBlocked,
                Resource = itemPlanning.Worker?.Name,
                parentId = parent,
                ActivityTitle = itemPlanning.Process.Type,
                WorkTime = TimeFormatter.GetFormattedWorkTime(itemPlanning.WorkTime),
                TooltipType = GanttTooltipType.PlanningActivity,
                StartDate = itemPlanning.InitDate,
                EndDate = itemPlanning.EndDate,
                CommintedDate = itemPlanning.LimitDate ?? DateTimeOffset.MinValue,
                IsChildTask = true,
            };

            return taskForGantt;
        }

        #endregion

        public static GanttDataConvertDto<TaskData> ConvertToGanttData(IEnumerable<ItemPlanning> itemsPlannings,
            IEnumerable<WarehouseProcessPlanning> processWarehouse, UserFormatOptions userFormat,
            EnumViewPlanning granularity = EnumViewPlanning.Priority)
        {
            GanttDataConvertDto<TaskData> propertiesGantt = new GanttDataConvertDto<TaskData>();

            // Hight Task
            TaskData taskPlanning = new TaskData(userFormat)
            {
                id = Guid.NewGuid(),
                IDCode = GetEnumDescription.GetItemDescription(EnumGanttLevels.Planning),
                TooltipType = GanttTooltipType.PlanningGeneral,
                FillProgress = true,
                isExpand = true,
                levelTask = 1,
            };

            TaskData taskWProcess = new TaskData(userFormat)
            {
                id = Guid.NewGuid(),
                IDCode = GetEnumDescription.GetItemDescription(EnumGanttLevels.WarehouseProcess),
                TooltipType = GanttTooltipType.PlanningWarehouse,
                isExpand = true,
                levelTask = 1,
            };

            TaskData taskEstimated = new TaskData(userFormat)
            {
                id = Guid.NewGuid(),
                IDCode = GetEnumDescription.GetItemDescription(EnumGanttLevels.EstimatedWorkLoad),
                TooltipType = GanttTooltipType.PlanningEstimation,
                isExpand = true,
                levelTask = 1,
            };

            // Tareas dentro de estimación
            TaskData taskEstimatedInput = new TaskData(userFormat)
            {
                id = Guid.NewGuid(),
                IDCode = GetEnumDescription.GetItemDescription(EnumGanttLevels.InboundWork),
                parentId = taskEstimated.id,
                IsOutbound = taskEstimated.IsOutbound,
                TooltipType = GanttTooltipType.PlanningEstimationIn,
                levelTask = 2,
            };

            TaskData taskEstimatedOutput = new TaskData(userFormat)
            {
                id = Guid.NewGuid(),
                IDCode = GetEnumDescription.GetItemDescription(EnumGanttLevels.OutboundWork),
                parentId = taskEstimated.id,
                IsOutbound = taskEstimated.IsOutbound,
                TooltipType = GanttTooltipType.PlanningEstimationOut,
                levelTask = 2,
            };


            propertiesGantt.TaskGantt.Add(taskPlanning);
            propertiesGantt.TaskGantt.Add(taskWProcess);
            propertiesGantt.TaskGantt.Add(taskEstimated);
            propertiesGantt.TaskGantt.Add(taskEstimatedInput);
            propertiesGantt.TaskGantt.Add(taskEstimatedOutput);

            GanttDataConvertDto<TaskData> propertiesWarehouse = AddTaskWarehouseProcessPlanning(processWarehouse, taskWProcess, userFormat);
            propertiesGantt.DependenciesGantt.AddRange(propertiesWarehouse.DependenciesGantt);
            propertiesGantt.ResourcesGantt.AddRange(propertiesWarehouse.ResourcesGantt);
            propertiesGantt.ResourcesAssignmentsGantt.AddRange(propertiesWarehouse.ResourcesAssignmentsGantt);
            propertiesGantt.TaskGantt.AddRange(propertiesWarehouse.TaskGantt);

            GanttDataConvertDto<TaskData> propertiesGanttOrderAndProcess = AddTaskOrdersAndProcess(itemsPlannings, taskPlanning, taskWProcess, taskEstimated, granularity, userFormat);
            propertiesGantt.DependenciesGantt.AddRange(propertiesGanttOrderAndProcess.DependenciesGantt);
            propertiesGantt.ResourcesGantt.AddRange(propertiesGanttOrderAndProcess.ResourcesGantt);
            propertiesGantt.ResourcesAssignmentsGantt.AddRange(propertiesGanttOrderAndProcess.ResourcesAssignmentsGantt);

            foreach (var task in propertiesGanttOrderAndProcess.TaskGantt)
            {
                if (task.isParentData ?? false)
                {
                    var filteredWorkOrders = propertiesGanttOrderAndProcess.TaskGantt
                      .Where(x => x.parentId == task.id);

                    double weightedSum = 0.0;
                    double totalDuration = 0.0;

                    var calc = filteredWorkOrders.Select(wo => new
                    {
                        weightedSum = (wo.progress / 100.00) * (wo.EndDate - wo.StartDate).TotalSeconds,
                        totalDuration = (wo.EndDate - wo.StartDate).TotalSeconds
                    });

                    var calcSum = calc.Sum(x => x.weightedSum) / calc.Sum(x => x.totalDuration);

                    task.progress = (int)(calcSum * 100.00);
                    task.FillProgress = false;
                }
            }

            propertiesGantt.TaskGantt.AddRange(propertiesGanttOrderAndProcess.TaskGantt);

            var estimated = SimulationToEstimatedConverter.ConvertPlanningToEstimation(itemsPlannings.Where(x => x.WorkOrderPlanning.IsEstimated).ToList());

            propertiesGantt.TaskGantt = TasksIndexConverter.AssignRootIndex(propertiesGantt.TaskGantt.Cast<TaskData>().ToList(), granularity);
            propertiesGantt = ConvertToGanttDataPreview(estimated, userFormat, propertiesGantt, taskEstimatedInput, taskEstimatedOutput, taskWProcess, itemsPlannings);
            return propertiesGantt;
        }

        public static GanttDataConvertDto<TaskData> ConvertToGanttDataPreview(PlanningPreview planning, UserFormatOptions userFormat,
            GanttDataConvertDto<TaskData> propertiesGantt = null,
            TaskData InputTask = null, TaskData OutputTask = null, TaskData WarehouseProcess = null, IEnumerable<ItemPlanning> itemsPlannings = null)
        {

            if (propertiesGantt == null)
            {
                propertiesGantt = new GanttDataConvertDto<TaskData>();

                InputTask = new TaskData(userFormat)
                {
                    id = Guid.NewGuid(),
                    IDCode = GetEnumDescription.GetItemDescription(EnumGanttLevels.InboundWork),
                    IsOutbound = false,
                    TooltipType = GanttTooltipType.PlanningEstimationInOrder,
                    levelTask = 1,
                };

                OutputTask = new TaskData(userFormat)
                {
                    id = Guid.NewGuid(),
                    IDCode = GetEnumDescription.GetItemDescription(EnumGanttLevels.OutboundWork),
                    IsOutbound = true,
                    TooltipType = GanttTooltipType.PlanningEstimationOutOrder,
                    levelTask = 1,
                };

                WarehouseProcess = new TaskData(userFormat)
                {
                    id = Guid.NewGuid(),
                    IDCode = GetEnumDescription.GetItemDescription(EnumGanttLevels.WarehouseProcess),
                    TooltipType = GanttTooltipType.PlanningWarehouseOrder,
                    levelTask = 1,
                };

                propertiesGantt.TaskGantt.Add(InputTask);
                propertiesGantt.TaskGantt.Add(OutputTask);
                propertiesGantt.TaskGantt.Add(WarehouseProcess);

                propertiesGantt.TaskGantt = TasksIndexConverter.AssignRootIndex((propertiesGantt.TaskGantt.Cast<TaskData>().ToList()));
            }

            var FilterInputs = planning.Entradas.ListaDeElementos.GroupBy(e => e.Type)
            .Select(g => new ItemPlanningPreview
            {
                Type = g.Key,
                Inicio = g.Min(e => e.Inicio),
                Fin = g.Max(e => e.Fin),
                Cantidad = g.Count(),
            })
            .ToList();

            var FilterOutputs = planning.Salidas.ListaDeElementos.GroupBy(e => e.Type)
            .Select(g => new ItemPlanningPreview
            {
                Type = g.Key,
                Inicio = g.Min(e => e.Inicio),
                Fin = g.Max(e => e.Fin),
                Cantidad = g.Count(),
            })
            .ToList();

            if (planning.WarehouseProcess != null)
            {
                var FilterWhProcess = planning.WarehouseProcess.ListaDeElementos.GroupBy(e => e.Type)
                .Select(g => new ItemPlanningPreview
                {
                    Type = g.Key,
                    Inicio = g.Min(e => e.Inicio),
                    Fin = g.Max(e => e.Fin)
                }).ToList();

                foreach (var item in FilterWhProcess)
                {
                    var task = new TaskData(userFormat)
                    {
                        id = Guid.NewGuid(),
                        parentId = WarehouseProcess.id,
                        StartDate = item.Inicio,
                        EndDate = item.Fin,
                        ActivityTitle = item.Type,
                        CommintedDate = planning.WarehouseProcess.Fin,
                        TooltipType = GanttTooltipType.PlanningActivity,
                        IsChildTask = true,
                        levelTask = 2,
                    };


                    propertiesGantt.TaskGantt.Add(task);

                    var resource = new ResourcesGantt
                    {
                        id = Guid.NewGuid(),
                        text = item.Type,
                    };

                    var resourceAssigments = new ResourcesAssignmentsGantt
                    {
                        id = Guid.NewGuid(),
                        taskId = task.id,
                        resourceId = resource.id,
                    };
                }

            }
            if (!(itemsPlannings is IEnumerable<ItemPlanning>))
                itemsPlannings = new List<ItemPlanning>();

            GanttDataConvertDto<TaskData> propertiesInputs = AddFilterInputs(FilterInputs, InputTask, planning, itemsPlannings.Where(x => x.WorkOrderPlanning.IsEstimated && !x.WorkOrderPlanning.IsOutbound), userFormat);
            propertiesGantt.DependenciesGantt.AddRange(propertiesInputs.DependenciesGantt);
            propertiesGantt.ResourcesGantt.AddRange(propertiesInputs.ResourcesGantt);
            propertiesGantt.ResourcesAssignmentsGantt.AddRange(propertiesInputs.ResourcesAssignmentsGantt);
            propertiesGantt.TaskGantt.AddRange(propertiesInputs.TaskGantt);

            GanttDataConvertDto<TaskData> propertiesOutputs = AddFilterOutputs(FilterOutputs, OutputTask, planning, itemsPlannings.Where(x => x.WorkOrderPlanning.IsEstimated && x.WorkOrderPlanning.IsOutbound), userFormat);
            propertiesGantt.DependenciesGantt.AddRange(propertiesOutputs.DependenciesGantt);
            propertiesGantt.ResourcesGantt.AddRange(propertiesOutputs.ResourcesGantt);
            propertiesGantt.ResourcesAssignmentsGantt.AddRange(propertiesOutputs.ResourcesAssignmentsGantt);
            propertiesGantt.TaskGantt.AddRange(propertiesOutputs.TaskGantt);


            return propertiesGantt;
        }


        private static GanttDataConvertDto<TaskData> AddTaskWarehouseProcessPlanning(IEnumerable<WarehouseProcessPlanning> processWarehouse, TaskData taskWProcess, UserFormatOptions userFormat)
        {
            GanttDataConvertDto<TaskData> propertiesGantt = new GanttDataConvertDto<TaskData>();
            foreach (var ptask in processWarehouse)
            {
                TaskData taskWarehouseDataChild = getTaskChildProcessPlanning(ptask, taskWProcess.id, userFormat);
                propertiesGantt.TaskGantt.Add(taskWarehouseDataChild);
                ResourcesGantt resourcesWarehouseGantt = loadWarehouseResourceForGantt(ptask);
                bool isExistWarehouseResource = propertiesGantt.ResourcesGantt.Any(r => r.id == resourcesWarehouseGantt.id);
                if (!isExistWarehouseResource)
                    propertiesGantt.ResourcesGantt.Add(resourcesWarehouseGantt);

                propertiesGantt.ResourcesAssignmentsGantt.Add(loadWarehouseResourcesAssignmentsForGantt(ptask));
            }

            return propertiesGantt;
        }

        private static GanttDataConvertDto<TaskData> AddTaskOrdersAndProcess(IEnumerable<ItemPlanning> itemsPlannings, TaskData taskPlanning, TaskData taskWProcess, TaskData taskEstimated, EnumViewPlanning granularity, UserFormatOptions userFormat)
        {
            GanttDataConvertDto<TaskData> propertiesGantt = new GanttDataConvertDto<TaskData>();
            Dictionary<string, Guid> flows = new Dictionary<string, Guid>();
            foreach (var taskChild in itemsPlannings.Where(x => !x.WorkOrderPlanning.IsEstimated))
            {

                if (taskChild.IsFaked != true)
                {
                    TaskData taskDataChild = getTaskChild(taskChild, userFormat);
                    propertiesGantt.TaskGantt.Add(taskDataChild);
                }


                TaskData taskWorkFlow = getTaskLevelWorkFlow(itemsPlannings, granularity, taskChild.WorkOrderPlanning, taskPlanning, taskWProcess, taskEstimated, ref flows, userFormat);
                TaskData taskParent = getTaskParent(granularity, taskChild.WorkOrderPlanning, taskPlanning, taskWProcess, taskEstimated, flows, userFormat);
                bool isExistTaskParent = propertiesGantt.TaskGantt.Any(x => x.id == taskParent.id);
                if (!isExistTaskParent)
                {
                    if (taskWorkFlow != null)
                        propertiesGantt.TaskGantt.Add(taskWorkFlow);
                    propertiesGantt.TaskGantt.Add(taskParent);
                }

                if (taskChild.IsFaked != true)
                {
                    ResourcesGantt resourcesGantt = loadResourceForGantt(taskChild);
                    bool isExistResource = propertiesGantt.ResourcesGantt.Any(r => r.id == resourcesGantt.id);
                    if (!isExistResource)
                        propertiesGantt.ResourcesGantt.Add(resourcesGantt);

                    propertiesGantt.ResourcesAssignmentsGantt.Add(loadResourcesAssignmentsForGantt(taskChild));
                }

            }

            return propertiesGantt;
        }

        private static GanttDataConvertDto<TaskData> AddFilterInputs(IEnumerable<ItemPlanningPreview> FilterInputs, TaskData InputTask, PlanningPreview planning, IEnumerable<ItemPlanning> itemsPlannings, UserFormatOptions userFormat)
        {
            GanttDataConvertDto<TaskData> propertiesGantt = new GanttDataConvertDto<TaskData>();
            if (FilterInputs.Any())
            {
                InputTask.StartDate = FilterInputs.OrderBy(f => f.Inicio).LastOrDefault().Inicio;
                InputTask.EndDate = FilterInputs.OrderBy(f => f.Inicio).LastOrDefault().Fin;
                InputTask.TotalOperators = GetOperators(itemsPlannings);
                InputTask.TotalEquipments = GetEquipments(itemsPlannings);
                InputTask.WorkTime = TimeFormatter.GetFormattedWorkTime(GetWorkedTime(itemsPlannings));
                InputTask.ProcessEstimated = FilterInputs;
                InputTask.progress = GetProgress(itemsPlannings);
                InputTask.levelTask = 1;
            }

            foreach (var item in FilterInputs)
            {
                var task = new TaskData(userFormat)
                {
                    id = Guid.NewGuid(),
                    parentId = InputTask.id,
                    StartDate = item.Inicio,
                    EndDate = item.Fin,
                    ActivityTitle = item.Type,
                    CommintedDate = planning.Entradas.Fin,
                    TotalOperators = GetOperators(itemsPlannings.Where(x => x.Process.Type == item.Type)),
                    IsOutbound = false,
                    WorkTime = TimeFormatter.GetFormattedWorkTime(GetWorkedTime(itemsPlannings.Where(x => x.Process.Type == item.Type))),
                    TooltipType = GanttTooltipType.PlanningActivity,
                    IsChildTask = true,
                    levelTask = 2,
                };

                propertiesGantt.TaskGantt.Add(task);

                var resource = new ResourcesGantt
                {
                    id = Guid.NewGuid(),
                    text = item.Type,
                };

                propertiesGantt.ResourcesGantt.Add(resource);

                var resourceAssigments = new ResourcesAssignmentsGantt
                {
                    id = Guid.NewGuid(),
                    taskId = task.id,
                    resourceId = resource.id,
                };
            }

            propertiesGantt.TaskGantt = TasksIndexConverter.AssignChildIndex(InputTask, propertiesGantt.TaskGantt, InputTask.index, 3);
            return propertiesGantt;
        }

        private static GanttDataConvertDto<TaskData> AddFilterOutputs(IEnumerable<ItemPlanningPreview> FilterOutputs, TaskData OutputTask, PlanningPreview planning, IEnumerable<ItemPlanning> itemsPlannings, UserFormatOptions userFormat)
        {
            GanttDataConvertDto<TaskData> propertiesGantt = new GanttDataConvertDto<TaskData>();
            if (FilterOutputs.Any())
            {
                OutputTask.StartDate = FilterOutputs.OrderBy(f => f.Inicio).LastOrDefault().Inicio;
                OutputTask.EndDate = FilterOutputs.OrderBy(f => f.Inicio).LastOrDefault().Fin;
                OutputTask.TotalOperators = GetOperators(itemsPlannings);
                OutputTask.TotalEquipments = GetEquipments(itemsPlannings);
                OutputTask.WorkTime = TimeFormatter.GetFormattedWorkTime(GetWorkedTime(itemsPlannings));
                OutputTask.ProcessEstimated = FilterOutputs;
                OutputTask.progress = GetProgress(itemsPlannings);
                OutputTask.levelTask = 1;
            }

            foreach (var item in FilterOutputs)
            {
                var task = new TaskData(userFormat)
                {
                    id = Guid.NewGuid(),
                    parentId = OutputTask.id,
                    StartDate = item.Inicio,
                    EndDate = item.Fin,
                    ActivityTitle = item.Type,
                    CommintedDate = planning.Entradas.Fin,
                    TotalOperators = GetOperators(itemsPlannings.Where(x => x.Process.Type == item.Type)),
                    IsOutbound = true,
                    WorkTime = TimeFormatter.GetFormattedWorkTime(GetWorkedTime(itemsPlannings.Where(x => x.Process.Type == item.Type))),
                    TooltipType = GanttTooltipType.PlanningActivity,
                    IsChildTask = true,
                    levelTask = 2,
                };


                propertiesGantt.TaskGantt.Add(task);

                var resource = new ResourcesGantt
                {
                    id = Guid.NewGuid(),
                    text = item.Type,
                };

                propertiesGantt.ResourcesGantt.Add(resource);

                var resourceAssigments = new ResourcesAssignmentsGantt
                {
                    id = Guid.NewGuid(),
                    taskId = task.id,
                    resourceId = resource.id,
                };
            }

            propertiesGantt.TaskGantt = TasksIndexConverter.AssignChildIndex(OutputTask, propertiesGantt.TaskGantt, OutputTask.index, 3);
            return propertiesGantt;
        }

        public static PlanningData ConvertOrdersToMetricsPlanning(IEnumerable<WorkOrderPlanning> workOrders, IEnumerable<ItemPlanning> simulationData, double Offset = 0)
        {
            string FormatSeconds(double seconds) =>
                TimeSpan.FromSeconds(Math.Max(0, seconds)).ToString(@"hh\:mm\:ss");

            var vehicleMetrics = workOrders
                .GroupBy(o =>
                {
                    bool isEstimated = o.IsEstimated || o.InputOrder == null;
                    var appt = o.AppointmentDate;

                    if (!isEstimated)
                    {
                        return new
                        {
                            VehicleCode = o.InputOrder.VehicleCode,
                            AppointmentDate = appt,
                            IsOutbound = o.InputOrder.IsOutbound
                        };
                    }

                    string hhmm = $"{appt:HH:mm}";
                    return new
                    {
                        VehicleCode = $"EST_{hhmm}_VEHICLE",
                        AppointmentDate = appt,
                        IsOutbound = o.IsOutbound
                    };
                })
                .Select(group =>
                {
                    var items = group.ToList();
                    var workOrderIds = items.Select(x => x.Id).ToHashSet();

                    int totalOrders = items.Count;

                    var appointment = group.Key.AppointmentDate;
                    var vehicleCode = group.Key.VehicleCode;
                    bool isOutbound = group.Key.IsOutbound;

                    double loadTimeSeconds = simulationData
                        .Where(ip =>
                            workOrderIds.Contains(ip.WorkOrderPlanningId) &&
                            ip.Process != null &&
                            ip.Process.Type == "Loading")
                        .Sum(ip => ip.WorkTime);


                    DateTime? appointmentEnd = null;
                    DateTime minInit = DateTime.MaxValue;
                    DateTime maxEnd = DateTime.MinValue;

                    int slaMetCount = 0;
                    int incompleteCount = 0;
                    bool allClosed = true;

                    double delaySum = 0;
                    double maxDelay = 0;

                    foreach (var o in items)
                    {
                        bool isEstimated = o.IsEstimated || o.InputOrder == null;
                        var status = isEstimated ? "Estimated" : o.InputOrder.Status;

                        if (o.AppointmentEndDate.HasValue && appointmentEnd == null)
                            appointmentEnd = o.AppointmentEndDate.Value;

                        if (o.InitDate < minInit) minInit = o.InitDate;
                        if (o.EndDate > maxEnd) maxEnd = o.EndDate;

                        if (o.SLAMet) slaMetCount++;
                        else incompleteCount++;

                        if (status != "Closed")
                            allClosed = false;

                        if (o.OrderDelay.HasValue)
                        {
                            double delay = o.OrderDelay.Value;
                            delaySum += delay;
                            if (delay > maxDelay) maxDelay = delay;
                        }
                    }

                    appointmentEnd ??= items
                        .Select(x => x.AppointmentEndDate)
                        .Where(d => d.HasValue)
                        .Select(d => d.Value)
                        .DefaultIfEmpty(maxEnd)
                        .Max();

                    double delaySeconds = (maxEnd - appointmentEnd.Value).TotalSeconds;
                    double slackSeconds = (appointmentEnd.Value - maxEnd).TotalSeconds;
                    double earlyStartSeconds = (appointment - minInit).TotalSeconds;

                    double otif = totalOrders == 0
                        ? 0
                        : (double)slaMetCount / totalOrders;

                    var orderMetrics = items.Select(o =>
                    {
                        bool isEstimated = o.IsEstimated || o.InputOrder == null;

                        string orderCode = isEstimated
                            ? $"EST_{appointment:HH:mm}_ORDER_{o.Id}"
                            : o.InputOrder.OrderCode;

                        string status = isEstimated ? "Estimated" : o.InputOrder.Status;

                        double delay = (o.EndDate - appointmentEnd.Value).TotalSeconds;
                        double slack = (appointmentEnd.Value - o.EndDate).TotalSeconds;

                        return new OrderMetricsDto
                        {
                            VehicleCode = vehicleCode,
                            OrderCode = orderCode,
                            OTIF = o.SLAMet ? 1 : 0,
                            IsClosed = status == "Closed",
                            Delay = FormatSeconds(delay),
                            Slack = FormatSeconds(slack)
                        };
                    }).ToList();

                    return new VehicleMetricsDto
                    {
                        VehicleAppointment = ConvertFromUtc(appointment,Offset),
                        Hour = $"{appointment:HH:mm}:00",
                        VehicleAppointmentEnd = ConvertFromUtc(appointmentEnd.Value,Offset),
                        VehicleTheoreticalDwellTime = $"{appointment:HH:mm} - {appointmentEnd:HH:mm}",
                        VehicleCode = vehicleCode,

                        LoadTime = loadTimeSeconds,

                        TotalOrders = totalOrders,
                        MinInit = ConvertFromUtc(minInit,Offset),
                        MaxEnd = ConvertFromUtc(maxEnd,Offset),

                        OTIF = otif,
                        Status = otif < 1,

                        Delay = FormatSeconds(delaySeconds),
                        AverageDelay = totalOrders > 0
                            ? FormatSeconds(delaySum / totalOrders)
                            : "00:00:00",

                        MaxDelay = FormatSeconds(maxDelay),
                        Slack = FormatSeconds(slackSeconds),
                        EarlyStart = FormatSeconds(earlyStartSeconds),

                        IsOut = isOutbound,
                        OrderMetrics = orderMetrics,
                        IncompleteOrders = incompleteCount,
                        IsClosed = allClosed
                    };
                })
                .OrderBy(x => x.VehicleAppointment)
                .ToList();

            return new PlanningData
            {
                VehicleMetrics = vehicleMetrics
            };
        }


      

        public static List<VehicleMetricsDto> ConvertOrdersToMetrics(IEnumerable<WorkOrderPlanningReturn> workOrders)
        {
            return workOrders
                .GroupBy(o => new { o.InputOrder.OrderSchedule.Id, o.InputOrder.VehicleCode, o.AppointmentDate, o.IsOutbound })
                .Select(g =>
                {
                    var totalOrders = g.Count();

                    var appointment = g.Key.AppointmentDate;
                    var isOut = g.Key.IsOutbound;

                    var appointmentEnd = g.First().AppointmentEndDate
                                         ?? g.Max(x => x.AppointmentEndDate)
                                         ?? g.Max(x => x.EndDate);
                   

                    // Tiempos reales
                    var minInit = g.Min(x => x.InitDate);
                    var maxEnd = g.Max(x => x.EndDate);

                    var realDuration = maxEnd - minInit;
                    var realHours = realDuration.TotalHours;

                    // Delay real respecto al compromiso
                    var delay = (maxEnd > appointmentEnd)
                                ? (maxEnd - appointmentEnd).TotalHours
                                : 0;

                    // Slack (tiempo sobrante antes del deadline)
                    var slack = (maxEnd < appointmentEnd)  ? (appointmentEnd - maxEnd).TotalHours: 0;

                    // Tiempo que empezó antes de la llegada del vehículo
                    var earlyStart = (appointment - minInit).TotalHours;

                    // Procesos
                    var totalProcesses = g.Sum(x => x.ItemPlanning?.Count ?? 0);

                    // OTIF
                    var otif = totalOrders == 0
                        ? 0
                        : g.Count(x => x.SLAMet) / (double)totalOrders;

                    string Format(double hours)
                        => TimeSpan.FromHours(hours).ToString(@"hh\:mm\:ss");

                    return new VehicleMetricsDto
                    {
                        VehicleAppointment = appointment,
                        VehicleAppointmentEnd = appointmentEnd,
                        VehicleTheoreticalDwellTime = $"{appointment:HH:mm} - {appointmentEnd:HH:mm}",
                        Hour = appointmentEnd.ToString("HH:mm:ss"),

                        TotalOrders = totalOrders,
                        TotalProcesses = totalProcesses,

                        MinInit = minInit,
                        MaxEnd = maxEnd,

                        OTIF = otif,

                        IsOut = isOut,

                        AverageDelay = Format(g.Average(x => x.OrderDelay ?? 0)),
                        MaxDelay = Format(g.Max(x => x.OrderDelay ?? 0)),

                        Workload = Format(realHours),
                        Delay = Format(delay),
                        Slack = Format(slack),
                        EarlyStart = Format(earlyStart),

                        orderSheduleId = g.Key.Id,
                    };
                })
                .OrderBy(x => x.VehicleAppointment)
                .ToList();
        }

        private static DateTime ConvertFromUtc(DateTime utc, double Offset)
        {
            if (utc.Kind != DateTimeKind.Utc)
                utc = DateTime.SpecifyKind(utc, DateTimeKind.Utc);

            var offset = TimeSpan.FromHours(Offset);

            return DateTime.SpecifyKind(utc + offset, DateTimeKind.Unspecified);
        }

        #region MethodsForGantt

        private static int GetEquipments(IEnumerable<ItemPlanning> itemsPlannings)
        {
            try
            {
                return itemsPlannings.Where(x => x.EquipmentGroup != null && x.EquipmentGroup.Name != null).Select(x => x.EquipmentGroup.Name).Distinct().Count();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private static int GetOperators(IEnumerable<ItemPlanning> itemsPlannings)
        {
            try
            {
                return itemsPlannings.Where(x => x.Worker != null && x.Worker.Name != null).Select(x => x.Worker.Name).Distinct().Count();
            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        private static double GetWorkedTime(IEnumerable<ItemPlanning> itemsPlannings) => itemsPlannings?.Sum(x => x.WorkTime) ?? 0;

        private static int GetProgress(IEnumerable<ItemPlanning> itemsPlannings)
        {
            try
            {
                int TotalProgress = 0;

                foreach (var item in itemsPlannings)
                {
                    if (item.WorkOrderPlanning.Progress != 0)
                        TotalProgress += Convert.ToInt32(item.WorkOrderPlanning.Progress);
                }

                return TotalProgress;

            }
            catch (Exception ex)
            {
                return 0;
            }
        }

        #endregion

    }
}
