namespace RuntimeNodes
{
    public class ResolveTypeResult
    {
        public string Message { get; private set; }
        public static ResolveTypeResult Failed(string failedMessage)
        {
            return new ResolveTypeResult { Message = failedMessage };
        }

        public static ResolveTypeResult Succeeded { get; } = new ResolveTypeResult() { Message = "Succeeded" };
        public static ResolveTypeResult Uncompleted { get; } = new ResolveTypeResult() { Message = "Uncompleted" };
    }
}