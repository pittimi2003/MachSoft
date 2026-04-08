namespace Mss.WorkForce.Code.Simulator.Helper.Methods
{
    public class FileWriter
    {
        private string fileName;
        private string header;

        public FileWriter(string fileName, string header)
        {
            this.fileName = fileName;
            this.header = header;
        }

        public void Write(string message)
        {
            // Obtener la ruta del escritorio del usuario
            string desktopPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Desktop);

            // Definir el nombre del archivo CSV
            string filePath = Path.Combine(desktopPath, fileName);

            // Si el archivo no existe, creamos la cabecera
            if (!File.Exists(filePath) && !string.IsNullOrEmpty(header))
                File.WriteAllText(filePath, header + System.Environment.NewLine);

            // Agregar la nueva línea al final del archivo
            using (StreamWriter swt = File.AppendText(filePath))
                swt.WriteLine(message);
        }
    }
}
