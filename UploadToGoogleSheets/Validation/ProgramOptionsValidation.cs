using NMSCD.BaitBox.Contracts;

namespace NMSCD.BaitBox.Validation
{
    public class ProgramOptionsValidation
    {
        public static Result ValidateOptions(ProgramOptions options)
        {
            if (options == null) return new Result(false, "Options were null");
            if (options.OutputType == OutputType.file && string.IsNullOrEmpty(options.OutputFile)) return new Result(false, "Output file is required when OutputType = file");
            if (options.OutputType == OutputType.googlesheet)
            {
                if (string.IsNullOrEmpty(options.GoogleAuth)) return new Result(false, "GoogleAuth is required when OutputType = googlesheet");
            }

            return new Result(true, string.Empty);
        }
    }
}
