using Microsoft.EntityFrameworkCore;
using PredikceVytěžováníFVE.Models.DB;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PredikceVytěžováníFVE.Data
{
    public class FVEDbContext : DbContext {
        public FVEDbContext(DbContextOptions<FVEDbContext> options) : base(options)
        { }

        public DbSet<MqttDataBto> mqttData { get; set; }

        public DbSet<TimeChartData<int>> forecastData { get; set; }

        public DbSet<TimeChartData<double>> spotData {  get; set; }

        public DbSet<testData> testData { get; set; }
    }
    [PrimaryKey(nameof(dateTime))]
    public class testData {
        public DateTime dateTime { get; set; }
    }
}
