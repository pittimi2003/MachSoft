using Mss.WorkForce.Code.Models.ModelSimulation;
using Mss.WorkForce.Code.Simulator.Helper;
using Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.GroupProcess;
using Mss.WorkForce.Code.Simulator.Simulation.Resources;

namespace Mss.WorkForce.Code.Simulator.Simulation.PreSimulation.WhatIf
{
    public static class WorkersNumber
    {
        #region Methods

        #region Public

        /// <summary>
        /// Create the minimum necessary workers for finishing a simulation on time.
        /// </summary>
        /// <param name="data">Data of the simulation.</param>
        /// <param name="resources">Dictionary of the resources already created.</param>
        /// <param name="processes">List of the generated processes.</param>
        /// <returns>Complete dictionary of resources, including the workers.</returns>
        public static Dictionary<Guid, Resource> CreateWorkers(DataSimulatorTablaRequest data, Dictionary<Guid, Resource> resources, IEnumerable<Grouping> processes)
        {
            // Genera un diccionario de tiempos con la carga que suma o resta en el momento
            Dictionary<RolShiftKey, List<TimeEvent>> eventsByRol = GenerateTimeEvents(data, processes);

            // Genera un diccionario con el número de operarios a crear por rol
            Dictionary<RolShiftKey, int> maxWorkersByRol = GenerateNumberWorkersPerRol(eventsByRol);

            // Genera los recursos de los trabajadores necesarios para simular
            Dictionary<Guid, Resource> workers = GenerateSimulationWorkers(data, resources, maxWorkersByRol);

            return workers;
        }

        #endregion

        #region Private

        /// <summary>
        /// Generates the time events for identifying max load points
        /// </summary>
        /// <param name="data">Data of the simulation.</param>
        /// <param name="processes">List of the generated processes.</param>
        /// <returns>Dictionary of the time events with the work load for using the sweep line algorithm</returns>
        private static Dictionary<RolShiftKey, List<TimeEvent>> GenerateTimeEvents(DataSimulatorTablaRequest data, IEnumerable<Grouping> processes)
        {
            // Diccionario para guardar los tiempos por rol
            var eventsByRol = new Dictionary<RolShiftKey, List<TimeEvent>>();

            foreach (var p in processes)
            {
                DateTime maxEnd = p.StartWorkingDate.Date.AddHours(data.Shift.Max(x => x.EndHour));

                // Rol por proceso. Cogemos siempre el de menor sequence
                var rolId = data.RolProcessSequence
                    .Where(x => x.ProcessId == p.Process.Id)
                    .OrderBy(x => x.Sequence)
                    .Select(x => x.RolId)
                    .First();

                // Cogemos los shifts que toca el proceso
                var shifts = data.Shift
                    .Where(x => p.StartWorkingDate.TimeOfDay.Hours < x.EndHour && p.MaxOnTimeDate.GetValueOrDefault(maxEnd).TimeOfDay.TotalHours > x.InitHour);

                foreach (var s in shifts)
                {
                    DateTime end = p.StartWorkingDate.Date.AddHours(s.EndHour);

                    // Miramos cuantos segundos disponibles tenemos para realizar ese proceso
                    var totalSeconds = (p.MaxOnTimeDate.GetValueOrDefault(end) - p.StartWorkingDate).TotalSeconds;

                    // Protección mínima
                    if (totalSeconds <= 0)
                        continue;

                    // Ratio por segundo de ESTE proceso
                    var rate = p.Duration / totalSeconds;

                    var key = new RolShiftKey()
                    {
                        rolId = rolId,
                        shiftId = s.Id
                    };

                    // Si no tenemos esa KEY, lo añadimos al diccionario
                    if (!eventsByRol.TryGetValue(key, out var events))
                    {
                        events = new List<TimeEvent>();
                        eventsByRol[key] = events;
                    }

                    // Añadimos la carga de trabajo (rate) al principio, con el comienzo del trabajo
                    events.Add(new TimeEvent
                    {
                        Time = p.StartWorkingDate,
                        Delta = rate
                    });

                    // Quitamos la carga del trabajo (rate) al final, con el fin del trabajo
                    events.Add(new TimeEvent
                    {
                        Time = p.MaxOnTimeDate.GetValueOrDefault(end),
                        Delta = -rate
                    });
                }
            }

            return eventsByRol;
        }

        /// <summary>
        /// Implements the sweep line algorithm to obtain the minimum workers number per rol
        /// </summary>
        /// <param name="eventsByRol">Dictionary of the time events with the work load for using the sweep line algorithm</param>
        /// <returns>Dictionary of the time events with workers number per rol id</returns>
        private static Dictionary<RolShiftKey, int> GenerateNumberWorkersPerRol(Dictionary<RolShiftKey, List<TimeEvent>> eventsByRol)
        {
            var maxWorkersByRol = new Dictionary<RolShiftKey, int>();

            // Recorremos cada uno de los turnos que tenemos almacenados
            foreach (var kv in eventsByRol)
            {
                double currentLoad = 0;
                double maxLoad = 0;

                // SWEEP LINE: 
                //  - Detecta picos de trabajo simultáneo en intervalos que se solapan o están contenidos.
                //  - Evita calcular promedios que diluyen la carga
                // Para cada uno de los turnos, sacamos los TimeEvent ordenados, y en cada uno añado carga al total (se puede restar si pasamos a MaxOnTimeDate)
                // y me quedo con el máximo de carga
                foreach (var e in kv.Value.OrderBy(x => x.Time).ThenByDescending(x => x.Delta))
                {
                    currentLoad += e.Delta;
                    maxLoad = Math.Max(maxLoad, currentLoad);
                }

                maxWorkersByRol[kv.Key] = (int)maxLoad == 0 ? (int)Math.Ceiling(maxLoad) : (int)Math.Round(maxLoad, 0);
            }

            return maxWorkersByRol;
        }

        /// <summary>
        /// Creates the workers resources for the simulation
        /// </summary>
        /// <param name="data">Data of the simulation.</param>
        /// <param name="resources">Dictionary of the resources already created.</param>
        /// <param name="maxWorkersByRol">Dictionary of the time events with workers number per rol id.</param>
        /// <returns>Resources dictionary for the simulation</returns>
        private static Dictionary<Guid, Resource> GenerateSimulationWorkers(DataSimulatorTablaRequest data, Dictionary<Guid, Resource> resources, Dictionary<RolShiftKey, int> maxWorkersByRol)
        {
            foreach (var w in maxWorkersByRol.Keys)
            {
                for (int i = 0; i < maxWorkersByRol[w]; i++)
                {
                    Guid id = Guid.NewGuid();
                    string name = $"WORKER_{(resources.Values.Count(x => x.Type == ResourceType.Worker) + 1).ToString("D3")}";
                    resources.Add(id, new Resource(id, name, ResourceType.Worker, null, w.rolId, data.Shift.Where(x => x.Id == w.shiftId).ToList(), new List<CustomBreak>(), 1));
                }
            }

            return resources;
        }

        #endregion

        #endregion
    }

    #region Auxiliar Structure

    internal struct RolShiftKey
    {
        internal Guid rolId { get; set; }
        internal Guid shiftId { get; set; }
    }

    #endregion
}
