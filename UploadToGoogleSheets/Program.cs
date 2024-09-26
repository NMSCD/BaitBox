using AssistantApps.NoMansSky.Info.Contract;
using AssistantApps.NoMansSky.Info.Service;
using CommandLine;
using Newtonsoft.Json;
using NMSCD.BaitBox.Contracts;
using NMSCD.BaitBox.Integration;
using NMSCD.BaitBox.Repository;
using NMSCD.BaitBox.Validation;

namespace NMSCD.BaitBox;
class Program
{
    private static readonly string baitSheetId = "1x9LFIzRIFG8B17wQqDNaD77atbtVtq9YK_PsbIJasiY";
    private static readonly string baitSheetRowSelection = "InProgress!A5:F";

    public static async Task Main(string[] args)
    {
        await Parser.Default.ParseArguments<ProgramOptions>(args)
            .WithParsedAsync(RunWithOptions);
    }

    private static async Task RunWithOptions(ProgramOptions initialOptions)
    {
        string currentDir = Directory.GetCurrentDirectory();
        var dotenv = Path.Combine(currentDir, ".env");
        var options = DotEnv.Load(dotenv, initialOptions);

        var validationResult = ProgramOptionsValidation.ValidateOptions(options);
        if (validationResult.HasFailed)
        {
            Console.WriteLine($"An error occurred while validating supplied options:\n{validationResult.ExceptionMessage}"); 
            return;
        }

        OutputType outputType = options.OutputType ?? OutputType.file;
        List<string> gameOutput = await LoadOutputFromGame(options);
        if (gameOutput.Count < 1) return;

        string assetsFolder = Path.Combine(currentDir, "Assets");
        NmsInfoService _nmsInfoService = new NmsInfoService(assetsFolder);
        List<BaseItemDetails> itemDetails = await _nmsInfoService.GetAllItemDetailsForLanguage("en");

        List<GameInfoToUpload> dataToExport = new List<GameInfoToUpload>();
        foreach (string outputLine in gameOutput)
        {
            GameInfoFromLine gameItem = GetGameInfoFromLine(outputLine);
            BaseItemDetails? baseItem = null;
            try
            {
                baseItem = itemDetails
                    .FirstOrDefault(x =>
                        x.Name.Equals(gameItem.ItemName, StringComparison.InvariantCultureIgnoreCase)
                    );
            }
            catch { }

            if (baseItem == null)
            {
                Console.WriteLine($"Could not find item with name '{gameItem.ItemName}'");
            }

            dataToExport.Add(new GameInfoToUpload
            {
                AppId = baseItem?.Id ?? string.Empty,
                ItemName = gameItem.ItemName,
                CatchRarity = gameItem.CatchRarity,
                SizeImprovement = gameItem.SizeImprovement,
                Other = gameItem.Other,
            });
        }

        switch(outputType)
        {
            case OutputType.file:
                await WriteToOutputFile(options, dataToExport);
                break;
            case OutputType.googlesheet:
                await WriteToGoogleSheet(options, dataToExport);
                break;
        }
    }

    private static async Task<List<string>> LoadOutputFromGame(ProgramOptions options)
    {
        string jsonFile = options.JsonFile ?? "Output.json";
        Console.WriteLine($"Loading '{jsonFile}'");
        string currentDir = Directory.GetCurrentDirectory();
        string jsonFilePath = Path.Combine(currentDir, jsonFile);
        try
        {
            string content = await File.ReadAllTextAsync(jsonFilePath);
            List<string>? result = JsonConvert.DeserializeObject<List<string>>(content);

            if (result != null) return result;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong reading '{jsonFile}'!\nFull path: {jsonFilePath} \n{ex}");
        }

        return [];
    }

    private static async Task WriteToOutputFile(ProgramOptions options, List<GameInfoToUpload> dataToExport)
    {
        string outputFile = options.OutputFile ?? "out.txt";
        Console.WriteLine($"Writing to '{outputFile}'");
        string currentDir = Directory.GetCurrentDirectory();
        string jsonFilePath = Path.Combine(currentDir, outputFile);

        List<string> textFileLines = new List<string>
        {
            "\tData extracted from game files (5.12) by ThatBomberBoi",
            string.Join("\t", new List<string>{ "AppId", "Name", "Catch Rarity %", "Size Improvement %", "Notes" })

        };
        foreach (var gameItem in dataToExport)
        {
            List<string> textFileLine = new List<string>
            {
                gameItem.AppId,
                gameItem.ItemName,
                gameItem.CatchRarity,
                gameItem.SizeImprovement,
                gameItem.Other.Equals("N/A") ? string.Empty : gameItem.Other,
            };
            textFileLines.Add(string.Join("\t", textFileLine));
        }

        try
        {
            await File.WriteAllLinesAsync(jsonFilePath, textFileLines);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Something went wrong writing '{outputFile}'!\nFull path: {jsonFilePath} \n{ex}");
        }
    }

    private static GameInfoFromLine GetGameInfoFromLine(string inputLine)
    {
        string initialSizeString = getValueBetween(inputLine, "Catch Size Improvement ", "<>");
        string sizeString = initialSizeString.Substring(initialSizeString.IndexOf(">") + 1);

        string initialOtherString = getValueBetween(inputLine, "Other notes: ", "<>");
        string otherString = initialOtherString.Substring(initialOtherString.IndexOf(">") + 1);

        return new GameInfoFromLine {
            ItemName = getValueBetween(inputLine, "Current Bait: <STELLAR>", "<>"),
            CatchRarity = getValueBetween(inputLine, "Catch Rarity Improvement: <TRADEABLE>", "<>"),
            SizeImprovement = sizeString,
            Other = otherString,
        };
    }

    private static string getValueBetween(string input, string starting, string end)
    {
        int indexOfStart = input.IndexOf(starting);
        int indexOfStartPlusStart = indexOfStart + starting.Length;
        string inputExcludingThStart = input.Substring(indexOfStartPlusStart);
        int indexOfEnd = inputExcludingThStart.IndexOf(end);
        string result = input.Substring(indexOfStartPlusStart, indexOfEnd);
        return result;
    }

    private static async Task WriteToGoogleSheet(ProgramOptions options, List<GameInfoToUpload> dataToExport)
    {
        GoogleSheetRepository gRepo = new GoogleSheetRepository(new HttpClient(), options.GoogleAuth!);
        var itemsResult = await gRepo.GetSheetContents(baitSheetId, "InProgress!A1:E");
        var itemsssResult = await gRepo.AddSheetContents(baitSheetId, "InProgress!A1:E", new List<List<string>>
        {
            new List<string>
            {
                "1",
                "2",
                "3",
                "4",
                "5",
            }
        });
        //var clearResult = await gRepo.ClearSheetContents(baitSheetId, $"InProgress!A1:E");
        if (itemsResult.IsSuccess)
        {

        }
    }
}