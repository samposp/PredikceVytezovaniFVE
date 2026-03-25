using System.Text.Json.Serialization;

namespace PredikceVytěžováníFVE.Models.Settings;

public class ModelInfo
{
    public int? HistoryDays { get; set; }
    public bool Retrain { get; set; } = false;
    public SarimaxModel? SARIMAX { get; set; }
    public XGBoostParams? XGBoostParams { get; set; }
}

public class SarimaxModel
{
    public List<int>? Order { get; set; }
    [JsonPropertyName("seasonal_order")]
    public List<int>? SeasonalOrder { get; set; }
}

public class XGBoostParams
{
    public float? Subsample { get; set; }
    [JsonPropertyName("n_estimators")]
    public int? NEstimators { get; set; }
    [JsonPropertyName("max_depth")]
    public int? MaxDepth { get; set; }
    [JsonPropertyName("learning_rate")]
    public float? LearningRate { get; set; }
    [JsonPropertyName("colsample_bytree")]
    public float? ColsampleBytree { get; set; }
}