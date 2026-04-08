import { TaskHandler } from "./handlers/TaskHandler.js";
import { GanttHandler } from "./handlers/GanttHandler.js";
import { SchedulerHandler } from "./handlers/SchedulerHandler.js";
import { PivotHandler } from "./handlers/PivotHandler.js";

export class Dispatcher {
    constructor() {
        this.handlers = {
            scheduler: new SchedulerHandler(),
            task: new TaskHandler(),
            GanttComponent: new GanttHandler(),
            PivotComponent: new PivotHandler(),
        };
    }

    invoke(group, methodName, ...args) {
        const handler = this.handlers[group];
        if (handler && typeof handler.invoke === "function") {
            return handler.invoke(methodName, args); 
        } else {
            console.warn(`Handler '${group}' no encontrado.`);
            return null;
        }
    }

}