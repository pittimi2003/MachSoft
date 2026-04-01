using MachSoft.UI.Models;
using MudBlazor;

namespace MachSoft.UI.Services;

internal sealed class MxSnackbarService(ISnackbar snackbar) : IMxSnackbarService
{
    public void Show(string message, MxSnackbarTone tone = MxSnackbarTone.Info, int? visibleStateDurationMs = null)
    {
        var severity = tone switch
        {
            MxSnackbarTone.Success => Severity.Success,
            MxSnackbarTone.Warning => Severity.Warning,
            MxSnackbarTone.Error => Severity.Error,
            _ => Severity.Info
        };

        snackbar.Add(message, severity, options =>
        {
            options.SnackbarTypeClass = BuildToneClass(tone);
            options.VisibleStateDuration = visibleStateDurationMs ?? 4000;
            options.ShowCloseIcon = true;
        });
    }

    private static string BuildToneClass(MxSnackbarTone tone) => tone switch
    {
        MxSnackbarTone.Success => "mx-snackbar mx-snackbar-success",
        MxSnackbarTone.Warning => "mx-snackbar mx-snackbar-warning",
        MxSnackbarTone.Error => "mx-snackbar mx-snackbar-error",
        _ => "mx-snackbar mx-snackbar-info"
    };
}
