namespace cAlgo.Robots;

public static class Logger
{
    public static void Log(string message)
    {
        Print(message, new object[] { });
    }

    public static Action<string, object[]> Print { get; set; }
}
