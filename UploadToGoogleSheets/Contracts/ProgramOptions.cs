using CommandLine;

namespace NMSCD.BaitBox.Contracts
{
    public enum OutputType
    {
        file,
        googlesheet,
    }

    public class ProgramOptions
    {
        [Option('i', "input", Required = false, HelpText = "The input file supplied by ThatBomberBoi, path is relative to the executable")]
        public string? JsonFile { get; set; }

        [Option("output", Required = false, HelpText = "The destination type of the export. 'file' or 'googlesheet'")]
        public OutputType? OutputType { get; set; }

        [Option("dest", Required = false, HelpText = "The output file to write data to. Required if output is 'file'")]
        public string? OutputFile { get; set; }

        [Option("auth", Required = false, HelpText = "The output file to write data . Required if output is 'googlesheet'")]
        public string? GoogleAuth { get; set; }

        [Option("sheetId", Required = false, HelpText = "The id of the google sheet. Required if output is 'googlesheet'")]
        public string? GoogleSheetId { get; set; }

        [Option("sheetName", Required = false, HelpText = "The name of the google sheet. Required if output is 'googlesheet'")]
        public string? GoogleSheetName { get; set; }
    }
}
