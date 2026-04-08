let itemsDashboard;
let minWidthDashboard;
const widthItem = 900;
let dashboardControl;
let dotNetHelper;
let itemWidgetName;
let loadOnlyWidget = false;
const Model = DevExpress.Dashboard.Model;

const domCache = {
    layoutItems: null,
    dashboardContainer: null
};

export function initializeDashboard(config) {

    window.dashboardTranslations = config.fieldTexts;
    changeLoadingText(config.fieldTexts.loadignData);

    DevExpress.localization.locale(config.region);

    dashboardControl = new DevExpress.Dashboard.DashboardControl(
        document.getElementById("web-dashboard"),
        { endpoint: "/api/dashboard", workingMode: "Interactive", enableExport: true }
    );

    dashboardControl.remoteService.headers = {
        "GroupWidgets": config.groupWidgets,
        "CurrentPlanning": config.currentPlanning
    };

    dashboardControl.on("dashboardInitialized", function () {
        const viewerApiExtension = dashboardControl.findExtension("viewerApi");
        if (!viewerApiExtension) return;

        if (!viewerApiExtension.__toolbarBound) {
            viewerApiExtension.on("itemCaptionToolbarUpdated", customizeItemToolbar);
            viewerApiExtension.__toolbarBound = true;
        }

        if (!viewerApiExtension.__optionsPreparedBound) {
            viewerApiExtension.on("itemWidgetOptionsPrepared", function (e) {
                const itemType = (e.dashboardItem && typeof e.dashboardItem.itemType === "function")
                    ? e.dashboardItem.itemType()
                    : null;

                if (itemType === "Grid" && e.options?.columns) {
                    e.options.columns.forEach(col => {
                        const df = col.dataField;
                        col.cellTemplate = function (container, options) {
                            const raw = options.data[df];
                            if (!raw) { container.text(""); return; }
                            const d = new Date(raw);
                            container.text(!isNaN(d.getTime()) ? formatCustomDate(d, config.dateHourFormat) : raw);
                        };
                    });
                }

                if (itemType === "Chart" && e.options && isTargetDataMember(e, "processinterleavingview")) {
                    applyAxisLabelFormatting(e, config.dateHourFormat, new Map());
                }
            });

            viewerApiExtension.__optionsPreparedBound = true;
        }

        if (!viewerApiExtension.__tooltipBound) {
            setupTooltipForItemByDataMember(viewerApiExtension, "processinterleavingview", config.dateHourFormat);
            viewerApiExtension.__tooltipBound = true;
        }
    });

    dashboardControl.render();
    changeNoDataText(config.fieldTexts.noDataText);
    loadWidgets(config.isWidget);
}

function isTargetDataMember(e, wantedDataMember) {
    try {
        const dm = e.dashboardItem?.dataMember?.();
        return dm === wantedDataMember;
    } catch {
        return false;
    }
}

function setupTooltipForItemByDataMember(viewerApiExtension, dataMember, datehourFormat) {
    const patched = new WeakSet();

    const handler = (e) => {
        if (!isTargetDataMember(e, dataMember)) return;

        const chart = (e.getWidget && e.getWidget()) || e.component || e.widget;
        if (!chart || typeof chart.option !== "function" || patched.has(chart)) return;
        patched.add(chart);

        requestAnimationFrame(() => {
            const cur = chart.option("tooltip") || {};
            chart.option("tooltip", {
                ...cur,
                enabled: true,
                shared: true,
                contentTemplate: null,
                customizeTooltip(info) {
                    const header = buildHeaderText(info.argument, info.argumentText, datehourFormat);

                    if (Array.isArray(info.points) && info.points.length) {
                        const rows = info.points.map(p => {
                            const color = (p.point && p.point.getColor && p.point.getColor()) || p.color || "#999";
                            const val = formatPercent(p.value);
                            return `
                <div class="tt-row">
                  <span class="tt-marker" style="background:${color}"></span>
                  <span>${escapeHtml(p.seriesName)}: ${val}</span>
                </div>`;
                        }).join("");
                        return { html: `<div class="dx-custom-tt"><div class="tt-header">${escapeHtml(header)}</div>${rows}</div>` };
                    }

                    const val = formatPercent(info.value);
                    const line = info.seriesName ? `${escapeHtml(info.seriesName)}: ${val}` : val;
                    return { html: `<div class="dx-custom-tt"><div class="tt-header">${escapeHtml(header)}</div><div class="tt-row"><span>${line}</span></div></div>` };
                }
            });
        });
    };

    viewerApiExtension.on("itemWidgetCreated", handler);
    viewerApiExtension.on("itemWidgetUpdated", handler);
}

function applyAxisLabelFormatting(e, datehourFormat, hourStats) {
    const ax = (e.options.argumentAxis = e.options.argumentAxis || {});
    ax.label = ax.label || {};
    const prev = ax.label.customizeText;
    const itemName = e.itemName;
    const isContinuous = ax.type === "continuous";

    ax.label.customizeText = function (arg) {
        if (isContinuous) {
            const v = arg?.value;
            if (isValidDate(v)) return formatCustomDate(v, datehourFormat);
            return typeof prev === "function" ? prev.call(this, arg) : (arg?.valueText ?? "");
        }
        const raw = (arg && arg.value !== undefined) ? arg.value : arg?.valueText;
        const n = Number(raw);
        const isHour = isHourValue(n);

        let s = hourStats.get(itemName);
        if (!s) s = { total: 0, valid: 0, set: new Set(), min: Infinity, max: -Infinity };
        s.total++;
        if (isHour) {
            s.valid++;
            s.set.add(n);
            if (n < s.min) s.min = n;
            if (n > s.max) s.max = n;
        }
        hourStats.set(itemName, s);

        const size = s.set.size;
        const contiguous = size > 0 && (s.max - s.min + 1 === size);
        const ratio = s.total ? s.valid / s.total : 0;
        const looksLikeHours = size >= 12 && contiguous && ratio >= 0.90;

        if (looksLikeHours) {
            const hour = isHour ? n : (Number.isFinite(s.min) ? s.min : 0);
            return formatHourLabel(hour, datehourFormat);
        }

        return typeof prev === "function" ? prev.call(this, arg) : (arg?.valueText ?? "");
    };
}

function buildHeaderText(argument, argumentText, datehourFormat) {
    if (isValidDate(argument)) return formatCustomDate(argument, datehourFormat);
    return argumentText || String(argument ?? "");
}

function isValidDate(v) {
    return v instanceof Date && !isNaN(v.getTime());
}

function formatPercent(value) {
    const num = Number(value);
    const scaled = Math.abs(num) <= 1 ? num * 100 : num;
    const fixed = DevExpress.localization.formatNumber(scaled, { type: "fixedPoint", precision: 2 });
    return `${fixed} %`;
}

function escapeHtml(s) {
    return String(s).replace(/[&<>"']/g, c => (
        { "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[c]
    ));
}

function formatCustomDate(date, datehourFormat) {
    const is12hours = datehourFormat.includes('tt');
    const cleanFormat = datehourFormat.replace('tt', '').trim();
    const formattedDate = DevExpress.localization.formatDate(date, cleanFormat);
    if (is12hours) {
        const hours = date.getHours();
        const ampm = hours >= 12 ? 'PM' : 'AM';
        return `${formattedDate} ${ampm}`;
    }
    return formattedDate;
}

const dashboardDescriptionProperty = {
    ownerType: Model.Dashboard,
    propertyName: "DashboardDescription",
    defaultValue: "",
    valueType: 'string'
};
Model.registerCustomProperty(dashboardDescriptionProperty);

export function customizeItemToolbar(e) {
    var translations = window.dashboardTranslations || {};

    var hint = e.dashboardItem.customProperties.getValue(dashboardDescriptionProperty.propertyName) ? e.dashboardItem.customProperties.getValue(dashboardDescriptionProperty.propertyName) : e.dashboardItem.name();
    e.options.actionItems.push({
        hint,
        name: "Info",
        icon: "dx-dashboard-help",
        type: "button"
    });

    e.options.actionItems.forEach((item, index) => {
        if (item.name === "export-menu") {
            item.hint = translations.export;
        }

        if (item.name === "maximize-down-item") {
            item.hint = translations.maximize;
        }
    });
}

export function reloadDataGant(groupWidgets, currentPlanning) {

    dashboardControl.remoteService.headers = {
        "GroupWidgets": groupWidgets,
        "CurrentPlanning": currentPlanning
    };

    dashboardControl.refresh();
}

export function disposeDashboard() {
    try {
        const dashboards = DevExpress?.Dashboard?.getDashboardControlInstances?.() || [];
        dashboards.forEach(d => {
            if (d && typeof d.dispose === "function") {
                d.dispose();
            }
        });
    } catch (err) {
        console.error("disposeDashboard error:", err);
    }
}
export function loadWidgets(isOnlyWidget) {
    if (isOnlyWidget == 'true') {
        loadOnlyWidget = true;
        realoadOnlyWidget();
    }
    else {
        loadOnlyWidget = false;
        dashboardControl.restoreDashboardItem();
    }
}
export function realoadOnlyWidget() {
    if (itemWidgetName != undefined || itemWidgetName != "") {

        let viewerApiExtension = dashboardControl.findExtension("viewerApi");
        if (viewerApiExtension) {
            viewerApiExtension.on('itemWidgetCreated', itemWidgetCreated);
            viewerApiExtension.on('itemCaptionToolbarUpdated', customizeCaptionToolbar);
        }
    }
}

export function itemWidgetCreated(e) {
    if (!domCache.layoutItems) {
        domCache.layoutItems = document.getElementsByClassName("dx-layout-item");
    }
    Array.from(domCache.layoutItems).forEach(item => {
        item.style.display = 'none';
    });

    if (e.itemName === itemWidgetName) {
        dashboardControl.maximizeDashboardItem(itemWidgetName);
        itemWidgetName = "";

        document.querySelector("#dashboardWidgetContainer").addEventListener("click", function (params) {
            if (params.target.closest('.dx-dashboard-export')) {
                loadOnlyWidget = true;
                customizeExportPopup();
            }
        });
    }
}

export function customizeCaptionToolbar(e) {
    var menuItems = e.options.actionItems;
    var index = e.options.actionItems.findIndex(x => x.name == "restore-item");
    if (index > -1) {
        menuItems.splice(index, 1);
    }
}

export function getDoNetHelper(netHelper) {
    dotNetHelper = netHelper;
}
export function moveCarousel(direction) {
    const dashboard = document.querySelector(".carousel-container-dahsboard");
    const scrollAmount = scrollRoll(dashboard.scrollWidth);
    if (direction === 'left') {
        dashboard.scrollLeft += scrollAmount;
    } else if (direction === 'right') {
        dashboard.scrollLeft -= scrollAmount;
    }
}
export function scrollRoll(widthScroll) {
    return (widthItem * widthScroll) / minWidthDashboard;
}
export function getItems(items) {
    itemsDashboard = items;
    const btnPrevious = document.getElementById("btnPreviousWidget");
    const btnNext = document.getElementById("btnNextWidget");
    if (items <= 3) {
        btnNext.classList.remove('mlx-btn-action-enabled');
        btnNext.classList.add('mlx-btn-action-disabled');
        btnPrevious.classList.remove('mlx-btn-action-enabled');
        btnPrevious.classList.add('mlx-btn-action-disabled');
    }
    else {
        btnNext.classList.remove('mlx-btn-action-disabled');
        btnNext.classList.add('mlx-btn-action-enabled');
        btnPrevious.classList.remove('mlx-btn-action-disabled');
        btnPrevious.classList.add('mlx-btn-action-enabled');
    }
}
export function setMinWidth(changeWidth) {
    if (changeWidth) {
        minWidthDashboard = (itemsDashboard - 1) * widthItem;
        document.getElementById("web-dashboard").style.minWidth = minWidthDashboard + "px";
    }
}

document.addEventListener("click", function (event) {
    if (event.target.closest('.dx-dashboard-maximize-item')) {
        itemWidgetName = dashboardControl.maximizedDashboardItemName;
        dotNetHelper.invokeMethodAsync('OpenPopup', 'OpenPopup');
    }

    if (event.target.closest('.dx-dashboard-export')) {
        let clickedElement = event.target;
        let parentDiv = clickedElement.closest('[data-layout-item-name]');
        itemWidgetName = parentDiv.getAttribute('data-layout-item-name');
        customizeExportPopup();
    }
});
document.querySelector("#web-dashboard").addEventListener("click", function (event) {
    if (event.target.closest('.dx-dashboard-export')) {
        customizeExportPopup();
    }
});

export function findWidgetByName(dashboard, widgetName) {
    return dashboard().items().find(function (item) {
        return item.componentName() === widgetName;
    });
}
export function customizeExportPopup() {
    const root = document.querySelector(".dx-popup-wrapper");
    if (!root) return;

    const popups = root.children;

    if (popups.length > 0) {
        Array.from(popups).forEach((popup) => {
            addButtonsToPopup(popup, window.dashboardTranslations);
        });
    }
}

export function addButtonsToPopup(popup, translations = {}) {
    popup.innerHTML = "";
    const container = document.createElement("div");
    container.style.padding = "2px";
    container.style.textAlign = "left";

    const pdfButton = createButtonExport(
        "iconExportToPDF",
        "mlx-ico-doc-pdf",
        translations.exportPDF,
        () => exportToPDF()
    );
    const excelButton = createButtonExport(
        "iconExportToExcel",
        "mlx-ico-doc-xml",
        translations.exportExcel,
        () => exportTo('Excel')
    );
    const imageButton = createButtonExport(
        "iconExportToImage",
        "mlx-ico-image",
        translations.exportImage,
        () => exportToPNG()
    );

    container.appendChild(pdfButton);
    container.appendChild(excelButton);
    container.appendChild(imageButton);
    popup.appendChild(container);
}
export function createButtonExport(id, iconClass, labelText, onClickHandler) {
    const div = document.createElement("div");
    div.style.cursor = "pointer";

    const button = document.createElement("button");
    button.id = id;
    button.className = "mlx-export-widgets";

    const icon = document.createElement("span");
    icon.className = iconClass;
    icon.style.color = 'red';

    const label = document.createElement("label");
    label.style.cssText = "font-size: 12px; padding-left: 10px; cursor: pointer;";
    label.textContent = labelText;

    button.addEventListener("click", onClickHandler);

    button.appendChild(icon);
    button.appendChild(label);
    div.appendChild(button);

    return div;
}

export function exportTo(format) {
    const exportExtension = dashboardControl.findExtension("dashboardExport");
    if (exportExtension) {
        const options = {
            fileName: `widgetExport_${format}`,
            size: { width: 800, height: 600 }
        };
        exportExtension.exportDashboardItemTo(itemWidgetName, format, options);
    } else {
        console.error("Extensión de exportación no encontrada");
    }
}
export function exportToPDF() {
    try {
        let { jsPDF } = window.jspdf;
        let widgetElement = getWidgetForExport();

        if (widgetElement) {
            HidePoupOfExport();

            html2canvas(widgetElement, {
                scale: 2,
            }).then((canvas) => {
                const imgData = canvas.toDataURL('image/png');
                const doc = new jsPDF(loadOnlyWidget ? 'landscape' : 'portrait');
                doc.addImage(imgData, 'PNG', 10, 10, 180, 0);
                doc.save('widget.pdf');
            });
        }

    }
    catch (error) {
        console.log("error");
    }
}

export function HidePoupOfExport() {
    const popups = document.querySelectorAll('.dx-overlay-content');
    popups.forEach(popup => popup.style.display = 'none');

}
export function exportToCSV() {
    const exportExtension = dashboardControl.findExtension("dashboardExport");
    if (exportExtension) {
        exportExtension.exportDashboardItemToExcel(itemWidgetName, {
            fileName: "widgetExport.xlsx"
        });
    }
}
export function exportToPNG() {
    var dashboardName = dashboardControl._controlOptions.dashboardId; //para poder tener el nombre del dashboard
    let widgetElement = getWidgetForExport();
    if (widgetElement) {

        HidePoupOfExport();
        html2canvas(widgetElement, {
            scale: 2,
        }).then((canvas) => {
            const imgData = canvas.toDataURL('image/png');

            const link = document.createElement('a');
            link.href = imgData;
            link.download = 'widget.png';
            link.click();
        });
    }

}

export function changeLoadingText(translate) {
    const dashboardContainer = document.getElementById("web-dashboard");
    if (!dashboardContainer) return;

    const observer = new MutationObserver((mutations) => {
        mutations.forEach(mutation => {
            mutation.addedNodes.forEach(node => {
                if (node.nodeType === 1) {
                    const loadingElements = node.querySelectorAll('.dx-dashboard-control-loading, .dx-dashboard-layout-state.dx-layout-item-loading');
                    loadingElements.forEach(el => {
                        if (el.textContent.includes('Loading...')) {
                            el.textContent = el.textContent.replace('Loading...', translate);
                        }
                    });

                    if ((node.classList.contains('dx-dashboard-control-loading') || node.classList.contains('dx-layout-item-loading'))
                        && node.textContent.includes('Loading...')) {
                        node.textContent = node.textContent.replace('Loading...', translate);
                    }
                }
            });
        });
    });

    observer.observe(dashboardContainer, { childList: true, subtree: true });

    setTimeout(() => {
        observer.disconnect();
    }, 5000);
}

export function getWidgetForExport() {
    let widgetElement = '';
    if (loadOnlyWidget) {
        widgetElement = document.querySelector(".dx-dashboard-fullscreen-item-base");
    }
    else {
        widgetElement = document.querySelector(`[data-layout-item-name="${itemWidgetName}"]`);
    }
    return widgetElement;
}

export function changeNoDataText(translate) {
    const dashboardContainer = document.getElementById("web-dashboard");
    if (!dashboardContainer) return;

    const replaceText = () => {
        const noDataElements = dashboardContainer.getElementsByClassName("dx-datagrid-nodata");

        Array.from(noDataElements).forEach(el => {
            const currentText = el.textContent.trim();
            if (currentText === "The grid has no data.") {
                el.textContent = translate;
            }
        });
    };
    replaceText();
    const observer = new MutationObserver(replaceText);
    observer.observe(dashboardContainer, { childList: true, subtree: true });
    setTimeout(() => observer.disconnect(), 10000);
}

