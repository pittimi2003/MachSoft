document.addEventListener("keydown", function (e) {

    const input = e.target;
    if (!input.classList.contains("mlx-timeinput-field"))
        return;

    const key = e.key;
    const keyCode = e.keyCode || e.which;

    //------------------------------------------
    // 1. ENTER — Confirmar edición
    //------------------------------------------
    const isEnter =
        key === "Enter" ||
        keyCode === 13 ||
        key === "" ||
        key === "Unidentified";

    if (!isEnter)
        return;

    // 1) DevExpress toma el valor del editor SOLO si ve un "input" antes del Enter
    input.dispatchEvent(new Event("input", { bubbles: true }));

    // 2) Actualizar el valor del modelo de Blazor ANTES de que DevExpress cierre la celda
    input.dispatchEvent(new Event("change", { bubbles: true }));

    // 3) NO bloqueamos el evento
    //    Nada de preventDefault
    //    Nada de stopPropagation
    //    DevExpress DEBE manejar el Enter
    //    (cerrar celda, validar, mover foco)

    // 4) Después de que DevExpress haya procesado el Enter,
    //    cerramos el editor con blur asincrónico
    requestAnimationFrame(() => {
        input.blur();
    });

}, true); // <-- capture = true para ejecutar ANTES de DevExpress

