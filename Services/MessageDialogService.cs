using System.Threading.Tasks;
using System.Windows;

namespace HCL_ODA_TestPAD.Services;

public class MessageDialogService : IMessageDialogService
{
    public async Task<MessageDialogResult> ShowOkCancelDialogAsync(string text, string title)
    {
        var result = MessageBox.Show(text, title, MessageBoxButton.OKCancel);
        return await Task.Run(() => result == MessageBoxResult.OK ? MessageDialogResult.OK : MessageDialogResult.Cancel);
    }

    public Task ShowInfoDialogAsync(string text, string title)
    {
        MessageBox.Show(text, title, MessageBoxButton.OK);
        return Task.CompletedTask;
    }
}
