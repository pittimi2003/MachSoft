using Mss.WorkForce.Code.Models.Models;

namespace Mss.WorkForce.Code.Simulator.Simulation.Resources
{
    /// <summary>
    /// Define a warehouse resource.
    /// </summary>
    public class Resource
    {
        #region Variables
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public Guid? RolId { get; set; }
        public List<Shift>? Shifts { get; set; }
        public List<CustomBreak>? Breaks { get; set; }
        public Area? CurrentArea { get; set; }
        public Guid? AreaId { get; set; }
        public string? ZoneType { get; set; }
        public int InUse { get; set; }
        public int Capacity { get; set; }
        public int? OutboundRange { get; set; }
        public int? InboundRange { get; set; }
        public double? MaxContainers { get; set; }
        public double? CurrentContainers { get; set; }
        public bool? IsOccupiedByVehicle { get; set; }
        public List<Guid> AssociatedDocks { get; set; }
        #endregion

        #region Constructors

        /// <summary>
        /// Constructor for worker resources
        /// </summary>
        /// <param name="id">Worker id</param>
        /// <param name="name">Worker name</param>
        /// <param name="type">Resource type</param>
        /// <param name="area">Current area of the worker (null when first created)</param>
        /// <param name="rolId">Rol assigned to the worker</param>
        /// <param name="shifts">Shifts assigned to the worker</param>
        /// <param name="breaks">Breaks assigned to the worker</param>
        /// <param name="capacity">Capacity of the resource</param>
        public Resource(Guid id, string name, string type, Area? area, Guid rolId, List<Shift> shifts, List<CustomBreak> breaks, int capacity) 
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
            this.CurrentArea = area;
            this.RolId = rolId;
            this.Shifts = shifts;
            this.Breaks = breaks;
            this.InUse = 0;
            this.Capacity = capacity;
        }

        /// <summary>
        /// Constructor for station resources
        /// </summary>
        /// <param name="id">Station id</param>
        /// <param name="name">Station name</param>
        /// <param name="type">Resource type</param>
        /// <param name="zoneType">Zone type</param>
        /// <param name="area">Area where the station is located</param>
        /// <param name="capacity">Capacity of the resource</param>
        /// <param name="maxContainers">Maximum quantity of containers admited in the station</param>
        /// <param name="inRange">Range for inbound docks</param>
        /// <param name="outRange">Range for outbound docks</param>
        public Resource(Guid id, string name, string type, string zoneType, Area area, int capacity, double maxContainers, Guid? areaId,
            int? inRange = null, int? outRange = null) 
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
            this.ZoneType = zoneType;
            this.CurrentArea = area;
            this.InUse = 0;
            this.Capacity = capacity;
            this.MaxContainers = maxContainers;
            this.CurrentContainers = 0;
            this.IsOccupiedByVehicle = false;
            this.InboundRange = inRange;
            this.OutboundRange = outRange;
            this.AreaId = areaId;
        }

        /// <summary>
        /// Constructor for stages resources
        /// </summary>
        /// <param name="id">Station id</param>
        /// <param name="name">Station name</param>
        /// <param name="type">Resource type</param>
        /// <param name="zoneType">Zone type</param>
        /// <param name="area">Area where the station is located</param>
        /// <param name="capacity">Capacity of the resource</param>
        /// <param name="maxContainers">Maximum quantity of containers admited in the station</param>
        public Resource(Guid id, string name, string type, string zoneType, Area area, int capacity, double maxContainers, List<Guid> associatedDocks, Guid? areaId)
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
            this.ZoneType = zoneType;
            this.CurrentArea = area;
            this.InUse = 0;
            this.Capacity = capacity;
            this.MaxContainers = maxContainers;
            this.CurrentContainers = 0;
            this.IsOccupiedByVehicle = false;
            this.AssociatedDocks = associatedDocks;
            this.AreaId = areaId;
        }

        /// <summary>
        /// Constructor for equipment resources
        /// </summary>
        /// <param name="id">Equipment group id</param>
        /// <param name="name">Equipment group name</param>
        /// <param name="type">Equipment group type</param>
        /// <param name="area">Area assigned to the equipment</param>
        /// <param name="capacity">Capacity of the resource</param>
        public Resource(Guid id, string name, string type, Area area, int capacity) 
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
            this.CurrentArea = area;
            this.InUse = 0;
            this.Capacity = capacity;
        }

        /// <summary>
        /// Default constructor
        /// </summary>
        public Resource()
        {

        }

        #endregion
    }
}
