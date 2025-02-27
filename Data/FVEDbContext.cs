using Microsoft.EntityFrameworkCore;
using PredikceVytěžováníFVE.Models.DB;

namespace PredikceVytěžováníFVE.Data {
    public class FVEDbContext : DbContext {
        public FVEDbContext(DbContextOptions<FVEDbContext> options) : base(options)
        { }

        public DbSet<MqttBo> mqttBos { get; set; }
    }
}
