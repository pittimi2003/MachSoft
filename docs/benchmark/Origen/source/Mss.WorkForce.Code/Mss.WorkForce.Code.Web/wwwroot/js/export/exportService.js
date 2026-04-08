function getElement(elementOrId) {
    if (!elementOrId) return null;
    if (elementOrId instanceof HTMLElement) return elementOrId;
    return document.getElementById(elementOrId);
    //return document.getElementsByClassName(elementOrId)[0];
}

function getJsPdfCtor() {
    // Con jspdf.umd.min.js → window.jspdf.jsPDF
    if (!window.jspdf || !window.jspdf.jsPDF) {
        throw new Error("jsPDF no está disponible. Verifica el orden de scripts.");
    }
    return window.jspdf.jsPDF;
}

function getHtml2Canvas() {
    if (!window.html2canvas) {
        throw new Error("html2canvas no está disponible. Verifica el orden de scripts.");
    }
    return window.html2canvas;
}

// Color por porcentaje (tu paleta)
function colorFromST(st) {
    if (st < 50) return [204, 229, 255];  // azul claro
    if (st < 80) return [255, 243, 205];  // amarillo
    return [248, 215, 218];               // rosa
}

// Dibuja texto centrado en una celda (con clipping básico)
function textInCell(doc, txt, x, y, w, h, opts = {}) {
    const { fontSize = 9, bold = false } = opts;
    if (bold) doc.setFont(undefined, "bold");
    doc.setFontSize(fontSize);

    const cx = x + w / 2;
    const cy = y + h / 2 + (fontSize * 0.35); // ajuste de baseline sencillo
    doc.text(String(txt ?? ""), cx, cy, { align: "center", baseline: "middle" });

    if (bold) doc.setFont(undefined, "normal");
}

// Dibuja la celda tipo “saturación”
function drawSaturationCell(doc, x, y, w, h, metric) {
    const [r, g, b] = colorFromST(metric.TotalSaturation);
    doc.setDrawColor(200, 200, 200);
    doc.setFillColor(r, g, b);
    doc.rect(x, y, w, h, "FD"); // fill + stroke

    // Sección superior: ST (porcentaje general) y “ST” a la derecha
    const topH = h * 0.45;
    doc.setDrawColor(204, 204, 204);
    doc.line(x, y + topH, x + w, y + topH);

    // ST grande (centrado)
    textInCell(doc, `${metric.TotalSaturation}%`, x, y, w, topH, { fontSize: 10, bold: true });

    // Etiqueta “ST” arriba a la derecha
    doc.setFontSize(8);
    doc.text("ST", x + w - 6, y + 8, { align: "right" });

    // Parte inferior: UR, UP, CT (tres columnas)
    const bottomY = y + topH;
    const colW = w / 3;

    // UR
    textInCell(doc, `${metric.ActualUtilization}%`, x, bottomY, colW, h - topH, { fontSize: 9 });
    doc.setFontSize(8);
    doc.text("UR", x + colW / 2, bottomY + (h - topH) - 6, { align: "center" });

    // UP
    textInCell(doc, `${metric.PlannedUtilization}%`, x + colW, bottomY, colW, h - topH, { fontSize: 9 });
    doc.setFontSize(8);
    doc.text("UP", x + colW + colW / 2, bottomY + (h - topH) - 6, { align: "center" });

    // CT
    textInCell(doc, `${metric.TotalCapacity}%`, x + 2 * colW, bottomY, colW, h - topH, { fontSize: 9 });
    doc.setFontSize(8);
    doc.text("CT", x + 2 * colW + colW / 2, bottomY + (h - topH) - 6, { align: "center" });
}

/**
 * Metodo para impresion simple
 */
export function printWholePage() {
    window.print();
}
export async function exportSchedulerPdfFromModel(model, options) {
    // model:
    // {
    //   title: "Yard Scheduler",
    //   start: "2025-07-31T11:00:00Z",       // hora inicial del rango visible
    //   hours: 48,                           // cuántas horas dibujar
    //   resources: [
    //     { id: 1, name: "Dock 1", isGroup: true, isBlock: false, resourceType: "in/out/none" },
    //     { id: 2, name: "Stage 1A", groupId: 1, isGroup: false, ... },
    //     ...
    //   ],
    //   metrics: [ // por hora y recurso
    //     { resourceId: 2, dateTime: "2025-07-31T12:00:00Z", totalSaturation: 64, actualUtilization: 71, plannedUtilization: 93, totalCapacity: 85 },
    //     ...
    //   ]
    // }

    const jsPDF = getJsPdfCtor();
    const doc = new jsPDF({ orientation: (options?.orientation ?? "landscape"), unit: "pt", format: (options?.format ?? "a4") });

    // Config layout
    const margin = options?.marginPt ?? 24;
    const pageW = doc.internal.pageSize.getWidth();
    const pageH = doc.internal.pageSize.getHeight();

    const title = options?.title ?? model?.title ?? "Scheduler";
    const headerH = 24;

    // Área de grid
    const gridX = margin;
    const gridY = margin + headerH + 6;
    const gridW = pageW - margin * 2;
    const gridH = pageH - margin * 2 - headerH - 16;

    // Definir timeline
    const start = new Date(model.start);
    const hoursTotal = model.hours ?? 24;

    // Columnas (horas)
    let hoursPerPage = hoursTotal;
    // Si quieres limitar ancho por página (ej: 12 horas por página)
    if (options?.hoursPerPage && options.hoursPerPage < hoursTotal) {
        hoursPerPage = options.hoursPerPage;
    }

    const cellW = gridW / hoursPerPage;
    const rowH = options?.rowHeight ?? 48;

    // Paginación vertical por recursos
    const resources = model.resources ?? [];
    const metrics = model.metrics ?? [];

    // Función para buscar métrica por recurso+hora
    function findMetric(resourceId, date) {
        const hourKey = new Date(date.getUTCFullYear(), date.getUTCMonth(), date.getUTCDate(), date.getUTCHours(), 0, 0);
        // Si tus fechas vienen en UTC string, normaliza:
        const keyISO = new Date(Date.UTC(hourKey.getFullYear(), hourKey.getMonth(), hourKey.getDate(), hourKey.getHours(), 0, 0)).toISOString();
        return metrics.find(m => m.resourceId === resourceId && (new Date(m.dateTime)).toISOString() === keyISO);
    }

    // Cabecera (título)
    doc.setFontSize(12);
    doc.setFont(undefined, "bold");
    doc.text(title, margin, margin + 12);
    doc.setFont(undefined, "normal");

    // Pintar por “páginas horizontales” (si hay más horas que hoursPerPage)
    const horizontalPages = Math.ceil(hoursTotal / hoursPerPage);

    let resIndex = 0;
    function drawHeaderHours(pageIndex) {
        // Encabezados de horas
        const baseHour = pageIndex * hoursPerPage;
        doc.setFontSize(9);
        for (let i = 0; i < hoursPerPage; i++) {
            const x = gridX + i * cellW;
            const hDate = new Date(start.getTime() + (baseHour + i) * 3600 * 1000);
            const label = hDate.toISOString().substring(11, 16) /* "HH:MM" */ + " " + hDate.toISOString().substring(0, 10); // simple
            doc.text(label, x + cellW / 2, gridY - 6, { align: "center" });

            // líneas verticales
            doc.setDrawColor(230, 230, 230);
            doc.line(x, gridY, x, gridY + gridH);
        }
        // última línea derecha
        doc.line(gridX + hoursPerPage * cellW, gridY, gridX + hoursPerPage * cellW, gridY + gridH);

        // línea superior e inferior de la grilla
        doc.setDrawColor(180, 180, 180);
        doc.line(gridX, gridY, gridX + hoursPerPage * cellW, gridY);
        doc.line(gridX, gridY + gridH, gridX + hoursPerPage * cellW, gridY + gridH);
    }

    for (let hp = 0; hp < horizontalPages; hp++) {
        if (hp > 0) doc.addPage();
        drawHeaderHours(hp);

        let y = gridY;
        let rowsPerPage = Math.floor(gridH / rowH);

        while (rowsPerPage-- > 0 && resIndex < resources.length) {
            const res = resources[resIndex++];

            // Nombre recurso en la izquierda (sobre la grilla)
            doc.setFontSize(9);
            doc.text(res.name ?? `#${res.id}`, gridX - 6, y + rowH / 2 + 3, { align: "right" });

            // borde especial si está bloqueado (left bar)
            if (res.isBlock) {
                doc.setDrawColor(226, 67, 67);
                doc.setLineWidth(3);
                doc.line(gridX, y, gridX, y + rowH);
                doc.setLineWidth(1);
            }

            // Dibujar celdas por hora visibles en esta página
            const baseHour = hp * hoursPerPage;
            for (let i = 0; i < hoursPerPage; i++) {
                const x = gridX + i * cellW;
                const hDate = new Date(start.getTime() + (baseHour + i) * 3600 * 1000);
                const m = findMetric(res.id, hDate);
                if (m) {
                    // Estructura esperada de 'm':
                    // { totalSaturation, actualUtilization, plannedUtilization, totalCapacity }
                    drawSaturationCell(doc, x, y, cellW, rowH, {
                        TotalSaturation: m.totalSaturation ?? m.TotalSaturation,
                        ActualUtilization: m.actualUtilization ?? m.ActualUtilization,
                        PlannedUtilization: m.plannedUtilization ?? m.PlannedUtilization,
                        TotalCapacity: m.totalCapacity ?? m.TotalCapacity
                    });
                } else {
                    // celda vacía con borde suave
                    doc.setDrawColor(230, 230, 230);
                    doc.rect(x, y, cellW, rowH, "S");
                }
            }

            // línea horizontal de fila
            doc.setDrawColor(200, 200, 200);
            doc.line(gridX, y + rowH, gridX + hoursPerPage * cellW, y + rowH);
            y += rowH;
        }

        // Footer simple
        doc.setFontSize(8);
        const nowText = new Date().toLocaleString();
        doc.text(nowText, margin, pageH - 8);
        doc.text(`${doc.internal.getNumberOfPages()}`, pageW - margin, pageH - 8, { align: "right" });
    }

    // (Opcional) si quieres anexar una tabla-resumen usando autoTable:
    // if (options?.table) {
    //   doc.addPage();
    //   doc.autoTable({
    //     head: options.table.head,
    //     body: options.table.body,
    //     startY: margin,
    //     margin: { left: margin, right: margin },
    //     theme: "grid",
    //     styles: { fontSize: 9, cellPadding: 3 }
    //   });
    // }

    doc.save(options?.filename ?? "scheduler.pdf");
}

export async function exportElementToPdfByHours(elementOrId, options) {
    const {
        filename = "export.pdf",
        format = "a4",
        orientation = "landscape",
        marginPt = 24,
        scale = 4,
        fitWidth = true,
        addFooterPageNumbers = true,
        addFooterDate = true,
        backgroundColor = "#FFFFFF",
        horasPorHoja = 8,         // <---- NUEVO parámetro configurable
        totalHoras = 24           // <---- Asume 24h, ajusta si tu scheduler tiene menos/más
    } = options || {};

    const el = getElement(elementOrId);
    if (!el) throw new Error("exportElementToPdfByHours: elemento no encontrado");

    const html2canvas = getHtml2Canvas();
    const jsPDF = getJsPdfCtor();

    const orig = { width: el.style.width, height: el.style.height, overflow: el.style.overflow };

    el.style.width = "min-content";
    const fullWidth = el.scrollWidth;
    const fullHeight = el.scrollHeight;

    try {
        el.style.width = `${fullWidth}px`;
        el.style.height = `${fullHeight}px`;
        el.style.overflow = "visible";

        await new Promise(r => requestAnimationFrame(r));

        const canvas = await html2canvas(el, {
            backgroundColor,
            scale,
            useCORS: true,
            allowTaint: true,
            logging: false,
            windowWidth: fullWidth,
            windowHeight: fullHeight
        });

        // DEBUG: inspeccionar el canvas resultante
        console.log('canvas px:', canvas.width, 'x', canvas.height);
        const dbg = window.open('', '_blank');
        if (dbg) {
            const img = new Image();
            img.src = canvas.toDataURL('image/png');
            img.style.maxWidth = 'none';
            img.style.display = 'block';
            dbg.document.body.style.margin = '0';
            dbg.document.body.appendChild(img);
        }


        const doc = new jsPDF({ orientation, unit: "pt", format });
        const pageW = doc.internal.pageSize.getWidth();
        const pageH = doc.internal.pageSize.getHeight();

        const pxToPt = 72 / 96;
        const imgWpx = canvas.width;
        const imgHpx = canvas.height;

        // ---- CÁLCULO DE CORTES HORIZONTALES (POR HORAS) ----
        const pxPorHora = imgWpx / totalHoras;
        const anchoCortePx = pxPorHora * horasPorHoja;
        const paginas = Math.ceil(totalHoras / horasPorHoja);

        const imgHpt = imgHpx * pxToPt;
        const maxH = pageH - marginPt * 2;

        // Ajuste para fitWidth en horizontal
        const maxW = pageW - marginPt * 2;

        const addFooter = (pageIndex) => {
            doc.setFontSize(9);
            if (addFooterDate) {
                const now = new Date().toLocaleString();
                doc.text(now, marginPt, pageH - 8);
            }
            if (addFooterPageNumbers) {
                doc.text(`${pageIndex}`, pageW - marginPt, pageH - 8, { align: "right" });
            }
        };

        for (let i = 0; i < paginas; i++) {
            const sx = Math.floor(i * anchoCortePx);

            // Crear un canvas temporal para el segmento
            const slice = document.createElement("canvas");
            slice.width = Math.min(anchoCortePx, imgWpx - sx);
            slice.height = imgHpx;

            const ctx = slice.getContext("2d");
            ctx.drawImage(
                canvas,
                sx, 0,                             // sx, sy: inicio del recorte
                slice.width, imgHpx,               // sw, sh: tamaño del recorte
                0, 0,                              // dx, dy: inicio en el canvas destino
                slice.width, imgHpx                // dw, dh: tamaño en el canvas destino
            );

            const imgData = slice.toDataURL("image/png");
            let sliceWpt = slice.width * pxToPt;
            let sliceHpt = imgHpx * pxToPt;

            // Ajustar a fitWidth si es necesario
            if (fitWidth && sliceWpt > maxW) {
                const ratio = maxW / sliceWpt;
                sliceWpt = maxW;
                sliceHpt = sliceHpt * ratio;
            }

            if (i > 0) doc.addPage();
            doc.addImage(imgData, "PNG", marginPt, marginPt, sliceWpt, sliceHpt);

            addFooter(i + 1);
        }

        doc.save(filename);
    } finally {
        // Restaurar estilos
        el.style.width = orig.width;
        el.style.height = orig.height;
        el.style.overflow = orig.overflow;
    }
}

async function captureElementInVerticalTiles(el, {
    fullWidth,
    fullHeight,
    tileHeightPx = 4000,
    scale = 2,
    backgroundColor = '#FFFFFF'
}) {
    const html2canvas = getHtml2Canvas();

    const orig = {
        transform: el.style.transform,
        transformOrigin: el.style.transformOrigin
    };
    el.style.transformOrigin = 'top left';

    const tiles = [];
    for (let sy = 0; sy < fullHeight; sy += tileHeightPx) {
        const h = Math.min(tileHeightPx, fullHeight - sy);

        // desplaza el contenido para que la franja [sy, sy+h) quede visible arriba
        el.style.transform = `translateY(${-sy}px)`;
        await new Promise(r => requestAnimationFrame(r));

        const tile = await html2canvas(el, {
            backgroundColor,
            scale,
            useCORS: true,
            allowTaint: true,
            logging: false,
            windowWidth: fullWidth,
            windowHeight: h
        });

        // algunos navegadores devuelven unos px extra: recortamos a h*scale
        if (tile.height !== h * scale) {
            const fixed = document.createElement('canvas');
            fixed.width = tile.width;
            fixed.height = Math.min(tile.height, h * scale);
            fixed.getContext('2d').drawImage(tile, 0, 0);
            tiles.push(fixed);
        } else {
            tiles.push(tile);
        }
    }

    // restaurar
    el.style.transform = orig.transform;
    el.style.transformOrigin = orig.transformOrigin;

    return tiles; // canvases apilables de arriba a abajo
}

export async function exportElementToPdfByHoursTiled(elementOrId, options) {
    const {
        filename = "Yard-Calendar.pdf",
        format = "a4",
        orientation = "landscape",
        marginPt = 24,
        scale = 3,
        fitWidth = true,
        addFooterPageNumbers = true,
        addFooterDate = true,
        backgroundColor = "#FFFFFF",
        horasPorHoja = 8,
        totalHoras = 24,
        tileHeightPx = 4000
    } = options || {};

    const el = getElement(elementOrId);
    if (!el) throw new Error("exportElementToPdfByHoursTiled: elemento no encontrado");

    const jsPDF = getJsPdfCtor();

    // 1) Forzar expansión del contenedor global (.mlx-scheduler) con !important
    const schedulerEl = document.querySelector('.mlx-scheduler');
    const origSchedulerInline = schedulerEl ? {
        maxHeight: schedulerEl.style.maxHeight,
        height: schedulerEl.style.height,
        overflow: schedulerEl.style.overflow
    } : null;

    if (schedulerEl) {
        schedulerEl.style.setProperty('max-height', 'none', 'important');
        schedulerEl.style.setProperty('height', 'auto', 'important');
        schedulerEl.style.setProperty('overflow', 'visible', 'important');
    }

    // 2) Preparar el elemento a capturar
    const orig = { width: el.style.width, height: el.style.height, overflow: el.style.overflow };

    // OJO: primero min-content, luego esperar, luego medir
    el.style.width = "min-content";
    await new Promise(r => requestAnimationFrame(r));

    const fullWidth = el.scrollWidth;
    const fullHeight = el.scrollHeight;

    try {
        el.style.width = `${fullWidth}px`;
        el.style.height = `${fullHeight}px`;
        el.style.overflow = "visible";

        await new Promise(r => requestAnimationFrame(r));

        // 3) Captura en tiras verticales
        const tiles = await captureElementInVerticalTiles(el, {
            fullWidth,
            fullHeight,
            tileHeightPx,
            scale,
            backgroundColor
        });

        // 4) Armar PDF (igual que antes)
        const doc = new jsPDF({ orientation, unit: "pt", format });
        const pageW = doc.internal.pageSize.getWidth();
        const pageH = doc.internal.pageSize.getHeight();
        const pxToPt = 72 / 96;

        const totalWpx = tiles[0].width;
        const pxPorHora = totalWpx / totalHoras;
        const anchoCortePx = pxPorHora * horasPorHoja;
        const paginasHoriz = Math.ceil(totalHoras / horasPorHoja);

        const footerReservePt = (addFooterDate || addFooterPageNumbers) ? 18 : 0;
        const maxW = pageW - marginPt * 2;
        const maxH = pageH - marginPt * 2 - footerReservePt;

        const addFooter = (pageIndex) => {
            if (!(addFooterDate || addFooterPageNumbers)) return;
            doc.setFontSize(9);
            const y = pageH - 8;
            if (addFooterDate) {
                const now = new Date().toLocaleString();
                doc.text(now, marginPt, y);
            }
            if (addFooterPageNumbers) {
                doc.text(`${pageIndex}`, pageW - marginPt, y, { align: "right" });
            }
        };

        let pageIndex = 0;

        for (let i = 0; i < paginasHoriz; i++) {
            const sx = Math.floor(i * anchoCortePx);
            const sliceWpx = Math.min(anchoCortePx, totalWpx - sx);

            for (let t = 0; t < tiles.length; t++) {
                const tile = tiles[t];

                const sub = document.createElement('canvas');
                sub.width = sliceWpx;
                sub.height = tile.height;

                const ctx = sub.getContext('2d');
                ctx.drawImage(tile, sx, 0, sliceWpx, tile.height, 0, 0, sliceWpx, tile.height);

                const subWpt = sub.width * pxToPt;
                const subHpt = sub.height * pxToPt;

                let ratio = fitWidth
                    ? Math.min(maxW / subWpt, maxH / subHpt)
                    : Math.min(1, maxW / subWpt, maxH / subHpt);

                const drawW = subWpt * ratio;
                const drawH = subHpt * ratio;

                if (pageIndex > 0) doc.addPage();

                if (footerReservePt > 0) {
                    doc.setFillColor(255, 255, 255);
                    doc.rect(0, pageH - (marginPt + footerReservePt), pageW, footerReservePt, 'F');
                }

                doc.addImage(sub.toDataURL("image/png"), "PNG", marginPt, marginPt, drawW, drawH);
                addFooter(++pageIndex);
            }
        }

        doc.save(filename);
    } finally {
        // Restaurar estilos del elemento capturado
        el.style.width = orig.width;
        el.style.height = orig.height;
        el.style.overflow = orig.overflow;

        // Restaurar .mlx-scheduler
        if (schedulerEl && origSchedulerInline) {
            // si tenía inline lo restauramos; si no, limpiamos
            if (origSchedulerInline.maxHeight) schedulerEl.style.maxHeight = origSchedulerInline.maxHeight; else schedulerEl.style.removeProperty('max-height');
            if (origSchedulerInline.height) schedulerEl.style.height = origSchedulerInline.height; else schedulerEl.style.removeProperty('height');
            if (origSchedulerInline.overflow) schedulerEl.style.overflow = origSchedulerInline.overflow; else schedulerEl.style.removeProperty('overflow');
        }
    }
}

/**
 * Excel combinado a partir del modelo (una sola tabla):
 *  - Metadatos: title, startUtc, hours
 *  - Resource: resourceId, resourceName, isGroup, isBlock, resourceType
 *  - Métricas: dateTime, totalSaturation, actualUtilization, plannedUtilization, totalCapacity
 */
export async function exportSchedulerExcelGridFromModel(model, {
    filename = "Yard-Calendar.xlsx",
    hours = model?.hours ?? 24,
    start = model?.start,
    headerEvery = 1,
    timeMode = "",          // ya alineado a UTC como acordamos
    cellFormatter,
} = {}) {
    if (!window.ExcelJS) { console.error("ExcelJS no está disponible."); return false; }
    const ExcelJS = window.ExcelJS;

    const resources = Array.isArray(model?.resources) ? model.resources : [];
    const metrics = Array.isArray(model?.metrics) ? model.metrics : [];

    // --- NUEVO: encabezados extra ---
    const warehouseName =
        model?.warehouseName ??
        model?.WarehouseName ?? "";

    // admite varias claves para los totales
    const totals = model?.totals ?? null;

    // helpers: UTC consistente
    const asDate = (v) => (v instanceof Date ? v : new Date(v));
    const startDate = asDate(start);

    const asLocalMetricDate = (v) => {
        if (typeof v === "string" && v.endsWith("Z")) {
            // "2025-08-14T17:00:00Z" -> "2025-08-14T17:00:00" (local)
            return new Date(v.slice(0, -1));
        }
        return asDate(v); // ya local o Date
    };

    const addHours = (d, h) => {
        const t = new Date(d.getTime());
        if (timeMode === "utc") t.setUTCHours(t.getUTCHours() + h);
        else t.setHours(t.getHours() + h);
        return t;
    };

    const hourLabel = (d) => {
        let hr = (timeMode === "utc") ? d.getUTCHours() : d.getHours();
        const ampm = hr >= 12 ? "pm" : "am";
        hr = hr % 12; if (hr === 0) hr = 12;
        return `${hr}${ampm}`;
    };

    const toHourIdx = (dt) => {
        const t = asLocalMetricDate(dt);
        const diffMs = t.getTime() - startDate.getTime();
        return Math.floor(diffMs / 3_600_000); // 1h en ms
    };

    // === MAPEO resourceId|hour => metric(s) ===
    const cellMap = new Map();                             // <-- si quieres acumular varias por celda:
    for (const m of metrics) {                             //     usa array y concat; aquí dejamos 1:1
        const idx = toHourIdx(m.dateTime);
        if (idx >= 0 && idx < hours) cellMap.set(`${m.resourceId}|${idx}`, m);
    }

    // === Excel ===
    const wb = new ExcelJS.Workbook();
    const ws = wb.addWorksheet("Grid", { properties: { tabColor: { argb: "FFE8F4FF" } } });

    // Definir columnas (A: recurso, B..: horas)
    const columns = [{ header: "", key: "_resource_", width: 22 }];
    for (let h = 0; h < hours; h++) {
        const d = addHours(startDate, h);
        const header = (h % headerEvery === 0) ? hourLabel(d) : "";
        columns.push({ header, key: `h${h}`, width: 10 });
    }
    ws.columns = columns;

    // --- NUEVO: insertar 2 filas arriba para encabezados personalizados ---
    // Insertamos 2 filas en la parte superior; el header de horas (que ExcelJS
    // pone en la fila 1) se desplaza a la fila 3 automáticamente.
    ws.spliceRows(1, 0, new Array(ws.columnCount).fill(null));
    ws.spliceRows(1, 0, new Array(ws.columnCount).fill(null));

    const lastCol = ws.columnCount;

    // Fila 1: WarehouseName fusionado
    if (warehouseName) {
        ws.mergeCells(1, 1, 1, lastCol);
        const t1 = ws.getCell(1, 1);
        t1.value = warehouseName;
        t1.font = { bold: true, size: 14 };
        t1.alignment = { horizontal: "center", vertical: "middle" };
        t1.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFEFF5FF" } };
    }

    // Fila 2: Totales (DataTotalStages) fusionado como texto compacto
    if (totals) {
        // si vienen ya como strings TotalSaturation/ActualUtilization/etc., úsalos;
        // si no, arma porcentajes con ST/UR/UP/CT numéricos
        const stLabel = totals.totalSaturation ?? (totals.st != null ? `${totals.st}%` : null);
        const urLabel = totals.actualUtilization ?? (totals.ur != null ? `${totals.ur}%` : null);
        const upLabel = totals.plannedUtilization ?? (totals.up != null ? `${totals.up}%` : null);
        const ctLabel = totals.totalCapacity ?? (totals.ct != null ? `${totals.ct}%` : null);

        const summary = [
            stLabel ? `ST: ${stLabel}` : null,
            urLabel ? `UR: ${urLabel}` : null,
            upLabel ? `UP: ${upLabel}` : null,
            ctLabel ? `CT: ${ctLabel}` : null
        ].filter(Boolean).join("   •   ");

        ws.mergeCells(2, 1, 2, lastCol);
        const t2 = ws.getCell(2, 1);
        t2.value = summary;
        t2.font = { italic: true, color: { argb: "FF444444" } };
        t2.alignment = { horizontal: "center", vertical: "middle" };
        t2.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFF8F8F8" } };
    }

    // Estilos del header de horas (ahora en fila 3)
    const headerRow = ws.getRow(3);
    headerRow.font = { bold: true };
    headerRow.alignment = { vertical: "middle", horizontal: "center" };
    headerRow.fill = { type: "pattern", pattern: "solid", fgColor: { argb: "FFF2F2F2" } };

    // Filtros y congelado (3 filas fijas: nombre + totales + horas)
    ws.views = [{ state: "frozen", xSplit: 1, ySplit: 3 }];
    ws.autoFilter = { from: { row: 3, column: 1 }, to: { row: 3, column: ws.columnCount } };

    // Escribir datos desde fila 4
    let rowIdx = 4;
    const defaultFormatter = (m) => {
        if (!m) return "";
        const parts = [];
        if (m.totalSaturation != null) parts.push(`ST:${m.totalSaturation}`);
        if (m.actualUtilization != null) parts.push(`UR:${m.actualUtilization}`);
        if (m.plannedUtilization != null) parts.push(`Up:${m.plannedUtilization}`);
        if (m.totalCapacity != null) parts.push(`CT:${m.totalCapacity}`);
        return parts.join(", ");
    };
    const fmt = cellFormatter || defaultFormatter;

    for (const r of resources) {
        const row = ws.getRow(rowIdx++);
        row.getCell(1).value = r.name ?? r.id ?? "";
        row.getCell(1).font = { bold: true };

        for (let h = 0; h < hours; h++) {
            const key = `${r.id}|${h}`;
            const m = cellMap.get(key);
            row.getCell(h + 2).value = fmt(m) || null;
        }
    }

    // Bordes y wrap
    const thin = { style: "thin", color: { argb: "FFDDDDDD" } };
    for (let r = 1; r <= ws.rowCount; r++) {
        for (let c = 1; c <= ws.columnCount; c++) {
            const cell = ws.getCell(r, c);
            cell.border = { top: thin, left: thin, bottom: thin, right: thin };
            cell.alignment = { ...(cell.alignment || {}), wrapText: true, vertical: "top" };
        }
    }

    // Descargar
    const buffer = await wb.xlsx.writeBuffer();
    const blob = new Blob([buffer], { type: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url; a.download = filename; a.style.display = "none";
    document.body.appendChild(a); a.click(); document.body.removeChild(a);
    setTimeout(() => URL.revokeObjectURL(url), 0);

    return true;
}
