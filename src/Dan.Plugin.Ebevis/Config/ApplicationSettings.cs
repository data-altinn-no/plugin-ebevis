namespace Dan.Plugin.Ebevis.Config
{
    public class ApplicationSettings
    {
        /// <summary>
        /// DAN environment the upstream plugins and the CCR proxy are resolved in: dev, test or prod.
        /// Dan.Common builds the plugin URLs (func-es{source}-...) and CCR URLs from this value.
        /// </summary>
        public string DanEnvironment { get; set; }

        /// <summary>
        /// Function host key sent as the "code" query parameter when calling the upstream plugins (ES_BR, ES_SVV, ES_ARBT).
        /// </summary>
        public string PluginCode { get; set; }
    }
}
