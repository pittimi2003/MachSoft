using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web;
using System.Diagnostics;

namespace Mss.WorkForce.Code.Models.DTO
{
    public class WorkersInRolDto
    {
        public Guid id { get; set; }

        public string name { get; set; }
        public int Workers { get; set; }

        /// <summary>
        /// Número workers necesarios para la carga de trabajo planificada.
        /// </summary>
        public int RequiredWorkers { get; set; }

        public bool IsNew { get; set; }
        public HashSet<string> ModifiedFields { get; set; } = new();

        public WorkersInRolDto DeepClone()
        {
            return new WorkersInRolDto
            {
                id = this.id,
                name = this.name,
                Workers = this.Workers,
                RequiredWorkers = this.RequiredWorkers,
                IsNew = this.IsNew,
                ModifiedFields = this.ModifiedFields != null
                ? new HashSet<string>(ModifiedFields) 
                : new HashSet<string>()
            };
        }
    }
}
