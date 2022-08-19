using Xunit.Abstractions;

namespace ChatService.Tests;

public class ConsoleWriter : StringWriter
{
    private readonly ITestOutputHelper output;

    public ConsoleWriter(ITestOutputHelper output)
    {
        this.output = output;
    }

    public override void WriteLine(string m)
    {
        output.WriteLine(m);
    }
}