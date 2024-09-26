
namespace NMSCD.BaitBox.Contracts
{
    public class GoogleSheet
    {
        public required string Range { get; set; }
        public required string MajorDimension { get; set; }
        public required List<List<string>> Values { get; set; }
    }
}
