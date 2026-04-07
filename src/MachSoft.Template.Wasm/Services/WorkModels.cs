namespace MachSoft.Template.Wasm.Services;

public sealed record WorkItem(int Id, string Code, string Name, string Status, string Owner, DateTime UpdatedAtUtc);

public static class WorkSeed
{
    public static IReadOnlyList<WorkItem> Items { get; } =
    [
        new(1001, "WK-1001", "Solicitud de alta de proveedor", "Pendiente", "M. Rivera", DateTime.UtcNow.AddMinutes(-35)),
        new(1002, "WK-1002", "Validación de lote contable", "En proceso", "L. Prado", DateTime.UtcNow.AddMinutes(-18)),
        new(1003, "WK-1003", "Conciliación bancaria", "Observado", "A. Torres", DateTime.UtcNow.AddMinutes(-7)),
        new(1004, "WK-1004", "Aprobación de orden", "Aprobado", "S. Medina", DateTime.UtcNow.AddMinutes(-2))
    ];
}
