
namespace Mss.WorkForce.Code.Web
{
    public class DataCharts
    {
        public DataCharts()
        { 
        }

        public int OrderId { get; set; }
        public string Region { get; set; }
        public string Country { get; set; }
        public string City { get; set; }
        public int Amount { get; set; }
        public DateTime Date { get; set; }


        public IEnumerable<DataCharts> GetDataCharts()
        {
            List<DataCharts> dataCharts = new List<DataCharts>() ;
           DataCharts item = null;
            for (int i = 0; i < 3; i++)
            {
                item = new DataCharts();
                item.OrderId = i;
                item.Region = "cdmx";
                item.Country = "Mexico";
                item.City = "CDMX";
                item.Amount = 152 + i * 2;
                item.Date = DateTime.Now.AddYears(-i);
                dataCharts.Add(item);
            }
            return dataCharts;
        }
    }

    
}
