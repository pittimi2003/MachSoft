export class TaskHandler {
    _sayHello(name) {
        console.log(`hola ${name} desde  class TaskHandler`);
        alert(`hola ${name} desde  class TaskHandler`);
    }

    invoke(methodName, args) {
        const method = this[`_${methodName}`];
        if (typeof method === "function") {
            method.apply(this, args);
        } else {
            console.warn(`Metodo '${methodName}' no encontrado en TaskHandler.`);
        }
    }
}