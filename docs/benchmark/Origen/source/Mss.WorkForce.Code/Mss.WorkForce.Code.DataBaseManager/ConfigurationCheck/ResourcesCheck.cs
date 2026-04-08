using Mss.WorkForce.Code.ConfigurationCheckLogs;
using Mss.WorkForce.Code.Models.Constants;
using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Models.Resources;

namespace Mss.WorkForce.Code.DataBaseManager.ConfigurationCheck
{
    public class ResourcesCheck
    {
        #region Variables
        private DataSimulatorTablaRequest? data;
        #endregion

        #region Constructor
        public ResourcesCheck(DataSimulatorTablaRequest? data) { this.data = data; }
        #endregion

        #region Methods

        #region Public

        /// <summary>
        /// Checks if all the required configuration for the resources for a simulation is completed. 
        /// </summary>
        /// <returns>True if everything is completed, false if not</returns>
        public bool Check(ref ConfigCheck configCheck)
        {
            bool roles = CheckRoles(ref configCheck);
            bool workers = CheckWorkers(ref configCheck);
            bool equipments = CheckEquipments(ref configCheck);

            return workers && roles && equipments ? true : false;
        }

        #endregion

        #region Private

        /// <summary>
        /// Checks if there is any mistake in the workers configuration
        /// </summary>
        /// <returns>True if everything is ok, false if not</returns>
        private bool CheckWorkers(ref ConfigCheck configCheck)
        {
            bool result = true;

            if (!this.data.AvailableWorker.Any())
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.WORKERS_NOT_CONFIGURED]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.WORKERS_NOT_CONFIGURED]);
                
                configCheck.KeyValuePairs["Resources"]
                    .Add(new ResourceMessage(ResourceKeyConstants.WORKERS_NOT_CONFIGURED, 
                    ResourceDefinitions.Messages[ResourceKeyConstants.WORKERS_NOT_CONFIGURED]));
                result = false;
            }
            else
            {
                var processesId = this.data.Process.Select(x => x.Id).Distinct();
                var workersRoles = this.data.AvailableWorker.Select(x => x.Worker.RolId).Distinct()
                    .Join(this.data.RolProcessSequence, w => w, r => r.RolId, (w, r) => new { ProcessId = r.ProcessId }).Select(x => x.ProcessId).Distinct();

                var missingProcesses = processesId.Except(workersRoles);
                foreach (var p in missingProcesses)
                {
                    var processName = this.data.Process.FirstOrDefault(x => x.Id == p)?.Name ?? "Unknown";
                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.WORKERS_NO_ROLES_FOR_PROCESS], processName));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.WORKERS_NO_ROLES_FOR_PROCESS], processName));
                    
                    configCheck.KeyValuePairs["Resources"]
                        .Add(new ResourceMessage(ResourceKeyConstants.WORKERS_NO_ROLES_FOR_PROCESS,
                            ResourceDefinitions.Messages[ResourceKeyConstants.WORKERS_NO_ROLES_FOR_PROCESS],
                            processName));

                    result = false;
                }
            }

            return result;
        }

        /// <summary>
        /// Checks if there is any mistake in the roles configuration
        /// </summary>
        /// <returns>True if everything is ok, false if not</returns>
        private bool CheckRoles(ref ConfigCheck configCheck)
        {
            bool result = true;

            var roles = this.data.RolProcessSequence.Select(x => x.ProcessId).Distinct();
            var processes = this.data.Process.Select(x => x.Id).Distinct();
            var missingProcesses = processes.Except(roles);
            foreach (var p in missingProcesses)
            {
                var processName = this.data.Process.FirstOrDefault(x => x.Id == p)?.Name ?? "Unknown";
                Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.ROLES_NO_FOR_PROCESS], processName));
                configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.ROLES_NO_FOR_PROCESS], processName));
                configCheck.KeyValuePairs["Resources"]
                    .Add(new ResourceMessage(ResourceKeyConstants.ROLES_NO_FOR_PROCESS,
                        ResourceDefinitions.Messages[ResourceKeyConstants.ROLES_NO_FOR_PROCESS], 
                        processName));
                result = false;
            }

            return result;
        }

        /// <summary>
        /// Checks if there is any mistake in the equipments configuration
        /// </summary>
        /// <returns>True if everything is ok, false if not</returns>
        private bool CheckEquipments(ref ConfigCheck configCheck)
        {
            bool result = true;

            if (!this.data.TypeEquipment.Any())
            {
                Console.WriteLine(ResourceDefinitions.Messages[ResourceKeyConstants.EQUIPMENTS_TYPE_NOT_CONFIGURED]);
                configCheck.Values.Add(ResourceDefinitions.Messages[ResourceKeyConstants.EQUIPMENTS_TYPE_NOT_CONFIGURED]);
                
                configCheck.KeyValuePairs["Resources"]
                    .Add(new ResourceMessage(ResourceKeyConstants.EQUIPMENTS_TYPE_NOT_CONFIGURED,
                        ResourceDefinitions.Messages[ResourceKeyConstants.EQUIPMENTS_TYPE_NOT_CONFIGURED]));

                result = false;
            }
            else
            {
                var areas = this.data.Area.Select(x => x.Id).Distinct();
                var equipmentGroups = this.data.EquipmentGroup.Select(x => x.AreaId).Distinct();
                var missingAreas = areas.Except(equipmentGroups);
                foreach (var e in missingAreas)
                {
                    var areaName = this.data.Area.FirstOrDefault(x => x.Id == e)?.Name ?? "Unknown";

                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.EQUIPMENTS_NOT_IN_AREA], areaName));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.EQUIPMENTS_NOT_IN_AREA], areaName));
                    configCheck.KeyValuePairs["Resources"]
                        .Add(new ResourceMessage(ResourceKeyConstants.EQUIPMENTS_NOT_IN_AREA,
                            ResourceDefinitions.Messages[ResourceKeyConstants.EQUIPMENTS_NOT_IN_AREA], 
                            areaName));
                    result = false;
                }

                var nullEquipments = this.data.EquipmentGroup.Where(x => x.Equipments == 0);
                foreach (var n in nullEquipments)
                {
                    var equipmentGroupName = this.data.EquipmentGroup.FirstOrDefault(x => x.Id == n.Id)?.Name ?? "Unknown";

                    Console.WriteLine(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.EQUIPMENTS_GROUP_EMPTY], equipmentGroupName));
                    configCheck.Values.Add(string.Format(ResourceDefinitions.Messages[ResourceKeyConstants.EQUIPMENTS_GROUP_EMPTY], equipmentGroupName));
                    
                    configCheck.KeyValuePairs["Resources"]
                        .Add(new ResourceMessage(ResourceKeyConstants.EQUIPMENTS_GROUP_EMPTY,
                            ResourceDefinitions.Messages[ResourceKeyConstants.EQUIPMENTS_GROUP_EMPTY], 
                            equipmentGroupName));

                    result = false;
                }
            }

            return result;
        }

        #endregion

        #endregion
    }
}
