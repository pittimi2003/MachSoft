using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;
using Mss.WorkForce.Code.Simulator.Simulation.Resources;

namespace Mss.WorkForce.Code.Simulator.Helper.Methods
{
    public static class GetDocks
    {
        /// <summary>
        /// Gets the posible docks for a process
        /// </summary>
        /// <param name="simulation">Simulation data</param>
        /// <param name="process">Process to look for a dock</param>
        /// <returns>IEnumerable of available docks</returns>
        public static IEnumerable<Resource> GetDockResources(Simulation.Simulation simulation, Grouping process, IEnumerable<Guid>? possibleDocks)
        {
            if (process.OrderId == null)
            {
                return simulation.Data.Zone.Where(x => x.AreaId == process.Process.Area.Id && x.Type == ZoneType.Dock)
                    .Join(simulation.Resources.Values.Where(x => x.Type == ResourceType.Zone), so => so.Id, r => r.Id, (so, r) => new Resource
                    {
                        Id = r.Id,
                        Type = r.Type,
                        InUse = r.InUse,
                        Capacity = r.Capacity,
                        ZoneType = r.ZoneType,
                        MaxContainers = r.MaxContainers,
                        CurrentContainers = r.CurrentContainers,
                        IsOccupiedByVehicle = r.IsOccupiedByVehicle
                    });
            }
            else
            {
                var docks = simulation.Data.Zone.Where(x => x.AreaId == process.Process.Area.Id && x.Type == ZoneType.Dock)
                    .Join(simulation.Data.Dock.Where(x => (x.AllowOutbound == simulation.Data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId).IsOutbound
                    && x.AllowInbound == !simulation.Data.InputOrder.FirstOrDefault(x => x.Id == process.OrderId).IsOutbound)
                    || (x.AllowInbound && x.AllowOutbound)),
                    s => s.Id, d => d.ZoneId, (s, d) => new
                    {
                        Id = s.Id,
                        InboundRange = d.InboundRange,
                        OutboundRange = d.OutboundRange
                    })
                    .Join(simulation.Resources.Values.Where(x => x.Type == ResourceType.Zone), so => so.Id, r => r.Id, (so, r) => new Resource
                    {
                        Id = r.Id,
                        Type = r.Type,
                        InUse = r.InUse,
                        Capacity = r.Capacity,
                        InboundRange = so.InboundRange,
                        OutboundRange = so.OutboundRange,
                        ZoneType = r.ZoneType,
                        MaxContainers = r.MaxContainers,
                        CurrentContainers = r.CurrentContainers,
                        IsOccupiedByVehicle = r.IsOccupiedByVehicle
                    });

                if (possibleDocks == null)
                    return docks;
                else 
                    return docks
                    .Join(possibleDocks, r => r.Id, pd => pd, (r, pd) => new Resource
                    {
                        Id = r.Id,
                        Type = r.Type,
                        InUse = r.InUse,
                        Capacity = r.Capacity,
                        InboundRange = r.InboundRange,
                        OutboundRange = r.OutboundRange,
                        ZoneType = r.ZoneType,
                        MaxContainers = r.MaxContainers,
                        CurrentContainers = r.CurrentContainers,
                        IsOccupiedByVehicle = r.IsOccupiedByVehicle
                    });
            }
        }
    }
}
