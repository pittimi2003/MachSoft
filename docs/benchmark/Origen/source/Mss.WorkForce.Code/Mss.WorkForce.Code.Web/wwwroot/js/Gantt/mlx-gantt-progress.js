/**
 * Version 1.0.1
 **/

import * as functGantt from './mlx-gantt.js';
import { TaskSegment } from './ObjectGantt/MainSegment.js'


export function getTaskProgress(taskData, withProgress, start, end) {
    try {
        if (!taskData || !functGantt.ReadyLoadGantt()) return;

        switch (taskData.TooltipType) {
            case "PlanningGeneral":
            case "PlanningFlow":
            case "PlanningOrder":
            case "PlanningActivity":
            case "PlanningPriority":
            case "PlanningEstimation":
            case "PlanningTrailer":
            case "PlanningEstimationIn":
            case "PlanningEstimationInOrder":
            case "PlanningEstimationOut":
            case "PlanningEstimationOutOrder":
            case "PlanningWarehouse":
            case 'PlanningWarehouseOrder':
            case "LaborEquipmentGeneral":
            case "LaborEquipmentOrder":
            case "LaborWorkerGeneral":
            case "LaborWorkerOrder":
            case "YardOrder":
                return drawBarProgressSingle(taskData, withProgress, start, end);
                break;
            case "LaborEquipmentActivity":
            case "LaborWorkerActivity":
                return drawBarProgressParentTaskEquipments(taskData, withProgress);
                break;
            case "YardGeneral":
                return drawBarProgressSegmentGroups(taskData, withProgress);
                break;
        }

    } catch (error) {
        console.log(error);
    }
}

// segmentos en grupo, para hacer un solo grupo con los segmentos empalmados
function drawBarProgressSegmentGroups(taskData, withProgress) {
    if (taskData != undefined) {
        let orderSegments = [];

        if (taskData.segments != null) {
            taskData.segments = taskData.segments.sort((a, b) => a.start - b.start);
            orderSegments = generateSegments(taskData.segments);
        }
        return getDrawSegments(taskData, orderSegments, withProgress);
    }
}

function generateGroupsSegments(segments) {
    const blocks = [];
    let currentBlock = [];

    for (let i = 0; i < segments.length; i++) {
        const current = segments[i];
        const prev = segments[i - 1];
        if (i === 0 || prev.end >= current.start) {
            currentBlock.push(new TaskSegment(current.id, current.start, current.end, current.progress, current.worktime));
        } else {
            blocks.push([...currentBlock]);
            currentBlock = [new TaskSegment(current.id, current.start, current.end, current.progress, current.worktime)];
        }

    }

    // Agregar el último grupo
    if (currentBlock.length > 0) {
        blocks.push(currentBlock);
    }

    return blocks;
}

function generateSegments(segments) {
    const segmentGroups = generateGroupsSegments(segments);
    const bloques = segmentGroups.map(group => {
        const start = Math.min(...group.map(item => item.start))
        const end = Math.max(...group.map(item => item.end));;
        const progress = getProgressLabor(group);
        return {
            start,
            end,
            segments: group,
            progress
        };
    });
    return bloques;
}


// segmentos individuales, se visualiza segmento por segmento
function drawBarProgressParentTaskEquipments(taskData, withProgress) {
    if (taskData != undefined) {
        let orderSegments = [];

        if (taskData.segments != null) {
            orderSegments = taskData.segments.sort((a, b) => a.start - b.start);
        }

        return getDrawSegments(taskData, orderSegments, withProgress);
    }
}

function getDrawSegments(dataTask, dataSegments, withProgress) {
    const taskType = functGantt.getTaskType(dataTask.TooltipType);

    dataTask = FillDateTime(dataTask);

    return drawSegments(dataTask, dataSegments, withProgress, taskType);
}

function FillDateTime(dataTask) {
    if (dataTask.start == null && dataTask.end == null) {
        dataTask.start = dataTask.segments[0].start;
        dataTask.end = dataTask.end = dataTask.segments[dataTask.segments.length - 1].end;
    }

    return dataTask;
}

function drawSegments(task, segments, withProgress, SizeTaskType) {
    const totalDuration = task.end - task.start;
    let html = "";

    if (segments == null)
        segments.push(task);

    segments.forEach((seg, index) => {
        const segStart = seg.start;
        const segEnd = seg.end;
        const segDuration = segEnd - segStart;
        const segmentWidth = Math.max((segDuration / totalDuration) * withProgress, 5); // mínimo visible
        const offset = ((segStart - task.start) / totalDuration) * withProgress;
        const progress = (seg.progress / 100) * segmentWidth;
        const color = hasOverlappingSegments(seg.segments) ? task.color : "var(--sp-color-alert-high)";
        const z = 1000 + index; // z-index único por segmento

        html += `<div class="task-container" style="
            position: absolute;
            left: ${offset}px;>
            <div class="task-process-Container">
                <div class="task-progress-Base task-progress-Gantt ${SizeTaskType}" style="width: ${segmentWidth}px; background-color: ${color};"></div>
                <div class="task-progress-Gantt task-bar-progress" style="width: ${progress}px; background-color: ${color};">  </div>
            </div>
        </div>`;
    });

    return html;
}

export function getProgressLabor(segments) {
    if (segments != null) {
        const progress = segments.reduce((sum, x) => sum + x.progress, 0);
        const totalProgress = progress / segments.length;

        return totalProgress;
    }
}
export function getProgressHigthTask(taskData) {
    const childrens = functGantt.getTaskChildren(taskData.id);

    if (!childrens.length) {
        return taskData.progress || 0;
    }
    
    let totalProgress = childrens.reduce((sum, child) => {
        return sum + getProgressHigthTask(child);
    }, 0);
    
    return totalProgress / childrens.length || 0;
}


export function getProgress(taskData) {
    let progressTotal = taskData.FillProgress ? getProgressHigthTask(taskData) : taskData.progress;

    return progressTotal;
}

// ✅ Renderiza una sola barra como antes
export function drawBarProgressSingle(taskData, withProgress, start, end) {
    try {
        if (functGantt.ReadyLoadGantt()) {
            if (taskData != undefined) {
                const taskType = functGantt.getTaskType(taskData.TooltipType);
                const progressTotal = Math.round((getProgress(taskData) / 100) * withProgress);
                const marginForResource = withProgress + 10;
                let bgColor = taskData.color;

                // Si la propiedad 'color' no esta definida, se establece por defecto --gl-color-smoke-800
                if (!bgColor)
                    bgColor = "#455D75";


                return `<div class="task-container style="width: 100px;">
                <div class="task-process-Container">
                    <div class="task-progress-Base task-progress-Gantt ${taskType}" style="width: ${withProgress}px; background-color: ${bgColor};"></div>
                    <div class="task-progress-Gantt task-bar-progress" style="width: ${progressTotal}px; background-color: ${bgColor};">  </div>

                </div>
                    ${taskData.TooltipType == 'LaborWorkerGeneral' ? drawBreak(taskData) : ``}                    
                    ${taskData.TooltipType == 'PlanningFlow' ? drawIconOutIn(taskData, positionIconOrder(), withProgress) : ``}
                    ${taskType == 'LevelAct' ? drawResource(marginForResource, taskData.Resource) : ``}
                    ${taskData.TooltipType == 'PlanningOrder' && taskData.CommittedHour != null ? pinterIcons(taskData, withProgress, start, end) : ``}
                    ${taskData.TooltipType == 'YardOrder' && taskData.CommintedDate != null ? drawClock(taskData, start, end) : ``}

            </div>`;
            }
        }
    }
    catch (error) {
        console.log(error);
    }
}


export function drawResource(marginForResource, resource) {
    if (resource) {
        return ` <div class="task-container-resource" style="margin-left: ${marginForResource}px;">
                    <div class="resources-text" >${resource}</div>
                </div>`
    }

    return ` <div></div>`;
}

export function drawCommitment(taskData) {
    const mlxIconMilestone = taskData.isOnTime ? "mlx-ico-milestone-with-border" : "mlx-ico-milestone-exceeded";
    return `<span class="${mlxIconMilestone}" style="position: absolute; left:${positionCommited}px"></span>`;
}


export function drawClock(taskData, start, end) {
    if (start && end) {
        const commitmentPosition = drawPointDate(start, end) - 15;
        const mlxIconMilestone = taskData.isOnTime ? "mlx-ico-clock-2-on-time" : "mlx-ico-clock-2-off-time";
        return `<span class="${mlxIconMilestone}" style="position: absolute; left:${commitmentPosition}px"></span>`;
    }
}


export function drawBloqued(taskData, icon) {
    if (taskData != undefined) {
        const taskType = functGantt.getTaskType(taskData.TooltipType);
        if (icon === undefined) return '';
        if (taskData.EndBlockDate === null) {
            return `<span class="${icon}" style="position: absolute; left:${18}px"></span>`;
        }
        const blockedPosition = taskType == 'LevelOrder' ? drawblock(taskData) : "";
        return `<span class="${icon}" style="position: absolute; left:${blockedPosition == 0 ? 18 : blockedPosition}px"></span>`;
    }
}

export function getResourcesForTask(resources) {
    return resources.map(res => res.text).join(", ");
}


// Se cambia esta funcion para intentar arreglar el commited
export function drawPoint(dateAppointment, start) {
    let commitmentPosition = 0;

    if (!dateAppointment) return 0;;

    const vehicleDate = parseAsLocal(dateAppointment);

    const committedMinutes = vehicleDate.getTime() / 60000;
    const startMinutes = start.getTime() / 60000;

    const pixelsPerMinute = updatePixelsPerHour();

    commitmentPosition = (committedMinutes - startMinutes) * pixelsPerMinute;

    return commitmentPosition;
}


//Elimina la zona horaria
function parseAsLocal(dateString) {
    const clean = dateString.replace(/([+-]\d{2}:\d{2}|Z)$/i, '');
    return new Date(clean);
}

//Devuelve una posicion basado en 2 fecha
export function drawPointDate(InitDate, EndDate) {

    const commitHour = timeToMinutes(EndDate);
    const startMinutes = timeToMinutes(InitDate);
    const pixelsPerMinute = updatePixelsPerHour();
    return (commitHour - startMinutes) * pixelsPerMinute;
    //return  pixelsPerMinute;
}


// Se agrega esta funcion para intentar incluir el candado
export function drawblock(task) {
    let blockedPosition = 0;

    if (task.EndBlockDate !== undefined) {
        const blockHour = getHour(task.EndBlockDate);
        const startMinutes = timeToMinutes(task.start);
        const blockMinutes = timeToMinutes(blockHour);
        const withColum = getWithColumn(task.EndBlockDate) / 60;

        // Distancia desde el inicio de la tarea hasta CommittedHour
        blockedPosition = (blockMinutes - startMinutes) * withColum;
    }

    return blockedPosition;
}

export function updatePixelsPerHour() {
    const columns = document.querySelectorAll('.dx-gantt-si');
    let withCol = 0;
    const instanceGantt = functGantt.getIntance();

    if (!instanceGantt) { return 1; }

    const scale = instanceGantt.option('scaleType');

    if (columns.length > 1) {
        const lastCol = columns[columns.length - 1];
        withCol = lastCol.clientWidth;
        switch (scale) {
            case 'hours':
                withCol = withCol / 60;
                break;
            case 'sixHours':
                withCol = withCol / 360;
                break;
            case 'minutes':
                withCol = withCol / 10;
                break;
        }
    }

    return withCol || 1;
}


export function getWithColumn(committedHour) {
    const columns = document.querySelectorAll('.dx-gantt-si');
    const normalizedHour = normalizeToHour(committedHour);
    let withCol = 0;
    columns.forEach(item => {
        if (item.innerText.includes(normalizedHour)) {
            withCol = item.clientWidth;
        }
    });

    return withCol;
}

export function normalizeToHour(timeString) {
    try {
        const [time, period] = timeString.split(' ');
        let [hours] = time.split(':').map(Number);
        return `${hours}:00 ${period}`;
    }
    catch (error) {
        console.log("Error in method => normalizeToHour()");
    }
}

export function timeToMinutes(date) {
    try {
        date = date ?? new Date();
        return date.getHours() * 60 + date.getMinutes();
    }
    catch (error) {
        console.log("Error in method => timeToMinutes() ");
    }
}

export function timeToMinutesUTC(date) {
    try {
        date = date ?? new Date();
        return date.getUTCHours() * 60 + date.getUTCMinutes();
    }
    catch (error) {
        console.log("Error in method => timeToMinutesUTC() ");
    }
}

export function getHour(committedHour) {
    try {
        const [time, period] = committedHour.split(" ");
        let [hours, minutes] = time.split(":").map(Number);

        if (period === "PM" && hours !== 12) {
            hours += 12;
        } else if (period === "AM" && hours === 12) {
            hours = 0;
        }

        return new Date(1970, 0, 1, hours, minutes);
    }
    catch (error) {
        console.log("Error to getHour()");
    }
}

export function drawBreak(taskData) {
    if (!taskData.breaksGantts || taskData.breaksGantts.length === 0) return '';

    var taskChildrens = functGantt.getTaskChildren(taskData.id);
    let earliestDate = null;

    taskChildrens.forEach(child => {
        const taskDoubleChilds = functGantt.getTaskChildren(child.id);
        taskDoubleChilds.forEach(grandChild => {
            const grandChildStart = new Date(grandChild.start);
            if (!earliestDate || grandChildStart < earliestDate) {
                earliestDate = grandChildStart;
            }
        });
    });

    if (!earliestDate) {
        earliestDate = new Date();
    }

    const breaksHTML = taskData.breaksGantts.map(x => {
        const startMinutes = timeToMinutes(earliestDate);
        const initBreakMinutes = timeToMinutes(x.InitBreak);
        const endBreakMinutes = timeToMinutes(x.EndBreak);
        const breakDuration = endBreakMinutes - initBreakMinutes;
        const pixelsPerMinute = updatePixelsPerHour();
        const breakLeft = (initBreakMinutes - startMinutes) * pixelsPerMinute;
        const breakWidth = breakDuration * pixelsPerMinute;

            return `
                <div class="task-break-overlay"
                    style="left: ${breakLeft}px; width: ${breakWidth}px;">
                    <span class="mlx-ico-break-at-work"></span>
                </div>`;
        }).join('');

    return breaksHTML;
}

function hasOverlappingSegments(segments) {
    if (!Array.isArray(segments)) return true;

    const sortedSegments = segments
        .filter(s => s.start && s.end)
        .sort((a, b) => new Date(a.start) - new Date(b.start));

    for (let i = 0; i < sortedSegments.length - 1; i++) {
        const current = sortedSegments[i];
        const next = sortedSegments[i + 1];

        // Si la fecha de fin del actual es mayor a la de inicio del siguiente, hay solapamiento
        if (new Date(current.end) > new Date(next.start)) {
            return false;
        }
    }

    return true;
}


let positionCommited = 0;
let positionIconOutIn = 0;
let positionBlock = 0;
function pinterIcons(taskData, size, start, end) {
    let iconBlock = '';
    positionCommited = drawPoint(taskData.VehicleDate, start);
    positionIconOutIn = positionIconOrder();

    if (taskData.isBlock) {
        positionBlock = positionBLockOrder();
        iconBlock = drawBlockOnTask(taskData, size, start, end);
    }

    const iconCommited = drawCommitment(taskData);
    const iconOutIn = drawIconOutIn(taskData, positionIconOutIn, size);

    return iconCommited + iconOutIn + iconBlock;
}

function drawIconOutIn(taskData, positon, size) {
    const color = getColorIconOutIn(taskData, size) ? "" : "color"; //por el momento no se ocupa porque la felcha siempre tiene color
    const mlxIconMilestone = taskData.IsOutbound ? 'mlx-ico-salida' : 'mlx-ico-entrada';
    return ` <span class="${mlxIconMilestone} color task-bar-progress" style="padding-top: 2px; left:${positon}px"> </span>`;
}

function positionIconOrder() {
    let position = -20;
    if (nearIcons(position, positionCommited))
        position = positionCommited - 20;

    return position
}

function positionBLockOrder() {
    let positionDrawBlock = -40;

    if (nearIcons(positionDrawBlock, positionCommited))
        positionDrawBlock = positionCommited - 20;

    if (nearIcons(positionIconOutIn, positionCommited))
        positionIconOutIn = positionCommited - 20;

    if (nearIcons(positionIconOutIn, positionDrawBlock))
        positionDrawBlock = positionIconOutIn - 20;


    return positionDrawBlock;
}

function drawBlockOnTask(taskData, size, start, end) {
    if (start && end) {
        const mlxIconMilestone = taskData.isBlock ? !isTheSmallSize(size) ? "mlx-ico-lock-close-light" : "mlx-ico-lock-close" : ""; // por el momento no se ocupa porque el candado será de un color
        return `<span class="mlx-ico-lock-close" style="position: absolute; left:${positionBlock}px"></span>`;
    }
}

function nearIcons(iconA, iconB, range = 15) {
    return Math.abs(iconA - iconB) <= range;
}


// indica si es commited esta dentro del rango de la orden o fuera de la barrra
function isDateInRange(dateToCheck, startDate, endDate) {
    const start = new Date(startDate);
    const end = new Date(endDate);
    const check = new Date(dateToCheck);
    if (isNaN(start) || isNaN(end) || isNaN(check)) return false;
    return check >= start && check <= end;
}

// obtiene el color del icono de la orden para que se visualice correctamente
function getColorIconOutIn(taskData, size) {
    const spaceAfter = taskData.IsOutbound ? size + positionIconOutIn : size - positionIconOutIn;
    if (spaceAfter < 12)
        return false;
    return true;
}

function adjustPosition(referencia, isOutbound, offset = 20) {
    return isOutbound ? referencia - offset : referencia + 10;
}

// indica el tamaño minimo de la barra al realizar zoom para poder hacer los cálculos
function isTheSmallSize(size) {
    return size <= 64;
}

// posiciona el icono de la orden (flecha) dependiendo de la orden y el zoom
// si es entrada, al inicio de la barra, si es salida al final de la barra

//function positionIconOrder(taskData, size) {
//    const isCommitedOutside = isDateInRange(taskData.CommintedDate, taskData.StartDate, taskData.EndDate);

//    const spaceAfter = size - positionCommited;
//    const sizeTask = size < 20 ? + size : 20;

//    let position = 0;
//    if (isCommitedOutside && size < 22) {
//        position = taskData.IsOutbound ? - positionCommited : positionCommited;
//    }

//    if (isCommitedOutside && !isNaN(spaceAfter)) {
//        const space = spaceAfter >= 30;
//        position += taskData.IsOutbound
//            ? (space ? size - 20 : positionCommited - sizeTask)
//            : (space ? 10 : positionCommited + spaceAfter);

//    } else
//        position += taskData.IsOutbound ? size - 20 : 10;

//    if (size <= 20)
//        position += taskData.IsOutbound ? -5 : 5;

//    return position;
//}

// posiciona el icono de bloqueo en base al tipo de orden, al zoom y a las posiciones de los otros iconos

//function positionBLockOrder(taskData, size) {
//    let position = isTheSmallSize(size) ? -10 : 20;
//    let positionDrawBlock = drawPointDate(taskData.start, taskData.end) - position;

//    if (nearIcons(positionDrawBlock, positionCommited))
//        if (isTheSmallSize(size))
//            positionDrawBlock += 5;
//        else if (taskData.IsOutbound)
//            positionDrawBlock = positionCommited - 20;

//    if (nearIcons(positionIconOutIn, positionCommited))
//        positionIconOutIn = adjustPosition(positionCommited, taskData.IsOutbound);

//    if (nearIcons(positionIconOutIn, positionDrawBlock)) {
//        if (taskData.IsOutbound) {
//            positionIconOutIn = positionDrawBlock - 20;
//            if (nearIcons(positionIconOutIn, positionCommited)) {
//                positionIconOutIn = adjustPosition(positionCommited, taskData.IsOutbound);
//            }
//        }
//        else
//            positionDrawBlock = positionIconOutIn + 20;
//    }


//    return positionDrawBlock;
//}





