using System;

namespace HCL_ODA_TestPAD.Services;

public interface IConsoleService
{
    string Decor { get; set; }
    IConsoleService WriteL(string text);
    IConsoleService WriteR(string text);
    Func<string, string, bool, IConsoleService> ConsoleActions { get; set; }
    Func<bool, IConsoleService> Decorate { get; set; }
}
