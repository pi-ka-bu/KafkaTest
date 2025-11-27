namespace KafkaTest.UI;

public class WriteColoredLine
{
    public string _message { get; set; }
    public WriteColoredLine(string text = "")
    {
        _message = text;
    }

    public static void WriteColorLine(string text = "", ConsoleColor foregroundColor = ConsoleColor.White, ConsoleColor backgroundColor = ConsoleColor.Black)
    {
        ConsoleColor originalForegroundColor = Console.ForegroundColor;
        ConsoleColor originalBackgroundColor = Console.BackgroundColor;

        Console.ForegroundColor = foregroundColor;
        Console.BackgroundColor = backgroundColor;
        Console.WriteLine(text);

        Console.ForegroundColor = originalForegroundColor;
        Console.BackgroundColor = originalBackgroundColor;
    }
}
