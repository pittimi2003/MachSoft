using Mss.WorkForce.Code.ConfigurationCheckLogs;
using Mss.WorkForce.Code.Models.Constants;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Models.Resources;

namespace Mss.WorkForce.Code.DataBaseManager.ConfigurationCheck
{
    public class ProcessCheck
    {
        #region Variables
        private DataSimulatorTablaRequest? data;
        #endregion

        #region Constructor
        public ProcessCheck(DataSimulatorTablaRequest? data) { this.data = data; }
        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Checks if all the required configuration for the processes for a simulation is completed. 
        /// </summary>
        /// <returns>True if everything is completed, false if not</returns>
        public bool Check(ref ConfigCheck configCheck)
        {
            if (configCheck.Values == null) configCheck.Values = new List<string>();
            if (configCheck.KeyValuePairs == null) configCheck.KeyValuePairs = new Dictionary<string, List<ResourceMessage>>();
            if (!configCheck.KeyValuePairs.ContainsKey("Processes")) configCheck.KeyValuePairs["Processes"] = new List<ResourceMessage>();
            if (!configCheck.KeyValuePairs.ContainsKey("Flows")) configCheck.KeyValuePairs["Flows"] = new List<ResourceMessage>();

            bool processes = CheckProcesses(ref configCheck);
            bool orderLoadRatio = CheckOrderLoadRatios(ref configCheck);
            bool orderSchedules = CheckOrderSchedules(ref configCheck);
            bool processPriorityOrder = CheckProcessPriorityOrders(ref configCheck);

            return processes && orderLoadRatio && orderSchedules && processPriorityOrder ? true : false;
        }

        #endregion

        #region Private

        /// <summary>
        /// Checks if there is any mistake in the processes configuration
        /// </summary>
        /// <returns>True if everything is ok, false if not</returns>
        private bool CheckProcesses(ref ConfigCheck configCheck)
        {
            bool result = true;

            if (!this.data.Process.Any(x => x.IsInitProcess))
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_INITIAL]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_INITIAL]);
                
                configCheck.KeyValuePairs["Processes"]
                    .Add(new ResourceMessage(ResourceKeyConstants.PROCESS_NO_INITIAL,
                        ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_INITIAL]));
                
                result = false;
            }
            else
            {
                var processes = this.data.Process.Where(x => x.IsInitProcess && x.PercentageInitProcess == 0);

                foreach (var p in processes)
                {
                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_INIT_PERCENTAGE_INVALID], p.Name));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_INIT_PERCENTAGE_INVALID], p.Name));
                    
                    configCheck.KeyValuePairs["Processes"]
                        .Add(new ResourceMessage(ResourceKeyConstants.PROCESS_INIT_PERCENTAGE_INVALID,
                            ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_INIT_PERCENTAGE_INVALID], 
                            p.Name));
                    
                    result = false;
                }
            }

            if (!this.data.ProcessDirectionProperty.Any(x => x.InitProcessId == x.EndProcessId))
            {
                if (!this.data.ProcessDirectionProperty.Any(x => x.IsEnd))
                {
                    Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_ENDING]);
                    configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_ENDING]);

                    configCheck.KeyValuePairs["Processes"]
                        .Add(new ResourceMessage(ResourceKeyConstants.PROCESS_NO_ENDING,
                            ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_ENDING]));

                    result = false;
                }
                else
                {
                    var directionProperties = this.data.ProcessDirectionProperty.Where(x => x.Percentage == 0);

                    foreach (var p in directionProperties)
                    {
                        Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_DIRECTION_PERCENTAGE_INVALID], p.Name));
                        configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_DIRECTION_PERCENTAGE_INVALID], p.Name));

                        configCheck.KeyValuePairs["Processes"]
                            .Add(new ResourceMessage(ResourceKeyConstants.PROCESS_DIRECTION_PERCENTAGE_INVALID,
                                ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_DIRECTION_PERCENTAGE_INVALID],
                                p.Name));

                        result = false;
                    }

                    var initProcesses = this.data.Process.Where(x => x.IsInitProcess);
                    foreach (var p in initProcesses)
                    {
                        var initDirectionProperties = this.data.ProcessDirectionProperty.Where(x => x.InitProcessId == p.Id);

                        List<(ProcessDirectionProperty ProcessDirectionProperty, int Position)> processDirectionProperties = new();
                        foreach (var item in initDirectionProperties) processDirectionProperties.Add((item, 1));
                        List<ProcessDirectionProperty> firstProcessesForPath = new List<ProcessDirectionProperty>();
                        firstProcessesForPath.AddRange(initDirectionProperties);

                        int i = 1;
                        while (firstProcessesForPath.Any())
                        {
                            i++;

                            var iteration = firstProcessesForPath.Select(x => x.EndProcessId);
                            var nextProcessesForPath = new List<ProcessDirectionProperty>();
                            foreach (var firstProcessId in iteration)
                            {
                                var newProcesses = this.data.ProcessDirectionProperty.Where(x => x.InitProcessId == firstProcessId).ToList();

                                foreach (var process in newProcesses) processDirectionProperties.Add((process, i));
                                if (newProcesses.Any()) nextProcessesForPath.AddRange(newProcesses);
                            }

                            firstProcessesForPath = nextProcessesForPath;
                        }

                        //proteger Max() en vacío
                        if (!processDirectionProperties.Any())
                        {
                            Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_INIT_NO_PATHS], p.Name));
                            configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_INIT_NO_PATHS], p.Name));

                            configCheck.KeyValuePairs["Processes"]
                                .Add(new ResourceMessage(ResourceKeyConstants.PROCESS_INIT_NO_PATHS,
                                    ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_INIT_NO_PATHS],
                                    p.Name));

                            result = false;
                            continue;
                        }
                        var maxPosition = processDirectionProperties.Max(x => x.Position);

                        if (!processDirectionProperties.Where(x => x.Position == maxPosition).All(x => x.ProcessDirectionProperty.IsEnd))
                        {
                            foreach (var pdp in processDirectionProperties.Where(x => x.Position == maxPosition && !x.ProcessDirectionProperty.IsEnd))
                            {
                                // acceso seguro al nombre del proceso final
                                var endName = pdp.ProcessDirectionProperty.EndProcess?.Name ?? "(unknown)";
                                Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_PATH_NO_END], p.Name, endName));
                                configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_PATH_NO_END], p.Name, endName));

                                configCheck.KeyValuePairs["Processes"]
                                    .Add(new ResourceMessage(ResourceKeyConstants.PROCESS_PATH_NO_END,
                                        ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_PATH_NO_END],
                                        p.Name, endName));
                            }
                            result = false;
                        }
                    }
                }
            }
            else
            {
                var processNames = this.data.ProcessDirectionProperty.Where(x => x.InitProcessId == x.EndProcessId).Select(x => x.InitProcess.Name).Distinct();

                foreach (var p in processNames)
                {
                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_SAME_DIRECTIONPROPERTY], p));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_SAME_DIRECTIONPROPERTY], p));

                    configCheck.KeyValuePairs["Processes"]
                        .Add(new ResourceMessage(ResourceKeyConstants.PROCESS_SAME_DIRECTIONPROPERTY,
                            ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_SAME_DIRECTIONPROPERTY],
                            p));

                    result = false;
                }
            }

            var processesWithNoTime = this.data.Process.Where(x => x.MinTime == 0);
            foreach (var p in processesWithNoTime)
            {
                Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_TIME], p.Name));
                configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_TIME], p.Name));
                
                configCheck.KeyValuePairs["Processes"]
                    .Add(new ResourceMessage(ResourceKeyConstants.PROCESS_NO_TIME,
                        ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_TIME],
                        p.Name));
                
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Checks if there is any mistake in the order load ratios configuration
        /// </summary>
        /// <returns>True if everything is ok, false if not</returns>
        private bool CheckOrderLoadRatios(ref ConfigCheck configCheck)
        {
            if (!this.data.LoadProfile.Any())
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_LOAD_PROFILE]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_LOAD_PROFILE]);
                
                configCheck.KeyValuePairs["Processes"]
                    .Add(new ResourceMessage(ResourceKeyConstants.PROCESS_NO_LOAD_PROFILE,
                        ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_LOAD_PROFILE]));

                return false;
            }
            else
            {
                if (!this.data.VehicleProfile.Any())
                {
                    Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_VEHICLE_PROFILE]);
                    configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_VEHICLE_PROFILE]);

                    configCheck.KeyValuePairs["Processes"].Add(new ResourceMessage(ResourceKeyConstants.PROCESS_NO_VEHICLE_PROFILE,
                            ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_VEHICLE_PROFILE]));
                    
                    return false;
                }
                else
                {
                    if (!this.data.OrderLoadRatio.Any())
                    {
                        Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_ORDER_LOAD_RATIOS]);
                        configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_ORDER_LOAD_RATIOS]);
                        
                        configCheck.KeyValuePairs["Processes"]
                            .Add(new ResourceMessage(ResourceKeyConstants.PROCESS_NO_ORDER_LOAD_RATIOS,
                            ResourceDefinitions.Messages[ResourceKeyConstants.PROCESS_NO_ORDER_LOAD_RATIOS]));

                        return false;
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks if there is any mistake in the order schedules configuration
        /// </summary>
        /// <returns>True if everything is ok, false if not</returns>
        private bool CheckOrderSchedules(ref ConfigCheck configCheck)
        {
            bool result = true;

            if (!this.data.OrderSchedule.Any())
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_ORDER_SCHEDULES]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_ORDER_SCHEDULES]);
                
                configCheck.KeyValuePairs["Flows"]
                    .Add(new ResourceMessage(ResourceKeyConstants.FLOW_NO_ORDER_SCHEDULES,
                        ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_ORDER_SCHEDULES]));
                
                result = false;
            }

            if (this.data.OrderSchedule.Any(x => x.IsOut) && !this.data.OutboundFlowGraph.Any())
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.OUTBOUND_NO_GRAPH]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.OUTBOUND_NO_GRAPH]);

                configCheck.KeyValuePairs["Flows"]
                    .Add(new ResourceMessage(ResourceKeyConstants.OUTBOUND_NO_GRAPH,
                        ResourceDefinitions.Messages[ResourceKeyConstants.OUTBOUND_NO_GRAPH]));

                result = false;
            }
            else if (this.data.OrderSchedule.Any(x => x.IsOut) && this.data.OutboundFlowGraph.Any())
            {
                if (this.data.OutboundFlowGraph.Count > 1)
                {
                    Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.OUTBOUNDFLOW_NOT_UNIQUE]);
                    configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.OUTBOUNDFLOW_NOT_UNIQUE]);

                    configCheck.KeyValuePairs["Flows"]
                        .Add(new ResourceMessage(ResourceKeyConstants.OUTBOUNDFLOW_NOT_UNIQUE,
                            ResourceDefinitions.Messages[ResourceKeyConstants.OUTBOUNDFLOW_NOT_UNIQUE]));

                    result = false;
                }
            }

            if (this.data.OrderSchedule.Any(x => !x.IsOut) && !this.data.InboundFlowGraph.Any())
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.INBOUND_NO_GRAPH]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.INBOUND_NO_GRAPH]);

                configCheck.KeyValuePairs["Flows"]
                    .Add(new ResourceMessage(ResourceKeyConstants.INBOUND_NO_GRAPH,
                        ResourceDefinitions.Messages[ResourceKeyConstants.INBOUND_NO_GRAPH]));

                result = false;
            }
            else if (this.data.OrderSchedule.Any(x => !x.IsOut) && this.data.InboundFlowGraph.Any())
            {
                if (this.data.InboundFlowGraph.Count > 1)
                {
                    Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.INBOUNDFLOW_NOT_UNIQUE]);
                    configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.INBOUNDFLOW_NOT_UNIQUE]);

                    configCheck.KeyValuePairs["Flows"]
                        .Add(new ResourceMessage(ResourceKeyConstants.INBOUNDFLOW_NOT_UNIQUE,
                            ResourceDefinitions.Messages[ResourceKeyConstants.INBOUNDFLOW_NOT_UNIQUE]));

                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if there is any mistake in the process priority orders configuration
        /// </summary>
        /// <returns>True if everything is ok, false if not</returns>
        private bool CheckProcessPriorityOrders(ref ConfigCheck configCheck)
        {
            if (!this.data.ProcessPriorityOrder.Any(x => x.IsActive))
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_PROCESS_PRIORITIES]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_PROCESS_PRIORITIES]);

                configCheck.KeyValuePairs["Flows"]
                    .Add(new ResourceMessage(ResourceKeyConstants.FLOW_NO_PROCESS_PRIORITIES,
                        ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_PROCESS_PRIORITIES]));

                return false;
            }

            return true;
        }

        #endregion

        #endregion
    }
}
