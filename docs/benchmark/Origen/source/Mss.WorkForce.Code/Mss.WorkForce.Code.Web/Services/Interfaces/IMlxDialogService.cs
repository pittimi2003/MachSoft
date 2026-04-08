namespace Mss.WorkForce.Code.Web.Services
{
    public interface IMlxDialogService
    {
        event Action<string, string, bool, string, bool, string>? OnShowDialog;
        event Action? OnCloseDialog;
        Task<bool> ShowDialogAsync(string title, string message, bool viewCancel =true, string cancelBtnText ="", bool viewAcept = true, string aceptBtnText = "");
        void CloseDialog(bool result);
    }
}
