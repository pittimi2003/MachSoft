using DevExpress.CodeParser;
using Microsoft.AspNetCore.Components;
using Mss.WorkForce.Code.Models.DTO;
using Mss.WorkForce.Code.Models.Enums;
using Mss.WorkForce.Code.Models.Models;
using Mss.WorkForce.Code.Web.Model;
using static Mss.WorkForce.Code.Models.DataAccess.DataAccess;

namespace Mss.WorkForce.Code.Web.Components.Shared
{
    public partial class MlxAlertConfiguration
    {
        #region Fields

        private AlertConfigurationDto _modelConfiguration;

        private bool EnableAddNewButton = false;

        private string S_Severity = string.Empty;
        private string S_Type = string.Empty;
        private string S_Actions = string.Empty;
        private string S_SelectType = string.Empty;
        #endregion

        #region Properties

        [Parameter] public EventCallback<ICollection<AlertConfigurationDto>> AlertConfigurationChanged { get; set; }

        [Parameter]
        public ICollection<AlertConfigurationDto> AlertConfigurations { get; set; } = new List<AlertConfigurationDto>();

        [Parameter]
        public string EntityName { get; set; }

        [Parameter]
        public int Columns { get; set; }

        [Parameter]
        public bool IsEditing { get; set; }
        public IEnumerable<SelectItemEnum> Types { get; set; }

        private FieldTypeEnum FieldType { get; set; } = FieldTypeEnum.None;
        private AlertConfigurationDto ModelConfiguration { get; set; } = new();

        #endregion

        #region Methods

        protected override void OnInitialized()
        {
            Transalate();
            //Types = EnumHelper.GetSelectItems<AlertType>().Select(x => new SelectItemEnum { Text = L.Loc(x.Text), Value = x.Value }).ToList();
            ReloadTypes();
        }

        private void Transalate()
        {
           S_Severity = L.Loc("Severity");
           S_Type = L.Loc("Type");
           S_Actions = L.Loc("Actions");
           S_SelectType = L.Loc("Select type...");
        }

        private async Task AddAlertConfiguration()
        {
            ModelConfiguration.Id = Guid.NewGuid();

            AlertConfigurations.Add(ModelConfiguration);

            await AlertConfigurationChanged.InvokeAsync(AlertConfigurations);
            ModelConfiguration = new();
            EnableAddNewButton = false;
            ReloadTypes();
        }

        private async void OnTypeChanged(AlertType? newType)
        {
            ModelConfiguration.Type = newType;
            EnableAddNewButton = true;
        }

        private void ReloadTypes()
        {
            var usados = AlertConfigurations.Select(x => x.Type).ToList();
            Types = EnumHelper.GetSelectItems<AlertType>().Select(x => new SelectItemEnum { Text = L.Loc(x.Text), Value = x.Value})
                .Where(item => !usados.Contains((AlertType)item.Value))
                .ToList();
        }

        private async Task RemoveAlertConfiguration(Guid id)
        {
            if (AlertConfigurations.FirstOrDefault(a => a.Id == id) is AlertConfigurationDto config)
            {
                AlertConfigurations.Remove(config);

                await AlertConfigurationChanged.InvokeAsync(AlertConfigurations);
                ReloadTypes();
            }
        }

        private void OnAlertConfigurationChanged(string fieldName)
        {
            EnableAddNewButton = true;
        }

        private string GetSeverityIcon(object alertSeverity)
        {
            switch ((AlertSeverity)alertSeverity)
            {
                case AlertSeverity.Error:
                    return "mlx-ico-error-bg-solid";

                case AlertSeverity.Warning:
                    return "mlx-ico-warning-bg-solid";

                case AlertSeverity.Information:
                    return "mlx-ico-info-bg-solid";
                default:
                    return "SeverityNotFound";
            }
        }


        string GetIconText(string name) => name switch
        {
            "Error" => EnumHelper.GetItemDescription(EnumSeverityIcon.Error),
            "Warning" => EnumHelper.GetItemDescription(EnumSeverityIcon.Warning),
            "Information" => EnumHelper.GetItemDescription(EnumSeverityIcon.Information),
            _ => name
        };

        string GetIconTextDisabled(string name) => name switch
        {
            "Error" => EnumHelper.GetItemDescription(EnumSeverityIconDisabled.Error),
            "Warning" => EnumHelper.GetItemDescription(EnumSeverityIconDisabled.Warning),
            "Information" => EnumHelper.GetItemDescription(EnumSeverityIconDisabled.Information),
            _ => name
        };

        private void ActivarSeverity(AlertSeverity tipo)
        {
            ModelConfiguration.Severity = tipo;
            EnableAddNewButton = ModelConfiguration.Type != null;
        }
        #endregion
    }
}
