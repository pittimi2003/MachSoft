using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Common;
using Mss.WorkForce.Code.Web.Components.BaseComponent;
using Mss.WorkForce.Code.Web.Model.Enums;
using Mss.WorkForce.Code.Web.Services;
using System.Collections.ObjectModel;

namespace Mss.WorkForce.Code.Web.Components.Pages.Configuration.TransactionLog
{
    public partial class TransactionLog : PageOperations, IDisposable
    {

        #region Fields

        private BaseComponente<TransactionDto>? baseComponentRef;
        private List<Guid> SelectedTransactionIds = new();
        private EventActions SelectMode = EventActions.UnSelected;

        #endregion

        #region Properties

        [Inject] private IContextConfig _contextConfig { get; set; }
        [Inject] private IInitialDataService _InitialDataService { get; set; }
        [Inject] private SendParamService _SendParamService { get; set; }
        [Inject] private LocalStorageService _storageService { get; set; }
        [Inject] private IMlxDialogService DialogService { get; set; }
        [Inject] private IEventServices eventServices { get; set; }
        [Inject] private IUserService UserService { get; set; }
        [Inject] private ITransactionService TransactionService { get; set; }

        private List<EventActions> Actions { get; set; }
        private string Cancel = string.Empty;
        private string Data = string.Empty;
        private string EmiterContainer { get; set; }
        private string EventListener { get; set; } = string.Empty;
        private GetAttributesDto<List<TransactionDto>> GetParametersDto { get; set; }
        private bool IsEditMode { get; set; } = false;
        private bool IsNewMode { get; set; } = false;
        private List<TransactionDto?> lstSelectedTransactions { get; set; } = new();
        private string Save = string.Empty;
        private bool savebtn { get; set; } = false;
        private TransactionDto? SelectedTransaction { get; set; } = null;
        private ReadOnlyCollection<TimeZoneInfo> TimeZones { get; set; }

        #endregion

        #region Methods

        public void BaseComponentActions(EventArguments eventArguments)
        {
            savebtn = (eventArguments.EventActions == EventActions.SaveValid && eventArguments.EventData is bool flag && flag);
            StateHasChanged();
        }

        public override void CancelEvent()
        {
            IsNewMode = false;
            SelectMode = EventActions.UnSelected;
        }

        public override bool DetailsActionsCancel(EventArguments eventArguments)
        {
            IsEditMode = false;
            StateHasChanged();
            return true;
        }

        public override async Task<bool> DetailsActionsUpdate(EventArguments eventArguments)
        {
            try
            {
                if (eventArguments.EventData is List<TransactionDto> transactionDto)
                {
                    await TransactionService.UpdateTransaction(transactionDto);
                    GetParametersDto.Model = GetTransactions();
                    IsEditMode = false;
                    eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
                    _SendParamService.ReloadInitData = true;
                    StateHasChanged();
                    return true;
                }
                else
                {
                    throw new InvalidOperationException("Invalid data type for update action.");
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void Dispose()
        {
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.GridPublish, GridActions);
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.ToolBarPublish, ToolBarActions);
            eventServices.Unsubscribe(GetParametersDto.NameContainer, GetParametersDto.DetailPublish, DetailsAction);
            eventServices.Unsubscribe(nameof(Transaction), EmiterContainer, BaseComponentActions);
        }

        public override bool GridActionsCollapseGrid(EventArguments eventArguments)
        {
            IsDetailsVisible = true;
            isExpandGrid = false;
            StateHasChanged();
            return true;
        }

        public override bool GridActionsExpandGrid(EventArguments eventArguments)
        {
            IsDetailsVisible = false;
            isExpandGrid = true;
            StateHasChanged();
            return true;
        }

        public override bool GridActionsMultiSelected(EventArguments eventArguments)
        {
            lstSelectedTransactions.Clear();
            SelectedTransactionIds.Clear();
            if (eventArguments.EventData is List<object> selectedItems && selectedItems.Count >= 1)
            {
                foreach (object selectedItem in selectedItems)
                {
                    if (!lstSelectedTransactions.Contains(selectedItem))
                        lstSelectedTransactions.Add(selectedItem as TransactionDto);
                }
                SelectedTransaction = selectedItems.FirstOrDefault() as TransactionDto;
                SelectedTransactionIds = lstSelectedTransactions.Select(w => w.Id).ToList();
                SelectMode = EventActions.MultiSelected;
                IsDetailsVisible = true;
                isExpandGrid = false;
            }

            return true;
        }

        public override bool GridActionsRefresh(EventArguments eventArguments)
        {
            GetParametersDto.Model = GetTransactions();
            eventServices.Publish(GetParametersDto.NameContainer, new EventArguments(GetParametersDto.NameContainer, EventActions.Refresh, null));
            return true;
        }

        public override bool GridActionsSelected(EventArguments eventArguments)
        {
            isExpandGrid = true;
            SelectedTransaction = null;
            lstSelectedTransactions.Clear();
            SelectedTransactionIds.Clear();
            IsDetailsVisible = false;
            SelectMode = EventActions.UnSelected;

            if (eventArguments.EventData is List<object> selectedItems && selectedItems.Count == 1)
            {
                lstSelectedTransactions.Add(selectedItems.FirstOrDefault() as TransactionDto);
                SelectedTransaction = selectedItems.FirstOrDefault() as TransactionDto;
                SelectedTransactionIds = lstSelectedTransactions.Select(w => w.Id).ToList();
                IsDetailsVisible = true;
                SelectMode = EventActions.Selected;
                isExpandGrid = false;
            }

            return true;
        }

        public override async Task SaveEvent()
        {
            try
            {
                var isValid = baseComponentRef != null ? baseComponentRef.Validate() : false;

                if (IsNewMode && SelectedTransaction != null && isValid)
                {
                    await TransactionService.AddTransaction(SelectedTransaction);
                    IsNewMode = false;
                    SelectedTransaction = null;

                    GetParametersDto.Model = GetTransactions();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al guardar el registro: {ex.Message}");
            }

            StateHasChanged();
        }

        public override bool ToolBarActionsNew(EventArguments eventArguments)
        {
            lstSelectedTransactions.Clear();
            SelectedTransactionIds.Clear();
            IsNewMode = true;
            IsDetailsVisible = false;
            AddNewItem();
            lstSelectedTransactions.Add(SelectedTransaction);
            return true;
        }

        public override bool ToolBarActionsUpdate(EventArguments eventArguments)
        {
            if (SelectedTransaction != null)
            {
                IsEditMode = true;
                isExpandGrid = false;
                IsDetailsVisible = true;
            }
            return true;
        }
        protected override void LoadData()
        {
            // Lógica específica para este componente

        }

        protected override void OnInitialized()
        {
            TranslateFields();
            GetParametersDto = new(l);
            GetParametersDto.Model = GetTransactions();

            GetParametersDto.NameContainer = nameof(Transaction);
            GetParametersDto.GridPublish = $"{nameof(GridActions)}{nameof(Transaction)}";
            GetParametersDto.ToolBarPublish = $"{nameof(ToolBarActions)}{nameof(Transaction)}";
            GetParametersDto.DetailPublish = $"{nameof(DetailsAction)}{nameof(Transaction)}";

            EventListener = $"{nameof(ToolBarActions)}{nameof(Transaction)}";
            EmiterContainer = $"{nameof(Transaction)}{nameof(BaseComponent)}";
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.GridPublish, GridActions);
            //eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.ToolBarPublish, ToolBarActions);
            eventServices.Subscribe(GetParametersDto.NameContainer, GetParametersDto.DetailPublish, DetailsAction);
            eventServices.Subscribe(nameof(Transaction), EmiterContainer, BaseComponentActions);

            Actions = new List<EventActions>() {}; // de momento no tiene

        }


        private void TranslateFields()
        {
            Data = Loc("DATA");
            Cancel = Loc("Cancel");
            Save = Loc("Save");
        }
        private void AddNewItem()
        {
            SelectedTransaction = TransactionDto.NewDto();
        }



        private List<TransactionDto> GetTransactions()
        {
            return TransactionService.GetTransactions(_InitialDataService.GetDatauser()?.Id ?? Guid.Empty).OrderByDescending(x => x.CreationDate).ToList();
        }

        private void OnRightSideVisibilityChanged(bool isVisible)
        {
            Console.WriteLine($"Right sidebar visibility changed: {isVisible}");
        }


        #endregion
    }
}
