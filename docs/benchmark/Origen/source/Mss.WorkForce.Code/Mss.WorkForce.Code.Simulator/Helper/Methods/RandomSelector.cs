using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Models.ModelSimulation;

namespace Mss.WorkForce.Code.Simulator.Helper
{
    /// <summary>
    /// Selects an option based on the probabilities given in the database.
    /// </summary>
    public static class RandomSelector
    {
        #region Methods
        #region Public
        /// <summary>
        /// Selects the init process based on the probabilities given in the database.
        /// </summary>
        /// <param name="processes">List of possible init processes with their probabilities</param>
        /// <param name="data">Data given in the database</param>
        /// <param name="rand">Instance of Random</param>
        /// <returns>Id of the selected process</returns>
        public static Process SelectInitProcess(List<Process> processes, DataSimulatorTablaRequest data, Random rand)
        {
            Dictionary<Guid, double> values = new Dictionary<Guid, double>();
            foreach (var process in processes)
                values.Add(process.Id, process.PercentageInitProcess.GetValueOrDefault(0));

            var option = ChooseOption(values, rand);
            return data.Process.FirstOrDefault(x => x.Id == option);
        }

        /// <summary>
        /// Selects the next process following a given one based on the probabilities given in the database.
        /// </summary>
        /// <param name="nextProcesses">List of possible next processes with their probabilities</param>
        /// <param name="data">Data given in the database</param>
        /// <param name="rand">Instance of Random</param>
        /// <returns>Id of the selected process</returns>
        public static Process SelectNextProcess(List<ProcessDirectionProperty> nextProcesses, DataSimulatorTablaRequest data, Random rand)
        {
            Dictionary<Guid, double> values = new Dictionary<Guid, double>();
            foreach (var nextProcess in nextProcesses)
                values.Add(nextProcess.Id, nextProcess.Percentage);

            var option = ChooseOption(values, rand);
            return data.ProcessDirectionProperty.FirstOrDefault(x => x.Id == option).EndProcess;
        }
        #endregion

        #region Private
        /// <summary>
        /// Chooses an option by its probability of the given ones
        /// </summary>
        /// <param name="values">Dictionary with key the option and value its probability</param>
        /// <param name="rand">Instance of Random</param>
        /// <returns>Id of the selected process</returns>
        /// <exception cref="Exception">Throws an exception if the the sum of the probabilities is not 1.</exception>
        private static Guid ChooseOption(Dictionary<Guid, double> values, Random rand)
        {
            List<Guid> options = new List<Guid>();
            List<double> probabilities = new List<double>();

            foreach (var option in values.Keys)
            {
                options.Add(option);
                probabilities.Add(Convert.ToDouble(values[option]));
            }

            // Generar un número aleatorio entre 0 y el máximo valor de la suma de las probabilidades
            int numeroAleatorio = rand.Next(Convert.ToInt32(probabilities.Sum()));

            double acumulador = 0;

            // Comparar el número aleatorio con las probabilidades acumuladas
            for (int i = 0; i < probabilities.Count; i++)
            {
                acumulador += probabilities[i];

                if (numeroAleatorio < acumulador)
                {
                    return options[i];
                }
            }

            // Como medida de seguridad, devolver la opción de mayor probabilidad en caso de error (aunque no debería suceder)
            Console.WriteLine("Not able to select an option using their percentages. Taking the most probable one. Please, check this issue for further actions.");
            return values.FirstOrDefault(x => x.Value == probabilities.Max()).Key;

        }
        #endregion
        #endregion
    }
}
