export class PivotHandler {

    static initialState = null;
    static warehouseName = null;
    static dataGantt = null;

    _loadWarehouseName(whName) {
        PivotHandler.warehouseName = whName;
    }

    _resetPivot() {
        var pivotInstance = $("#pivotgrid").dxPivotGrid("instance");
        pivotInstance.getDataSource().state(PivotHandler.initialState);
        localStorage.removeItem(`pivot-config-${window.currentUser}`);
    }

    _collapsedExpanded(isExpand) {
        if (isExpand) {
            this.#expandRows();
        } else {
            this.#collapsedRows();
        }

        this.#ChangeBtnExpandToCollapsed(!isExpand);
    }

    #collapsedRows() {
        var pivotInstance = $("#pivotgrid").dxPivotGrid("instance");
        const rows = pivotInstance.getDataSource()._fields.filter(item => item.area == "row");
        if (rows.length > 0) {
            rows.forEach(data => {
                pivotInstance.getDataSource().collapseAll(data.dataField);
            });
        }
    }

    #expandRows() {
        var pivotInstance = $("#pivotgrid").dxPivotGrid("instance");
        const rows = pivotInstance.getDataSource()._fields.filter(item => item.area == "row");
        if (rows.length > 0) {
            rows.forEach(data => {
                pivotInstance.getDataSource().expandAll(data.dataField);
            });
        }
    }

    #ChangeBtnExpandToCollapsed(isCollapsed) {
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

    _exportPivotToCsv() {
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
            headerCell.value = 'Planning - ' + PivotHandler.warehouseName;
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

    _loadColumnsDefault(data) {
        try {
            var pivotInstance = $("#pivotgrid").dxPivotGrid("instance");
            if (data != undefined) {
                const taskForGantt = this.#converterJson(data);
                const fields = [];
                taskForGantt.sort((a, b) => a.index - b.index).forEach(column => {
                    var options = this.#getFieldOptions(column.dataField, column.caption);
                    if (options)
                        fields.push(options);
                });


                pivotInstance.option("dataSource", {
                    fields: fields,
                });

                PivotHandler.initialState = pivotInstance.getDataSource().state();
                this.#reloadPivotGrid();
            }
        }
        catch (ex) {
            console.log(ex);
        }
    }

    #reloadPivotGrid() {
        var pivotInstance = $("#pivotgrid").dxPivotGrid("instance");
        pivotInstance.getDataSource().reload();
    }

    #converterJson(jsonDataTask) {
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

    #getFieldOptions(dataField, caption) {

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

    _drawChartAndPivotTogether(chartType) {
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
                onDrawn: (e) => {
                    this.#updateChartVisibility(e.component);
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

    #updateChartVisibility() {
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

    _ClosePopupColumnChooser() {
        var pivotInstance = $("#pivotgrid").dxPivotGrid("instance");
        if (pivotInstance._fieldChooserPopup._currentVisible) {
            pivotInstance._fieldChooserPopup.hide();
        }
    }

    _loadDataForPivot(data) {
        try {
            if (data != undefined) {
                const taskForGantt = this.#converterJson(data);
                PivotHandler.dataGantt = taskForGantt
                this.#deleteValuesNull(PivotHandler.dataGantt);
            }
            this.#reloadPivotGrid();
        }
        catch (ex) {
            console.log(ex);
        }
    }

    #deleteValuesNull(dataGantt) {
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
                var pivotInstance = $("#pivotgrid").dxPivotGrid("instance");
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

    invoke(methodName, args) {
        const method = this[`_${methodName}`];
        if (typeof method === "function") {
            method.apply(this, args);
        } else {
            console.warn(`Método '${methodName}' no encontrado en GanttHandler.`);
        }
    }
}
