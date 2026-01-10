using Microsoft.EntityFrameworkCore;
using PredikceVytěžováníFVE.Models.DB;
using PredikceVytěžováníFVE.Services;

namespace PredikceVytěžováníFVE.Data {
    public class FVEDbContext(DbContextOptions<FVEDbContext> options) : DbContext(options) {
        public DbSet<MqttData> MqttData { get; set; }
        public DbSet<SpotBo> SpotData { get; set; }
    }
}
