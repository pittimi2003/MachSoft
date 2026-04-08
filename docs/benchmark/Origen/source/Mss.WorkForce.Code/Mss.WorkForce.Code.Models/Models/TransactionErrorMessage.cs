namespace Mss.WorkForce.Code.Models.Models
{
    public static class TransactionErrorMessage
    {
        public static string UpsertInputOrder(string warehouseCode) =>  $"Error trying to upsert the input orders into {warehouseCode} Warehouse.\n\nError trace: ";
        public static  string UpsertInputOrderProcesses(string warehouseCode) => $"Error trying to upsert the input order processes into {warehouseCode} Warehouse.\n\nError trace: ";

        public static string UpsertWarehouseProcesses(string warehouseCode) => $"Error trying to upsert the warehouse processes into {warehouseCode} Warehouse.\n\nError trace: ";
    }
}