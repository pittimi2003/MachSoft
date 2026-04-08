using Mss.WorkForce.Code.Simulator.Simulation.Resources;

namespace Mss.WorkForce.Code.Simulator.Helper.Methods
{
    public static class MovePositions
    {
        /// <summary>
        /// Move the positions of the docks depending of the selected strategy in order to select the correct one.
        /// </summary>
        /// <param name="temp">Temporary docks list</param>
        /// <param name="n">Positions to move</param>
        /// <returns>List of ordered docks</returns>
        public static List<Resource> Move(List<Resource> temp, int n)
        {
            int count = temp.Count();
            if (count == 0) return temp;

            // Asegurarse de que n no sea mayor que el tamaño de la lista
            n = n % count;

            // Obtener los últimos n elementos y los primeros (count - n) elementos
            var parteFinal = temp.GetRange(count - n, n);
            var parteInicial = temp.GetRange(0, count - n);

            // Limpiar la lista original y agregar las partes en el nuevo orden
            temp.Clear();
            temp.AddRange(parteFinal);
            temp.AddRange(parteInicial);

            return temp;
        }
    }
}
