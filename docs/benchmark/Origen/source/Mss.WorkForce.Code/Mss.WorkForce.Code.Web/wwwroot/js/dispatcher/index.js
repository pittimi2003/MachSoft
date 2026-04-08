import { Dispatcher } from "./Dispatcher.js";

const dispatcherInstance = new Dispatcher();

export function invokeFunction(group, methodName, ...args) {
    dispatcherInstance.invoke(group, methodName, ...args);
}

window.invokeFunction = invokeFunction;