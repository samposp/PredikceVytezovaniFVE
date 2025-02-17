namespace PredikceVytěžováníFVE.Helpers {
    public class UriHelper {
        public static string CreateUrl(List<string> path, Dictionary<string, string> queryParams) {
            string url = string.Join("/", path);
            url += "?";
            url += string.Join("&", queryParams.Select(x => x.Key + "=" + x.Value));
            return url;
        }
    }
}
