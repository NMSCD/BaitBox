using NMSCD.BaitBox.Contracts;

namespace NMSCD.BaitBox.Integration
{
    public static class DotEnv
    {
        public static string EnvKeyInputJsonFile = "INPUT_JSON_FILE";
        public static string EnvKeyOutputType = "OUTPUT_TYPE";
        public static string EnvKeyOutputFile = "OUTPUT_FILE";
        public static string EnvKeyGoogleAuth = "GOOGLE_AUTH";
        public static string EnvKeyGoogleSheetId = "GOOGLE_SHEET_ID";
        public static string EnvKeyGoogleSheetName = "GOOGLE_SHEET_NAME";

        public static ProgramOptions Load(string filePath, ProgramOptions options)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            if (!File.Exists(filePath))
                return options;

            foreach (var line in File.ReadAllLines(filePath))
            {
                var parts = line.Split(
                    '=',
                    StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length != 2)
                    continue;

                //Environment.SetEnvironmentVariable(parts[0], parts[1]);
                values.Add(parts[0], parts[1]);
            }


            if (values.ContainsKey(EnvKeyInputJsonFile)) options.JsonFile = values[EnvKeyInputJsonFile];
            if (values.ContainsKey(EnvKeyOutputType)) options.OutputType = (OutputType)Enum.Parse(typeof(OutputType), values[EnvKeyOutputType], true);
            if (values.ContainsKey(EnvKeyOutputFile)) options.OutputFile = values[EnvKeyOutputFile];
            if (values.ContainsKey(EnvKeyGoogleAuth)) options.GoogleAuth = values[EnvKeyGoogleAuth];
            if (values.ContainsKey(EnvKeyGoogleSheetId)) options.GoogleSheetId = values[EnvKeyGoogleSheetId];
            if (values.ContainsKey(EnvKeyGoogleSheetName)) options.GoogleSheetName = values[EnvKeyGoogleSheetName];

            return options;
        }
    }
}