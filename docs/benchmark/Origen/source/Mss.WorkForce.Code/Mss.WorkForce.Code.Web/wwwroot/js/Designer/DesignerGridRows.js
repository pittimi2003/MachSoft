
    window.mssDesigner = window.mssDesigner || { };

    // NO manejar Delete si estás editando un campo de texto
    window.mssDesigner.canHandleDeleteKey = function () {
        var el = document.activeElement;
    if (!el)
    return true;

    var tag = el.tagName ? el.tagName.toUpperCase() : "";

    if (tag === "INPUT" || tag === "TEXTAREA")
    return false;

    if (el.isContentEditable)
    return false;

    return true;
    };

    // Volver a poner el foco en el contenedor principal
    window.mssDesigner.focusRootDesigner = function () {
        var root = document.getElementById('designer-root');
    if (root) {
        root.focus();
        }
    };

