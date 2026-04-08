namespace Mss.WorkForce.Code.Web.Model.Enums
{
    public enum eGanttMethods
    {
        updateToDay,
        zoom,
        exportGanttToPdf,
        exportGanttToCsv,
        printGantt,
        collapsedExpandedClick,
		ReadyLoadGantt,
		repaintGantt,
        getStripLine,
        deleteFiltersColumns,
        paintRowParent,
        closePopup,
        changeNameBtnPriority,
        showToltip,

        //Gantt
        loadIntanceGantt,
		loadDataForGantt,

        //pivot
        resetPivot,
		collapsedExpanded,
		exportPivotToCsv,
        loadColumnsDefault,
        drawChartAndPivotTogether,
        reloadPivotGrid,
        loadWarehouseName,
        ClosePopupColumnChooser,
        loadDataForPivot,
        disposeInstanceGantt,


        //schedule
        GoToNow,
	}

    public enum eSiteMethods
    {
        getReorderedFieldNames,
    }
}
