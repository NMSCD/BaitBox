namespace NMSCD.BaitBox.Contracts
{
    public class ResultWithValue<T> : Result
    {
        public T Value { get; set; }

        public ResultWithValue(bool isSuccess, T value, string exceptionMessage) : base(isSuccess, exceptionMessage)
        {
            Value = value;
            IsSuccess = isSuccess;
            ExceptionMessage = exceptionMessage;
        }

        public override string ToString()
        {
            return $"Success: {IsSuccess}, ExceptionMessage: {ExceptionMessage}";
        }

        public static ResultWithValue<T> FromResult(Result oldResult, T result)
        {
            return new ResultWithValue<T>(oldResult.IsSuccess, result, oldResult.ExceptionMessage);
        }
    }
}
