
export class GanttHandler {

    // #region -------------------VARIABLES--------------------------------//

    static isGanttLoaded = true;      // Indicador para rastrear el estado del Gantt
    static fechaHoraria = null;
    static showToltipGantt = true;  

    // #endregion

    _updateToDay() {
        let ganttInstance = null;
        const $el = $("#GanttComponent");
        if ($el.data("dxGantt")) {
            ganttInstance = $el.dxGantt("instance");
        }

        if (!ganttInstance) {
            console.warn("Gantt instance no encontrada.");
            return;
        }

        const stripLineLeft = document.querySelector('.current-time')?.offsetLeft;
        const ganttView = ganttInstance._ganttView;

        if (stripLineLeft != null && ganttView) {
            const containerGantt = ganttView._taskAreaContainer;
            containerGantt.scrollLeft = stripLineLeft - 100;
        }
    }

    // #region ------------------ZOOM--------------------------------------//
    _zoom(isZoomIn) {
        let ganttInstance = null;
        const $el = $("#GanttComponent");
        if ($el.data("dxGantt")) {
            ganttInstance = $el.dxGantt("instance");
        }

        if (ganttInstance) {

            if (isZoomIn)
                ganttInstance.zoomIn();
            else {
                ganttInstance.zoomOut();
            }
            this.#BlockButton();
        }
    }

    // #endregion

    // #region -------------------BUTTONS----------------------------------//
    #BlockButton() {
        try {
            const ganttInstance = $("#GanttComponent").dxGantt("instance");
            const scale = ganttInstance.option('scaleType');
            const currentZoom = ganttInstance._ganttView._ganttViewCore.currentZoom;
            const btnZoomOut = $('#btnZoomOut');
            const btnZoomIn = $('#btnZoomIn');
            switch (scale) {
                case 'minutes':
                    if (currentZoom == 3) {
                        this.#disableButtonZoom(btnZoomIn);
                    }
                    else {
                        this.#enableButtonZoom(btnZoomIn);
                    }
                    break;
                case 'sixHours':
                    if (currentZoom == 1) {
                        this.#disableButtonZoom(btnZoomOut);
                    }
                    else if (currentZoom > 1) {
                        this.#enableButtonZoom(btnZoomOut);
                    }
                    break;

                default:
                    this.#enableButtonsZoom();
                    break;
            }
        }
        catch (error) {
            //logError("BlockButton", error);
        }
    }

    #disableButtonZoom(button) {
        button.removeClass('top-bar-button');
        button.addClass('top-bar-button-block');
        button.prop('disabled', true);
    }

    #enableButtonZoom(button) {
        button.removeClass('top-bar-button-block');
        button.addClass('top-bar-button');
        button.prop('disabled', false);
    }

    #enableButtonsZoom() {
        const btnZoomOut = $('#btnZoomOut');
        const btnZoomIn = $('#btnZoomIn');

        btnZoomOut.addClass('top-bar-button');
        btnZoomOut.removeClass('top-bar-button-block');
        btnZoomIn.addClass('top-bar-button');
        btnZoomIn.removeClass('top-bar-button-block');
        btnZoomOut.prop('disabled', false);
        btnZoomIn.prop('disabled', false);
    }

    #disableButtonsZoom(btnZoomOut, btnZoomIn) {
        btnZoomOut.addClass('top-bar-button');
        btnZoomOut.removeClass('top-bar-button-block');
        btnZoomIn.addClass('top-bar-button');
        btnZoomIn.removeClass('top-bar-button-block');
    }

    #EnableButtonsForDrawerInfoVisibility() {
        const btnDraweInfoPlanning = $('#BtnDrawerInfoPlanning');
        btnDraweInfoPlanning.prop("disabled", false);

        const btnDraweInfoPeview = $('#btnDrawerInfoPreview');
        btnDraweInfoPeview.prop("disabled", false);

        const btnReloadPreview = $('#btnReloadPreview');
        btnReloadPreview.prop("disabled", false);

        const btnAddTemporalidad = $('#btnAddTemporalidad');
        btnAddTemporalidad.prop("disabled", false);

        const btnEditTemporalidad = $('#btnEditTemporalidad');
        btnEditTemporalidad.prop("disabled", false);

        document.querySelectorAll('.mlx-component-visible').forEach(element => {
            element.removeAttribute('disabled');
        });

    }

    // #endregion

    // #region -------------------EXPORT-----------------------------------//
    _printGantt() {
        let ganttInstance = null;
        const $el = $("#GanttComponent");
        if ($el.data("dxGantt")) {
            ganttInstance = $el.dxGantt("instance");
        }

        if (ganttInstance) {
            window.print();
        }
    }

    _exportGanttToCsv(cultureCode) {
        let ganttInstance = null;
        const $el = $("#GanttComponent");
        if ($el.data("dxGantt")) {
            ganttInstance = $el.dxGantt("instance");
        }

        if (ganttInstance) {
            var tasks = ganttInstance.option("tasks").dataSource;
            var columns = ganttInstance.option("columns");
            this.#generateCSV(tasks, columns, cultureCode);
        }
    }

    #generateCSV(tasks, columns, cultureCode) {
        try {

            const booleanTranslations = {
                "es": { true: "Verdadero", false: "Falso" },
                "en": { true: "True", false: "False" },
            };

            const tr = booleanTranslations[cultureCode] || { true: "true", false: "false" };

            let csvContent = "\uFEFF" + columns.map(col => col.caption).join(",") + "\n";

            tasks._items.forEach(task => {
                let row = columns
                    .map(col => {
                    const dataField = col.dataField;
                    if (task.hasOwnProperty(dataField)) {
                        let value = task[dataField];
                        if (Array.isArray(value)) {
                            return value.join("; ");
                        }

                        if (value instanceof Date) {
                            return value.toLocaleDateString(cultureCode, {
                                weekday: "short",
                                day: "2-digit",
                                month: "short",
                                year: "numeric",
                                hour: "2-digit",
                                minute: "2-digit",
                                second: "2-digit",
                            });
                        }

                        if (typeof value === "boolean") {
                            return tr[value];
                        }
                        return value;
                    }
                    return "";
                }).join(",");

                csvContent += row + "\n";
            });

            const blob = new Blob([csvContent], { type: "text/csv;charset=utf-8;" });
            const link = document.createElement("a");
            link.href = URL.createObjectURL(blob);
            link.download = "gantt_data.csv";
            link.click();
        }
        catch (error) {
            //logError("generateCSV", error);
        }
    }

    _exportGanttToPdf(idGantt) {
        let ganttInstance = null;
        const $el = $("#GanttComponent");
        if ($el.data("dxGantt")) {
            ganttInstance = $el.dxGantt("instance");
        }

        if (ganttInstance) {
            const ganttElement = document.querySelector(`#${idGantt}`);
            const isLandscape = true;
            let exportMode = 'all';
            const dataRangeMode = 'all';
            const { jsPDF } = window.jspdf;
            html2canvas(ganttElement, {
                useCORS: true,
                allowTaint: true,
                scale: 1,
            }).then((canvas) => {
                const imgData = canvas.toDataURL('image/png');

                const pdf = new jsPDF({
                    orientation: isLandscape ? 'landscape' : 'portrait',
                    format: 'a0',
                    exportMode,
                    dateRange: dataRangeMode,
                });

                const pageHeight = pdf.internal.pageSize.height;
                const canvasHeight = canvas.height;
                const canvasWidth = canvas.width;

                let currentY = 0;

                pdf.addImage(imgData, 'PNG', 0, currentY, canvasWidth, pageHeight);
                currentY += pageHeight;

                while (currentY < canvasHeight) {
                    pdf.addPage();
                    pdf.addImage(imgData, 'PNG', 0, -currentY, canvasWidth, pageHeight);
                    currentY += pageHeight;
                }

                pdf.save('gantt.pdf');
            });
        }

    }

    // #endregion

    // #region -------------------COLLAPSED--------------------------------//
    _collapsedExpandedClick(isCollapsed) {
        const ganttInstance = $("#GanttComponent").dxGantt("instance");
        if (isCollapsed) {
            const taskExpanded = ganttInstance._treeList.option("expandedRowKeys");
            const taskGenerales = ganttInstance._tasks.filter(t => t.parentId === null && taskExpanded.includes(t.id));
            ganttInstance.beginUpdate();
            taskGenerales.forEach(x => ganttInstance.collapseTask(x.id));
            ganttInstance.endUpdate();
            this.#CollapsedAllLevels();
        }
        else {
            ganttInstance.expandAll();
        }

        this.#changeBtnExpandForEachTask(isCollapsed);
        this.#ChangeBtnExpandToCollapsed(isCollapsed);
        
        this._paintRowParent();
    }

    #CollapsedAllLevels() {
        setTimeout(() => {
            const ganttInstance = $("#GanttComponent").dxGantt("instance");
            ganttInstance.collapseAll();
            this._paintRowParent();
        }, 100);
    }

    #changeBtnExpandForEachTask(isExpand) {
        try {
            dataForGantt.TaskGantt.forEach(task => {
                const taskType = this.#getTaskType(task.TooltipType);
                if (taskType != 'LevelAct') {
                    task.isExpand = !isExpand;
                }
            });
        }
        catch (error) {
            //logError("changeBtnExpandForEachTask", error);
        }
    }

    #ChangeBtnExpandToCollapsed(isCollapsed) {
        const btnCollapsed = $('#btnCollapsed');
        const btnExpand = $('#btnExpand');
        if (isCollapsed) {

            btnCollapsed.removeClass('top-bar-button-show');
            btnCollapsed.addClass('top-bar-button-hide');
            btnExpand.removeClass('top-bar-button-hide');
            btnExpand.addClass('top-bar-button-show');
        }
        else {
            btnCollapsed.removeClass('top-bar-button-hide');
            btnCollapsed.addClass('top-bar-button-show');
            btnExpand.removeClass('top-bar-button-show');
            btnExpand.addClass('top-bar-button-hide');
        }
    }

    // #endregion

    // #region ------------------LINEA NOW-------------------------------//

    _getStripLine(timeZone) {
        try {
            if (!GanttHandler.showToltipGantt) return ''

            const fechaUTC = new Date(new Date().toISOString().replace("Z", ""));
            const offsetMs = timeZone * 60 * 60 * 1000;
            GanttHandler.fechaHoraria = new Date(fechaUTC.getTime() + offsetMs);
        }
        catch (error) {
            logError("getStripLine", error);
        }
    }

    _showToltip(show) {
        GanttHandler.showToltipGantt = show;
    }

    // #endregion

    // #region ------------------COLUMNS-------------------------------//

    _deleteFiltersColumns(repaint) {
        const ganttInstance = $("#GanttComponent").dxGantt("instance");
        if (ganttInstance && repaint && ganttInstance._savedSortFilterState.filter != null)
            ganttInstance.repaint();
    }

    // #endregion

    _paintRowParent() {
        try {
            var elements = document.querySelectorAll('.dx-gantt .dx-row.dx-data-row.dx-row-lines.dx-column-lines');
            elements.forEach(row => {
                if ($(row).data().options == undefined) return;
                const idObject = $(row).data().options.data.id;
                if (!idObject) return;

                // obtenmos la instancia de la tarea
                const task = this.#getTaskOfGantt(idObject);

                if (!task) return;

                const color = (task.BackgroundColorRow || '').trim();
                row.style.backgroundColor = color !== '' ? color : '#FFFFFF';
            });
        }
        catch (error) {
            //logError("paintRowParent", error);
        }
    }

    #getTaskOfGantt(id) {
        const ganttInstance = $("#GanttComponent").dxGantt("instance");
        return ganttInstance._tasksRaw.find(t => t.id === id);
    }

    #getTaskType(type) {
        switch (type) {
            case "PlanningGeneral":
            case "PlanningEstimation":
            case "PlanningTrailer":
            case "PlanningPriority":
            case "PlanningWarehouse":
            case "LaborEquipmentGeneral":
            case "LaborWorkerGeneral":
            case "YardGeneral":
                return 'LevelHigth';
                break;

            case "PlanningFlow":
            case "PlanningEstimationIn":
            case "PlanningEstimationOut":
                return 'LevelMed';
                break;

            case "None":
            case "PlanningOrder":
            case "PlanningEstimationInOrder":
            case "PlanningEstimationOutOrder":
            case "PlanningWarehouseOrder":
            case "LaborEquipmentOrder":
            case "LaborWorkerOrder":
                return 'LevelOrder';
                break;

            case "PlanningActivity":
            case "LaborWorkerActivity":
            case "LaborEquipmentActivity":
            case "YardOrder":
            default:
                return 'LevelAct';
                break;
        }
    }

    _ReadyLoadGantt() {
        return GanttHandler.isGanttLoaded;
    }

    _repaintGantt(showDashboard, isPlanning) {
        if (GanttHandler.isGanttLoaded) {
            const ganttInstance = $("#GanttComponent").dxGantt("instance");
            const ganttContainer = document.getElementById("GanttContainer");
            const dashboard = document.getElementById("DashboardContainer");

            if (ganttInstance && ganttContainer) {
                ganttContainer.classList.remove("mlx-container-gantt-60", "mlx-container-gantt-80", "mlx-container-gantt-40");

                if (showDashboard) {
                    if (dashboard) dashboard.style.display = "block";
                    if (isPlanning)
                        ganttContainer.classList.add("mlx-container-gantt-60");
                    else
                        ganttContainer.classList.add("mlx-container-gantt-40");
                } else {
                    if (dashboard) dashboard.style.display = "none";
                    ganttContainer.classList.add("mlx-container-gantt-80");
                }

                ganttInstance.option("height", ganttContainer.clientHeight);
            }
        }
    }

    _closePopup() {
        const pivotIntance = $('#pivotgrid').dxPivotGrid('instance');
        if (pivotIntance) {
            pivotIntance._fieldChooserPopup.hide();
        }
    }


    invoke(methodName, args) {
        const method = this[`_${methodName}`];
        if (typeof method === "function") {
            method.apply(this, args);
        } else {
            console.warn(`Método '${methodName}' no encontrado en GanttHandler.`);
        }
    }

    _disposeInstanceGantt() {
        const ganttInstance = $("#GanttComponent").dxGantt("instance");
        ganttInstance = null;
    }
}
