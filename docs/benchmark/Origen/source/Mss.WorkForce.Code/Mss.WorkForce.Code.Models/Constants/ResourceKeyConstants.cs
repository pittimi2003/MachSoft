namespace Mss.WorkForce.Code.Models.Constants
{
    public static class ResourceKeyConstants
    {
        // AREAS
        public const string AREAS_NOT_CREATED = "AREAS_NOT_CREATED";
        public const string AREA_NO_ZONES = "AREA_NO_ZONES";

        // ROUTES
        public const string ROUTES_NOT_CREATED = "ROUTES_NOT_CREATED";
        public const string ROUTE_NOT_FOUND = "ROUTE_NOT_FOUND";
        public const string ROUTES_NOT_CONFIGURED = "ROUTES_NOT_CONFIGURED";

        // DOCKS
        public const string DOCKS_NOT_CREATED = "DOCKS_NOT_CREATED";
        public const string DOCKS_NO_OUTBOUND = "DOCKS_NO_OUTBOUND";
        public const string DOCKS_NO_INBOUND = "DOCKS_NO_INBOUND";
        public const string DOCKS_NO_INBOUND_RANGES = "DOCKS_NO_INBOUND_RANGES";
        public const string DOCKS_DUPLICATE_INBOUND_RANGES = "DOCKS_DUPLICATE_INBOUND_RANGES";
        public const string DOCKS_NO_OUTBOUND_RANGES = "DOCKS_NO_OUTBOUND_RANGES";
        public const string DOCKS_DUPLICATE_OUTBOUND_RANGES = "DOCKS_DUPLICATE_OUTBOUND_RANGES";
        public const string DOCKS_NO_SELECTION_STRATEGY = "DOCKS_NO_SELECTION_STRATEGY";

        // ZONES
        public const string ZONES_NOT_CREATED = "ZONES_NOT_CREATED";
        public const string ZONE_NO_CONTAINERS_CAPACITY = "ZONE_NO_CONTAINERS_CAPACITY";
        public const string ZONE_NO_EQUIPMENTS_CAPACITY = "ZONE_NO_EQUIPMENTS_CAPACITY";
        public const string NARROW_AISLE_ZERO_PICKERS = "NARROW_AISLE_ZERO_PICKERS";
        public const string STAGE_NO_DOCKS = "STAGE_NO_DOCKS";
        public const string STAGE_NO_FLOWS = "STAGE_NO_FLOWS";

        // FLOWS
        public const string FLOW_NO_INBOUND_GRAPH = "FLOW_NO_INBOUND_GRAPH";
        public const string FLOW_INBOUND_NO_MAX_TIME = "FLOW_INBOUND_NO_MAX_TIME";
        public const string FLOW_INBOUND_NO_STRATEGY = "FLOW_INBOUND_NO_STRATEGY";
        public const string FLOW_INBOUND_NO_AVERAGE_LINES = "FLOW_INBOUND_NO_AVERAGE_LINES";
        public const string FLOW_NO_OUTBOUND_GRAPH = "FLOW_NO_OUTBOUND_GRAPH";
        public const string FLOW_OUTBOUND_NO_MAX_TIME = "FLOW_OUTBOUND_NO_MAX_TIME";
        public const string FLOW_OUTBOUND_NO_STRATEGY = "FLOW_OUTBOUND_NO_STRATEGY";
        public const string FLOW_OUTBOUND_NO_AVERAGE_LINES = "FLOW_OUTBOUND_NO_AVERAGE_LINES";
        public const string FLOW_NO_ORDER_SCHEDULES = "FLOW_NO_ORDER_SCHEDULES";
        public const string FLOW_NO_PROCESS_PRIORITIES = "FLOW_NO_PROCESS_PRIORITIES";
        public const string OUTBOUND_NO_GRAPH = "OUTBOUND_NO_GRAPH";
        public const string INBOUND_NO_GRAPH = "INBOUND_NO_GRAPH";
        public const string OUTBOUNDFLOW_NOT_UNIQUE = "OUTBOUNDFLOW_NOT_UNIQUE";
        public const string INBOUNDFLOW_NOT_UNIQUE = "INBOUNDFLOW_NOT_UNIQUE";

        // PROCESSES
        public const string PROCESS_NO_INITIAL = "PROCESS_NO_INITIAL";
        public const string PROCESS_INIT_PERCENTAGE_INVALID = "PROCESS_INIT_PERCENTAGE_INVALID";
        public const string PROCESS_NO_ENDING = "PROCESS_NO_ENDING";
        public const string PROCESS_SAME_DIRECTIONPROPERTY = "PROCESS_SAME_DIRECTIONPROPERTY";
        public const string PROCESS_DIRECTION_PERCENTAGE_INVALID = "PROCESS_DIRECTION_PERCENTAGE_INVALID";
        public const string PROCESS_INIT_NO_PATHS = "PROCESS_INIT_NO_PATHS";
        public const string PROCESS_PATH_NO_END = "PROCESS_PATH_NO_END";
        public const string PROCESS_NO_TIME = "PROCESS_NO_TIME";
        public const string PROCESS_NO_LOAD_PROFILE = "PROCESS_NO_LOAD_PROFILE";
        public const string PROCESS_NO_VEHICLE_PROFILE = "PROCESS_NO_VEHICLE_PROFILE";
        public const string PROCESS_NO_ORDER_LOAD_RATIOS = "PROCESS_NO_ORDER_LOAD_RATIOS";

        // WORKERS
        public const string WORKERS_NOT_CONFIGURED = "WORKERS_NOT_CONFIGURED";
        public const string WORKERS_NO_ROLES_FOR_PROCESS = "WORKERS_NO_ROLES_FOR_PROCESS";

        // ROLES
        public const string ROLES_NO_FOR_PROCESS = "ROLES_NO_FOR_PROCESS";

        // EQUIPMENTS
        public const string EQUIPMENTS_TYPE_NOT_CONFIGURED = "EQUIPMENTS_TYPE_NOT_CONFIGURED";
        public const string EQUIPMENTS_NOT_IN_AREA = "EQUIPMENTS_NOT_IN_AREA";
        public const string EQUIPMENTS_GROUP_EMPTY = "EQUIPMENTS_GROUP_EMPTY";

    }
}
