/**
 * Version 1.0.5
 **/

import * as functToolTip from './mlx-gantt-tooltip.js';
import * as functBarProgress from './mlx-gantt-progress.js';
import FilterManager from "./ObjectGantt/FilterManager.js";
import {
    EnumTaskStatus,
    EnumTaskType,
    EnumColumnFields,
    EnumPriorityOrder,
    PriorityTextMap
} from '../Enums/enums.js';


// #region -------------------------VARIABLES--------------------------------------------------//
let formatDateType = "DD/MM/YYYY"; // variable que indica el tipo de formato que se va a manejar
export let ganttInstance = null;          // Variable para la instancia global del Gantt
let columnsForGantt = null;
let fechaHoraria = null;

let selectTask = null;          // tarea seleccionada
let listSeletedTask = [];
let isGanttLoaded = false;      // Indicador para rastrear el estado del Gantt
let isEditableGantt = false;    // Variable que indica si el gantt es solo,lectura  
let showToltipGantt = true;     // Variable que indica si se quiere mostar los toltip informativos
let expandToAllLevels = true;   // Indica si hay que expandir las tareas a todos los niveles

let dotNetHelper;
let alertInterop = null;

// Variable para pintar el gantt principal (GanttComponent) - sirve para tener todas las tareas en un solo arreglo
let tasksForGantt = null;

// Variable para filtros
let lastSortField = null;
let lastSortDirection = null;

// Variables para guardar los datos en el diagrama de gantt
let dataForGantt = {};

// Para determinar qué tareas cargar según el modo
let tasksToLoad;

let filtrosActivos = {};
let dataHeaderFilter = [];
let datosNivel = [];
window.idsEliminadosGantt = new Set();
let filterPopupInstance = null;
let userFormats = null; 

const filterManager = new FilterManager();

window.loadedChildren ??= new Set();
let offLazyReset = false;

let freshDS = new DevExpress.data.DataSource();

// #endregion

// #region --------------------BOTONES PARA MENU CONTEXTUAL---------------------------------------//

const expand =
{
    name: 'Expanded',
    template: () => {
        const label = typeof L === 'function' ? L('Expand subtask') : 'Expand subtask';
        return `<i class="mlx-ico-expand-vrt"></i> <span class="menu-label">${label}</span>`;
    }
};

const collapsed =
{
    name: 'Collapsed',
    template: () => {
        const label = typeof L === 'function' ? L('Collapse subtask') : 'Collapse subtask';
        return `<i class="mlx-ico-contract-vrt"></i></i> <span class="menu-label">${label}</span>`;
    }
};

const disablelockTask = {
    name: 'DisableLock',
    disabled: true,
    className: "disabled-option",
    template: () => {
        const label = typeof L === 'function' ? L('Lock') : 'Lock';
        return `<i class="mlx-ico-lock-close-light"></i> <span class="menu-label">${label}</span>`;
    }
};

const lockTask = {
    name: 'Lock',
    template: () => {
        const label = typeof L === 'function' ? L('Lock') : 'Lock';
        return `<i class="mlx-ico-lock-close"></i> <span class="menu-label">${label}</span>`;
    }
};

const unlockTask = {
    name: 'Unlock',
    template: () => {
        const label = typeof L === 'function' ? L('Unlock') : 'Unlock';
        return `<i class="mlx-ico-lock-open"></i> <span class="menu-label">${label}</span>`;
    }
};

const cancelTaskActive =
{
    name: 'CancelTaskActive',
    template: () => {
        const label = typeof L === 'function' ? L('Cancel order') : 'Cancel order';
        return `<div class="menu-separator"></div> <i class="mlx-ico-trash"></i> <span class=" menu-label-red">${label}</span>`;
    }
};

const cancelTaskDisable =
{
    name: 'CancelTaskDisable',
    disabled: true,
    className: "disabled-option",
    template: () => {
        const label = typeof L === 'function' ? L('Cancel order') : 'Cancel order';
        return `<div class="menu-separator"></div> <i class="mlx-ico-trash"></i> <span class=" menu-label-red">${label}</span>`;
    }
};

const changePriority = {
    name: 'ChangePriority',
    template: () => {
        const label = typeof L === 'function' ? L('Change priority') : 'Change priority';
        return `<i class="mlx-ico-change-priority"></i> <span class=" menu-label">${label}</span>`;
    }
};

const disableChangePriority = {
    name: 'DisableChangePriority',
    disabled: true,
    className: "disabled-option",
    template: () => {
        const label = typeof L === 'function' ? L('Change priority') : 'Change priorityk';
        return `<i class="mlx-ico-change-priority"></i> <span class="menu-label">${label}</span>`;
    }
};

// #endregion

// #region -------------------CARGA DE DATOS-------------------------//

window.addEventListener("error", (e) => {
    if (e.message?.includes("internalId")) {
        console.warn("DevExtreme Gantt error controlado", e);
        e.preventDefault();
        return true;
    }
});

export function getDoNetHelper(netHelper) {
    if (netHelper != undefined)
        dotNetHelper = netHelper;
}

export function registerAlertInterop(dotNetRef) {
    alertInterop = dotNetRef;
}

export function converterJson(jsonDataTask) {
    try {
        let dataGantt = JSON.parse(jsonDataTask, (key, value) => {
            if (typeof value === "string" && value.startsWith("new Date")) {
                return eval(value);
            }

            return value;
        });

        return dataGantt;
    }
    catch (error) {
        logError("converterJson", error);
    }
}

function buildHasChildrenMap(tasks) {
    const containFilter = Object.keys(filtrosActivos).length > 0;
    const hasChildrenMap = new Map();
    for (const t of tasks.filter(x => !x.isExpand)) {
        if (t.parentId != null && (containFilter || t.IsChildTask)) {
            hasChildrenMap.set(t.parentId, true);
        }
    }

    window.hasChildrenMap = hasChildrenMap;
}

export function loadDataForGantt(jsonData, formats, loadChildTasks) {
    try {
        DevExpress.localization.locale('en-US');
        initializeGanttState();
        userFormats = formats;
        if (!loadDataEmpty(jsonData, formats)) return false;
        dataForGantt = converterJson(jsonData);
        window.lazyEnabled = !loadChildTasks;
        if (window.lazyEnabled) window.loadedChildren = new Set();

        if (!ganttInstance)
            drawModelGantt(dataForGantt.TaskGantt, formats);

        if (!loadDataEmpty(dataForGantt?.TaskGantt, formats)) return false;

        const ordered = [...dataForGantt.TaskGantt].sort((a, b) => a.index - b.index);
        window.allTasksData = ordered;

        tasksToLoad = loadChildTasks
            ? ordered
            : ordered.filter(t => !t.IsChildTask);

        let tasksToShow = tasksToLoad;

        if (Object.keys(filtrosActivos).length > 0) {
            initFilterManager();
            const filtradas = filterManager.obtenerDatosFiltrados();
            const ids = new Set(filtradas.map(f => f.id));
            tasksToShow = tasksToLoad.filter(t => ids.has(t.id));
        }

        freshDS = new DevExpress.data.DataSource({
            store: new DevExpress.data.ArrayStore({
                data: tasksToShow,
                key: "id"
            }),
            paginate: false
        });

        tasksForGantt = freshDS;
        if (ganttInstance) {
            ganttInstance.beginUpdate();
            ganttInstance.option("tasks", {
                dataSource: freshDS,
                keyExpr: "id",
                parentIdExpr: "parentId"
            });

            if (showToltipGantt) {
                ganttInstance.option("stripLines", [
                    { title: "Now", start: fechaHoraria, cssClass: 'current-time' }
                ]);
            } else {
                ganttInstance.option("stripLines", []);
            }
            ganttInstance._treeList?.refresh?.();
            freshDS.load();
            ganttInstance.endUpdate();
        } else {
            drawModelGantt(freshDS, formats);
        }

        window.childTasksLoaded = loadChildTasks;
        buildHasChildrenMap(ordered);

        listSeletedTask = ordered.filter(x => x.Multiselect);
        isGanttLoaded = true;

    } catch (error) {
        logError("loadDataForGantt", error);
        resetGantt(formats);
    } finally {
        //deleteHeaderFilters(false);

        initFilterManager();
        return isGanttLoaded;
    }
};

function loadDataEmpty(values, formats) {
    if (!values || !values.length) {
        tasksToLoad = [];
        isGanttLoaded = false;
        window.childTasksLoaded = [];
        resetGantt(formats);
        return false;
    }

    return true;
}

export function resetInstanceGantt() {
    try {
        const $el = $("#GanttComponent");
        const inst = $el.data("dxGantt") || ($el.dxGantt ? $el.dxGantt("instance") : null);

        ganttInstance?._ganttView?._taskAreaContainer?._scrollView?.off("scroll");

        if (inst && !inst._disposed) {
            inst.dispose();
        }

        $el.off();
        $el.empty();

    } catch (error) {
        logError("resetInstanceGantt", error);
    } finally {
        ganttInstance = null;
        isGanttLoaded = false;
        tasksForGantt = null;
        dataForGantt = {};
        lastSortField = null;
        lastSortDirection = null;
        selectTask = null;
        listSeletedTask = [];
        initializeGanttState();
        deleteHeaderFilters(false, false);
    }
}

export function initializeGanttState() {
    window.lazyEnabled = false;
    window.childTasksLoaded = false;
    window.loadedChildren = new Set();
    window.hasChildrenMap = new Map();
    filtrosActivos = {};
    dataHeaderFilter = [];
    filterPopupInstance = null;
    selectTask = null;
    listSeletedTask = [];
    expandToAllLevels = true;
    offLazyReset = false;
    window.allTasksData = [];
    dataForGantt = {};
}

export function drawModelGantt(dataTask, formats) {
    try {
        $("#GanttComponent").dxGantt({
            showRowLines: false,
            showColumnLines: false,
            repaintChangesOnly: true,
            //allowSelection: false,
            allowResizing: false,
            showResources: false,
            showDependencies: false,
            autoUpdateParentTasks: false,
            validateDependencies: false,
            onCustomCommand: onCustomCommandClick,

            tasks: {
                dataSource: dataTask,
                keyExpr: "id",
                parentIdExpr: "parentId",
            },

            dependencies: { dataSource: [] },
            resources: { dataSource: [] },
            resourceAssignments: { dataSource: [] },

            sorting: { mode: "none" },

            taskContentTemplate: function (e) {
                const taskData = getTaskOfGantt(e.taskData.id);
                return $(functBarProgress.getTaskProgress(taskData, e.taskSize.width, e.taskData.start, e.taskData.end));
            },

            stripLines: [{ title: L("Now"), start: fechaHoraria, cssClass: 'current-time' }],

            editing: { enabled: false },
            validation: { autoUpdateParentTasks: true },

            taskTooltipContentTemplate: getTaskTooltipContentTemplate,

            scaleType: 'hours',
            scaleTypeRange: { min: 'minutes', max: 'sixHours' },

            onScaleCellPrepared: function (e) {
                try {
                    const el = e.scaleElement?.jquery ? e.scaleElement[0] : e.scaleElement;
                    if (!el) return;
                    const d = e.startDate instanceof Date ? e.startDate : new Date(e.startDate);
                    if (isNaN(d)) return;
                    if (['hours', 'sixHours'].includes(e.scaleType)) {
                        el.textContent = formatTimeForScale(d, formats.hourFormat);
                        return;
                    }
                    if (['days', 'weeks', 'months'].includes(e.scaleType)) {
                        el.textContent = formatFullDate(d, formats.regionFormat);
                        return;
                    }
                } catch (err) {
                    console.error('onScaleCellPrepared error:', err);
                }
            },

            zoomLevel: 1,
            taskListWidth: 850,
            headerFilter: handleHeaderFilter(),
            export: {
                fileName: "Gantt",
                pdf: true
            },

            onContentReady: handleContentReady,
            onTaskClick: handleTaskClick,
            onContextMenuPreparing: handleContextMenuPreparing,
            onOptionChanged: handleOptionChangedGantt,
            //onTaskUpdated: function (e) {
            //    actualizarTemplate(e.component);
            //}

        });

        ganttInstance = $("#GanttComponent").dxGantt("instance");

    } catch (error) {
        logError("drawModelGantt", error);
        resetGantt(formats);
    }
}

function formatFullDate(date, region) {
    return date.toLocaleDateString(region, {
        weekday: 'long',
        year: 'numeric',
        month: 'long',
        day: '2-digit'
    });
}

export function formatTimeForScale(date, pattern) {
    const two = n => String(n).padStart(2, '0');
    const Hn = date.getHours();
    const H = two(Hn);
    const hh = two((Hn % 12) || 12);
    const mm = two(date.getMinutes());
    const tt = Hn < 12 ? 'AM' : 'PM';

    if (pattern && /tt/i.test(pattern)) {
        return `${hh}:${mm} ${tt}`;
    }
    if (pattern && pattern.includes('.')) {
        return `${H}.${mm}`;
    }
    return `${H}:${mm}`;
}

export function updateProgressBar() {
    try {
        const task = ganttInstance._tasks;
        task.forEach(t => {
            const data = getTaskOfGantt(t.id);
            if (data) {
                const taskType = getTaskType(data.TooltipType);
                if (taskType == EnumTaskType.LEVEL_ORDER || data.TooltipType == 'LaborWorkerGeneral' || data.TooltipType == 'YardOrder') {
                    ganttInstance.option('selectedRowKey', data.id);
                }
            }
        });
    }
    catch (error) {
        logError("updateProgressBar", error);
    }
}

export function resetGantt(formats) {
    $("#GanttComponent").dxGantt({
        tasks: {
            dataSource: [],
            keyExpr: "id",
            parentIdExpr: "parentId",
        },
        dependencies: {
            dataSource: []
        },
        resources: {
            dataSource: []
        },
        resourceAssignments: {
            dataSource: []
        },
        stripLines: [{
            title: "Now",
            start: fechaHoraria,
            cssClass: 'current-time',
        }],
        scaleType: 'hours',
        headerFilter: handleHeaderFilter(),
        onScaleCellPrepared: function (e) {
            try {
                const el = e.scaleElement?.jquery ? e.scaleElement[0] : e.scaleElement;
                if (!el) return;

                const d = e.startDate instanceof Date ? e.startDate : new Date(e.startDate);
                if (isNaN(d)) return;

                if (['hours', 'sixHours'].includes(e.scaleType)) {
                    el.textContent = formatTimeForScale(d, formats.hourFormat);
                    return;
                }
                if (['days', 'weeks', 'months'].includes(e.scaleType)) {
                    el.textContent = formatFullDate(d, formats.regionFormat);
                    return;
                }

            } catch (err) {
                console.error('onScaleCellPrepared error:', err);
            }
        },

        sorting: {
            mode: "none",
        },
        taskContentTemplate: function (e) {
            const taskData = getTaskOfGantt(e.taskData.id);
            return $(functBarProgress.getTaskProgress(taskData, e.taskSize.width, e.taskData.start, e.taskData.end));
        },
        onCustomCommand: onCustomCommandClick,

        onContentReady: handleContentReady,

        onTaskClick: handleTaskClick,

        onContextMenuPreparing: handleContextMenuPreparing,

        onOptionChanged: handleOptionChangedGantt,
    });
}

export function initializeGanttTasks(dataSource) {
    if (Array.isArray(dataSource) && dataSource.length > 0) {
        window.dataTaskInformation = dataSource;
        const firstParent = dataSource.filter(x => x.parentId == null)[0];
        if (firstParent) {
            const typeGantt = getTaskType(firstParent.TooltipType);

            if (typeGantt == EnumTaskType.LEVEL_HIGTH) {
                functToolTip.precalculateDatesFromChildren(firstParent.id);
            }
        }
    }
}

// #endregion

// #region ------------------ZOOM--------------------------------------//
export function zoom(isZoomIn) {
    if (isGanttLoaded) {
        if (isZoomIn)
            ganttInstance.zoomIn();
        else {
            ganttInstance.zoomOut();
        }
        BlockButton();
    }
}

// #endregion

// #region -------------------MENU CONTEXTUAL-------------------------//
export function getContextMenuTask(data) {
    var items = [];
    if (data != null && showToltipGantt) {
        var task = getTaskOfGantt(data.id);
        var taskChildrens = getTaskChildren(task.id);
        const taskType = getTaskType(task.TooltipType);
        const enumTaskStatus = L(EnumTaskStatus.WAITING);
        switch (taskType) {
            case EnumTaskType.LEVEL_HIGTH:
            case EnumTaskType.LEVEL_MED:
                if (!task.isExpand)
                    items.push(expand);
                else
                    items.push(collapsed);
                break;

            case EnumTaskType.LEVEL_ORDER:
                if (taskChildrens.length > 0) {
                    if (!task.isExpand)
                        items.push(expand);
                    else
                        items.push(collapsed);
                }

                if (task.TooltipType == 'PlanningOrder') {
                    if (task.start > fechaHoraria)
                        items.push(changePriority);
                    else
                        items.push(disableChangePriority);

                    if (task.isBlock)
                        items.push(unlockTask);
                    else if (task.Status.toLowerCase() != enumTaskStatus.toLowerCase()) {
                        items.push(disablelockTask);
                        items.push(cancelTaskDisable);
                    }
                    else {
                        items.push(lockTask);
                        items.push(cancelTaskActive)
                    }

                }

                break;
        }
    }

    return items;
}

// #endregion

// #region ------------------LINEA NOW-------------------------------//
export function getStripLine(timeZone) {
    try {
        if (!showToltipGantt) return ''
        const fechaUTC = new Date(new Date().toISOString().replace("Z", ""));
        const offsetMs = timeZone * 60 * 60 * 1000;
        fechaHoraria = new Date(fechaUTC.getTime() + offsetMs);
    }
    catch (error) {
        logError("getStripLine", error);
    }
}

export function UpdateToDay() {
    try {
        if (isGanttLoaded) {
            const currentTime = document.querySelector('.current-time');
            if (!currentTime) return;
            const stripLineLeft = currentTime.offsetLeft;
            if (ganttInstance._ganttView != undefined) {
                const containerGantt = ganttInstance._ganttView._taskAreaContainer;
                containerGantt.scrollLeft = stripLineLeft - 100;
            }
        }
    }
    catch (error) {
        logError("UpdateToDay", error);
    }
}


// scheduler
export function GoToNow() {
    try {
        const currentTimeIndicator = document.querySelector('.dxbl-sc-time-marker-line');
        const scrollContainer = document.querySelector('.dxbl-scroll-viewer-content');
        if (currentTimeIndicator && scrollContainer) {
            const offset = currentTimeIndicator.offsetLeft;
            scrollContainer.scrollLeft = offset - 250;
        }

    } catch (ex) {
        console.error("GoToNow", ex);
    }
}


// #endregion

// #region -----------------BARRA DE HERRAMIENTAS-------------------//
export function onCustomCommandClick(e) {
    try {
        var id = e.component._ganttView._ganttViewCore.currentSelectedTaskID;
        if (id == "" && selectTask != null)
            id = selectTask.id;
        selectTask = getTaskOfGantt(id);
        const values = listSeletedTask.map(t => t.id);
        switch (e.name) {
            case 'AddDependency':
                console.log('Agregar dependencia correctamente');
                break;
            case 'Completed':
                console.log('Tarea completada correctamente');
                break;
            case 'ExpandedCollapsed':
                console.log('Expandido/colapsado correctamente');
                break;
            case 'Expanded':
                ExpandCollapsedTask(true, selectTask);
                break;
            case 'Collapsed':
                ExpandCollapsedTask(false, selectTask);
                break;
            case 'Lock':
                if (dotNetHelper != undefined)
                    dotNetHelper.invokeMethodAsync('ToolBarActionsPopup', values, selectTask, 'LockTask');
                break;

            case 'Unlock':
                if (dotNetHelper != undefined)
                    dotNetHelper.invokeMethodAsync('ToolBarActionsPopup', values, selectTask, 'UnlockTask');
                break;

            case 'CancelTaskActive':
                expandToAllLevels = false;
                if (dotNetHelper != undefined)
                    dotNetHelper.invokeMethodAsync('ToolBarActionsPopup', values, selectTask, 'CancelTask');
                break;
            case 'ChangePriority':
                expandToAllLevels = false;
                if (dotNetHelper != undefined)
                    dotNetHelper.invokeMethodAsync('ToolBarActionsPopup', values, selectTask, 'ChangePriority');
                break;
        }
    }
    catch (error) {
        logError("onCustomCommandClick", error);
    }
}

export async function ExpandCollapsedTask(isExpand, task) {
    if (!task || !ganttInstance) return;

    if (isExpand) {
        const alreadyLoaded =
            window.loadedChildren?.has(task.id) ||
            task.childrenLoaded === true ||
            (task.items && task.items.length > 0);

        if (!alreadyLoaded && window.lazyEnabled && typeof loadChildTasks === 'function') {
            await expandWithLoading(task.id, task, dotNetHelper);
            ganttInstance.beginUpdate();
            ganttInstance.expandTask(task.id);
            ganttInstance.endUpdate();
            task.isExpand = isExpand;
            return;
        }
    }

    ganttInstance.beginUpdate();
    if (isExpand)
        ganttInstance.expandTask(task.id);
    else
        ganttInstance.collapseTask(task.id);
    ganttInstance.endUpdate();
    task.isExpand = isExpand;
}

export function getTaskTooltipContentTemplate(task) {
    // si es modo lectura no retorna nada para que no se visualice el tooltip
    if (!showToltipGantt) return '';
    initializeGanttTasks(tasksForGantt.store()._array);
    const tooltip = functToolTip.getTaskToolTipDataPlanning(task);
    $(document.createElement('div')).appendTo(tooltip);
    return tooltip;
}

// calcula el progreso total en base al progreso de los hijos
export function getProgressParent(id) {
    try {
        const childs = getTaskChildren(id);
        let totalProgress = 0;
        childs.forEach((child) => {
            totalProgress += child.progress;
        });
        const percentage = childs.length > 0 ? (totalProgress / childs.length) : 0;
        const result = Math.ceil(Math.min(Math.max(percentage, 0), 100));
        return isNaN(result) ? 0 : result;
    }
    catch (error) {
        logError("getProgressParent", error);
    }
}

export function getResourceForActivity(taskId) {
    try {
        return dataForGantt.ResourcesAssignmentsGantt.filter(r => r.taskId === (taskId));
    }
    catch (error) {
        logError("getResourceForActivity", error);
    }
}

export function getTaskChildren(id) {
    try {
        return dataForGantt.TaskGantt.filter(t => t.parentId === (id));
    }
    catch (error) {
        logError("getTaskChildren", error);
    }
}

export function getTaskOfGantt(id) {
    try {
        if (!dataForGantt || !Array.isArray(dataForGantt.TaskGantt)) {
            return null;
        }

        return dataForGantt.TaskGantt.find(t => t.id === id) ?? null;
    }
    catch (error) {
        logError("getTaskOfGantt", error);
        return null;
    }
}

export function taskContentTemplateForGantt(e) {
    const taskData = getTaskOfGantt(e.taskData.id);
    return $(functBarProgress.getTaskProgress(taskData, e.taskSize.width, e.taskData.start, e.taskData.end));
}

export function repaintGantt(showDashboard, isPlanning) {
    if (isGanttLoaded) {
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

// #endregion

// #region ********************VARIABLES***********************************/
export function getIntance() {
    return ganttInstance;
}

export function ReadyFormatDate() {
    return formatDateType;
}

export function ReadyLoadGantt() {
    return isGanttLoaded;
}

export function ReadyTaskGantt() {
    return dataForGantt.TaskGantt;
}

export function isEditGannt(isEdit) {
    isEditableGantt = isEdit;
}

export function showToltip(show) {
    showToltipGantt = show;
}

export function resetData() {
    dataForGantt = {};
    tasksForGantt = null;
}

export function isReadyGantt(isReady) {
    // Se cambia el estado del gantt porque se carga el pivot
    isGanttLoaded = isReady;
}

export function clearListSelectedTask(clean) {
    if (clean) {
        GetMultiselectFalse(false);
        listSeletedTask = [];
    }
}

export function addOrderFilters(dataField) {
    lastSortField = dataField;
    lastSortDirection = 'asc';
}

// #endregion

// #region ********************BARRA DE HERRAMIENTAS***********************************/

export function printGantt() {
    if (isGanttLoaded)
        window.print();
}

export function exportGanttToCsv() {
    if (isGanttLoaded) {
        var tasks = ganttInstance.option("tasks").dataSource;
        var columns = ganttInstance.option("columns");
        generateCSV(tasks, columns);
    }
}

//export function exportGanttToPdf(idGantt) {
//    const ganttInstance = $(`#${idGantt}`).dxGantt('instance');
//    const { jsPDF } = window.jspdf;
//    DevExpress.pdfExporter.exportGantt({
//        component: ganttInstance,
//        createDocumentMethod: (args) => new jsPDF({
//            orientation: args.orientation || 'portrait',
//            format: 'a0',
//            exportMode: 'all',
//            dateRange: 'all'
//        }),
//    }).then((doc) => {
//        doc.save('gantt.pdf');
//    });
//}

export function exportGanttToPdf(idGantt) {
    if (isGanttLoaded) {
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

export function applyInlineStyles(element) {
    const computedStyles = window.getComputedStyle(element);

    // Aplicar cada propiedad computada como estilo inline
    for (let property of computedStyles) {
        element.style[property] = computedStyles.getPropertyValue(property);
    }

    // Recorrer los hijos para aplicar estilos en línea de manera recursiva
    for (let child of element.children) {
        applyInlineStyles(child);
    }
}

export function generateCSV(tasks, columns) {
    try {
        let csvContent = columns.map(col => col.caption).join(",") + "\n";

        tasks._items.forEach(task => {
            let row = columns.map(col => {
                const dataField = col.dataField;
                if (task.hasOwnProperty(dataField)) {
                    if (Array.isArray(task[dataField])) {
                        return task[dataField].join("; ");
                    }
                    return task[dataField];
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
        logError("generateCSV", error);
    }
}

// #endregion

// #region *****************************BUTTONS********************************** */
export function BlockButton() {
    try {
        const scale = ganttInstance.option('scaleType');
        const currentZoom = ganttInstance._ganttView._ganttViewCore.currentZoom;
        const btnZoomOut = $('#btnZoomOut');
        const btnZoomIn = $('#btnZoomIn');
        switch (scale) {
            case 'minutes':
                if (currentZoom == 3) {
                    disableButtonZoom(btnZoomIn);
                }
                else {
                    enableButtonZoom(btnZoomIn);
                }
                break;
            case 'sixHours':
                if (currentZoom == 1) {
                    disableButtonZoom(btnZoomOut);
                }
                else if (currentZoom > 1) {
                    enableButtonZoom(btnZoomOut);
                }
                break;

            default:
                enableButtonsZoom();
                break;
        }
    }
    catch (error) {
        logError("BlockButton", error);
    }
}

export function disableButtonZoom(button) {
    button.removeClass('top-bar-button');
    button.addClass('top-bar-button-block');
    button.prop('disabled', true);
}

export function enableButtonZoom(button) {
    button.removeClass('top-bar-button-block');
    button.addClass('top-bar-button');
    button.prop('disabled', false);
}

export function enableButtonsZoom() {
    const btnZoomOut = $('#btnZoomOut');
    const btnZoomIn = $('#btnZoomIn');

    btnZoomOut.addClass('top-bar-button');
    btnZoomOut.removeClass('top-bar-button-block');
    btnZoomIn.addClass('top-bar-button');
    btnZoomIn.removeClass('top-bar-button-block');
    btnZoomOut.prop('disabled', false);
    btnZoomIn.prop('disabled', false);
}

export function disableButtonsZoom(btnZoomOut, btnZoomIn) {
    btnZoomOut.addClass('top-bar-button');
    btnZoomOut.removeClass('top-bar-button-block');
    btnZoomIn.addClass('top-bar-button');
    btnZoomIn.removeClass('top-bar-button-block');
}

export function EnableButtonsForDrawerInfoVisibility() {
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

// #region *************************Events****************************************** */
export function AddEvents() {
    document.addEventListener('click', function (event) {
        const target = event.target;
        if (target.closest('.dx-treelist-collapsed')) {
            if (selectedTask)
                selectTask.isExpand = true;
            EventCollapsed();
        }
        else if (target.closest('.dx-treelist-expanded')) {
            if (selectedTask)
                selectTask.isExpand = false;
            EventExpand();
        } else if (target.closest('td.dxbl-calendar-day.dxbl-calendar-selected-range')) {
            AddCLickRangeTemp();
        }
        else if (target.closest("div.dxbl-btn-group.dxbl-btn-group-right")) {
            AddItemsRange();
        }
    });
    AddEventScrollForPintTask();
}

export function AddCLickRangeTemp() {
    try {
        const elementsBoxShadow = Array.from(document.querySelectorAll('td.dxbl-calendar-day'))
            .filter(elemento => window.getComputedStyle(elemento).getPropertyValue('box-shadow') !== 'none');
        elementsBoxShadow.forEach(elemento => {
            const boxShadow = window.getComputedStyle(elemento).getPropertyValue('box-shadow');

            if (boxShadow !== 'none') {
                elemento.style.boxShadow = 'none';
            }
        });
        AddBorderItemsInRows();
    }
    catch (error) {
        logError("AddCLickRangeTemp", error);
    }
}

export function AddItemsRange() {
    AddBorderItemsSelect();
    AddBorderItemsInRows();
}

export function AddEventScrollForPintTask() {
    try {
        if (ganttInstance != null) {
            ganttInstance._treeList.on("onTaskUpdated", handleTaskUpdated);
            if (ganttInstance._ganttView != null) {
                const taskAreaContainer = ganttInstance._ganttView._taskAreaContainer;
                taskAreaContainer._scrollView.on("scroll", function (e) {
                    BlockButton();
                });

            }
        }
    }
    catch (error) {
        logError("AddEventScrollForPintTask", error);
    }
}

export function paintRowParent() {
    try {
        var elements = document.querySelectorAll('.dx-gantt .dx-row.dx-data-row');
        if (elements.length >= 1) {
            elements.forEach(row => {
                if ($(row).data().options == undefined) return;
                const idObject = $(row).data().options.data.id;
                if (!idObject) return;

                // obtenmos la instancia de la tarea
                const task = getTaskOfGantt(idObject);

                if (!task) return;

                const color = (task.BackgroundColorRow || '').trim();
                row.style.backgroundColor = color !== '' ? color : '#FFFFFF';
            });
        }
    }
    catch (error) {
        logError("paintRowParent", error);
    }
}

export function AddBorderItemsSelect() {
    const rangeDate = document.querySelectorAll('.dxbl-calendar-selected-range');

    if ((document.querySelectorAll('td.dxbl-calendar-selected-item').length - 2) > 1) {
        rangeDate.forEach(currentItem => {
            const nextItem = currentItem.nextElementSibling;
            const previousItem = currentItem.previousElementSibling;
            if (nextItem && nextItem.classList.contains('dxbl-calendar-selected-item')) {
                AddBorderRigthItemSelec(nextItem);
            }
            else if (previousItem && previousItem.classList.contains('dxbl-calendar-selected-item')) {
                AddBorderLeftItemSelect(previousItem);
            }
        });

    } else if ((document.querySelectorAll('td.dxbl-calendar-selected-item').length - 2) == 1) {
        const nextItem = rangeDate[0].nextElementSibling;
        AddBorderOneItem(nextItem);
    }
}

// poner bordes redonde al termino e inicio de cda fila
export function AddBorderItemsInRows() {
    const rowSelect = document.querySelectorAll(".dxbl-calendar-week-row");
    rowSelect.forEach(row => {
        const firstItem = row.querySelector('td:nth-child(2)');
        const lastItem = row.querySelector('td:last-child');
        if (firstItem && firstItem.classList.contains('dxbl-calendar-selected-item') && !firstItem.classList.contains('dxbl-calendar-selected-range')) {
            AddBorderRigth(firstItem);
        }
        if (lastItem && lastItem.classList.contains('dxbl-calendar-selected-item') && !lastItem.classList.contains('dxbl-calendar-selected-range')) {
            AddBorderLeft(lastItem);
        }
        ChangeStyleToday(row);
    });
}

export function ChangeStyleToday(row) {
    let today = row.querySelector('dxbl-calendar .dxbl-calendar-content .dxbl-calendar-day.dxbl-calendar-today');
    if (today && today.classList.contains("dxbl-calendar-selected-item")) {
        today.style.border = "none";
        today.style.borderRadius = "0px";
    }
    else {
        today = row.querySelector('td.dxbl-calendar-today');
        if (today)
            today.style.borderRadius = "999px";
    }
}

export function AddBorderRigthItemSelec(element) {
    element.style.position = 'relative';
    element.style.zIndex = '-1';
    element.style.boxShadow = '-10px 0px 0px rgba(91, 159, 218, 0.24)';
}

export function AddBorderLeftItemSelect(element) {
    element.style.position = 'relative';
    element.style.zIndex = '-1';
    element.style.boxShadow = '10px 0px 0px rgba(91, 159, 218, 0.24)';
}

export function AddBorderOneItem(element) {
    element.style.position = 'relative';
    element.style.zIndex = '-1';
    element.style.boxShadow = '10px 0px 0px rgba(91, 159, 218, 0.24), -10px 0px 0px rgba(91, 159, 218, 0.24)';
}

export function AddBorderLeft(element) {
    element.style.borderRadius = "0px 999px 999px 0px";
}

export function AddBorderRigth(element) {
    element.style.borderRadius = "999px 0px 0px 999px";
}

function handleContentReady(e) {
    ganttInstance = e.component;
    ganttInstance.option('showRowLines', true);
    //Desactiva el TaskDetails al dar doble clic sobre la tarea
    e.component._showDialog({});
    e.component._dialogInstance.infoMap.TaskEdit = undefined;

    EnableButtonsForDrawerInfoVisibility();
    AddEvents();
    const dataSource = e.component.option("tasks.dataSource");
    initializeGanttTasks(dataSource);
    isGanttLoaded = true;
}

function handleTaskClick(e) {
    selectTask = getTaskOfGantt(e.key);
}

function handleContextMenuPreparing(e) {
    e.items = getContextMenuTask(e.data);
}

function handleOptionChangedGantt(e) {
    if (e.name === 'tasks' && e.value?.dataSource && e.value?.dataSource.length > 0) {
        const dataSource = e.value.dataSource;
        initializeGanttTasks(dataSource.store()._array);
    }
}

function handleTaskUpdated(e) {
    ganttInstance.option('selectedRowKey', e.key);
}

function handleHeaderFilter() {
    return {
        visible: true,
    };
}

// #endregion

// #region *************************COLLAPSED****************************************** */
export function collapsedExpandedClick(isCollapsed) {
    if (isGanttLoaded) {
        if (isCollapsed) {
            const taskExpanded = ganttInstance._treeList.option("expandedRowKeys");
            const taskGenerales = dataForGantt.TaskGantt.filter(t => t.parentId === null && taskExpanded.includes(t.id));
            ganttInstance.beginUpdate();
            taskGenerales.forEach(x => ganttInstance.collapseTask(x.id));
            ganttInstance.endUpdate();
            CollapsedAllLevels();
        }
        else {
            ganttInstance.expandAll();
        }

        changeBtnExpandForEachTask(isCollapsed);
        ChangeBtnExpandToCollapsed(isCollapsed);
    }
}

export function CollapsedAllLevels() {
    setTimeout(() => {
        ganttInstance.beginUpdate();
        ganttInstance.collapseAll();
        ganttInstance.endUpdate();
    }, 100);
}

export function changeBtnExpandForEachTask(isExpand) {
    try {
        dataForGantt.TaskGantt.forEach(task => {
            const taskType = getTaskType(task.TooltipType);
            if (taskType != EnumTaskType.LEVEL_ACT) {
                task.isExpand = !isExpand;
            }
        });
    }
    catch (error) {
        logError("changeBtnExpandForEachTask", error);
    }
}

export function EventCollapsed() {
    const existElementExpand = document.querySelectorAll('.dx-treelist-collapsed').length <= 0;
    if (existElementExpand) {
        ChangeBtnExpandToCollapsed(false);
    }
}

export function EventExpand() {
    const existElementCollapsed = document.querySelectorAll('.dx-treelist-expanded').length <= 0;
    if (existElementCollapsed) {
        ChangeBtnExpandToCollapsed(true);
    }
}

export function ChangeBtnExpandToCollapsed(isCollapsed) {
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

export function expandGanttToLevel() {
    try {
        setTimeout(() => {
            if (ganttInstance) {
                ganttInstance.beginUpdate();
                if (isGanttLoaded && ganttInstance._tasksRaw.length > 0) {
                    expanAllLevels();
                    const tasksHisgth = ganttInstance._tasksRaw;
                    tasksHisgth.forEach(task => {
                        if (!task.parentId) {
                            var t = getTaskOfGantt(task.id);
                            if (t != undefined)
                                t.isExpand = true;
                        }
                    });
                    ganttInstance.endUpdate();

                    const isViewYard = dataForGantt.TaskGantt.some(
                        task => task.TooltipType === "YardGeneral"
                    );

                    ChangeBtnExpandToCollapsed(!isViewYard);
                }
            }
        }, 100);
    }
    catch (error) {
        logError("expandGanttToLevel", error);
    }
}


export function expanAllLevels() {
    try {
        if (selectTask && !expandToAllLevels)
            findTaskForSelection();
        else
            ganttInstance.expandAllToLevel(1);


        expandToAllLevels = true;
    }
    catch (error) {
        logError("expanAllLevels", error);
    }
}

function findTaskForSelection() {
    let taskSelect = dataForGantt.TaskGantt.find(x => selectTask.InputOrderId != undefined && x.InputOrderId == selectTask.InputOrderId);
    if (!taskSelect)
        taskSelect = dataForGantt.TaskGantt.find(a => a.Multiselect);

    if (taskSelect)
        ganttInstance.expandToTask(taskSelect.id);
    else
        ganttInstance.expandAllToLevel(1);
}

// #endregion

// #region *************************COLUMNS****************************************** */
export function UpdateColumns(dataColumns) {
    try {
        if (isGanttLoaded) {
            columnsForGantt = JSON.parse(dataColumns);

            const columnConfigMap = {
                date: (column) => setDateColumn(column, true),
                datetime: (column) => {
                    column.format = "hh:mm a";
                    column.cellTemplate = withTask((container, options, task) => {
                        if (task && getTaskType(task.TooltipType) === EnumTaskType.LEVEL_ORDER) {
                            covertToAMPM(container, options);
                        }
                    });
                },
                checkbox: (column) => {
                    column.dataType = "booleano";

                    column.calculateCellValue = (data) => !!data.Multiselect;

                    column.cellTemplate = withTask((container, options, task) => {
                        container.empty?.() || (container.innerHTML = "");

                        if (task && AddMultiselectInput(task.TooltipType)) {
                            selectedTask(container, options);
                        }
                    });
                },
                booleano: (column) => {
                    column.cellTemplate = addImageColumns(column.dataField);
                },
                commettext: (column) => {
                    column.cellTemplate = withTask(setTaskDateColumn(column.dataField));
                },
            };

            if (ganttInstance) {

                ganttInstance.beginUpdate();
                columnsForGantt.forEach(column => {
                    delete column.width; //para tener un tamaño minimo en columnas y no se encimen iconos con txt
                    column.minWidth = column.minWidth ?? 60;
                    const type = column.dataType?.toLowerCase();
                    if (columnConfigMap[type]) {
                        columnConfigMap[type](column);
                    }

                if (column.dataField === EnumColumnFields.PRIORITY) {
                    column.cellTemplate = withTask((container, options, task) => {
                        if (!task) {
                            container[0].innerText = "";
                            return;
                        }
                        container[0].innerText = task.Priority ?? "";
                    });
                }

                if (column.dataField === EnumColumnFields.END_BLOCK_DATE) {
                    column.cellTemplate = withTask((container, options, task) => {
                        container[0].innerText = task.isBlock
                            ? (task.EndBlockDate ?? "")
                            : "";
                    });
                }

                //genérico: texto + tooltip
                if (!column.cellTemplate) {
                    column.cellTemplate = defaultTextTemplate;
                }
            });

                const prevTemplate = columnsForGantt[0].cellTemplate;
                columnsForGantt[0].cellTemplate = withTask((container, options, task) => {
                    treeCellWithLazyBtn(container, options, task);
                    if (typeof prevTemplate === 'function') {
                        prevTemplate(container, options);
                    }
                });

                ganttInstance.option("columns", columnsForGantt);
                ganttInstance.endUpdate();
            }
        }

        const ganttContainer = document.querySelector('.dx-gantt');
        if (ganttContainer)
            ganttContainer.removeEventListener("click", handleClickColumn, true);

    } catch (error) {
        logError("UpdateColumns", error);
    }
}

function withTask(templateFn) {
    return function (container, options) {
        const task = getTaskOfGantt(options.data.id);
        if (task !== undefined) {
            templateFn(container, options, task);
            filterManager.changeFilterIconColor();
            paintRowParent();
        }
    };
}

function setDateColumn(column, needsAMPM) {
    column.format = getColumnDateFormat();

    if (needsAMPM) {
        column.cellTemplate = withTask((container, options, task) => {
            if (!task || !task.TooltipType) return;
            if (getTaskType(task.TooltipType) !== EnumTaskType.LEVEL_HIGTH) {
                covertToAMPM(container, options);
            }
        });
    }
}

function defaultTextTemplate(container, options) {
    const text = options.text;
    if (typeof text === 'string' && text.trim().length > 0) {
        container[0].innerText = text;

        requestAnimationFrame(() => {
            const cell = container[0];

            function updateTooltip() {
                cell.title = (cell.offsetWidth < cell.scrollWidth) ? text : "";
            }
            updateTooltip();
            const observer = new ResizeObserver(updateTooltip);
            observer.observe(cell);
        });
    }
}

function addImageColumns(dataField) {
    return (container, options) => {
        switch (dataField) {
            case 'isBlock':
                return getImageBlock(container, options);

            case 'HasAlerts':
                return getImageAlerts(container, options);
        }
    };
}

function setTaskDateColumn(dataField) {
    return (container, task) => {
        switch (dataField) {
            case 'CreationDate':
            case 'ReleaseDate':
                if (getTaskType(task.data.TooltipType) === EnumTaskType.LEVEL_ORDER) {
                    container[0].innerHTML = task.data[dataField];
                }
                break;

            case 'progress':
                container[0].innerHTML = getProgressFoColumn(task.data.id, task.column.dataField);
                break;

        }
    };
}

export function ColumnHeaderClicks() {
    const ganttContainer = document.querySelector('.dx-gantt');

    if (ganttContainer)
        ganttContainer.addEventListener("click", handleClickColumn, true);

}

export function handleClickColumn(e) {

    const ganttContainer = document.querySelector('.dx-gantt');
    const header = e.target.closest('[role="columnheader"]');
    if (!header || !ganttContainer.contains(header)) return;

    if (e.target.closest('.dx-header-filter')) {
        filterManager.openHeaderFilter(e, header, columnsForGantt);
        return;
    }

    const headers = Array.from(ganttContainer.querySelectorAll('[role="columnheader"]'));
    const index = headers.indexOf(header);
    const columnInfo = columnsForGantt[index];

    if (columnInfo.dataField) {
        toggleSort(columnInfo.dataField);
        setTimeout(() => {
            updateIcons(index);
            if (isFieldOnlyChildTask(lastSortField))
                collapsedExpandedClick(false);
        }, 100);
    }
}

export function initFilterManager() {
    filterManager.activeFilters = filtrosActivos;
    filterManager.dataTaskForGantt = dataForGantt?.TaskGantt;
    filterManager.ganttInstance = $("#GanttComponent").dxGantt("instance");
    filterManager.userFormats = userFormats;
}

export function toggleSort(dataField) {
    if (ganttInstance && tasksForGantt) {
        dataHeaderFilter = ganttInstance.option("tasks.dataSource")._items;
        if (lastSortField === null) {
            addOrderFilters(dataField);
            tasksForGantt.store()._array = sortedTask(dataField);

        } else if (lastSortField === dataField) {
            lastSortDirection = lastSortDirection === 'asc' ? 'desc' : 'asc';
            tasksForGantt.store()._array = sortedTask(dataField);

        } else {
            lastSortField = null;
            lastSortDirection = null;
            tasksForGantt.store()._array = (dataHeaderFilter && dataHeaderFilter.length > 0)
                ? dataHeaderFilter
                : tasksToLoad;
        }
        ganttInstance.option('tasks.dataSource', tasksForGantt);
        ganttInstance.option("tasks").dataSource.reload();
    }
}

function sortedTask(fieldName) {
    const baseArray = ganttInstance.option("tasks.dataSource").store()._array;

    return [...baseArray].sort((a, b) => {
        let result = 0;
        if (fieldName === EnumColumnFields.PRIORITY) {
            return compareByPriority(
                a[fieldName],
                b[fieldName],
                lastSortDirection
            );
        }
        if (typeof a[fieldName] === 'string') {
            result = a[fieldName].localeCompare(b[fieldName]);
        } else if (typeof a[fieldName] === 'number' || a[fieldName] instanceof Date) {
            result = a[fieldName] > b[fieldName] ? 1 : (a[fieldName] < b[fieldName] ? -1 : 0);
        }
        return lastSortDirection === 'asc' ? result : -result;
    });
}

function compareByPriority(firstValue, secondValue, direction) {
    const firstPriority = getPriorityValue(firstValue);
    const secondPriority = getPriorityValue(secondValue);

    if (firstPriority === 999 && secondPriority === 999) {
        return 0;
    }

    if (firstPriority === 999) return 1;
    if (secondPriority === 999) return -1;

    return direction === 'asc'
        ? firstPriority - secondPriority
        : secondPriority - firstPriority;
}


function getPriorityValue(text) {
    if (!text) return 999;
    const key = PriorityTextMap[text.toUpperCase()] ?? text.toUpperCase();
    const index = EnumPriorityOrder.indexOf(key);
    return index === -1 ? 999 : index;
}

export function updateIcons(index) {
    const ganttContainer = document.querySelector('.dx-gantt');
    const headerColumn = Array.from(ganttContainer.querySelectorAll('[role="columnheader"]'))[index];
    if (!headerColumn) return;

    const iconsExis = headerColumn.querySelector(".dx-column-indicators");
    if (!iconsExis) return;

    iconsExis.querySelectorAll('.sort-arrow').forEach(el => el.remove());
    const icon = document.createElement('span');
    icon.classList.add('sort-arrow');
    const sortClass = lastSortDirection === 'asc'
        ? 'mlx-ico-arrow-up-narrow'
        : lastSortDirection === 'desc'
            ? 'mlx-ico-arrow-down-narrow'
            : null;

    sortClass && icon.classList.add(sortClass);
    iconsExis.insertBefore(icon, iconsExis.firstChild);
}

function isFieldOnlyChildTask(fieldName) {
    const childTaskList = tasksForGantt.store()._array.filter(x => x.IsChildTask);
    const childsId = new Set(childTaskList.map(t => t.id));

    if (fieldName) {
        return tasksForGantt.store()._array.every(task => {
            const hasValue = task[fieldName] != null && task[fieldName] !== '';
            if (!hasValue) return true;
            return childsId.has(task.id);
        });
    }
    else { return false; }
}

export function drawerInfoVisibility(showInfoDrawer) {
    ganttInstance.beginUpdate();
    const container = document.getElementById('ganttContainer');
    container.className = showInfoDrawer ? 'mlx-container-gantt-60' : 'mlx-container-gantt-80';
    ganttInstance.endUpdate();
}

export function deleteHeaderFilters(viewFilter, updateGantt) {
    filterManager.sequentialDataFiltering = dataHeaderFilter = [];
    filterManager._InitialStatus();
    if (Object.keys(filtrosActivos).length > 0 || !viewFilter) {
        filterManager.activeFilters = filtrosActivos = {};
    }

    if (updateGantt)
        filterManager.updateGantt();

}

// #endregion

// #region *************************LOCK/UNLOCK****************************************** */

export function LockUnlockTask(task, isLock, dateTimeBlock) {
    try {
        applyBlockToLocalData(task.id, isLock, dateTimeBlock);
        updateBlockCells(task.id, isLock, dateTimeBlock);
        ganttInstance.option("selectedRowKey", task.id);
        ganttInstance.option("selectedRowKey", null);

    } catch (error) {
        logError("LockUnlockTask", error);
    }
}

// agrega la imagen para tareas bloqueadas sobre las columnas
export function getImageBlock(container, options) {
    var task = getTaskOfGantt(options.data.id);
    if (task != undefined && task.isBlock) {
        const icon = `<div class="mlx-ico-lock-close"> </div>`;
        container[0].innerHTML = icon;
    }
}

export function showDataSubTask(container, options) {
    try {
        var task = getTaskOfGantt(options.data.id);
        const taskType = getTaskType(task.TooltipType);
        if (taskType != EnumTaskType.LEVEL_HIGTH)
            container[0].innerHTML = "";
    }
    catch (error) {
        logError("showDataSubTask", error);
    }
}

export function getImageAlerts(container, options) {
    const task = getTaskOfGantt(options.data.id);
    if (task) {
        if (task.levelTask === 3 && task.Alerts?.length > 0) {
            const unreadCount = task.Alerts.filter(alert => !alert.IsRead).length;
            const displayStyle = unreadCount > 0 ? 'inline-block' : 'none';
            const severityClass = getSeverityClass(task.Alerts);
            const icon = `
            <div class="alert-icon-container">
                <button id="task-alert-${task.id}" class="top-bar-button top-bar-button-show alerts" title="Information"
                        onclick="import('./js/Gantt/mlx-gantt.js').then(m => m.handleAlertClick('${task.id}'))">
                    <span class="mlx-ico-bell"></span>
                    <span class="badge ${severityClass}" data-task-id="${task.id}" style="display: ${displayStyle};">${unreadCount}</span>
                </button>
            </div>`;
            container[0].innerHTML = icon;
        }
    }
}

export function getSeverityClass(alerts) {
    if (alerts.some(a => !a.IsRead && a.AlertSeverity === 0)) return "red";
    if (alerts.some(a => !a.IsRead && a.AlertSeverity === 1)) return "orange";
    if (alerts.some(a => !a.IsRead && a.AlertSeverity === 2)) return "blue";
}

// pinta el progress correctamente (5%)
export function getProgressFoColumn(Dataid, DataField) {
    try {
        var task = getTaskOfGantt(Dataid);
        switch (task.TooltipType) {
            case 'LaborEquipmentGeneral':
            case 'LaborWorkerGeneral':
            case 'PlanningFlow':
            case 'PlanningOrder':
            case 'PlanningActivity':
                return task[DataField];

            default:
                return "";

        }
    }
    catch (error) {
        logError("getProgressFoColumn", error);
    }
}

export function getProductivityColumn(container, options) {
    try {
        const raw = options.value ?? options.data?.Productivity ?? "";
        const value = parseFloat(raw || 0);
        const formatted = Number.isInteger(value)
            ? value + " %"
            : value.toFixed(2) + " %";

        container[0].innerText = formatted;

        requestAnimationFrame(() => {
            const cell = container[0];
            function updateTooltip() {
                if (cell.offsetWidth < cell.scrollWidth) {
                    cell.title = formatted;
                } else {
                    cell.title = "";
                }
            }

            updateTooltip();
            const observer = new ResizeObserver(() => {
                updateTooltip();
            });
            observer.observe(cell);
        });
    } catch (error) {
        logError("getProductivityColumn", error);
    }
}

export function covertToAMPM(container, options) {
    try {
        let formattedDate = options.text;
        formattedDate = formattedDate.replace(/a.m./g, 'AM').replace(/p.m./g, 'PM');
        container[0].innerHTML = formattedDate;
        //añade tooltip de title 
        requestAnimationFrame(() => {
            const cell = container[0];
            function updateTooltip() {
                if (cell.offsetWidth < cell.scrollWidth) {
                    cell.title = formattedDate;
                } else {
                    cell.title = "";
                }
            }
            updateTooltip(); //hace la primera medida al cargar los datos en las celdas
            const observer = new ResizeObserver(() => {
                updateTooltip(); // Cada vez que cambie el tamaño de las columnas
            });
            observer.observe(cell);
        });

    }
    catch (error) {
        logError("covertToAMPM", error);
    }
}

function selectedTask(container, options) {
    const task = getTaskOfGantt(options.data.id);
    const rowElement = container[0]?.closest("tr");
    if (!rowElement) return;

    const iconContainer = rowElement.querySelector(".dx-treelist-icon-container");
    if (iconContainer && !iconContainer.querySelector(`input[data-id="${options.data.id}"]`)) {
        const check = document.createElement("input");
        check.type = "checkbox";
        check.className = "custom-check";
        check.dataset.id = options.data.id;
        check.checked = task.Multiselect;

        iconContainer.insertBefore(check, iconContainer.firstChild);

        check.addEventListener("click", e => e.stopPropagation());
        check.addEventListener("change", e => {
            const checked = e.target.checked;
            task.Multiselect = checked;
            updateTaskListSelected(task, checked);

            if (getTaskChildren(task.id).length > 0) {
                toggleChildren(getTaskChildren(task.id), checked);
                toogleParent(task);
            }
        });
    }
}

function ChangeColorRow(task, checked) {
    const element = document.querySelector(`input[type="checkbox"][data-id="${task.id}"]`);
    if (element) {
        const elemetRow = element.closest("tr");
        const color = (task.BackgroundColorRow || '').trim();
        if (checked)
            task.BackgroundColorRow = "#F5EE27";
        else
            task.BackgroundColorRow = task.color !== '' ? task.color : '#FFFFFF';
    }
}

function GetMultiselectFalse(selecTask) {
    const dataTask = ganttInstance.option("tasks.dataSource").store()._array;
    var multiselectTasks = dataTask.filter(a => a.Multiselect);
    multiselectTasks.forEach(x => {
        x.Multiselect = selecTask;
    });
}

function updateTaskListSelected(task, checked) {
    if (checked && !listSeletedTask.some(t => t.id === task.id)) {
        listSeletedTask.push(task);
    }
    else {
        const deleteTask = listSeletedTask.findIndex(t => t.id == task.id);
        listSeletedTask.splice(deleteTask, 1);
    }
}

function toggleChildren(children, checked) {
    children.forEach(child => {
        if (getTaskType(child.TooltipType) != EnumTaskType.LEVEL_ACT) {
            child.Multiselect = checked;
            updateTaskListSelected(child, checked);

            const childInput = document.querySelector(`input[data-id="${child.id}"]`);
            if (childInput) {
                childInput.checked = checked;
            }

            if (getTaskChildren(child.id).length > 0) {
                toggleChildren(getTaskChildren(child.id), checked);
            }

            deleteOrdersNull();
        }
    });
}

function toogleParent(task) {
    const taskParent = getTaskOfGantt(task.parentId);
    if (taskParent == undefined) return;

    const taskChildrensSelected = getTaskChildren(taskParent.id);
    const elementSelected = taskChildrensSelected.every(h => h.Multiselect);

    taskParent.Multiselect = elementSelected;
    const parentInput = document.querySelector(`input[data-id="${taskParent.id}"]`);
    if (parentInput) {
        parentInput.checked = elementSelected;
    }

    toogleParent(taskParent);
}

function deleteOrdersNull() { listSeletedTask = listSeletedTask.filter(x => x.title != null); }

function CheckFilters() {
    if (ganttInstance && ganttInstance._savedSortFilterState.filter == null)
        return [];

    let result = [];
    const filter = ganttInstance._savedSortFilterState.filter;
    if (typeof filter[0] === "string") {
        result.push({ field: filter[0], value: filter[2] });
    }

    else {
        for (let i = 0; i < filter.length; i++) {
            if (Array.isArray(filter[i])) {
                result.push({
                    field: filter[i][0],
                    value: filter[i][2]
                });
            }
        }
    }

    return result;
}

export function closePopup() {
    const pivotIntance = $('#pivotgrid').dxPivotGrid('instance');
    if (pivotIntance) {
        pivotIntance._fieldChooserPopup.hide();
    }
}

// #endregion

// #region *************************ALERTS****************************************** */
export function handleAlertClick(taskId) {
    if (alertInterop) {
        alertInterop.invokeMethodAsync("ShowTaskAlerts", taskId);
    }
}

export function updateAlertBadge(taskId, unreadCount, alertId = null) {
    const badge = document.querySelector(`#task-alert-${taskId} .badge`);
    if (badge) {
        badge.textContent = unreadCount;
        badge.style.display = unreadCount > 0 ? "inline-block" : "none";
    }

    //Si viene un alertId significa que se invoco desde ToggleReadStatus(AlertMessageDto msg) (opcion read/unread de cada alerta)
    if (alertId) {
        const task = dataForGantt.TaskGantt.find(t => t.id === taskId);
        if (task && Array.isArray(task.Alerts)) {
            const alert = task.Alerts.find(a => a.Id === alertId);
            if (alert) {
                alert.IsRead = !alert.IsRead;
            }
        }
    }
    //si no trae alertId se invoco desde MarkAllAsRead() (opcion Mark all as read de cada tarea padre 
    //*** La opcion Mark all as read por severidad es updateAllAlertBadge)
    else {
        const task = dataForGantt.TaskGantt.find(t => t.id === taskId);
        if (task && Array.isArray(task.Alerts)) {
            task.Alerts.forEach(alert => {
                alert.IsRead = true;
            });
        }
    }

}

//Opcion Mark all as read por severidad (iconos de la barra superior)
export function updateAllAlertBadge(badgeData, severity = null) {
    const badges = document.querySelectorAll('.alerts .badge');

    badges.forEach(badge => {
        const taskId = badge.getAttribute("data-task-id");
        const count = badgeData[taskId] || 0;

        if (count > 0) {
            badge.textContent = count;
            badge.style.display = "inline-block";
        } else {
            badge.textContent = "";
            badge.style.display = "none";
        }

        const task = dataForGantt.TaskGantt.find(t => t.id === taskId);
        if (task && Array.isArray(task.Alerts)) {
            task.Alerts.forEach(alert => {
                if (alert.AlertSeverity === severity) {
                    alert.IsRead = true;
                }
            });
        }
    });
}

// #endregion

export function getColumnDateFormat(defaultFmt = "dd/MM/yyyy hh:mm a") {
    try {

        const firstTask = dataForGantt?.TaskGantt?.[0];
        if (firstTask && firstTask.DateTimeFormatt) {
            return firstTask.DateTimeFormatt.replace("tt", "a");
        }
    } catch (e) {
        console.warn("getColumnDateFormat: usando formato default", e);
    }
    return defaultFmt;
}

export function getTaskType(type) {
    switch (type) {
        case "PlanningGeneral":
        case "PlanningEstimation":
        case "PlanningTrailer":
        case "PlanningPriority":
        case "PlanningWarehouse":
        case "LaborEquipmentGeneral":
        case "LaborWorkerGeneral":
        case "YardGeneral":
            return EnumTaskType.LEVEL_HIGTH;
            break;

        case "PlanningFlow":
        case "PlanningEstimationIn":
        case "PlanningEstimationOut":
            return EnumTaskType.LEVEL_MED;
            break;

        case "None":
        case "PlanningOrder":
        case "PlanningEstimationInOrder":
        case "PlanningEstimationOutOrder":
        case "PlanningWarehouseOrder":
        case "LaborEquipmentOrder":
        case "LaborWorkerOrder":
            return EnumTaskType.LEVEL_ORDER;
            break;

        case "PlanningActivity":
        case "LaborWorkerActivity":
        case "LaborEquipmentActivity":
        case "YardOrder":
        default:
            return EnumTaskType.LEVEL_ACT;
            break;
    }
}

function AddMultiselectInput(type) {
    switch (type) {
        case "PlanningGeneral":
        case "PlanningTrailer":
        case "PlanningPriority":
        case "LaborEquipmentGeneral":
        case "LaborWorkerGeneral":
        case "YardGeneral":
        case "PlanningFlow":
        case "PlanningOrder":
        case "LaborEquipmentOrder":
        case "LaborWorkerOrder":
            return true;

        default:
            return false;
    }
}

export function aplicarFiltro(column, valores) {
    initFilterManager();
    filterManager.setFilter(column, valores);
    filterManager.changeFilterIconColor();
}

export function logError(location, error) {
    console.log("Error en ", location, "() =>", error);
}


// #region *************************LAZY LOAD****************************************** */
export function loadChildTasks(parentTaskId) {
    if (!window.allTasksData || !ganttInstance) return false;
    const DataTasks = dataHeaderFilter.length > 0 ? dataHeaderFilter : window.allTasksData;

    const childTasks = DataTasks.filter(
        t => t.parentId === parentTaskId);

    if (childTasks.length === 0) return false;

    const currentDS = ganttInstance.option("tasks").dataSource;
    if (!currentDS || typeof currentDS.store !== "function") return false;

    const store = currentDS.store();
    if (!store) return false;

    const existing = new Set((store._array ?? []).map(t => t.id));
    const toInsert = childTasks.filter(c => !existing.has(c.id));

    if (toInsert.length === 0) return false;

    if (typeof store.push === "function") {
        store.push(toInsert.map(data => ({ type: "insert", data })));
    } else if (Array.isArray(store._array)) {
        store._array.push(...toInsert);
    }

    const parent = (store._array || []).find(t => t.id === parentTaskId);
    if (parent) {
        parent.childrenLoaded = true;
        window.loadedChildren.add(parentTaskId);
    }

    return true;
}

function renderLazyExpandBtn(container, task) {
    if (!window.lazyEnabled) return;
    const id = task?.id;
    const hasKids = (id && window.hasChildrenMap?.get(id)) || false;
    const alreadyLoaded = window.loadedChildren?.has(id) || task?.childrenLoaded === true;

    if (!hasKids || alreadyLoaded || task.isExpand) return;

    const rawContainer = container instanceof jQuery ? container[0] : container;
    const cell = rawContainer?.closest?.('td');

    const iconContainer = cell.querySelector('.dx-treelist-icon-container');
    const emptySpaces = iconContainer.querySelectorAll('.dx-treelist-empty-space');

    const containFilter = Object.keys(filtrosActivos).length > 0;

    let targetSpace = null;

    if (!emptySpaces?.length) return;

    //Para tomar siempre el último espacio dependiendo de su longitud
    if (containFilter) {
        targetSpace = emptySpaces[emptySpaces.length - 1];
    }
    else {
        if (task.levelTask <= 2) return;
        targetSpace = emptySpaces[Math.min(1, emptySpaces.length - 1)];
    }

    const span = document.createElement('span');
    span.className = containFilter ? '' : 'dx-icon dx-icon-chevrondown lazy-expand-btn';
    const clickHandler = async (ev) => {
        ev.stopPropagation();
        ev.preventDefault();
        await expandWithLoading(id, task, dotNetHelper);
    };

    if (containFilter && task.levelTask > 1) {
        targetSpace.addEventListener('click', clickHandler);
    } else {
        span.addEventListener('click', clickHandler);
    }

    targetSpace.innerHTML = '';
    if (containFilter)
        targetSpace.classList.add('dx-treelist-collapsed');
    targetSpace.appendChild(span);

    simulateExternalClick();
}

function treeCellWithLazyBtn(container, options, task) {
    container.empty();
    renderLazyExpandBtn(container, task);

    const text = options.text ?? '';
    const span = document.createElement('span');
    span.textContent = text;
    container[0].appendChild(span);

    requestAnimationFrame(() => {
        function updateTooltip() {
            span.title = (span.offsetWidth < span.scrollWidth) ? text : '';
        }
        updateTooltip();
        const obs = new ResizeObserver(updateTooltip);
        obs.observe(span);
    });
}

function simulateExternalClick() {
    const container = document.querySelector('.mlx-preview-container') || document.body;
    if (container) {
        container.dispatchEvent(new MouseEvent("mousedown", { bubbles: true }));
        container.dispatchEvent(new MouseEvent("mouseup", { bubbles: true }));
        container.dispatchEvent(new MouseEvent("click", { bubbles: true }));
    }
}

async function expandWithLoading(id, task, dotNetHelper) {
    if (dotNetHelper)
        dotNetHelper.invokeMethodAsync("showLoadingJs", true);

    const initialChildCount = document.querySelectorAll('tr[aria-level="4"]').length;
    const result = loadChildTasks(id);

    if (!result) {
        if (dotNetHelper)
            dotNetHelper.invokeMethodAsync("showLoadingJs", false);
        return false;
    }

    if (task) {
        task.childrenLoaded = true;
        task.isExpand = true;
    }
    window.loadedChildren.add(id);
    ganttInstance?.expandTask?.(id);

    const maxAttempts = 50;
    let attempts = 0;

    return new Promise((resolve) => {
        const checkIfDone = () => {
            const loadingPanel = document.querySelector('dxbl-loading-panel');
            const isLoadingVisible = loadingPanel?.getAttribute('panel-visible') === 'true';
            const currentRows = [...document.querySelectorAll('tr[aria-level="4"]')];
            const currentChildCount = currentRows.length;

            if (!isLoadingVisible && currentChildCount > initialChildCount) {
                clearInterval(intervalId);
                if (dotNetHelper)
                    dotNetHelper.invokeMethodAsync("showLoadingJs", false);
                resolve(true);
            } else if (++attempts >= maxAttempts) {
                clearInterval(intervalId);
                if (dotNetHelper)
                    dotNetHelper.invokeMethodAsync("showLoadingJs", false);
                resolve(false);
            }
        };

        const intervalId = setInterval(checkIfDone, 100);
    });
}

function renderLazyBtnForLevels(levelType, dataPlaning) {
    if (levelType == 1 || levelType == 2) {
        buildHasChildrenMap(dataPlaning);
    }
}

// #endregion


export function deleteTasksFromGanttDom(ids) {
    const allIdsToDelete = new Set(ids);

    ids.forEach(id => {
        const task = getTaskOfGantt(id);
        if (task && task.parentId) {
            const parentTask = getTaskOfGantt(task.parentId);
          
            if (parentTask && parentTask.Multiselect) {
                allIdsToDelete.add(parentTask.id);
            }
        }
    });

    allIdsToDelete.forEach(id => {
        ganttInstance.option("editing", {
            enabled: true,
            allowTaskDeleting: true
        });
        ganttInstance.deleteTask(id);
        window.idsEliminadosGantt.add(id);
    });

    if (tasksToLoad) {
        tasksToLoad = tasksToLoad.filter(task =>
            !window.idsEliminadosGantt.has(task.id)
        );
    }

    if (window.allTasksData) {
        window.allTasksData = window.allTasksData.filter(task =>
            !window.idsEliminadosGantt.has(task.id)
        );
    }

    setTimeout(() => {
        ganttInstance.option("editing", {
            enabled: false,
            allowTaskDeleting: false
        });
    }, 50);
}

export function updateTaskPriority(taskIds, newPriority) {
    try {
        if (!ganttInstance || !Array.isArray(taskIds) || taskIds.length === 0) return;

        const normalizedPriority = newPriority === "Very Low" ? "Very low" : newPriority;

        const tree = ganttInstance._treeList;
        if (!tree) return;

        const hasAnySelectedTask = taskIds.some(id =>
            listSeletedTask.some(t => t.id === id)
        );

        taskIds.forEach(taskId => {
            const rowIndex = tree.getRowIndexByKey(taskId);
            if (rowIndex >= 0) {
                tree.cellValue(rowIndex, "Priority", normalizedPriority);

                const task = getTaskOfGantt(taskId);
                if (task) {
                    task.Priority = normalizedPriority;
                }

                const taskInSelectedList = listSeletedTask.find(t => t.id === taskId);
                if (taskInSelectedList) {
                    taskInSelectedList.Priority = normalizedPriority;
                }

                listSeletedTask = listSeletedTask.filter(t => t.id !== taskId);
            }
        });
        clearListSelectedTask(hasAnySelectedTask);
        tree.refresh();

    } catch (error) {
        console.error('Error en updateTaskPriority:', error);
    }

}

function applyBlockToLocalData(taskId, isBlock, dateTimeBlock) {
    const parent = getTaskOfGantt(taskId);
    if (parent) {
        parent.isBlock = isBlock;
        parent.EndBlockDate = dateTimeBlock;
    }

    const childs = getTaskChildren(taskId);
    childs.forEach(ch => {
        ch.isBlock = isBlock;
        ch.EndBlockDate = dateTimeBlock;
    });
}

function getDataPlanning() {

    if (!dataForGantt?.TaskGantt?.length)
        return {
            incluidos: [],
            excluidos: [],
        };

    if (window.childTasksLoaded) return { incluidos: dataForGantt.TaskGantt, excluidos: [] }

    let ids = new Set();
    const root = dataForGantt.TaskGantt.find(x => x.index == 1000);

    if (!root) return { incluidos: [], excluidos: [] };

    ids.add(root.id);

    const tareasDescendientes = getTaskChildren(root.id);
    tareasDescendientes.forEach(tarea => {
        ids.add(tarea.id);
        incluirDescendientes(tarea.id, dataForGantt.TaskGantt, ids);
    });

    return {
        incluidos: dataForGantt.TaskGantt.filter(t => ids.has(t.id)),
        excluidos: dataForGantt.TaskGantt.filter(t => !ids.has(t.id))
    };
}

function updateBlockCells(taskId, isBlock, dateTimeBlock) {
    const tree = ganttInstance?._treeList;
    if (!tree) return;

    const all = [taskId, ...getTaskChildren(taskId).map(c => c.id)];

    all.forEach(id => {
        const rowIndex = tree.getRowIndexByKey(id);
        if (rowIndex >= 0) {
            tree.cellValue(rowIndex, "isBlock", isBlock);
            tree.cellValue(rowIndex, "EndBlockDate", isBlock ? dateTimeBlock : "");
        }
    });

    tree.refresh();
}
