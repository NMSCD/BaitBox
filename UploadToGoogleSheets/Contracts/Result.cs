namespace NMSCD.BaitBox.Contracts
{
    public class Result
    {
        public bool IsSuccess { get; set; }
        public bool HasFailed => !IsSuccess;
        public string ExceptionMessage { get; set; }

        public Result(bool isSuccess, string exceptionMessage)
        {
            IsSuccess = isSuccess;
            ExceptionMessage = exceptionMessage;
        }

        public override string ToString()
        {
            return $"Success: {IsSuccess}, ExceptionMessage: {ExceptionMessage}";
        }
    }
}
