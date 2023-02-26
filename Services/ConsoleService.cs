using System;
using System.Linq;

namespace HCL_ODA_TestPAD.Services;

public class ConsoleService : IConsoleService
{
    public string Decor { get; set; } = string.Concat(Enumerable.Repeat("=", 50));

    public IConsoleService WriteL(string text)
    {
        return ConsoleActions(text, null, true);
    }

    public IConsoleService WriteR(string text)
    {
        return ConsoleActions(null, text, false);
    }

    public Func<string, string, bool, IConsoleService> ConsoleActions { get; set; }

    public Func<bool, IConsoleService> Decorate { get; set; }
}
