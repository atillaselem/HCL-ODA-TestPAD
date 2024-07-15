using System.Threading.Tasks;

namespace HCL_ODA_TestPAD.Services;

public enum MessageDialogResult
{
    Ok,
    Cancel
}

public interface IMessageDialogService
{
    Task<MessageDialogResult> ShowOkCancelDialogAsync(string text, string title);
    Task ShowInfoDialogAsync(string text, string title);
}
