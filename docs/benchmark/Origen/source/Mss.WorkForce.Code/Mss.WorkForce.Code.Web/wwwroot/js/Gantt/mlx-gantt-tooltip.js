/**
 * Version 1.0.2
 **/


import * as functGantt from './mlx-gantt.js';


/* ******************TOOLTIP PLANNING*****************************************************/
export function getTaskToolTipDataPlanning(taskData) {
    const task = functGantt.getTaskOfGantt(taskData.id); //para obtener todas las propiedades de las tareas
    task.start = taskData.start;
    task.end = taskData.end;
    switch (task.TooltipType) {
        case 'PlanningEstimation':
        case 'PlanningGeneral':
        case 'PlanningWarehouse':
            return getToolTipHighTaskPlanning(task);
            break;

        case 'PlanningFlow':
        case 'PlanningPriority':
        case 'PlanningTrailer':
            return getTaskTooltipParentTask(task);
            break;

        case "PlanningOrder":
        case "PlanningEstimationInOrder":
        case "PlanningEstimationOutOrder":
        case "PlanningWarehouseOrder":
            return getTaskTooltipOrderTask(task);
            break;

        case "YardGeneral":
            return getToolTipYardForDock(task);

        case "LaborWorkerOrder":
            return getToolTipWorkerFlow(task);
            break;

        case "YardOrder":
            return getTaskTooltipYardForAppointment(task);
            break;

        case 'LaborWorkerActivity':
            return getToolTipWorkerProcess(task);
            break;

        case 'PlanningEstimationIn':
        case 'PlanningEstimationOut':
            return getToolTipEstimatedParentPlanning(task);
            break;

        case 'LaborWorkerGeneral':
            return getToolTipWorkerParent(task);
            break;

        case 'LaborEquipmentGeneral':
            return getToolTipLaborEquipmentParent(task);
            break;

        case 'LaborEquipmentOrder':
            return getToolTipLaborEquipmentFlow(task);   // Nivel 2
            break;

        case 'LaborEquipmentActivity':
            return getToolTipLaborEquipmentProcess(task); // Nivel 3
            break;

        case 'None':
        case 'PlanningActivity':
        default:
            return getTaskTooltipSubTask(task);
            break;

    }
}

export function getToolTipHighTaskPlanning(data) {
    const formatUserDatetime = data.DateTimeFormatt;
    const childrens = functGantt.getTaskChildren(data.id);
    if (childrens.length > 0) {
        childrens.sort((a, b) => new Date(a.start) - new Date(b.start));
        const startDate = formatUserDate(childrens.find(child => child.start != null)?.start ?? '', formatUserDatetime);
        childrens.sort((a, b) => new Date(a.end) - new Date(b.end));
        const lastChild = childrens[childrens.length - 1];
        const endDate = formatUserDate(lastChild.end, formatUserDatetime);
        const progress = childrens.reduce((sum, x) => sum + x.progress, 0);
        const totalProgress = parseFloat((progress / childrens.length).toFixed(1));
        const totalProgressformatted = formatDecimal(totalProgress, data.DecimalSeparator);
      
        return `<div id="toolTip" class="task-tooltip HighTask">
                ${getStartUpDataForToolTips('', data.IDCode ?? '', data.TooltipType == 'PlanningEstimation' ? 0 : totalProgressformatted, data.TooltipType == 'PlanningEstimation')} 
                
                <div class="task-details">
                    ${getDivStartEndDate(startDate, endDate, true)}
                    <div class="task-content-higthTask"> 
                        ${data.TooltipType != 'PlanningEstimation' ? getInfoForHighTask(childrens, formatUserDatetime) : getToolTipEstimatedHigth(childrens, formatUserDatetime)}
                    </div>
                </div>
        </div>`;
    }
}

export function getToolTipYardForDock(data) {
    const formatUserDatetime = data.DateTimeFormatt;

    return `<div id="toolTip" class="task-tooltip HighTask" >
        ${getStartUpDataForToolTips(data.DockName, data.ProcessType, data.Saturation, false)} 
        <div class="task-details">
            ${getDivStartEndDate(formatUserDate(data.start, formatUserDatetime), formatUserDate(data.end, formatUserDatetime), true)}
            <div class="task-dates task-columns" style="padding-bottom: 15px;">
                <span class="task-label">${data.ProcessType} ${L('Total')}</span>
                <span class="task-value">${data.AttendedAppointments}/${data.TotalAppointments}</span>
            </div>
        </div>
    </div>`;
}

export function precalculateDatesFromChildren(parentId) {
    if (!window.dataTaskInformation) return;

    const children = window.dataTaskInformation.filter(t => t.parentId === parentId);

    children.forEach(child => {
        if (!child.start || !child.end) {
            const subchildren = window.dataTaskInformation.filter(
                g => g.parentId === child.id && g.start && g.end
            );

            if (subchildren.length > 0) {
                child.start = subchildren.reduce(
                    (min, g) => min < g.start ? min : g.start,
                    subchildren[0].start
                );

                child.end = subchildren.reduce(
                    (max, g) => max > g.end ? max : g.end,
                    subchildren[0].end
                );
            }
        }
    });
}

export function getInfoSecondLevel(data) {
    const childrens = functGantt.getTaskChildren(data.id);
    if (childrens.length > 0) {
        return `<div class="task-content-higthTask">                 
                    ${getInfoForHighTask(childrens)}
                </div>`;
    }
}

export function getInfoSecondLevelOutTotal(data) {
    const formatUserDatetime = data.DateTimeFormatt;
    const childrens = functGantt.getTaskChildren(data.id);
    if (childrens.length > 0) {
        return `<div class="task-content-higthTask">                 
                    ${getInfoForHighTaskOutTotal(childrens, formatUserDatetime)}
                </div>`;
    }
}

export function preloadTooltipDataForChildren(parentId, tooltipFunction) {
    if (!window.dataTaskInformation) return;

    const children = window.dataTaskInformation.filter(t => t.parentId === parentId);

    children.forEach(child => {
        if (!child.start || !child.end) {
            const subchildren = window.dataTaskInformation.filter(
                g => g.parentId === child.id && g.start && g.end
            );
            if (subchildren.length > 0) {
                child.start = subchildren.reduce((min, g) => min < g.start ? min : g.start, subchildren[0].start);
                child.end = subchildren.reduce((max, g) => max > g.end ? max : g.end, subchildren[0].end);
                child.progress = Math.round(
                    subchildren.reduce((sum, g) => sum + (g.progress || 0), 0) / subchildren.length
                );
            }
        }
        if (tooltipFunction) {
            tooltipFunction(child);
        }
    });
}


export function getToolTipLaborEquipmentParent(task) {
    //const taskParent = functGantt.getTaskOfGantt(task.parentId);
    const formatUserDatetime = task.DateTimeFormatt;
    const title = task.title ?? '';
    const startDate = formatUserDate(task.start, formatUserDatetime);
    const endDate = formatUserDate(task.end, formatUserDatetime);

    const equipmenttype = task.TypeEquipmentName;
    const equipmentgroup = task.EquipmentGroupName;
    const efficiency = task.EfficiencyFormatted;
    const productivity = task.ProductivityFormatted;
    const workingTime = task.WorkTime;
    const totalorders = task.TotalOrders;
    const closedorders = task.ClosedOrders;
    const equipments = task.Equipments;
    const usage = task.progress;//Pendiente revisar si es este campo

    const tooltTip = `
        <div id="toolTip" class="task-tooltip HighTask">

            <div class="task-details">
                <div class="task-columns">
                    ${getStartUpDataForToolTipsWithOutSeparator(equipmenttype, equipmentgroup)} 
                    ${addProgressTooltip(task.progress)}
                </div>
            </div>
                <div class="task-details">                    
                    <div class="task-columns">
                        ${addProgressTooltipWithLabel(efficiency, "Efficiency")}
                        ${addProgressTooltipWithLabel(productivity, "Productivity")}
                        ${addProgressTooltipWithLabel(usage, "Usage")}
                    </div> 

                    <div class="task-time task-columns ">
                        ${getGenericSpan("Start", startDate)}
                    </div> 
                     <div class="task-time task-columns ">
                        ${getGenericSpan("End", endDate)}
                    </div>
                     <div class="task-time task-columns task-separator">
                        ${getSpanWorkingTime(workingTime)}
                    </div>  
                    
                    <div class="task-columns">
                        ${getGenericSpan("Equipments", equipments)}
                    </div> 

                    <div class="task-columns">
                        ${getGenericSpan("Total orders", totalorders)}
                    </div>
                    <div class="task-columns task-separator">
                        ${getGenericSpan("Closed orders", closedorders)}
                    </div>
                     
				</div>

            </div>`;

    return tooltTip;

}

export function getToolTipLaborEquipmentFlow(task) {
    const formatUserDatetime = task.DateTimeFormatt;
    const borderColor = task.IsOutbound ? 'Out' : 'Input';
    const startDate = formatUserDate(task.start, formatUserDatetime);
    const endDate = formatUserDate(task.end, formatUserDatetime);
    const progress = task.progress ?? 0;
    const flow = task.WorkFlow;
    const efficiency = task.EfficiencyFormatted;
    const productivity = task.ProductivityFormatted;
    const workingTime = task.WorkTime;
    const totalorders = task.TotalOrders;
    const closedorders = task.ClosedOrders;
    const equipments = task.Equipments;
    const usage = task.progress;

    const tooltTip = `
        <div id="toolTip" class="task-tooltip childless} ${borderColor}">
                ${getStartUpDataForToolTips(task.WorkFlow ?? '', '', progress, false)} 
                
                <div class="task-details">
                    
                    <div class="task-columns">
                        ${addProgressTooltipWithLabel(efficiency, "Efficiency")}
                        ${addProgressTooltipWithLabel(productivity, "Productivity")}
                        ${addProgressTooltipWithLabel(usage, "Usage")}
                    </div> 

                    <div class="task-times task-columns ">
                        ${getSpanCommitedHour(task.CommittedHour)}
                    </div> 

                    <div class="task-times task-columns ">
                        ${getGenericSpan("Start", startDate)}
                    </div> 
                     <div class="task-times task-columns ">
                        ${getGenericSpan("End", endDate)}
                    </div>
                     <div class="task-times task-columns task-separator">
                        ${getSpanWorkingTime(workingTime)}
                    </div>  
                    
                    <div class="task-columns">
                        ${getGenericSpan("Total orders", totalorders)}
                    </div>
                    <div class="task-columns task-separator">
                        ${getGenericSpan("Closed orders", closedorders)}
                    </div>
                     

                    <div>
                        ${getTotalProcessByTitle(task)}
                    </div>
              </div>

          </div>`;

    return tooltTip;

}

export function getToolTipLaborEquipmentProcess(task) {
    const formatUserDatetime = task.DateTimeFormatt;
    const borderColor = task.IsOutbound ? 'Out' : 'Input';
    const startDate = formatUserDate(task.start, formatUserDatetime);
    const endDate = formatUserDate(task.end, formatUserDatetime);
    const flow = task.WorkFlow;
    const efficiency = task.EfficiencyFormatted;
    const productivity = task.ProductivityFormatted;
    const workingTime = task.WorkTime;
    const totalorders = task.TotalActivities;
    const closedorders = task.ClosedActivities;
    const usage = task.progress;
    const title = (task.ActivityTitle ?? '').toUpperCase();
    const progress = getAverageSegmentProgress(task);

    const tooltTip = `
        <div id="toolTip" class="task-tooltip childless} ${borderColor}">
                ${getStartUpDataForToolTips(title, '', progress, false)} 
                
                <div class="task-details">
                    
                    <div class="task-columns">
                        ${addProgressTooltipWithLabel(efficiency, "Efficiency")}
                        ${addProgressTooltipWithLabel(productivity, "Productivity")}
                        ${addProgressTooltipWithLabel(usage, "Usage")}
                    </div> 

                    <div class="task-time task-columns ">
                        ${getGenericSpan("Start", startDate)}
                    </div> 
                     <div class="task-time task-columns ">
                        ${getGenericSpan("End", endDate)}
                    </div>
                     <div class="task-time task-columns task-separator">
                        ${getSpanWorkingTime(workingTime)}
                    </div>  
                    
                    <div class="task-columns">
                        ${getGenericSpan("Total activities", totalorders)}
                    </div>
                    <div class="task-columns task-separator">
                        ${getGenericSpan("Closed activities", closedorders)}
                    </div>
                     
                    <div class="task-columns">
                        ${getSegmentsInfoTooltip(task)}
                    </div>
				</div>

            </div>`;

    return tooltTip;

}


export function getToolTipWorkerParent(task) {
    const formatUserDatetime = task.DateTimeFormatt;
    const startDate = formatUserDate(task.start, formatUserDatetime);
    const endDate = formatUserDate(task.end, formatUserDatetime);

    //Campos nuevos
    const worker = task.WorkerName;
    const shift = task.ShiftName;
    const rol = task.RolName;

    const equipmentGroup = task.EquipmentGroup ?? '';//pendiente
    const equipmentType = task.EquipmentType ?? '';//pendiente

    const workingTime = task.WorkTime;
    const efficiency = task.EfficiencyFormatted;
    const productivity = task.ProductivityFormatted ?? '';
    const totalorders = task.TotalOrders;
    const closedorders = task.ClosedOrders;
    const totalordersbyprocess = task.TotalOrders;
    const closedordersbyprocess = task.ClosedOrders;
    const activitytitle = task.ActivityTitle !== null ? task.ActivityTitle : '';
    const breaks = (task.Breaks === "" || task.Breaks == null) ? 0 : task.Breaks;
    const team = task.TeamName;

    const tooltTip = `
        <div id="toolTip" class="task-tooltip HighTask">
            <div class="task-columns">
                ${getTaskTooltipContentWorker(task)}
                ${addProgressTooltip(task.progress)}
            </div>
                <div class="task-separator"></div>
                <div class="task-details">
                    <div class="task-columns">
                        <div class="task-times task-columns task-separator">
                            ${addProgressTooltipWithLabel(efficiency, "Efficiency")}
                            ${addProgressTooltipWithLabel(productivity, "Productivity")}
                        </div>
                    </div> 
                    <div class="task-columns task-dates">
                        ${getSpanWorkingTime(workingTime)}
                    </div> 
                    <div class="task-columns task-dates">
                        ${getGenericSpan("Breaks", breaks)}
                    </div> 
                    <div class="task-columns task-dates">
                        ${getGenericSpan("Shift", shift)}
                    </div> 
                    <div class="task-columns task-dates">
                        ${getGenericSpan("Role", rol)}
                    </div> 

                    <div class="task-columns task-separator task-dates">
                        ${getGenericSpan("Team", team)}
                    </div>

                    <div class="task-columns">
                        ${getGenericSpan("Total orders", totalorders)}
                    </div>
                    <div class="task-columns task-separator">
                        ${getGenericSpan("Closed orders", closedorders)}
                    </div>

                    ${getInfoSecondLevelOutTotal(task)}
                     
				</div>

            </div>`;

    return tooltTip;
}


export function getToolTipWorkerFlow(task) {
    const formatUserDatetime = task.DateTimeFormatt;
    const borderColor = task.IsOutbound ? 'Out' : 'Input';
    const startDate = formatUserDate(task.start, formatUserDatetime);
    const endDate = formatUserDate(task.end, formatUserDatetime);
    const equipmentType = task.EquipmentType ?? '';
    const equipmentGroup = task.EquipmentGroup ?? '';
    const workingTime = task.WorkTime;
    const efficiency = task.EfficiencyFormatted;
    const productivity = task.ProductivityFormatted ?? '';
    const totalorders = task.TotalOrders;
    const closedorders = task.ClosedOrders;
    const progress = task.progress ?? 0;
    const breaks = (task.Breaks === "" || task.Breaks == null) ? 0 : task.Breaks;

    const tooltTip = `
        <div id="toolTip" class="task-tooltip childless} ${borderColor}"> 
                
                ${getStartUpDataForToolTips(task.WorkFlow ?? '', '', progress, false)} 
                <div class="task-details">
                    <div class="task-columns">
                        ${addProgressTooltipWithLabel(efficiency, "Efficiency")}
                        ${addProgressTooltipWithLabel(productivity, "Productivity")}
                    </div> 

                    <div class="task-times task-columns ">
                        ${getSpanCommitedHour(task.CommittedHour)}
                    </div> 

                    <div class="task-times task-columns ">
                        ${getGenericSpan("Start", startDate)}
                    </div> 
                     <div class="task-times task-columns ">
                        ${getGenericSpan("End", endDate)}
                    </div>
                    <div class="task-times task-columns ">
                        ${getSpanWorkingTime(workingTime)}
                    </div>
                    
                    <div class="task-times task-columns task-separator">
                       ${getGenericSpan("Breaks", breaks)}
                    </div>
                    
                    <div class="task-columns">
                        ${getGenericSpan("Total orders", totalorders)}
                    </div>
                    <div class="task-columns task-separator">
                        ${getGenericSpan("Closed orders", closedorders)}
                    </div>
                     
                     <div>
                        ${getTotalProcessByTitle(task)}
                    </div>
 
                </div>

            </div>`;

    return tooltTip;
}


export function getToolTipWorkerProcess(task) {
    const formatUserDatetime = task.DateTimeFormatt;
    const borderColor = task.IsOutbound ? 'Out' : 'Input';
    const startDate = formatUserDate(task.start, formatUserDatetime);
    const endDate = formatUserDate(task.end, formatUserDatetime);
    const equipmentType = task.EquipmentType ?? '';
    const workingTime = task.WorkTime;
    const efficiency = task.EfficiencyFormatted;
    const productivity = task.ProductivityFormatted ?? '';
    const totalorders = task.TotalActivities;
    const closedorders = task.ClosedActivities;
    const title = (task.ActivityTitle ?? '').toUpperCase();
    const progress = getAverageSegmentProgress(task);
    const breaks = (task.Breaks === "" || task.Breaks == null) ? 0 : task.Breaks;

    const tooltTip = `
        <div id="toolTip" class="task-tooltip childless} ${borderColor}"> 
                
                ${getStartUpDataForToolTips(title, '', progress, false)} 
                <div class="task-details">
                    <div class="task-columns">
                        ${addProgressTooltipWithLabel(efficiency, "Efficiency")}
                        ${addProgressTooltipWithLabel(productivity, "Productivity")}
                    </div> 

                    <div class="task-times task-columns ">
                        ${getGenericSpan("Start", startDate)}
                    </div> 
                     <div class="task-times task-columns ">
                        ${getGenericSpan("End", endDate)}
                    </div>
                    <div class="task-times task-columns task-separator">
                        ${getSpanWorkingTime(workingTime)}
                    </div>
                    
                    <div class="task-columns">
                        ${getGenericSpan("Total activities", totalorders)}
                    </div>
                    <div class="task-columns task-separator">
                        ${getGenericSpan("Closed activities", closedorders)}
                    </div>

                    <div class="task-columns">
                        ${getSegmentsInfoTooltip(task)}
                    </div>
                     
				</div>

            </div>`;

    return tooltTip;
}


export function getToolTipEstimatedParentPlanning(task) {
    const taskParent = functGantt.getTaskOfGantt(task.parentId);
    const title = task.isParent ? taskParent.title ?? '' : task.title ?? '';
    const borderColor = task.IsOutbound ? 'Out' : 'Input';
    const startDate = formatDate(task.start);
    const endDate = formatDate(task.end);
    const workingTime = task.WorkTime;
    const tooltTip = `
        <div id="toolTip" class="task-tooltip childless} ${borderColor}">
                ${getStartUpDataForToolTips(title, task.IDCode, 0, true)} 
                
                <div class="task-details">
                     ${getDivStartEndDate(startDate, endDate, true)}
                    <div class="task-time task-columns task-separator">
                        ${getSpanWorkingTime(workingTime)}
                    </div>
                     ${getResourcesEstimated(task)}
                     ${getTotalProcessEstimated(task)}
				</div>

            </div>`;

    return tooltTip;
}

/**
 * Obtiene el contenido general del tooltip
 */
export function getTaskTooltipParentTask(data) {
    const activity = getTitleForTooltipParent(data);
    const formatUserDatetime = data.DateTimeFormatt;
    const taskChild = functGantt.getTaskChildren(data.id);
    const toolTipClass = taskChild.length == 0 ? 'childless' : '';
    const totalOrders = functGantt.getTaskChildren(data.id).length;
    const borderColor = data.color != "" ? data.IsOutbound ? 'Out' : 'Input' : 'HighTask';
    const startDate = formatUserDate(data.start, formatUserDatetime);
    const endDate = formatUserDate(data.end, formatUserDatetime);
    const lockedOrder = taskChild.filter(item => item.isBlock === true).length;
    const closedOrder = taskChild.filter(item => item.progress === 100).length;


    const value = `${closedOrder}/${totalOrders}`;

    const tooltTip =
        `<div id="toolTip" class="task-tooltip ${toolTipClass} ${borderColor}">
                ${getStartUpDataForToolTips('', activity, data.progress, false)}                
                <div class="task-details">
                    ${getDivStartEndDate(startDate, endDate, true)}
                    <div class="task-columns">
                         ${getGenericSpan("Locked orders", lockedOrder)}
                    </div>
                    <div class="task-columns">
                        ${getGenericSpan("Total orders", value)}
                    </div>
				</div>

        </div>`;

    return tooltTip;
}

/**
 * Obtiene el contenido general del tooltip
 */
export function getTaskTooltipOrderTask(data) {
    const formatUserDatetime = data.DateTimeFormatt;
    const taskParent = functGantt.getTaskOfGantt(data.parentId);
    const title = getTitleForTooltipParent(data);
    const activity = data.isParent ? taskParent.title ?? '' : data.title ?? '';
    const toolTipClass = functGantt.getTaskChildren(data.id).length == 0 ? 'childless' : '';
    const borderColor = data.IsOutbound ? 'Out' : 'Input';
    const startDate = formatUserDate(data.start, formatUserDatetime);
    const endDate = formatUserDate(data.end, formatUserDatetime);

    const tooltTip =
        `<div id="toolTip" class="task-tooltip ${toolTipClass} ${borderColor}">
                ${getStartUpDataForToolTips(title, activity, data.progress, false)}
                <div class="task-details">
                    <div class="task-time task-columns">
                        ${getSpanCommitedHour(data.CommittedHour)}
                    </div>
                    ${getDivStartEndDate(startDate, endDate, true)}
                    ${getTaskTooltipContentParent(data)}
				</div>
        </div>`;

    return tooltTip;
}

export function getTaskTooltipSubTask(data) {
    const formatUserDatetime = data.DateTimeFormatt;
    const taskParent = functGantt.getTaskOfGantt(data.parentId);
    const title = data.title ?? taskParent.title ?? '';
    const activity = data.ActivityTitle ?? '';
    const borderColor = data.IsOutbound ? 'Out' : 'Input';
    const startDate = formatUserDate(data.start, formatUserDatetime);
    const endDate = formatUserDate(data.end, formatUserDatetime);
    const workingTime = data.WorkTime;
    const equipmentGroup = data.EquipmentGroup ?? '';
    const equipmentType = data.EquipmentType ?? '';
    const tooltTip = `
            <div id="toolTip" class="task-tooltip subTask ${borderColor}">
                ${getStartUpDataForToolTips(title, activity, taskParent.TooltipType == 'PlanningEstimation' ? 0 : data.progress, taskParent.TooltipType == 'PlanningEstimation')} 
                
                <div class="task-details">
                    <div class="task-time task-columns task-separator">
                        ${getSpanCommitedHour(data.CommittedHour)}
                    </div>

                    ${getDivStartEndDate(startDate, endDate, true)}
                    
                    <div class="task-time task-columns task-separator">
                        ${getSpanWorkingTime(workingTime)}
                    </div>

                    <div class="task-rows task-separator">
                        <div >
                            <span class="task-label ">${L('Equipments')}</span>
                        </div>
                        <div class="task-groups">
                            <div class="task-columns">
                                <span class="task-label-sub">${L('Group')}</span>
                                <span class="task-value">${equipmentGroup}</span>
                            </div>

                            <div  class="task-columns">    
                                <span class="task-label-sub">${L('Type')}</span>
                                <span class="task-value">${equipmentType}</span>
                            </div>
                        </div>
                    </div>

                    <div class="task-resource">
                        ${data.Resource != undefined ? getContentForResource(data) : ''}
                    </div>

				</div>

            </div>`;

    return tooltTip;
}

export function getTaskTooltipYardForAppointment(data) {
    const formatUserDatetime = data.DateTimeFormatt;
    const taskParent = functGantt.getTaskOfGantt(data.parentId);
    const title = data.AppointmentCode;
    const activity = data.ProcessType;
    const borderColor = data.color;
    const committed = data.AppointmentDate;
    const startDate = formatUserDate(data.start, formatUserDatetime);
    const endDate = formatUserDate(data.end, formatUserDatetime);
    const customer = data.Customer;
    const yard = data.YardCode;
    const dock = taskParent.DockName;
    const vehicleType = data.VehicleType;
    const licence = data.License;

    const tooltTip = `
            <div id="toolTip" class="task-tooltip subTask" style="border-left-color: ${borderColor};">
                ${getStartUpDataForToolTips(title, activity, data.progress, false)} 
                <div class="task-time task-columns">
                    ${getGenericSpan("Committed", committed)}
                </div> 
                ${getDivStartEndDate(startDate, endDate, true)}
                
                <div class="task-time task-columns">
                    ${getGenericSpan("Customer", customer)}
                </div>
                <div class="task-time task-columns">
                    ${getGenericSpan("Yard", yard)}
                </div>
                <div class="task-time task-columns">
                    ${getGenericSpan("Dock", dock)}
                </div>
                <div class="task-time task-columns">
                    ${getGenericSpan("Vehicle type", vehicleType)}
                </div>
                 <div class="task-time task-columns task-separator">
                    ${getGenericSpan("Licence", licence)}
                </div>
                <div class="task-time task-columns">
                    <span class="task-label">${L('Orders numbers')}</span>
                    <span class="task-value">${data.CompletedOrders}/${data.TotalOrders}</span>
                </div>
            </div>`;

    return tooltTip;
}

export function getToolTipEstimatedHigth(processEstimated, formatUserDatetime) {
    return `${processEstimated.map((p) => {
        const totalProcess = (p.ProcessEstimated ?? []).reduce((sum, x) => sum + x.Cantidad, 0);
        return contentForTaskChildHighTask(p, totalProcess, formatUserDatetime);
    }).join('')}`;
}

export function getInfoForHighTask(childrensParent, formatUserDatetime) {
    const taskType = childrensParent[0].TooltipType;
    const childTotal = getGeneralChild(childrensParent, taskType);
    let result = '';
    childTotal.grouped.forEach((child) => {
        const grandchildren = functGantt.getTaskChildren(child.id) || [];
        const totalOrders = grandchildren.length;
        const closedOrders = grandchildren.filter(g => g.progress === 100).length;

        const enrichedChild = {
            ...child,
            ClosedOrders: closedOrders
        };

        result += contentForTaskChildHighTask(enrichedChild, totalOrders, formatUserDatetime);
    });

    return result;
}

export function getInfoForHighTaskOutTotal(childrensParent, formatUserDatetime) {
    const taskType = childrensParent[0].TooltipType;
    const childTotal = getGeneralChild(childrensParent, taskType);
    let result = '';

    childTotal.grouped.forEach((child) => {
        const grandchildren = functGantt.getTaskChildren(child.id) || [];

        const totalOrders = grandchildren.length;
        const closedOrders = grandchildren.filter(g => g.progress === 100).length;

        const enrichedChild = {
            ...child,
            ClosedOrders: closedOrders
        };

        result += contentForTaskChildHighTask(enrichedChild, totalOrders, formatUserDatetime);
    });

    return result;

}

export function contentForTaskChildHighTask(child, totalActivity, formatUserDatetime) {
    if (child.start != undefined && child.end != undefined) {
        const startDate = formatUserDate(child.start, formatUserDatetime);
        const endDate = formatUserDate(child.end, formatUserDatetime);
        const closedTotal = child.ClosedOrders ?? 0;
        const title = getTitleForTooltipParent(child);
        const taskType = functGantt.getTaskType(child.TooltipType);
        const value = taskType == 'LevelHigth' ? `${closedTotal}/${totalActivity}` : `${totalActivity}`;
        return `   <div class="task-dates task-columns">
                        <span class="task-label HighTask task-title">${title}</span>
                    </div>
                    ${getDivStartEndDate(startDate, endDate, false)}
                    <div class="task-dates task-columns" style="padding-bottom: 15px;">
                        <span class="task-label">${L('Total orders')}</span>
                        <span class="task-value">${value}</span>
                    </div>`;
    }
}

export function contentForTaskChildHighTaskOutTotal(child, totalActivity) {
    if (child.start != undefined && child.end != undefined) {
        const startDate = formatDate(child.start);
        const endDate = formatDate(child.end);
        const closedTotal = child.ClosedOrders ?? 0;
        const title = getTitleForTooltipParent(child);

        const value = child.CodeType == 1 ? `${closedTotal}/${totalActivity}` : `${totalActivity}`;
        return `   <div class="task-dates task-columns">
                        <span class="task-label HighTask task-title">${title}</span>
                    </div>
                    ${getDivStartEndDate(startDate, endDate, false)}
                   `;
    }
}

export function getResourcesEstimated(task) {
    return `<div class="task-contentChilds task-dates">
                    <span class="task-label">${L('Resources')}</span>
                    <div class="task-dates task-columns">
						<span class="task-label task-label-child">${L('Operators')}</span>
						<span class="task-value">${task.TotalOperators}</span>
					</div>
					<div class="task-dates task-columns task-separator">
						<span class="task-label task-label-child">${L('Equipments')}</span>
						<span class="task-value">${task.TotalEquipments}</span>
					</div>
             </div>`;
}

export function getTotalProcessEstimated(task) {
    const taskChild = functGantt.getTaskChildren(task.id);
    let process;
    if (task.ProcessEstimated != null && task.ProcessEstimated.length > 0) {
        process = task.ProcessEstimated.map(childEstimated => {
            const order = taskChild.find(c => c.ActivityTitle == childEstimated.Type);
            return getTaskForOrderEstimated(childEstimated, order);
        }).join("");
        return `<div class="task-contentChilds task-dates">
                <span class="task-label">${L('Processes')}</span>
                ${taskChild.length > 0 ? process : ``}
            </div>`;
    }
    else
        return ``;
}

/**
 * Retorna la información que tiene el tooltip full
 */
export function getTaskTooltipContentParent(taskParent) {
    const formatUserDatetime = taskParent.DateTimeFormatt;
    const task = functGantt.getTaskOfGantt(taskParent.id);
    return `<div class="task-contentFull">
                    <div class="task-time task-columns">
                        <span class="task-label">${L('Customer')}</span>
                        <span class="task-value">${task.Customer != undefined ? task.Customer : ''}</span>
                    </div>
					<div class="task-dates task-columns task-separator">
						<span class="task-label">${L('Dock')}</span>
						<span class="task-value">${task.DockName != undefined ? task.DockName : ''}</span>
					</div>
                        ${getTotalProcess(task, formatUserDatetime)}
				</div>`;
}

/**
* Retorna la información del worker
*/
export function getTaskTooltipContentWorker(task) {
    return `<div class="task-resource">
                ${task.Resource != undefined ? getContentForResource(task) : `<div> </div> `}
            </div> `;
}

export function getTaskForOrderEstimated(taskEstimated, order) {
    if (taskEstimated.Inicio != undefined && taskEstimated.Fin != undefined) {
        const workingTime = order.WorkTime;
        const timePeriodStart = formateTime(order.start);
        const timePeriodEnd = formateTime(order.end);
        const activity = taskEstimated.Type ?? '';
        const totalRes = order.TotalOperators;
        return `<div class="task-details">
                <div class="task-dates task-columns task-content-activity">
                    <span class="task-title-activities task-title">${activity}</span>
                    <span class="task-value">${taskEstimated.Cantidad}</span>
                </div>
                <div class="task-time task-columns">
                     <span class="task-label task-label-child">${L('Time period')}</span>
                     <span class="task-value">${timePeriodStart} - ${timePeriodEnd}</span>
                </div>
			    <div class="task-dates task-columns">
			    	<span class="task-label task-label-child">${L('Resource')}</span>
			    	<span class="task-value">${totalRes}</span>
			    </div>
			    <div class="task-dates task-columns">
			    	<span class="task-label task-label-child">${L('Worked time')}</span>
			    	<span class="task-value">${workingTime}</span>
			    </div>
            </div>`;
    }
}

export function getTotalProcess(task, formatUserDatetime) {
    const childrens = functGantt.getTaskChildren(task.id);
    const childTotal = getGeneralChild(childrens, task.TooltipType);
    const process = `${childTotal.grouped.map((child) => {
        return (childTotal.repeated[child.ActivityTitle])
            ? getTaskForChildren(child, childTotal.repeated[child.ActivityTitle], formatUserDatetime)
            : getTaskForChildren(child, 1, formatUserDatetime);
    }).join('')}`;

    return `<div class="task-contentChilds task-dates">
        <span class="task-label">${L('Processes')}</span>
        ${childrens.length > 0 ? process : ``}
    </div>`;
}

export function getTotalProcessByTitle(task) {
    const childrens = functGantt.getTaskChildren(task.id);
    const childTotal = getGeneralChildByTitle(childrens);
    const process = `${childTotal.grouped.map((child) => {
        return getGroupTaskChildren(child)
    }).join('')}`;

    return ` <div class="task-columns"> 
        <span class="task-label">${L('Processes')}</span>
        </div>
        ${childrens.length > 0 ? process : ``}
     `;
}

export function getSegmentsInfoTooltip(task) {
    if (!task.segments || task.segments.length === 0) return '';
    const title = (task.ActivityTitle ?? '').toUpperCase();
    const sortedSegments = task.segments.sort((a, b) => a.progress - b.progress);
    const formatUserDatetime = task.DateTimeFormatt;
    return `
        <div class="task-contentChilds task-dates">
            <span class="task-label">${L('Activities')}</span>
            ${sortedSegments.map((segment, i) => {
                const timePeriodStart = formatUserDate(segment.start, formatUserDatetime);
                const timePeriodEnd = formatUserDate(segment.end, formatUserDatetime);
        const progress = segment.progress;

        return `
                    <div class="task-details">
                        <div class="task-dates task-columns task-content-activity">
                            <span class="task-title-activities task-title">${title}</span>
                            <span class="task-value">${i + 1}</span>
                        </div>
                        <div class="task-time task-columns">
                             <span class="task-label task-label-child">${L('Start')}</span>
                             <span class="task-value">${timePeriodStart}</span>
                        </div>
                        <div class="task-dates task-columns">
                             <span class="task-label task-label-child">${L('End')}</span>
                             <span class="task-value">${timePeriodEnd}</span>
                        </div>
                        <div class="task-dates task-columns">
                             <span class="task-label task-label-child">${L('Worked time')}</span>
                             <span class="task-value">${segment.worktime}</span>
                        </div>
                        <div class="task-dates task-columns">
                             <span class="task-label task-label-child">${L('Progress')}</span>
                             <span class="task-value">${segment.progress}%</span>
                        </div>
                    </div>
                `;
    }).join('')}
        </div>
    `;
}

export function getAverageSegmentProgress(task) {
    if (!task.segments || task.segments.length === 0) return 0;

    const totalProgress = task.segments.reduce((sum, seg) => sum + seg.progress, 0);
    const avgProgress = Math.round(totalProgress / task.segments.length);
    return avgProgress;
}



/* ******************METODOS TRANFORMACIÓN*****************************************************/
export function getTitleForTooltipParent(task) {
    switch (task.TooltipType) {
        case 'PlanningFlow':
            return task.WorkFlow ?? '';
            break;

        case 'PlanningOrder':
            return functGantt.getTaskOfGantt(task.parentId).WorkFlow ?? '';
            break;

        case 'PlanningPriority':
            return task.Priority ?? '';
            break;

        case 'PlanningTrailer':
            return task.Trailer ?? '';
            break;

        case 'LaborEquipmentOrder':
        case 'LaborWorkerOrder':
            return task.WorkFlow ?? '';
            break;

        case 'PlanningEstimationIn':
        case 'PlanningEstimationOut':
            return task.IDCode ?? '';
            break;

        case 'YardOrder':
            return task.YardCode ?? '';
            break;
        case 'None':
        default: return '';
            break;

    }
}

export function formatDate(dateInput) {
    try {
        if (!dateInput) return '';

        const date = new Date(dateInput);
        if (isNaN(date.getTime())) return '';

        let month = date.getMonth() + 1;
        let day = date.getDate();
        let year = date.getFullYear();

        month = month < 10 ? '0' + month : month;
        day = day < 10 ? '0' + day : day;

        const formattedTime = formateTime(date);
        const formattedDate = reemplaceTypeFormatDate(day, month, year);

        return `${formattedDate} - ${formattedTime}`;
    } catch (error) {
        console.log(error);
        return '';
    }
}

export function formatDecimal(num, decimalSeparator) {
    if (Number.isInteger(num)) {
        return num.toString();
    }

    return num.toFixed(1).replace('.', decimalSeparator);
}

export function formatUserDate(dateInput, userFormat) {
    try {
        if (!dateInput) return '';

        const date = new Date(dateInput);
        if (isNaN(date.getTime())) return '';

        const day = String(date.getDate()).padStart(2, '0');
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const year = String(date.getFullYear());

        const hours24 = date.getHours();
        const hours12 = String(hours24 % 12 || 12).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');
        const ampm = hours24 < 12 ? 'AM' : 'PM';

        let formatted = userFormat;

        formatted = formatted.replaceAll('yyyy', year);
        formatted = formatted.replaceAll('MM', month);
        formatted = formatted.replaceAll('dd', day);
        formatted = formatted.replaceAll('HH', String(hours24).padStart(2, '0'));
        formatted = formatted.replaceAll('hh', hours12);
        formatted = formatted.replaceAll('mm', minutes);
        formatted = formatted.replaceAll('ss', seconds);
        formatted = formatted.replaceAll('tt', ampm);

        return formatted;
    } catch (error) {
        console.error(error);
        return '';
    }
}


export function formateTimeCustom(dateInput, userFormat) {
    try {
        if (!dateInput) return '';

        const date = new Date(dateInput);
        if (isNaN(date.getTime())) return '';

        const hours24 = date.getHours();
        const hours12 = String(hours24 % 12 || 12).padStart(2, '0');
        const minutes = String(date.getMinutes()).padStart(2, '0');
        const seconds = String(date.getSeconds()).padStart(2, '0');
        const ampm = hours24 < 12 ? 'AM' : 'PM';

        userFormat = userFormat
            .replaceAll('yyyy', '')
            .replaceAll('MM', '')
            .replaceAll('dd', '');

        //userFormat = userFormat
        //    .replaceAll(':ss', '')
        //    .replaceAll('.ss', '')
        //    .trim();

        let formatted = userFormat;

        formatted = formatted.replaceAll('HH', String(hours24).padStart(2, '0'));
        formatted = formatted.replaceAll('hh', hours12);
        formatted = formatted.replaceAll('mm', minutes);
        formatted = formatted.replaceAll('ss', seconds);
        formatted = formatted.replaceAll('tt', ampm);

        formatted = formatted.trim();

        formatted = formatted
            .replace(/^[\W_]+|[\W_]+$/g, '')
            .replace(/[\W_]{2,}/g, match => match[0]);

        return formatted;
    } catch (error) {
        console.log(error);
        return '';
    }
}

export function formateTime(dateString) {
    try {
        if (dateString != null) {
            let hours = dateString.getHours();
            let minutes = dateString.getMinutes();

            const ampm = hours >= 12 ? 'PM' : 'AM';
            hours = hours % 12; // Convierte a 12 horas
            hours = hours ? hours : 12;
            minutes = minutes < 10 ? '0' + minutes : minutes;

            return `${hours}:${minutes} ${ampm}`;
        }
    } catch (error) {
        console.log(error);
    }
}

export function reemplaceTypeFormatDate(day, month, year) {
    let formattedDate = functGantt.ReadyFormatDate()
        .replace("DD", day)
        .replace("MM", month)
        .replace("YYYY", year);

    // Si el formato seleccionado incluye horas, minutos, segundos, se eliminan porque el formato para visualizar no se ocupan
    if (functGantt.ReadyFormatDate().includes("HH") || functGantt.ReadyFormatDate().includes("MM") || functGantt.ReadyFormatDate().includes("SS")) {
        formattedDate = formattedDate
            .replace("HH", '')
            .replace("MM", '')
            .replace("SS", '')
            .replace(/:+/g, ' ').trim();
    }

    return formattedDate;
}

/**
 * Indica el tiempo trabajado
 * @param {any} workTime 
 * @returns el trabajo convertido en texto con horas y minutos
 */
export function getWorkingTime(workTime) {
    const totalMinutes = Math.floor(workTime / 60);

    if (totalMinutes < 60) {
        return `${totalMinutes} min`;

    } else {
        const hours = Math.floor(totalMinutes / 60);
        const minutes = Math.round(totalMinutes % 60);
        return `${hours} h ${minutes} min`;
    }
}

/**
 * Retorna todas las subtareas que contiene cada una de las tareas Padre
 * e indica cual subtarea y cuantas veces se repite
 */
export function getGeneralChild(childrens, taskType) {
    const childGrouped = [];
    const repetitionInfo = {};
    let titleType = null;
    switch (taskType) {
        case 'PlanningPriority':
            titleType = 'Priority';

            break;
        case 'PlanningTrailer':
            titleType = 'Trailer';
            break;

        case 'PlanningFlow':
        case 'LaborWorkerOrder':
            titleType = 'WorkFlow';
            break;

        case 'PlanningOrder':
        default:
            titleType = 'ActivityTitle';
            break;
    }
    childrens.forEach((child) => {
        let existingChild = childGrouped.find((t) => t[titleType] === child[titleType]);

        if (existingChild && existingChild[titleType] !== 0 && existingChild.TooltipType !== 'LaborWorkerGeneral') {
            if (new Date(child.start) < new Date(existingChild.start)) {
                existingChild.start = child.start;
            }

            if (new Date(child.end) > new Date(existingChild.end)) {
                existingChild.end = child.end;
            }

            repetitionInfo[child[titleType]] = (repetitionInfo[child[titleType]] || 1) + 1;
        } else {
            childGrouped.push({ ...child });
        }
    });

    return {
        grouped: childGrouped,
        repeated: repetitionInfo
    };
}

export function getGeneralChildByTitle(childrens) {
    const childGrouped = [];
    const repetitionInfo = {};

    childrens.forEach((child) => {
        let existingChild = childGrouped.find((t) => t.ActivityTitle === child.ActivityTitle);

        if (existingChild) {
            if (new Date(child.start) < new Date(existingChild.start)) {
                existingChild.start = child.start;
            }

            if (new Date(child.end) > new Date(existingChild.end)) {
                existingChild.end = child.end;
            }

            repetitionInfo[child.ActivityTitle] = (repetitionInfo[child.ActivityTitle] || 1) + 1;
        } else {
            childGrouped.push({ ...child });
        }
    });

    return {
        grouped: childGrouped,
        repeated: repetitionInfo
    };
}




/* **********************DATOS IDIVIDUALES***********************************************************/
export function getSpanWorkingTime(workingTime) {
    return `<span class="task-label">${L('Worked time')}</span>
                                <span class="task-value">${workingTime}</span>`;
}

export function getGenericSpan(title, value, symbol = "") {
    return `<span class="task-label">${L(title)}</span>
                                <span class="task-value">${value}${symbol}</span>`;
}


export function getSpanCommitedHour(commitedHour) {
    
    return `<span class="task-label">${L('Committed')}</span>
                                <span class="task-value">${commitedHour}</span>`;
}

export function addProgressTooltip(progress) {
    return `<span class="task-NumProgress">${progress}%</span>`;
}

export function addProgressTooltipWithLabel(progress, label) {
    return `
        <div class="task-progress-block">
            <div class="task-progress-label">${L(label)}</div>
            <span class="task-NumProgressTwoElements">${progress}%</span>
        </div>
    `;
}

/**
 * Retorna el contenido por cada recurso que trabaja en la subtarea
 */
export function getContentForResource(task) {
    try {
        const icono = getIcon(task.Resource);
        const iconColor = task.IsOutbound ? 'Out' : 'Input';
        return ` <div class="task-content-resource">
                <div class="task-icon ${iconColor}">${icono}</div>
                <div class="task-title">${task.Resource}</div>
             </div>`;
    } catch (error) {
        console.log(error);
    }

}

/**
 * Se crea el icono de cada recurso
 */
export function getIcon(resource) {
    if (resource != undefined)
        return resource.charAt(0);
}

/**
 * Obtiene el contenido que tiene cada subtarea para visualizarlo en la tarea padre
 */
export function getTaskForChildren(child, totalActivity, formatUserDatetime) {
    if (child.start != undefined && child.end != undefined) {
        const workingTime = child.WorkTime;
        const timePeriodStart = formateTimeCustom(child.start, formatUserDatetime);
        const timePeriodEnd = formateTimeCustom(child.end, formatUserDatetime);
        const activity = child.ActivityTitle ?? '';
        const totalRes = functGantt.getResourceForActivity(child.id);
        return `<div class="task-details">
                <div class="task-dates task-columns task-content-activity">
                    <span class="task-title-activities task-title">${activity}</span>
                    <span class="task-value">${totalActivity}</span>
                </div>
                <div class="task-time task-columns">
                     <span class="task-label task-label-child">${L('Time period')}</span>
                     <span class="task-value">${timePeriodStart} - ${timePeriodEnd}</span>
                </div>
			    <div class="task-dates task-columns">
			    	<span class="task-label task-label-child">${L('Resource')}</span>
			    	<span class="task-value">${totalRes.length}</span>
			    </div>
			    <div class="task-dates task-columns">
			    	<span class="task-label task-label-child">${L('Worked time')}</span>
			    	<span class="task-value">${workingTime}</span>
			    </div>
            </div>`;
    }
}

export function getGroupTaskChildren(child) {
    if (child.start != undefined && child.end != undefined) {
        const ClosedActivities = child.ClosedActivities;
        const TotalActivities = child.TotalActivities;
        const text = `${ClosedActivities}/${TotalActivities}`
        const activity = child.ActivityTitle ?? '';
        return `<div class="task-columns">
          ${getGenericSpan(activity, text)}
            </div>`;
    }
}

export function getStartUpDataForToolTips(title, activity, progress, addProgress) {
    return `<div class="task-columns task-separator">
        <div class="task-rigth">
            <span class="task-id">${title}</span>
            <div class="task-title task-title-font">${activity}</div>
        </div>
        ${!addProgress ? addProgressTooltip(progress) : `<div></div> `}
    </div>`;
}

export function getStartUpDataForToolTipsWithOutSeparator(title, activity) {

    const { truncated, tooltipText } = truncateTextTooltip(activity);

    return `<div class="task-columns">
        <div class="task-rigth">
            <span class="task-id">${title}</span>
            <div class="task-title task-title-font"${tooltipText}>${truncated}</div>
        </div>
    </div>`;
}

export function truncateTextTooltip(text) {
    const safeText = text ?? '';
    const isTruncated = safeText.length > 15;
    const truncated = isTruncated
        ? safeText.substring(0, 15) + "..."
        : safeText;

    const tooltipText = isTruncated ? ` title="${safeText}"` : '';

    return { truncated, tooltipText };
}

export function getDivStartEndDate(startDate, endDate, withSeparator) {
    const taskSeparator = withSeparator ? 'task-separator' : '';
    return `<div class="task-dates task-columns">
						<span class="task-label">${L('Start')}</span>
						<span class="task-value">${startDate}</span>
					</div>
					<div class="task-dates task-columns ${taskSeparator}">
						<span class="task-label">${L('End')}</span>
						<span class="task-value">${endDate}</span>
					</div>`;
}
