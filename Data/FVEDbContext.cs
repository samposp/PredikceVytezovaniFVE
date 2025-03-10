using Microsoft.EntityFrameworkCore;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;

namespace PredikceVytěžováníFVE.Data {
    public class FVEDbContext : DbContext {
        public FVEDbContext(DbContextOptions<FVEDbContext> options) : base(options)
        { }

        public DbSet<MqttData> mqttData { get; set; }
    }
}
