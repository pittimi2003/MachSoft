namespace MachSoft.UI.Models;

/// <summary>
/// Define cómo se comporta un sidebar dentro de <c>MxWorkspaceLayout</c>.
/// </summary>
public enum MxSidebarMode
{
    /// <summary>
    /// El sidebar ocupa una columna real dentro del layout y reduce el ancho útil del contenido central.
    /// </summary>
    Inline = 0,

    /// <summary>
    /// El sidebar se superpone sobre el contenido central y no altera el cálculo de columnas del área principal.
    /// </summary>
    Overlay = 1
}
