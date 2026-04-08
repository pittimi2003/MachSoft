export class SchedulerHandler { 

    /**
     * Metodo para construir diseño manualmente con libreria jspdf
     * model: recibe el modelo de datos para construir el diseño
     * options: opciones de impresion
     */
    async _exportPdfFromData(model, options) {
        const { exportSchedulerPdfFromModel } = await import("../../export/exportService.js");
        await exportSchedulerPdfFromModel(model, options);
        return true;
    }

    // Exportar a PDF por hojas: delega al servicio genérico
    /**
     * Exportar a PDF por hojas: delega al servicio generico
     * hostId: Id del Contenedor 
     * options: parametros de impresion
     */
    async _exportPdfByHours(hostId, options) {
        const { exportElementToPdfByHoursTiled } = await import("../../export/exportService.js");
        await exportElementToPdfByHoursTiled(hostId, options);
        return true;
    }   

    /**
     * Exporta un XLSX COMBINADO:
     *  Hoja "Combined": title, startUtc, hours, resource(…), metric(…)
     *  Hoja "Resources": catálogo de recursos
     * Requiere window.ExcelJS (exceljs.min.js)
     */
    async _exportExcelCombined(model, filename = "Yard-Calendar.xlsx") {
        const { exportSchedulerExcelGridFromModel } = await import("../../export/exportService.js");
        await exportSchedulerExcelGridFromModel(model, { filename });
        return true;
    } 

    /**
     * 
     * Metodo para impresion simple
     */
    async _printWholePage() {
        const { printWholePage } = await import("../../export/exportService.js");
        printWholePage();
        return true;
    }    

    _GoToNow() {
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

    /**
     * 
     * Router interno
     * 
     */
    invoke(methodName, args) {
        const method = this[`_${methodName}`];
        if (typeof method !== "function") {
            throw new Error(`Método '${methodName}' no encontrado en SchedulerHandler.`);
        }
        return method.apply(this, args);
    }
}
