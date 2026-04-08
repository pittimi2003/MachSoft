
namespace Mss.WorkForce.Code.Web.Services
{
    public class MlxDialogService : IMlxDialogService
    {
        private TaskCompletionSource<bool>? _dialogTaskCompletionSource;

        public event Action<string, string, bool, string, bool, string>? OnShowDialog;
        public event Action? OnCloseDialog;

        public Task<bool> ShowDialogAsync(string title, string message, bool viewCancel = true, string cancelBtnText = "", bool viewAcept = true, string aceptBtnText = "")
        {
            _dialogTaskCompletionSource = new TaskCompletionSource<bool>();
            OnShowDialog?.Invoke(title, message,viewCancel, cancelBtnText, viewAcept, aceptBtnText);
            return _dialogTaskCompletionSource.Task;
        }

        public void CloseDialog(bool result)
        {
            _dialogTaskCompletionSource?.SetResult(result);
            OnCloseDialog?.Invoke();
        }
    }
}
