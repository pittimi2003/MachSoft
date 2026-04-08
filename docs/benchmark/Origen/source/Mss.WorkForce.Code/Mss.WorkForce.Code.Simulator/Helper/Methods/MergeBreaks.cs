using Mss.WorkForce.Code.Simulator.Simulation.Resources;

namespace Mss.WorkForce.Code.Simulator.Helper.Methods
{
    public static class MergeBreaks
    {
        public static List<CustomBreak> Merge(List<CustomBreak> breaks)
        {
            if (breaks.Count == 0) return breaks;

            var all = new List<CustomBreak>();

            // 1️ Normalizar los que cruzan medianoche
            foreach (var b in breaks)
            {
                if (b.EndBreak < b.InitBreak)
                {
                    // Ejemplo: 22:00 → 02:00
                    // Se parte en dos breaks:
                    all.Add(new CustomBreak
                    {
                        InitBreak = b.InitBreak,
                        EndBreak = 24 // medianoche
                    });
                    all.Add(new CustomBreak
                    {
                        InitBreak = 0, // medianoche
                        EndBreak = b.EndBreak
                    });
                }
                else all.Add(b);
            }

            // 2️ Ordenar por inicio
            all = all.OrderBy(b => b.InitBreak).ToList();

            // 3️ Fusionar (mergear) los solapados o contiguos
            var merged = new List<CustomBreak>();

            var current = all[0];

            foreach (var next in all.Skip(1))
            {
                // Si se solapan o se tocan, los fusionamos
                if (next.InitBreak <= current.EndBreak) current.EndBreak = Math.Max(current.EndBreak, next.EndBreak);
                else
                {
                    // Si no, guardamos el actual y seguimos
                    merged.Add(current);
                    current = next;
                }
            }

            // Agregar el último
            merged.Add(current);

            return merged;
        }
    }
}
