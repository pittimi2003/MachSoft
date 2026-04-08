namespace Mss.WorkForce.Code.Web.Services
{
    public class SendParamService 
    {
        /// <summary>
        /// Warehouse ID to selected Warehouse in view
        /// </summary>
        public Guid WarehouseId { get; set; }
        public string ActiveTab { get; set; } = "tab1";
        public Guid ActiveProjectId { get; set; }
        public string? ActiveProjectName { get; set; }

        public bool ReloadInitData { get => reloadInitData; 
            set { 
                reloadInitData = value;
                if (reloadInitData)
                    NotifyStateChange();
            }
        }

        /// <summary>
        /// Flag to force reload data in MainLayout
        /// </summary>
        private bool reloadInitData;

        public event Action? OnChange;

        private void NotifyStateChange()
        {
            OnChange?.Invoke();
        }
    }
}
