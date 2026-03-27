using Microsoft.EntityFrameworkCore;
using PredikceVytěžováníFVE.Models;
using PredikceVytěžováníFVE.Models.DB;

namespace PredikceVytěžováníFVE.Data {
    public class FVEDbContext(DbContextOptions<FVEDbContext> options) : DbContext(options) {
        public DbSet<MqttData> MqttData { get; set; }
        public DbSet<SpotBo> SpotData { get; set; }
        public DbSet<PvForecastBo> PvForecastData { get; set; }
        public DbSet<ForecastSolarBo> ForecastSolarData { get; set; }
        public DbSet<WeatherForecastBo> WeatherForecastData { get; set; }
        public DbSet<HourlyBo> HourlyData { get; set; }
        public DbSet<ConsumptionForecastBo> ConsumptionForecastData { get; set; }
        public DbSet<ControlPredictionBo> PredictedControlData { get; set; }
    }
}
