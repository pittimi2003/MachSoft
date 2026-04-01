using MachSoft.UI.Models;

namespace MachSoft.UI.Services;

public interface IMxSnackbarService
{
    void Show(string message, MxSnackbarTone tone = MxSnackbarTone.Info, int? visibleStateDurationMs = null);
}
