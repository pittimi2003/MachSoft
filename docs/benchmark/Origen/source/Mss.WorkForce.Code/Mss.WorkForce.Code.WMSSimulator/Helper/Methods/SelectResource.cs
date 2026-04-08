using Microsoft.EntityFrameworkCore;
using Mss.WorkForce.Code.Models.DBContext;

namespace Mss.WorkForce.Code.WMSSimulator.Helper.Methods
{
    public static class SelectResource
    {
        #region Methods

        /// <summary>
        /// Selects a random available worker that matches the necessary role
        /// </summary>
        /// <param name="context">WFM data</param>
        /// <param name="order">Order that needs the worker</param>
        /// <param name="processType">Type of the process to assign the worker</param>
        /// <param name="processDuration">Duration, in seconds, of the process to create</param>
        /// <returns>Worker name</returns>
        public static string Worker(ApplicationDbContext? context, Models.Models.InputOrder order, string processType, double processDuration)
        {
            var start = DateTime.UtcNow.AddSeconds(-processDuration).TimeOfDay.TotalHours;
            var end = DateTime.UtcNow.TimeOfDay.TotalHours;

            var workers = context.AvailableWorkers
                .Where(x => x.Worker.Team.WarehouseId == order.WarehouseId)
                .Join(context.Workers, aw => aw.WorkerId, w => w.Id, (aw, w) => new
                {
                    WorkerId = Guid.NewGuid(),
                    Name = w.Name,
                    AvailableWorkerId = aw.Id,
                    TeamId = w.TeamId,
                    Team = context.Teams.FirstOrDefault(x => x.Id == w.TeamId),
                    RolId = w.RolId,
                    Rol = context.Roles.FirstOrDefault(x => x.Id == w.RolId)
                })
                .Join(context.RolProcessSequences.Where(x => x.Process.Type == processType && x.Rol.WarehouseId == order.WarehouseId),
                aw => aw.Rol.Name, rp => rp.Rol.Name, (aw, rp) => new
                {
                    WorkerId = aw.WorkerId,
                    AvailableWorkerId = aw.AvailableWorkerId,
                    Name = aw.Name,
                    TeamId = aw.TeamId,
                    Team = aw.Team,
                    RolId = aw.RolId,
                    Rol = aw.Rol
                })
                .Join(context.Schedules.Include(m => m.Shift), aw => aw.AvailableWorkerId, s => s.AvailableWorkerId, (aw, s) => new
                {
                    AvailableWorkerId = aw.AvailableWorkerId,
                    Name = aw.Name,
                    InitShift = s.Shift.InitHour,
                    EndShift = s.Shift.EndHour
                })
                .Where(x =>
                    (x.InitShift < x.EndShift && start >= x.InitShift && end < x.EndShift) || // Turno normal
                    (x.InitShift > x.EndShift && (start >= x.InitShift || end < x.EndShift))  // Cruza medianoche
                ).ToList();

            if (workers.Any())
            {
                return workers[RandomSelector.Select(0, workers.Count() - 1)].Name;
            }
            else
            {
                Console.WriteLine($"{SGALogger.LogWarning}: No workers for a process {processType} at {end}. Returning a same rol worker.");

                var backupWorkers = context.AvailableWorkers
                .Where(x => x.Worker.Team.WarehouseId == order.WarehouseId)
                .Where(x => x.Worker.Rol.Name == context.RolProcessSequences.FirstOrDefault(x => x.Process.Type == processType && x.Rol.WarehouseId == order.WarehouseId).Rol.Name)
                .ToList();

                return backupWorkers[RandomSelector.Select(0, backupWorkers.Count() - 1)].Name;
            }
        }

        /// <summary>
        /// Selects a random equipment
        /// </summary>
        /// <param name="context">WFM data</param>
        /// <param name="order">Order that needs the equipment</param>
        /// <returns>Equioment name</returns>
        public static string Equipment(ApplicationDbContext? context, Models.Models.InputOrder order)
        {
            var equipments = context.EquipmentGroups.Where(x => x.Area.Layout.WarehouseId == order.WarehouseId).ToList();
            return equipments[RandomSelector.Select(0, equipments.Count() - 1)].Name;
        }

        #endregion
    }
}
