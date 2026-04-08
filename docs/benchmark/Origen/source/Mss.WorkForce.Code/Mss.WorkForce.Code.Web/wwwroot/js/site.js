/**
 * Mostrar/Abrir  modal de bootstrap
 * @param {string} idModal - Id del componente del modal
 */
function openMlxModal(idModal) {
    try {
        var modalElement = document.getElementById(idModal);
        const modal = new bootstrap.Modal(modalElement);
        modal.show();
    } catch (e) {
        console.log("Error open id modal: " + idModal);
    }
}

/**
 * Ocultar/Cerrar  modal de bootstrap
 * @param {any} idModal - Id del componente del modal
 */
function closeMlxModal(idModal) {
    try {
        var modalElement = document.getElementById(idModal);

        if (modalElement && bootstrap.Modal.getInstance(modalElement)) {
            bootstrap.Modal.getInstance(modalElement).hide();
        } else {
            console.log("No active modal instance found for: " + idModal);
        }
    } catch (e) {
        console.log("Error close id modal: " + idModal);
    }
}

function loadDataUser(loadUser) {
    window.currentUser = loadUser;
}

function toggleFullscreen() {
    const toggleIcon = document.getElementById("fullscreenToggle");
    if (!document.fullscreenElement) {
        document.documentElement.requestFullscreen().then(() => {
            toggleIcon.classList.remove("mlx-ico-maximize-screen");
            toggleIcon.classList.add("mlx-ico-minimize-screen");
            toggleIcon.setAttribute("title", "Minimize");
        }).catch(err => {
            console.error(`Error al intentar maximizar: ${err.message}`);
        });
    } else {
        if (document.exitFullscreen) {
            document.exitFullscreen().then(() => {
                toggleIcon.classList.remove("mlx-ico-minimize-screen");
                toggleIcon.classList.add("mlx-ico-maximize-screen");
                toggleIcon.setAttribute("title", "Maximize");
            });
        }
    }
}

function loadLicences() {
    const licencesForAbout = [
        { name: "Billing", status: "Enabled" },
        { name: "Dashboard", status: "Enabled" },
        { name: "AccountDirective", status: "Disabled" },
        { name: "GNA", status: "Enabled" },
        { name: "OwnerExtensions", status: "Enabled" },
        { name: "TPLPortal", status: "Enabled" },
        { name: "ValueAddedService", status: "Enabled" },
        { name: "CashAndCarry", status: "Enabled" },
        { name: "Ecommerce", status: "Enabled" },
        { name: "Sage200c", status: "Enabled" },
        { name: "ExternalDevices", status: "Enabled" },
        { name: "LaborManagement", status: "Enabled" },
        { name: "PalletShuttleService", status: "Enabled" },
        { name: "GalileoDesigner", status: "Enabled" },
        { name: "VoicePicking", status: "Enabled" },
        { name: "SmartUI", status: "Enabled" },
        { name: "PalletShuttle", status: "Enabled" },
        { name: "PrinterService", status: "Enabled" },
        { name: "Manufacturing", status: "Enabled" },
        { name: "AGV", status: "Enabled" },
        { name: "APS3D", status: "Enabled" },
        { name: "GalileoFaults", status: "Enabled" },
        { name: "EasyWMS", status: "Enabled" },
        { name: "YardManagement", status: "Enabled" },
        { name: "Slotting", status: "Enabled" },
        { name: "Deliveries", status: "Enabled" },
        { name: "SageX3", status: "Enabled" },
        { name: "EDSService", status: "Enabled" },
        { name: "Gateway", status: "Enabled" },
        { name: "Mecalux", status: "Enabled" },
        { name: "MarketPlace", status: "Enabled" },
        { name: "PTLService", status: "Enabled" },
        { name: "Common", status: "Enabled" },
        { name: "Notifications", status: "Enabled" },
    ];

    const container = document.getElementById('licences-container');

    licencesForAbout.forEach(licence => {
        const licenceDiv = document.createElement('div');
        licenceDiv.classList.add('licence-content');

        const keyLicenceSpan = document.createElement('span');
        keyLicenceSpan.textContent = licence.name;
        keyLicenceSpan.classList.add('licence-key');

        const statusLicenceSpan = document.createElement('span');
        statusLicenceSpan.textContent = licence.status;
        statusLicenceSpan.classList.add('licence-value');

        licenceDiv.appendChild(keyLicenceSpan);
        licenceDiv.appendChild(statusLicenceSpan);

        container.appendChild(licenceDiv);
    });
}

document.addEventListener("fullscreenchange", () => {
    const toggleIcon = document.getElementById("fullscreenToggle");
    if (document.fullscreenElement) {
        toggleIcon.classList.remove("mlx-ico-maximize-screen");
        toggleIcon.classList.add("mlx-ico-minimize-screen");
        toggleIcon.setAttribute("title", "Minimize");
    } else {
        toggleIcon.classList.remove("mlx-ico-minimize-screen");
        toggleIcon.classList.add("mlx-ico-maximize-screen");
        toggleIcon.setAttribute("title", "Maximize");
    }
});

function applyNumberAndColonRestrictionToInputs() {
    const inputs = document.getElementsByClassName('roller-input');
    Array.from(inputs).forEach(input => {
        input.onkeypress = function (event) {
            const charCode = event.which || event.keyCode;
            const isBackspace = charCode === 8;
            const isColon = charCode === 58; // ASCII code for ':'
            const isDigit = charCode >= 48 && charCode <= 57; // Digitos 0-9
            return isBackspace || isColon || isDigit;
        };
    });
}


let outsideClickListeners = {};
let scrollListeners = {};

function detectOutsideClick(dotNetObjectRef, componentId, pickerId) {
    function onClick(event) {
        const element = document.getElementById(componentId);
        const picker = document.getElementById(pickerId);

        if (element && !element.contains(event.target) && picker && !picker.contains(event.target)) {
            try {
                if (dotNetObjectRef && dotNetObjectRef._id) {
                    dotNetObjectRef.invokeMethodAsync('HidePicker');
                }
            } catch (err) {
                console.warn("dotNetObjectRef no disponible:", err);
            }
        }
    }
    outsideClickListeners[componentId] = onClick;
    document.addEventListener("click", onClick);

    document.querySelectorAll("button").forEach(button => {
        button.addEventListener("click", onClick);
    });
}

function removeOutsideClickListener(componentId) {
    const onClick = outsideClickListeners[componentId];
    if (onClick) {
        document.removeEventListener("click", onClick);
        delete outsideClickListeners[componentId];
    }

    document.querySelectorAll("button").forEach(button => {
        button.removeEventListener("click", onClick);
    });
}

function removeAllPickers() {
    document.querySelectorAll(".roller-timepicker").forEach(picker => {
        picker.remove();
    });
}

function positionPicker(inputId, pickerId) {
    const input = document.getElementById(inputId);
    const picker = document.getElementById(pickerId);
    const rec = picker.getBoundingClientRect();
    const screenWidth = window.innerWidth;
    if (!input || !picker) return;
    const inputRect = input.getBoundingClientRect();
    const scrollTop = window.scrollY || document.documentElement.scrollTop;
    const pickerHeight = picker.offsetHeight;
    picker.style.position = "absolute";
    picker.style.left = `${inputRect.left}px`;
   
    if (rec.right >= screenWidth) {
        picker.style.left = "auto";
        picker.style.right  = "0px";
    }
    // Verificar si hay espacio debajo del input
    const spaceBelow = window.innerHeight - (inputRect.bottom + scrollTop);
    if (spaceBelow < pickerHeight) {
        picker.style.top = `${inputRect.top + scrollTop - pickerHeight}px`;
    } else {
        picker.style.top = `${inputRect.bottom + scrollTop}px`;
    }
    // Mover el picker al contenedor global
    document.getElementById("global-timepicker-container").appendChild(picker);
}

function attachScrollListener(dotNetRef, componentId, containerSelector) {
    let scrollContainer;
    if (containerSelector === "window") {
        scrollContainer = window;
    } else {
        scrollContainer = document.querySelector(containerSelector);
        if (!scrollContainer) {
            console.warn("No se encontró el contenedor con el selector:", containerSelector);
            return;
        }
    }

    function onScroll() {
        console.log("Scroll event triggered in container:", containerSelector);
        dotNetRef.invokeMethodAsync('HidePicker');
    }

    scrollListeners[componentId] = { container: scrollContainer, handler: onScroll };

    // Si se está usando window, usar addEventListener sobre window; de lo contrario, sobre el contenedor.
    scrollContainer.addEventListener("scroll", onScroll);
}

function removeScrollListener(componentId) {
    const listenerData = scrollListeners[componentId];
    if (listenerData) {
        listenerData.container.removeEventListener("scroll", listenerData.handler);
        delete scrollListeners[componentId];
    }
}

function executeAction(action, componentId) {
    const element = document.getElementById(componentId);

    if (element) {

        const btnAdd = element.querySelector('button[name="action-add"]');
        const btnEdit = element.querySelector('button[name="action-edit"]');
        const btnDelete = element.querySelector('button[name="action-trash"]');
        const btnSave = element.querySelector('button[name="action-save"]');
        const bntReadOnly = element.querySelector('button[name="action-readonly"]');

        switch (action) {
            case 'add':
                btnAdd.className = 'mlx-component-hidden';
                btnEdit.className = 'mlx-component-hidden';
                btnDelete.className = 'mlx-component-hidden';
                btnSave.className = 'mlx-component-visible';
                bntReadOnly.className = 'mlx-component-visible';
                btnSave.disabled = true;
                break;
            case 'edit':
                btnAdd.className = 'mlx-component-hidden';
                btnEdit.className = 'mlx-component-hidden';
                btnDelete.className = 'mlx-component-hidden';
                btnSave.className = 'mlx-component-visible';
                bntReadOnly.className = 'mlx-component-visible';
                btnSave.disabled = false;
                break;
            case 'readOnly':
                btnAdd.className = 'mlx-component-visible';
                btnEdit.className = 'mlx-component-visible';
                btnDelete.className = 'mlx-component-hidden';
                btnSave.className = 'mlx-component-hidden';
                bntReadOnly.className = 'mlx-component-hidden';
                break;
            case 'delete':
                break;

            case 'select':
                btnDelete.className = 'mlx-component-visible';
                break;

            case 'unselect':
                btnDelete.className = 'mlx-component-hidden';
                break;

        }
    }
}
function enableActionButton(componentId) {
    const element = document.getElementById(componentId);
    if (element) {
        const btnSave = element.querySelector('button[name="action-save"]');
        btnSave.disabled = false;
    }
}

function generateUUID() {
    var d = new Date().getTime();
    var d2 = ((typeof performance !== 'undefined') && performance.now && (performance.now() * 1000)) || 0;
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, function (c) {
        var r = Math.random() * 16;
        if (d > 0) {
            r = (d + r) % 16 | 0;
            d = Math.floor(d / 16);
        } else {
            r = (d2 + r) % 16 | 0;
            d2 = Math.floor(d2 / 16);
        }
        return (c === 'x' ? r : (r & 0x3 | 0x8)).toString(16);
    });
}
function getDoNetHelper(netHelper) {
    dotNetHelper = netHelper;
}

//inicia script para cambiar de orden en un solo contenedor
window.initSingleContainerReorder = function () {
    const container = document.getElementById("dragContainer");
    if (!container) return;

    let draggedItem = null;
    let itemPositions = [];
    let lastTime = 0;
    const Delay = 20; // milisegundos

    container.querySelectorAll(".cc-drag-item").forEach(item => {
        item.addEventListener("dragstart", (e) => {
            draggedItem = item;
            item.classList.add("dragging");
            e.dataTransfer.effectAllowed = "move";

            itemPositions = [...container.querySelectorAll(".cc-drag-item:not(.dragging)")].map(child => ({
                element: child,
                box: child.getBoundingClientRect()
            }));
        });

        item.addEventListener("dragend", () => {
            item.classList.remove("dragging");
            draggedItem = null;
            itemPositions = []; // limpiar cache
        });
    });

    container.addEventListener("dragover", (e) => {
        e.preventDefault();

        const now = Date.now();
        if (now - lastTime < Delay) return;
        lastTime = now;

        if (!draggedItem) return;

        const afterElement = getDragAfterElement(e.clientY);
        if (afterElement == null) {
            container.appendChild(draggedItem);
        } else {
            container.insertBefore(draggedItem, afterElement);
        }
    });

    container.addEventListener("drop", (e) => {
        e.preventDefault();
        draggedItem = null;
        itemPositions = [];
    });

    function getDragAfterElement(y) {
        return itemPositions.reduce((closest, childData) => {
            const offset = y - childData.box.top - childData.box.height / 2;
            if (offset < 0 && offset > closest.offset) {
                return { offset: offset, element: childData.element };
            } else {
                return closest;
            }
        }, { offset: Number.NEGATIVE_INFINITY }).element;
    }
};
//Finaliza script para cambiar de orden en un solo contenedor

//limpa eventos del drag
window.cleanupSingleContainerReorder = function () {
    const container = document.getElementById("dragContainer");
    if (!container) return;

    const cloned = container.cloneNode(true);
    container.parentNode.replaceChild(cloned, container);
};



window.getReorderedFieldNames = function () {
    const container = document.getElementById("dragContainer");
    if (!container) return [];

    const items = Array.from(container.querySelectorAll(".cc-drag-item"));
    return items.map(el => el.id);
};

window.setTooltipForSelectedSite = function (tooltipText, containerSelector) {
    const container = document.querySelector(containerSelector);
    
    const inputEl = container.querySelector("input.dxbl-text-edit-input");
    if (inputEl) {
        inputEl.setAttribute("title", tooltipText);
    }

    const dropButton = container.querySelector("button.dxbl-edit-btn-dropdown");
    if (dropButton) {
        dropButton.setAttribute("title", tooltipText);
        dropButton.setAttribute("aria-label", tooltipText);
    }
};


//reloj
window.utcClockIntervals = {};
window.utcClockSettings = {}; 
function formatTime(format, hours24, minutes) {
    const is12h = /\btt\b/i.test(format);
    const sep = format.includes('.') ? '.' : ':';
    const pad2 = n => String(n).padStart(2, '0');

    if (is12h) {
        const h12 = (hours24 % 12) || 12;
        const ampm = hours24 >= 12 ? 'PM' : 'AM';
        return `${pad2(h12)}${sep}${pad2(minutes)} ${ampm}`;
    }
    return `${pad2(hours24)}${sep}${pad2(minutes)}`;
}

function updateUtcClockElements(className) {
    const settings = window.utcClockSettings[className];
    if (!settings) return;

    const { offsetMinutes, format } = settings;

    const now = new Date();
    const adjusted = new Date(now.getTime() + offsetMinutes * 60000);

    const hours24 = adjusted.getUTCHours();
    const minutes = adjusted.getUTCMinutes();

    const text = formatTime(format, hours24, minutes);
    const elements = document.querySelectorAll(`.${className}`);
    elements.forEach(el => {
        if ('value' in el) el.value = text;
        else el.textContent = text;
    });
}

window.startUtcClock = function (className, offsetMinutes = 0, format = "HH:mm:ss") {
    window.utcClockSettings[className] = { offsetMinutes, format };

    if (window.utcClockIntervals[className]) {
        clearInterval(window.utcClockIntervals[className]);
        delete window.utcClockIntervals[className];
    }

    updateUtcClockElements(className);

    const now = new Date();
    const delay = (60 - now.getUTCSeconds()) * 1000 - now.getUTCMilliseconds();

    setTimeout(() => {
        updateUtcClockElements(className);
        const intervalId = setInterval(() => {
            updateUtcClockElements(className);
        }, 60000);
        window.utcClockIntervals[className] = intervalId;
    }, Math.max(0, delay));
};

window.forceUpdateUtcClock = function (className, offsetMinutes, format) {
    window.utcClockSettings[className] = { offsetMinutes, format };
    updateUtcClockElements(className);
};



function loadDataUser(loadUser) {
    window.currentUser = loadUser;
}

window.updateNowMarker = function (lineLeftPx, imageLeftPx) {
    // Forzar el left de la línea
    window.forceCssLeftForSelector('.dxbl-sc-time-marker-line', lineLeftPx, 'forced-left-line');
    // Forzar el left de la imagen
    window.forceCssLeftForSelector('.dxbl-sc-timecells-container .dxbl-sc-time-marker-image', imageLeftPx, 'forced-left-img');
}

window.forceCssLeftForSelector = function (selector, leftPx, styleId) {
    styleId = styleId || 'forced-left-' + selector.replace(/[^a-zA-Z0-9]/g, '_');
    let prevStyle = document.getElementById(styleId);
    if (prevStyle) prevStyle.remove();

    const style = document.createElement('style');
    style.id = styleId;

    if (leftPx == -1)
        style.innerHTML = `${selector} { left: unset !important; display: none !important; }`;
    else
        style.innerHTML = `${selector} { left: ${leftPx}px !important; }`;

    document.head.appendChild(style);
}

//animacion de scroll en el menu de home al abrir
window.menuHomeScrollAnimation = function (tSel, sSel, p = 12, ms = 1600) {
    const t = document.querySelector(tSel),
        s = document.querySelector(sSel) || document.scrollingElement || document.documentElement;
    if (!t || !s) return;

    const relTop = (el, anc) => { let y = 0; for (; el && el !== anc; el = el.offsetParent) y += el.offsetTop; return y; };

    const step = () => {
        const topWithin = relTop(t, s);
        const bottom = topWithin + t.offsetHeight;
        const visTop = s.scrollTop;
        const visBottom = visTop + s.clientHeight;

        if (bottom > visBottom - p) {
            const needed = bottom - s.clientHeight + p;
            const maxTop = s.scrollHeight - s.clientHeight;
            s.scrollTop = Math.min(Math.max(needed, visTop), maxTop);
        }
    };

    let stop = false;
    (function loop() { if (stop) return; step(); requestAnimationFrame(loop); })();
    setTimeout(() => stop = true, ms);
};
    
function openUrlInNewTab(url) {
    window.open(url, '_blank');
}

window.schedulerInterop = {
    attachZoomHandler: function (element, dotNetHelper) {
        if (!element) return;

        element.addEventListener("wheel", function (e) {
            if (e.ctrlKey) {
                e.preventDefault();
                const zoomIn = e.deltaY < 0; // scroll hacia arriba
                dotNetHelper.invokeMethodAsync("ChangeZoom", zoomIn);
            }
        }, { passive: false });
    }
};

let schedulerScrollRatio = 0;

function saveSchedulerScroll(schedulerId) {
    const el = document.querySelector(`#${schedulerId} .dxbl-scroll-viewer-content`);
    if (el) {
        const maxScroll = el.scrollWidth - el.clientWidth;
        schedulerScrollRatio = maxScroll > 0 ? el.scrollLeft / maxScroll : 0;
    }
}

function restoreSchedulerScroll(schedulerId) {
    const el = document.querySelector(`#${schedulerId} .dxbl-scroll-viewer-content`);
    if (el) {
        const maxScroll = el.scrollWidth - el.clientWidth;
        el.scrollLeft = schedulerScrollRatio * maxScroll;
    }
}