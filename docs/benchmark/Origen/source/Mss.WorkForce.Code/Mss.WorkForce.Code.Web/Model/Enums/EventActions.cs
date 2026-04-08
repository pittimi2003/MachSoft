namespace Mss.WorkForce.Code.Web.Model.Enums
{
    public enum EventActions
    {
        New,
        Cancel,
        Insert,
        Update,
        Delete,
        Clone,
        Designer,
        Selected,
        MultiSelected,
        UnSelected,
        Refresh,
        CollapseGrid,
        ExpandGrid,
        Edit,
        SaveValid,
        Enabled,
        Disabled,
        ChangePassword,
        Closing,
        UpdateTimeGantt,

        //Barra de herramientas
        RevertColumns,
        UpdateColumns,
        ChooseColumns,
        ZoomOutZoomIn,
        ExportPDF,
        ExportCSV,
        Print,
        CollapsedExpanded,
        ToNow,
        ShowTimeIntervalModal,
        ApplyFilterTimeInterval,
        ShowAlertModal,
        ChangeView,
       

        //Carga de datos
        Preview,
        Planning,
        Dashboard,
        LaborManagement,

        //Datos para el Pivot
        PivotDataFilter,
        PivotColumns,

        //popup
        Open,
      
        //Schedule
        GotoNowJS,
        ExportServicePDF,
		ExportServiceExcel,
        ExportServicePrint,
        CollapsedExpandedScheduler,

        //LiveScheduleGrid
        UpdateScheduleGrid
    }
}
