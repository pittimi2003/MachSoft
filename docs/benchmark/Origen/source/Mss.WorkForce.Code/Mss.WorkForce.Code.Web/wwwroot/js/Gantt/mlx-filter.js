/**
 * Version 1.0.0
 **/


import * as functGantt from './mlx-gantt.js';

//-------------------------VARIABLES--------------------------------------------------//
let pivotInstance = null;
let dataGantt = null;
let initialState = null;
let warehouseName = null;

export function drawFilterPivot(fieldChooserTexts) {
    pivotInstance = $('#pivotgrid').dxPivotGrid({
        rowHeaderLayout: "standard",
        allowFiltering: true,
        allowSorting: true,
        showBorders: true,
        headerFilter: {
            search: {
                enabled: true,
            },
            showRelevantValues: true,
        },
        fieldPanel: {
            showColumnFields: true,
            showDataFields: true,
            showFilterFields: true,
            showRowFields: true,
            allowFieldDragging: true,
            visible: true,
        },
        fieldChooser: {
            enabled: true,
            allowSearch: true,
            layout: 1,
            texts: {
                allFields: fieldChooserTexts.allFields,
                columnFields: fieldChooserTexts.columnFields,
                dataFields: fieldChooserTexts.dataFields,
                rowFields: fieldChooserTexts.rowFields,
                filterFields: fieldChooserTexts.filterFields
            },
        },
        stateStoring: {
            enabled: false,
        },
        loadPanel: {
            enabled: false
        },
        onChanged: function (e) {
            UpdateStateStorage();
        },
        onContentReady: function () {
            updateChartVisibility();

            TranslateFields(fieldChooserTexts);
        },
        onCellPrepared: function (e) { e.cellElement.css("text-align", "center"); },

    }).dxPivotGrid('instance');

    const dataSource = pivotInstance.getDataSource();
    dataSource.on('changed', function () {
        updateChartVisibility();
    });

    TranslateFieldChooser(fieldChooserTexts);
    window.hideDevExtremePlaceholder?.();
}

export function TranslateFields(fieldChooserTexts = {}) {
    const placeholders = document.querySelectorAll('.dx-empty-area-text');
    placeholders.forEach(p => {
        if (p.textContent.includes("Drop Filter Fields Here") && fieldChooserTexts.dropFilterFieldsHere) {
            p.textContent = fieldChooserTexts.dropFilterFieldsHere;
        }
    });

    const fieldChooserButton = document.querySelector('.dx-pivotgrid-field-chooser-button');
    if (fieldChooserButton && fieldChooserButton.title === "Show Field Chooser" && fieldChooserTexts.showFieldChooser) {
        fieldChooserButton.title = fieldChooserTexts.showFieldChooser;
    }

    const grandTotals = document.querySelectorAll('.dx-grandtotal');
    grandTotals.forEach(gt => {
        if (gt.textContent.trim() === "Grand Total" && fieldChooserTexts.grandTotal) {
            gt.textContent = fieldChooserTexts.grandTotal;
        }
    });
}

export function TranslateFieldChooser(translations = {}) {
    if (!pivotInstance._fieldChooserPopup) return;

    pivotInstance._fieldChooserPopup.option('onShowing', () => {
        const title = document.querySelector(".dx-popup-title .dx-toolbar-before .dx-item-content");
        if (title && title.textContent.trim() === "Field Chooser" && translations.fieldChooserTitle) {
            title.textContent = translations.fieldChooserTitle;
        }
    });
}


window.hideDevExtremePlaceholder = function () {
    const style = document.createElement("style");
    style.textContent = `
        div.dx-placeholder {
            display: none !important;
        }
    `;
    document.head.appendChild(style);
};

export function drawChartAndPivotTogether(chartType) {
    try {
        const pivotGridChart = $('#pivotchart').dxChart({
            commonSeriesSettings: {
                type: chartType,
            },
            palette: ['#51a7c4', '#805198', '#dbab4a', '#c3cedc'],
            tooltip: {
                enabled: true,
                customizeTooltip(args) {
                    return {
                        text: `${args.seriesName}: ${args.originalValue}`
                    };
                }
            },
            argumentAxis: {
                label: {
                    overlappingBehavior: "rotate",
                    wordWrap: "normal",
                    displayMode: "standard",
                }
            },
            size: {
                height: 350,

            },
            legend: {
                visible: true
            },
            export: {
                enabled: false
            },
            redrawOnResize: true,
            onDrawn: function () {
                updateChartVisibility();
            },

        }).dxChart('instance');

        const pivotGrid = $('#pivotgrid').dxPivotGrid('instance');

        pivotGrid.bindChart(pivotGridChart, {
            dataFieldsDisplayMode: 'splitPanes',
            alternateDataFields: false,
        });
    } catch (ex) {
        console.error(ex);
    }
}

export function updateChartVisibility() {
    const $chartElement = $('#pivotchart');

    if ($chartElement.length === 0 || !$chartElement.data('dxChart')) {
        return;
    }
    const chartInstance = $chartElement.dxChart('instance');

    if (!chartInstance) {
        return;
    }
    const seriesList = chartInstance.getAllSeries();
    const hasData = seriesList.some(series => series.getPoints().length > 0);

    if (hasData) {
        $chartElement.show();
    } else {
        $chartElement.hide();
    }
}

export function UpdateStateStorage() {
    pivotInstance.option('stateStoring.enabled', true);
    pivotInstance.option('stateStoring.type', 'localStorage');
    pivotInstance.option('stateStoring.storageKey', `pivot-config-${window.currentUser}`);
}

export function loadColumnsDefault(data) {
    try {
        if (data != undefined) {
            const taskForGantt = functGantt.converterJson(data);
            const fields = [];
            taskForGantt.sort((a, b) => a.index - b.index).forEach(column => {
                var options = getFieldOptions(column.dataField, column.caption);
                if (options)
                    fields.push(options);
            });


            pivotInstance.option("dataSource", {
                fields: fields,
            });

            initialState = pivotInstance.getDataSource().state();
            reloadPivotGrid();
        }
    }
    catch (ex) {
        console.log(ex);
    }
}

export function loadDataForPivot(data) {
    try {
        if (data != undefined) {
            const taskForGantt = functGantt.converterJson(data);
            dataGantt = taskForGantt
            deleteValuesNull(dataGantt);
        }
        reloadPivotGrid();
    }
    catch (ex) {
        console.log(ex);
    }
}

export function loadWarehouseName(whName) {
    warehouseName = whName;
}

export function deleteValuesNull(dataGantt) {
    try {
        if (dataGantt != undefined) {
            const fechaUTC = new Date(new Date().toISOString().replace("Z", ""));
            dataGantt = dataGantt.map(item => {
                const cleanedItem = {};
                for (const key in item) {
                    if ((key === "start" || key === "end") && (item[key] === null || item[key] === "")) {
                        cleanedItem[key] = fechaUTC;
                    } else {
                        cleanedItem[key] = item[key] === null ? "" : item[key];
                    }
                }
                return cleanedItem;
            });

            pivotInstance.option("dataSource", {
                store: dataGantt,
                retrieveFields: false,
            });
        }
    }
    catch (ex) {
        console.log(ex);
    }
}

export function resetPivot() {
    try {
        localStorage.removeItem(`pivot-config-${window.currentUser}`);
        pivotInstance.getDataSource().state(initialState);
    }
    catch (ex) {
        console.log(ex);
    }
}

export function getFieldOptions(dataField, caption) {

    const areaRow = "row";
    const areaColumn = "column";
    const areaData = "data";

    switch (dataField) {
        case 'ActivityTitle':
        case 'WorkFlow':
        case 'title':
            return {
                dataField,
                area: areaRow,
                caption,
                visible: true,
            };
            break
        case 'Priority':
            return {
                dataField,
                area: areaColumn,
                caption,
                visible: true,
            };
            break;
        case 'PivotCount':
            return {
                dataField,
                caption,
                area: areaData,
                visible: true,
            };
            break;
        case 'OrderProgress':
            return {
                dataField,
                caption,
                format: "percent",
                precision: 1,
                visible: true,
            };
            break;
        case 'SumActualWorkTime':
        case 'SumTimeOrderDelay':
            return {
                dataField,
                caption,
                area: areaData,
                visible: true,
                summaryType: "sum",
                customizeText: function (cellInfo) {
                    const totalSeconds = cellInfo.value || 0;
                    const hours = Math.floor(totalSeconds / 3600);
                    const minutes = Math.floor((totalSeconds % 3600) / 60);
                    return `${hours}:${minutes}`;
                }
            };
            break;
        default:
            return {
                dataField,
                caption,
                visible: true,
            };
            break;
    }

    return null;

}


export function getFieldOptionsAsync(dataField, caption) {
    return new Promise((resolve) => {
        const areaRow = "row";
        const areaColumn = "column";
        const areaData = "data";

        switch (dataField) {
            case 'PivotCount':
                return
                resolve({
                    dataField,
                    area: areaData,
                    caption,
                    visible: true,
                });
                break;
            case 'ActivityTitle':
            case 'WorkFlow':
            case 'title':
                return resolve({
                    dataField,
                    area: areaRow,
                    caption,
                    visible: true,
                });
                break;
            case 'Priority':
                return resolve({
                    dataField,
                    area: areaColumn,
                    caption,
                    visible: true,
                });
                break;
            case 'progress':
            case 'OrderProgress':
                return resolve({
                    dataField,
                    caption,
                    format: "percent",
                    precision: 1,
                    visible: true,
                });
                break;

            default:
                return resolve({
                    dataField,
                    caption,
                    visible: true,
                });
                break;
        }

        return resolve(null);
    });
}

export function reloadPivotGrid() {
    try {

        pivotInstance.getDataSource().reload();
    }
    catch (ex) {
        console.log(ex);
    }
}

export function collapsedExpanded(isExpand) {
    if (isExpand) {
        expandRows();
    } else {
        collapsedRows();
    }

    ChangeBtnExpandToCollapsed(!isExpand);
}

export function collapsedRows() {
    const rows = pivotInstance.getDataSource()._fields.filter(item => item.area == "row");
    if (rows.length > 0) {
        rows.forEach(data => {
            pivotInstance.getDataSource().collapseAll(data.dataField);
        });
    }
}

export function expandRows() {
    const rows = pivotInstance.getDataSource()._fields.filter(item => item.area == "row");
    if (rows.length > 0) {
        rows.forEach(data => {
            pivotInstance.getDataSource().expandAll(data.dataField);
        });
    }
}

export function changeExport() {
    try {
        const anyTaskExpanded = pivotInstance.getDataSource()._fields.filter(item => item.expanded).length > 0;
        ChangeBtnExpandToCollapsed(!anyTaskExpanded);
    }
    catch (ex) {
        console.log(ex);
    }
}


export function exportPivotToCsv() {
    try {

        // variables para mostrar la informaión a exportar
        const exportHeaderOptions = {
            exportRowFieldHeaders: true,        //filas
            exportColumnFieldHeaders: true,     //columnas
            exportDataFieldHeaders: true,       //dato
            exportFilterFieldHeaders: false,    //filtros
        };

        const pivotComponent = DevExpress.ui.dxPivotGrid.getInstance(document.getElementById("pivotgrid"));
        const workbook = new ExcelJS.Workbook();
        const worksheet = workbook.addWorksheet('pivotgrid');

        DevExpress.excelExporter.exportPivotGrid({
            component: pivotComponent,
            worksheet,
            topLeftCell: { row: 4, column: 1 },
            keepColumnWidths: false,
            ...exportHeaderOptions,
        }).then((cellRange) => {

            // Header
            const headerRow = worksheet.getRow(2);
            headerRow.height = 50;

            const columnFromIndex = worksheet.views[0].xSplit + 1;
            const columnToIndex = columnFromIndex + 3;
            worksheet.mergeCells(20, columnFromIndex, 20, columnToIndex);

            const headerCell = headerRow.getCell(columnFromIndex);
            headerCell.value = 'Planning - ' + warehouseName;
            headerCell.font = { name: 'Segoe UI Light', size: 22, bold: true };
            headerCell.alignment = { horizontal: 'left', vertical: 'middle', wrapText: false };

            // Footer
            const footerRowIndex = cellRange.to.row + 2;
            const footerCell = worksheet.getRow(footerRowIndex).getCell(cellRange.to.column);
            footerCell.value = 'WorkForce Management';
            footerCell.font = { color: { argb: 'BFBFBF' }, italic: true };

            footerCell.alignment = { horizontal: 'right' };

        }).then(() => {
            workbook.xlsx.writeBuffer().then((buffer) => {
                saveAs(new Blob([buffer], { type: 'application/octet-stream' }), 'PivotGrid.xlsx');
            });
        });

    }
    catch (ex) {
        console.log(ex);
    }
}


export function ChangeBtnExpandToCollapsed(isCollapsed) {
    const btnCollapsed = $('#btnCollapsedPivot');
    const btnExpand = $('#btnExpandPivot');
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

export function ClosePopupColumnChooser() {
    if (pivotInstance._fieldChooserPopup._currentVisible) {
        pivotInstance._fieldChooserPopup.hide();
    }
}