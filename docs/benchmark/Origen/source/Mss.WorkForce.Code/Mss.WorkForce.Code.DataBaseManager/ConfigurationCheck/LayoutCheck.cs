using Mss.WorkForce.Code.ConfigurationCheckLogs;
using Mss.WorkForce.Code.Models.Constants;
using Mss.WorkForce.Code.Models.DTO.Designer;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Models.Resources;

namespace Mss.WorkForce.Code.DataBaseManager.ConfigurationCheck
{
    public class LayoutCheck
    {
        #region Variables
        private DataSimulatorTablaRequest? data;
        #endregion

        #region Constructor
        public LayoutCheck(DataSimulatorTablaRequest? data) { this.data = data; }
        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Checks if all the required configuration in the layout for a simulation is completed. 
        /// </summary>
        /// <returns>True if everything is completed, false if not</returns>
        public bool Check(ref ConfigCheck configCheck)
        {
            if (!this.data.Area.Any())
            {
                AddMessages(ref configCheck, ResourceKeyConstants.AREAS_NOT_CREATED, "Areas");
                AddMessages(ref configCheck, ResourceKeyConstants.ROUTES_NOT_CREATED, "Routes");
                AddMessages(ref configCheck, ResourceKeyConstants.DOCKS_NOT_CREATED, "Docks");
                AddMessages(ref configCheck, ResourceKeyConstants.ZONES_NOT_CREATED, "Zones");
                return false;
            }
            else
            {
                bool result = true;

                foreach (var a in this.data.Area)
                {
                    if (!this.data.Zone.Any(x => x.AreaId == a.Id))
                    {
                        Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.AREA_NO_ZONES], a.Name));
                        configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.AREA_NO_ZONES], a.Name));

                        configCheck.KeyValuePairs["Areas"]
                            .Add(new ResourceMessage(ResourceKeyConstants.AREA_NO_ZONES,
                                ResourceDefinitions.Messages[ResourceKeyConstants.AREA_NO_ZONES],
                                a.Name));

                        result = false;
                    }
                }

                bool routes = false;
                if (this.data.Route.Any()) routes = CheckRoutes(ref configCheck);
                else AddMessages(ref configCheck, ResourceKeyConstants.ROUTES_NOT_CREATED, "Routes");

                bool docks = false;
                if (this.data.Dock.Any()) docks = CheckDocks(ref configCheck);
                else AddMessages(ref configCheck, ResourceKeyConstants.DOCKS_NOT_CREATED, "Docks");

                bool zones = false;
                if (this.data.Zone.Any()) zones = CheckZones(ref configCheck);
                else AddMessages(ref configCheck, ResourceKeyConstants.ZONES_NOT_CREATED, "Zones");

                bool flows = CheckFlows(ref configCheck);

                return result && routes && docks && zones && flows ? true : false;

            }
        }

        #endregion

        #region Private

        private void AddMessages(ref ConfigCheck configCheck, string message, string keyValuePairs)
        {
            Console.WriteLine(ResourceDefinitions.Messages[message]);
            configCheck.Values.Add(ResourceDefinitions.Messages[message]);

            configCheck.KeyValuePairs[keyValuePairs]
                .Add(new ResourceMessage(message,
                    ResourceDefinitions.Messages[message]));
        }

        /// <summary>
        /// Checks if there is any mistake in the routes configuration
        /// </summary>
        /// <returns>True if everything is ok, false if not</returns>
        private bool CheckRoutes(ref ConfigCheck configCheck)
        {
            var routes = new Dijkstra(this.data).CreateRoutes(this.data.Warehouse.Id);
            if (routes.Any(x => x.Time == int.MaxValue))
            {
                var noRoutes = routes.Where(x => x.Time == int.MaxValue);

                foreach (var r in noRoutes)
                {
                    var departureArea = this.data.Area.FirstOrDefault(x => x.Id == r.DepartureAreaId)?.Name ?? "Unknown";
                    var arrivalArea = this.data.Area.FirstOrDefault(x => x.Id == r.ArrivalAreaId)?.Name ?? "Unknown";

                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.ROUTE_NOT_FOUND], departureArea, arrivalArea));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.ROUTE_NOT_FOUND], departureArea, arrivalArea));

                    configCheck.KeyValuePairs["Routes"]
                        .Add(new ResourceMessage(ResourceKeyConstants.ROUTE_NOT_FOUND,
                            ResourceDefinitions.Messages[ResourceKeyConstants.ROUTE_NOT_FOUND],
                            departureArea, arrivalArea));
                }

                return false;
            }

            if (!routes.Any())
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.ROUTES_NOT_CONFIGURED]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.ROUTES_NOT_CONFIGURED]);

                configCheck.KeyValuePairs["Routes"]
                    .Add(new ResourceMessage(ResourceKeyConstants.ROUTES_NOT_CONFIGURED,
                            ResourceDefinitions.Messages[ResourceKeyConstants.ROUTES_NOT_CONFIGURED]));
                return false;
            }

            return true;
        }

        /// <summary>
        /// Checks if there is any mistake in the docks configuration
        /// </summary>
        /// <returns>True if everything is ok, false if not</returns>
        private bool CheckDocks(ref ConfigCheck configCheck)
        {
            bool result = true;

            var dockProcesses = this.data.Process.Where(x => x.Type == ProcessType.Loading || x.Type == ProcessType.Inbound);
            foreach (var p in dockProcesses)
            {
                var isOut = p.Type == ProcessType.Loading ? true : false;

                if (isOut)
                {
                    if (!this.data.Dock.Any(x => x.Zone.AreaId == p.AreaId && x.AllowOutbound))
                    {
                        Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_OUTBOUND], p.Name));
                        configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_OUTBOUND], p.Name));

                        configCheck.KeyValuePairs["Docks"]
                            .Add(new ResourceMessage(ResourceKeyConstants.DOCKS_NO_OUTBOUND,
                                ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_OUTBOUND],
                                p.Name));

                        result = false;
                    }
                }
                else
                {
                    if (!this.data.Dock.Any(x => x.Zone.AreaId == p.AreaId && x.AllowInbound))
                    {
                        Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_INBOUND], p.Name));
                        configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_INBOUND], p.Name));

                        configCheck.KeyValuePairs["Docks"]
                            .Add(new ResourceMessage(ResourceKeyConstants.DOCKS_NO_INBOUND,
                                ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_INBOUND],
                                p.Name));

                        result = false;
                    }
                }
            }

            if (this.data.Dock.Any(x => x.AllowInbound) && !this.data.Dock.Any(x => x.InboundRange != null))
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_INBOUND_RANGES]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_INBOUND_RANGES]);

                configCheck.KeyValuePairs["Docks"]
                    .Add(new ResourceMessage(ResourceKeyConstants.DOCKS_NO_INBOUND_RANGES,
                        ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_INBOUND_RANGES]));

                result = false;
            }
            else
            {
                if (this.data.Dock.Where(x => x.AllowInbound).GroupBy(x => x.InboundRange).Any(x => x.Count() > 1))
                {
                    Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_DUPLICATE_INBOUND_RANGES]);
                    configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_DUPLICATE_INBOUND_RANGES]);

                    configCheck.KeyValuePairs["Docks"]
                        .Add(new ResourceMessage(ResourceKeyConstants.DOCKS_DUPLICATE_INBOUND_RANGES,
                            ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_DUPLICATE_INBOUND_RANGES]));

                    result = false;
                }
            }

            if (this.data.Dock.Any(x => x.AllowOutbound) && !this.data.Dock.Any(x => x.OutboundRange != null))
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_OUTBOUND_RANGES]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_OUTBOUND_RANGES]);

                configCheck.KeyValuePairs["Docks"]
                    .Add(new ResourceMessage(ResourceKeyConstants.DOCKS_NO_OUTBOUND_RANGES,
                        ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_OUTBOUND_RANGES]));

                result = false;
            }
            else
            {
                if (this.data.Dock.Where(x => x.AllowOutbound).GroupBy(x => x.OutboundRange).Any(x => x.Count() > 1))
                {
                    Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_DUPLICATE_OUTBOUND_RANGES]);
                    configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_DUPLICATE_OUTBOUND_RANGES]);

                    configCheck.KeyValuePairs["Docks"]
                        .Add(new ResourceMessage(ResourceKeyConstants.DOCKS_DUPLICATE_OUTBOUND_RANGES,
                            ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_DUPLICATE_OUTBOUND_RANGES]));

                    result = false;
                }
            }

            if (!this.data.DockSelectionStrategy.Any())
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_SELECTION_STRATEGY]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_SELECTION_STRATEGY]);

                configCheck.KeyValuePairs["Docks"]
                    .Add(new ResourceMessage(ResourceKeyConstants.DOCKS_NO_SELECTION_STRATEGY,
                        ResourceDefinitions.Messages[ResourceKeyConstants.DOCKS_NO_SELECTION_STRATEGY]));

                result = false;
            }

            return result;
        }

        /// <summary>
        /// Checks if there is any mistake in the zones configuration
        /// </summary>
        /// <returns>True if everything is ok, false if not</returns>
        private bool CheckZones(ref ConfigCheck configCheck)
        {
            bool result = true;

            foreach (var z in this.data.Zone)
            {
                // Comprobación de capacidades de contenedores
                // Comprobación de capacidades de equipamientos
                // Si esa zona tiene NarrowAisle y está a true, mirar si tiene MaxPickers

                if (z.MaxContainers == null || z.MaxContainers <= 0)
                {
                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.ZONE_NO_CONTAINERS_CAPACITY], z.Name));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.ZONE_NO_CONTAINERS_CAPACITY], z.Name));

                    configCheck.KeyValuePairs["Zones"]
                        .Add(new ResourceMessage(ResourceKeyConstants.ZONE_NO_CONTAINERS_CAPACITY,
                            ResourceDefinitions.Messages[ResourceKeyConstants.ZONE_NO_CONTAINERS_CAPACITY], z.Name));

                    result = false;
                }

                if (z.MaxEquipments != null && z.MaxEquipments == 0)
                {
                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.ZONE_NO_EQUIPMENTS_CAPACITY], z.Name));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.ZONE_NO_EQUIPMENTS_CAPACITY], z.Name));

                    configCheck.KeyValuePairs["Zones"]
                        .Add(new ResourceMessage(ResourceKeyConstants.ZONE_NO_EQUIPMENTS_CAPACITY,
                            ResourceDefinitions.Messages[ResourceKeyConstants.ZONE_NO_EQUIPMENTS_CAPACITY],
                            z.Name));

                    result = false;
                }

                if (z.Type == ZoneType.Rack.ToString())
                {
                    var rack = this.data.Rack.FirstOrDefault(x => x.ZoneId == z.Id);

                    if (rack.NarrowAisle != null && rack.NarrowAisle.Value && rack.MaxPickers == 0)
                    {
                        Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.NARROW_AISLE_ZERO_PICKERS], z.Name));
                        configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.NARROW_AISLE_ZERO_PICKERS], z.Name));

                        configCheck.KeyValuePairs["Zones"]
                            .Add(new ResourceMessage(ResourceKeyConstants.NARROW_AISLE_ZERO_PICKERS,
                                ResourceDefinitions.Messages[ResourceKeyConstants.NARROW_AISLE_ZERO_PICKERS],
                                z.Name));

                        result = false;
                    }
                }

                if (z.Type == ZoneType.DriveIn.ToString())
                {
                    var driveIn = this.data.DriveIn.FirstOrDefault(x => x.ZoneId == z.Id);

                    if (driveIn.NarrowAisle != null && driveIn.NarrowAisle.Value && driveIn.MaxPickers == 0)
                    {
                        Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.NARROW_AISLE_ZERO_PICKERS], z.Name));
                        configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.NARROW_AISLE_ZERO_PICKERS], z.Name));

                        configCheck.KeyValuePairs["Zones"]
                            .Add(new ResourceMessage(ResourceKeyConstants.NARROW_AISLE_ZERO_PICKERS,
                                ResourceDefinitions.Messages[ResourceKeyConstants.NARROW_AISLE_ZERO_PICKERS],
                                z.Name));

                        result = false;
                    }
                }

                if (z.Type == ZoneType.Stage.ToString())
                {
                    var stage = this.data.Stage.FirstOrDefault(x => x.ZoneId == z.Id);
                    var associatedDocks = this.data.AvailableDocksPerStage.Where(x => x.StageId == stage.Id);

                    if (!associatedDocks.Any())
                    {
                        Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.STAGE_NO_DOCKS], z.Name));
                        configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.STAGE_NO_DOCKS], z.Name));

                        configCheck.KeyValuePairs["Zones"]
                            .Add(new ResourceMessage(ResourceKeyConstants.STAGE_NO_DOCKS,
                                ResourceDefinitions.Messages[ResourceKeyConstants.STAGE_NO_DOCKS],
                                z.Name));

                        result = false;
                    }

                    if (!stage.IsIn && !stage.IsOut)
                    {
                        Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.STAGE_NO_FLOWS], z.Name));
                        configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.STAGE_NO_FLOWS], z.Name));

                        configCheck.KeyValuePairs["Zones"]
                            .Add(new ResourceMessage(ResourceKeyConstants.STAGE_NO_FLOWS,
                                ResourceDefinitions.Messages[ResourceKeyConstants.STAGE_NO_FLOWS],
                                z.Name));

                        result = false;
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if there is any mistake in the flows configuration
        /// </summary>
        /// <returns>True if everything is ok, false if not</returns>
        private bool CheckFlows(ref ConfigCheck configCheck)
        {
            bool result = true;

            if (this.data.Process.Any(x => !x.IsOut) && !this.data.InboundFlowGraph.Any())
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_INBOUND_GRAPH]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_INBOUND_GRAPH]);

                configCheck.KeyValuePairs["Flows"]
                    .Add(new ResourceMessage(ResourceKeyConstants.FLOW_NO_INBOUND_GRAPH,
                        ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_INBOUND_GRAPH]));

                result = false;
            }
            else
            {
                var inFlows = this.data.InboundFlowGraph.Where(x => x.MaxVehicleTime == null);
                foreach (var i in inFlows)
                {
                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_INBOUND_NO_MAX_TIME], i.Name));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_INBOUND_NO_MAX_TIME], i.Name));

                    configCheck.KeyValuePairs["Flows"]
                        .Add(new ResourceMessage(ResourceKeyConstants.FLOW_INBOUND_NO_MAX_TIME,
                            ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_INBOUND_NO_MAX_TIME],
                            i.Name));

                    result = false;
                }

                var inFlowsDockSelection = this.data.InboundFlowGraph.Where(x => x.DockSelectionStrategyId == null);
                foreach (var i in inFlowsDockSelection)
                {
                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_INBOUND_NO_STRATEGY], i.Name));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_INBOUND_NO_STRATEGY], i.Name));

                    configCheck.KeyValuePairs["Flows"]
                        .Add(new ResourceMessage(ResourceKeyConstants.FLOW_INBOUND_NO_STRATEGY,
                            ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_INBOUND_NO_STRATEGY],
                            i.Name));

                    result = false;
                }

                var inLinesPerOrder = this.data.InboundFlowGraph.Where(x => x.AverageLinesPerOrder == 0 || x.AverageLinesPerOrder == null);
                foreach (var i in inLinesPerOrder)
                {
                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_INBOUND_NO_AVERAGE_LINES], i.Name));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_INBOUND_NO_AVERAGE_LINES], i.Name));
                    configCheck.KeyValuePairs["Flows"]
                        .Add(new ResourceMessage(ResourceKeyConstants.FLOW_INBOUND_NO_AVERAGE_LINES,
                            ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_INBOUND_NO_AVERAGE_LINES],
                            i.Name));

                    result = false;
                }
            }

            if (this.data.Process.Any(x => x.IsOut) && !this.data.OutboundFlowGraph.Any())
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_OUTBOUND_GRAPH]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_OUTBOUND_GRAPH]);

                configCheck.KeyValuePairs["Flows"]
                    .Add(new ResourceMessage(ResourceKeyConstants.FLOW_NO_OUTBOUND_GRAPH,
                        ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_NO_OUTBOUND_GRAPH]));

                result = false;
            }
            else
            {
                var outFlows = this.data.OutboundFlowGraph.Where(x => x.MaxVehicleTime == null);
                foreach (var o in outFlows)
                {
                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_OUTBOUND_NO_MAX_TIME], o.Name));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_OUTBOUND_NO_MAX_TIME], o.Name));
                    
                    configCheck.KeyValuePairs["Flows"]
                        .Add(new ResourceMessage(ResourceKeyConstants.FLOW_OUTBOUND_NO_MAX_TIME,
                            ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_OUTBOUND_NO_MAX_TIME],
                            o.Name));

                    result = false;
                }

                var outFlowsDockSelection = this.data.OutboundFlowGraph.Where(x => x.DockSelectionStrategyId == null);
                foreach (var o in outFlowsDockSelection)
                {
                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_OUTBOUND_NO_STRATEGY], o.Name));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_OUTBOUND_NO_STRATEGY], o.Name));
                    
                    configCheck.KeyValuePairs["Flows"]
                        .Add(new ResourceMessage(ResourceKeyConstants.FLOW_OUTBOUND_NO_STRATEGY,
                            ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_OUTBOUND_NO_STRATEGY],
                            o.Name));

                    result = false;
                }

                var outLinesPerOrder = this.data.OutboundFlowGraph.Where(x => x.AverageLinesPerOrder == 0 || x.AverageLinesPerOrder == null);
                foreach (var o in outLinesPerOrder)
                {
                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_OUTBOUND_NO_AVERAGE_LINES], o.Name));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_OUTBOUND_NO_AVERAGE_LINES], o.Name));
                    
                    configCheck.KeyValuePairs["Flows"]
                        .Add(new ResourceMessage(ResourceKeyConstants.FLOW_OUTBOUND_NO_AVERAGE_LINES,
                            ResourceDefinitions.Messages[ResourceKeyConstants.FLOW_OUTBOUND_NO_AVERAGE_LINES],
                            o.Name));

                    result = false;
                }
            }

            return result;
        }

        #endregion

        #endregion
    }
}
