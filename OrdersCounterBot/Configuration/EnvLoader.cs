namespace OrdersCounterBot.Configuration
{
    public static class EnvLoader
    {
        public static void Load() 
        {
            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Development";
            var envFile = environment == "Production" ? ".env.production" : ".env";
            DotNetEnv.Env.Load(envFile);
        }

        public static string GetApiToken()
        {
            string? apiToken = Environment.GetEnvironmentVariable("API_TOKEN");
            if (string.IsNullOrEmpty(apiToken)) throw new Exception("API_TOKEN is missing in environment.");
            return apiToken;
        }

        public static string GetDataPath()
        {
            string? dataPath = Environment.GetEnvironmentVariable("DATA_PATH");
            if (string.IsNullOrEmpty(dataPath)) throw new Exception("DATA_PATH is missing in environment.");
            return dataPath;
        }

        public static string GetListenerPrefix()
        {
            return Environment.GetEnvironmentVariable("LISTENER") ?? "http://localhost:8080/";
        }
    }
}
