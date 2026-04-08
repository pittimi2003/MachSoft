namespace Mss.WorkForce.Code.Web
{
    public interface IDataCharts
    {
        Task<IEnumerable<DataCharts>> GetChartsAsync();
    }
}
