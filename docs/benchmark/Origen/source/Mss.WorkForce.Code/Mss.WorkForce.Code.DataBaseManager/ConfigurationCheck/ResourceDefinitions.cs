using Mss.WorkForce.Code.Models.Constants;

namespace Mss.WorkForce.Code.DataBaseManager.ConfigurationCheck
{
    public static class ResourceDefinitions
    {
        public static readonly Dictionary<string, string> Messages = new()
        {
            // ------------------ AREAS ------------------
            { ResourceKeyConstants.AREAS_NOT_CREATED, "There are not areas created for this warehouse. The areas must be created in the designer." },
            { ResourceKeyConstants.AREA_NO_ZONES, "Area {0} has no zones configured. The zones must be created in the designer." },

            // ------------------ ROUTES ------------------
            { ResourceKeyConstants.ROUTES_NOT_CREATED, "There are no routes created for this warehouse. The routes must be created in the designer." },
            { ResourceKeyConstants.ROUTE_NOT_FOUND, "There is not route from area {0} to area {1}. The route must be configured in the designer." },
            { ResourceKeyConstants.ROUTES_NOT_CONFIGURED, "There are not configured routes between areas. Routes must be configured in the designer." },

            // ------------------ DOCKS ------------------
            { ResourceKeyConstants.DOCKS_NOT_CREATED , "There are no docks created for this warehouse. The docks must be created in the designer." },
            { ResourceKeyConstants.DOCKS_NO_OUTBOUND, "It is not possible to assign outbound docks for the process {0}. The outbound property for docks must be configured in the designer." },
            { ResourceKeyConstants.DOCKS_NO_INBOUND, "It is not possible to assign inbound docks for the process {0}. The inbound property for docks must be configured in the designer." },
            { ResourceKeyConstants.DOCKS_NO_INBOUND_RANGES, "There are not ranges configured for inbound docks. The range for the inbound property in docks must be configured in the designer." },
            { ResourceKeyConstants.DOCKS_DUPLICATE_INBOUND_RANGES, "There are repeating ranges for inbound docks. The range property on docks should not be repeated and must be configured in the designer." },
            { ResourceKeyConstants.DOCKS_NO_OUTBOUND_RANGES, "There are not ranges configured for outbound docks. The range for the outbound property in docks must be configured in the designer." },
            { ResourceKeyConstants.DOCKS_DUPLICATE_OUTBOUND_RANGES, "There are repeating ranges for outbound docks. The range property on docks should not be repeated and must be configured in the designer." },
            { ResourceKeyConstants.DOCKS_NO_SELECTION_STRATEGY, "There is not dock selection strategy available for inflow or outflow. The dock type selection must be configured in the flow properties." },

            // ------------------ ZONES ------------------
            { ResourceKeyConstants.ZONES_NOT_CREATED , "There are no zones created for this warehouse. The zones must be created in the designer." },
            { ResourceKeyConstants.ZONE_NO_CONTAINERS_CAPACITY, "Zone {0} does not have a maximum container capacity configured. The maximum container capacity property for zones must be greater than 0 and must be configured in the designer." },
            { ResourceKeyConstants.ZONE_NO_EQUIPMENTS_CAPACITY, "Zone {0} is configured with maximum equipments 0. The max equipments property should be greater than 0 and must be configured in the zone properties." },
            { ResourceKeyConstants.NARROW_AISLE_ZERO_PICKERS, "Narrow aisle for zone {0} is configured with maximum pickers 0. The maximum pickers property for zone {0} should be greater than 0 and must be configured in the zone properties." },
            { ResourceKeyConstants.STAGE_NO_DOCKS, "Zone {0} has no associated docks. Stages must have at least 1 dock associated. Docks for stages must be associated in the designer. " },
            { ResourceKeyConstants.STAGE_NO_FLOWS, "Zone {0} has no active flow. Stages must have at least 1 active flow. Flows for stages must be activated in the designer. " },

            // ------------------ FLOWS ------------------
            { ResourceKeyConstants.FLOW_NO_INBOUND_GRAPH, "There is not a inbound flow configured. The inbound must be configured in the designer." },
            { ResourceKeyConstants.FLOW_INBOUND_NO_MAX_TIME, "The inbound flow {0} does not have a maximum vehicle time in dock configured. The maximum time per vehicle property for the flow must be configurated in the designer." },
            { ResourceKeyConstants.FLOW_INBOUND_NO_STRATEGY, "The inbound flow {0} has no docks with a strategy assigned for this flow. The Allow inbound property for docks in the inbound flow must be configurated in the designer." },
            { ResourceKeyConstants.FLOW_INBOUND_NO_AVERAGE_LINES, "There are not average lines per order configured for inboundflow {0}. The average lines per order property must be configurated in the flow properties." },
            { ResourceKeyConstants.FLOW_NO_OUTBOUND_GRAPH, "There is not a outbound flow configured. The inbound must be configured in the designer." },
            { ResourceKeyConstants.FLOW_OUTBOUND_NO_MAX_TIME, "The outbound flow {0} does not have a maximum vehicle time in dock configured. The maximum time per vehicle property for the flow must be configurated in the designer." },
            { ResourceKeyConstants.FLOW_OUTBOUND_NO_STRATEGY, "The outbound flow {0} has no docks with a strategy assigned for this flow. The dock type selection must be configured in the flow properties." },
            { ResourceKeyConstants.FLOW_OUTBOUND_NO_AVERAGE_LINES, "There are not average lines per order configured for outboundflow {0}. The average lines per order property must be configurated in the flow properties." },
            { ResourceKeyConstants.FLOW_NO_ORDER_SCHEDULES, "There are not order schedules configured. The property must be configured in Logistics Profiles > Order Profiles." },
            { ResourceKeyConstants.FLOW_NO_PROCESS_PRIORITIES, "There are not roles assigned to the processes. Roles must be configured in Work Profiles > Base Profiles." },
            { ResourceKeyConstants.OUTBOUND_NO_GRAPH, "There is no outbound flow created while outbound work load is. Flows must be created in the designer and must be unique." },
            { ResourceKeyConstants.INBOUND_NO_GRAPH, "There is no inbound flow created while inbound work load is. Flows must be created in the designer and must be unique." },
            { ResourceKeyConstants.OUTBOUNDFLOW_NOT_UNIQUE, "There are more than 1 outbound flow created. Flows must be unique. Flows must be deleted in the designer." },
            { ResourceKeyConstants.INBOUNDFLOW_NOT_UNIQUE, "There are more than 1 inbound flow created. Flows must be unique. Flows must be deleted in the designer." },

            // ------------------ PROCESSES ------------------
            { ResourceKeyConstants.PROCESS_NO_INITIAL, "There are not initial processes configured to start the simulation. The started property must be configurated in the process properties." },
            { ResourceKeyConstants.PROCESS_INIT_PERCENTAGE_INVALID, "The percentage of the initial process {0} must be greater than 0. The percentage must be set in the process properties." },
            { ResourceKeyConstants.PROCESS_NO_ENDING, "There are not end processes configured to terminate the simulation. The end property must be configurated  in the process direction property." },
            { ResourceKeyConstants.PROCESS_DIRECTION_PERCENTAGE_INVALID, "The execution probability percentage for {0} must be greater than 0. The execution percentage must be configured in the process address properties." },
            { ResourceKeyConstants.PROCESS_INIT_NO_PATHS, "The initial process {0} has no routes assigned. Routes for the process flow must be configured in the designer." },
            { ResourceKeyConstants.PROCESS_PATH_NO_END, "Process {0} has no routes configured to process {1}. Routes must be configured in the designer." },
            { ResourceKeyConstants.PROCESS_NO_TIME, "The {0} process does not have a preprocessing, processing and postprocessing time configured. The time must be configured in the process properties." },
            { ResourceKeyConstants.PROCESS_NO_LOAD_PROFILE, "There are not load profiles configured. Load profiles must be configured in Logistics Profiles > Load Profiles." },
            { ResourceKeyConstants.PROCESS_NO_VEHICLE_PROFILE, "There are not vehicle profiles configured. Vehicle profiles must be configured in Logistics Profiles > Load profiles." },
            { ResourceKeyConstants.PROCESS_NO_ORDER_LOAD_RATIOS, "There are not order loading profiles configured. Order loading profiles must be configured in Logistics Profiles > Order profiles." },
            { ResourceKeyConstants.PROCESS_SAME_DIRECTIONPROPERTY, "Process {0} has at least one process direction to himself. These directions must be deleted." },
            

            // ------------------ WORKERS ------------------
            { ResourceKeyConstants.WORKERS_NOT_CONFIGURED, "There are not workers configured for the warehouse. Workers must be configured in Work Profiles > Worker Profiles." },
            { ResourceKeyConstants.WORKERS_NO_ROLES_FOR_PROCESS, "There are not workers assigned roles to run the process {0}. Workers must be configured in Work Profiles > Worker Profiles." },

            // ------------------ ROLES ------------------
            { ResourceKeyConstants.ROLES_NO_FOR_PROCESS, "There are not roles assigned to run the process {0}. Roles must be configured in Work Profiles > Base Profiles." },

            // ------------------ EQUIPMENTS ------------------
            { ResourceKeyConstants.EQUIPMENTS_TYPE_NOT_CONFIGURED, "There are not equipments configured. Equipment types must be configured in Work Profiles > Equipment Profiles." },
            { ResourceKeyConstants.EQUIPMENTS_NOT_IN_AREA, "There are not equipments assigned to area {0}. The equipments must be configured in Work Profiles > Equipment Profiles." },
            { ResourceKeyConstants.EQUIPMENTS_GROUP_EMPTY, "The equipment group {0} has 0 pieces of equipment assigned. Equipment groups must be configured in Work Profiles > Equipment Profiles." }
        };
    }
}
