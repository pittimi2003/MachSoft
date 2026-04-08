/**
 * Provides utility functions to manipulate canvas objects.
 * Includes actions like deleting, repositioning, resizing, and retrieving canvas element data.
 */

window.SetdotnetHelper = function (dotnetHelper) {
    dotnetLocalHelper = dotnetHelper;
}

// Displays the current canvas percentage text
const updateZoomDisplay = () => {
    const zoom = Canvas.getZoom();

    if (zoom <= 0.02) {
        document.getElementById('zoomPercentageText').value = `0%`;
        return;
    }

    const percentage = Math.round(zoom * 100);
    document.getElementById('zoomPercentageText').value = `${percentage}%`;
};

// Enables or disables the ZoomIn ZoomOut buttons when they reach their maximum and minimum size.
const enableDisableZoomButtons = () => {
    const zoomOutBtn = document.getElementById('zoomOutBtn');
    const totalZoom = Canvas.getZoom();
    zoomOutBtn.disabled = totalZoom <= 0.01;
};

const SetNullDrawingBrushAndMode = () => {
    Canvas.freeDrawingBrush = null;
    Canvas.isDrawingMode = false;
}

const AddingDragPainting = () => {
    Canvas.defaultCursor = "crosshair";
    SetNullDrawingBrushAndMode();
}

const SetZoomToSelection = () => {
    Canvas.defaultCursor = "zoom-in";
    SetNullDrawingBrushAndMode();
    Canvas.renderAll();
    updateZoomDisplay();
}

const deselectButton = () => {
    if (activeButton != null) {
        activeButton.classList.remove('mlx-float-menu-barTool-item-active');
        activeButton = null;
    }
    clearCursor();
    currentMode = '';
    Canvas.isDrawingMode = false;
    SelectAndEventElement(true);
}

const scaleEquipmentsWithArea = (areaGroup, scaleX, scaleY, prevWidth, prevHeight) => {
    let areaId = areaGroup.id.replace('groupArea', '');

    let equipments = Canvas.getObjects().filter(obj => obj.groupId === 'groupEquipment' + areaId || obj.groupId === 'groupProcess' + areaId);
    let stations = Canvas.getObjects().filter(obj => obj.groupId === 'groupStation' + areaId || obj.groupId === 'groupShelf' + areaId);

    equipments.forEach(equipment => {
        let newLeft = areaGroup.left + (equipment.left - areaGroup.left) * (scaleX / (prevWidth / areaGroup.width));
        let newTop = areaGroup.top + (equipment.top - areaGroup.top) * (scaleY / (prevHeight / areaGroup.height));

        equipment.set({
            left: newLeft,
            top: newTop,
            scaleX: scaleX,
            scaleY: scaleY
        });
    });

    stations.forEach(station => {
        let newLeft = areaGroup.left + (station.left - areaGroup.left) * (scaleX / (prevWidth / areaGroup.width));
        let newTop = areaGroup.top + (station.top - areaGroup.top) * (scaleY / (prevHeight / areaGroup.height));

        station.set({
            left: newLeft,
            top: newTop
        });
    });

    Canvas.requestRenderAll();
};

const moveEquipmentsWithArea = (areaGroup, deltaX, deltaY) => {
    let areaId = areaGroup.id.replace('groupArea', '');

    let objectsToMove = Canvas.getObjects().filter(obj =>
        obj.groupId === 'groupEquipment' + areaId || obj.groupId === 'groupStation' + areaId || obj.groupId === 'groupProcess' + areaId || obj.groupId === 'groupShelf' + areaId
    );

    objectsToMove.forEach(obj => {
        obj.left += deltaX;
        obj.top += deltaY;
        obj.setCoords();
    });

    Canvas.requestRenderAll();
};

function calculateUnitMeasurement(minZoom) {
    typeUnit = 1;
    if (modelData.parameterList.length > 0) {
        ParametersList = modelData.parameterList.find(m => m.name == 'UnitMeasurement').value;
        if (ParametersList === "Metric") {
            typeUnit = minZoom;
            isImperial = false;
        }
        else if (ParametersList === "Imperial") {
            typeUnit = minZoom / 3.281
            isImperial = true;
        }
    }
}

function ResetCopyPasteObject() {
    pasteCount = 0;
    copiedObjects = [];
    document.getElementById("btnCopy").disabled = true;
    document.getElementById("btnPaste").disabled = true;
}

function DesactivateButton(button) {
    button.classList.remove('mlx-float-menu-barTool-item-active');
    if (currentMode == modes.addText) {
        firstTimeWriting = true;
        Canvas.discardActiveObject();
    }

    activeButton = null;
    clearCursor();
    currentMode = '';
    Canvas.isDrawingMode = false;
}

function initializeCopyNameTracker() {
    copyNameTracker = {}; // Reiniciar el diccionario

    const validTypes = ['groupArea', 'groupStation', 'groupShelf', 'groupProcess', 'groupBuffer', 'groupEquipment'];
    const objects = Canvas.getObjects().filter(obj => validTypes.includes(obj.elementType));


    const parseName = (full) => {
        if (!full) return null;
        const cleaned = full.replace(/\s\(\d+%\)$/, '').trim(); // quita " (12%)"
        const m = cleaned.match(/^(.*?)(?:\sCopy\s(\d+))?$/);
        if (!m) return null;
        return { base: m[1].trim(), idx: parseInt(m[2], 10) || 0 };
    };

    objects.forEach(group => {

        const candidates = [
            group.baseName,      
            group.name,          
            group.data?.baseName,
            group.data?.name
        ];

        let parsed = null;
        for (const c of candidates) {
            parsed = parseName(c);
            if (parsed) break;
        }

        if (!parsed) {
            const textObj = group._objects?.find(o => typeof o.text === 'string' && o.text);
            if (textObj) parsed = parseName(textObj.text);
        }

        if (parsed) {
            const { base, idx } = parsed;
            copyNameTracker[base] = Math.max(copyNameTracker[base] || 0, idx);
        }

        if (group.elementType === 'groupProcess' && Array.isArray(group.steps)) {
            group.steps.forEach(step => {
                if (!step?.Name) return;
                const p = parseName(step.Name);
                if (p) {
                    copyNameTracker[p.base] = Math.max(copyNameTracker[p.base] || 0, p.idx);
                }
            });
        }
    });
}


function getNextAvailableCopyName(originalText) {
    // Elimina cualquier sufijo " Copy X" si existe
    let baseText = originalText.replace(/\sCopy\s\d+$/, '').trim();
    baseText = baseText.replace(/\s\(\d+%\)$/, '').trim();

    if (!copyNameTracker[baseText]) {
        const regex = new RegExp(`^${escapeRegExp(baseText)}\\sCopy\\s(\\d+)$`, 'i');

        const maxExisting = Math.max(
            0,
            ...Canvas.getObjects()
                .filter(obj =>
                    (obj.type === 'text' || obj.type === 'textbox') &&
                    typeof obj.text === 'string' &&
                    regex.test(obj.text)
                )
                .map(obj => {
                    const match = obj.text.match(regex);
                    return match ? parseInt(match[1], 10) : 0;
                })
        );

        copyNameTracker[baseText] = maxExisting;
    }

    copyNameTracker[baseText]++;
    //TODO: Revisar si requiere traducción
    return `${baseText} Copy ${copyNameTracker[baseText]}`;
}

// Escapa caracteres especiales para usar en RegExp (importante si hay paréntesis, etc.)
function escapeRegExp(text) {
    return text.replace(/[.*+?^${}()|[\]\\]/g, '\\$&');
}
function addToAcumulable(newJson) {
    try {
        let parsedAcumulable = JSON.parse(acumulable);  // Convierte acumulable a objeto JSON
        let parsedNewJson = JSON.parse(newJson);  // Convierte el nuevo JSON a objeto JSON

        // Si hay "Objects" en el nuevo JSON, los añadimos al acumulador
        if (parsedNewJson.Objects && Array.isArray(parsedNewJson.Objects)) {
            parsedAcumulable.Objects.push(...parsedNewJson.Objects);
        }

        // Si hay "Areas" en el nuevo JSON, los añadimos al acumulador
        if (parsedNewJson.Areas && Array.isArray(parsedNewJson.Areas)) {
            parsedAcumulable.Areas.push(...parsedNewJson.Areas);
        }

        if (parsedNewJson.Routes && Array.isArray(parsedNewJson.Routes)) {
            parsedAcumulable.Routes.push(...parsedNewJson.Routes);
        }

        if (parsedNewJson.Equipments && Array.isArray(parsedNewJson.Equipments)) {
            parsedAcumulable.Equipments.push(...parsedNewJson.Equipments);
        }

        if (parsedNewJson.Stations && Array.isArray(parsedNewJson.Stations)) {
            parsedAcumulable.Stations.push(...parsedNewJson.Stations);
        }

        if (parsedNewJson.Shelf && Array.isArray(parsedNewJson.Shelf)) {
            parsedAcumulable.Shelf.push(...parsedNewJson.Shelf);
        }

        if (parsedNewJson.Process && Array.isArray(parsedNewJson.Process)) {
            parsedAcumulable.Process.push(...parsedNewJson.Process);
        }

        if (parsedNewJson.ProcessDirections && Array.isArray(parsedNewJson.ProcessDirections)) {
            parsedAcumulable.ProcessDirections.push(...parsedNewJson.ProcessDirections);
        }

        // Convertimos el objeto actualizado de vuelta a JSON string
        acumulable = JSON.stringify(parsedAcumulable);
    } catch (error) {
        console.error("Error al procesar JSON:", error);
    }
}

function ChangeProcessOutColor(ResetColor = false) {
    const colorOut = ResetColor ? '#000000' : OutRouteActive ? ColorProcessOutDirections : '#000000';
    const directions = modelProcessDirections
        .filter(x => x.InitProcessIsOut)
        .flatMap(x => [x.InitProcessId, x.EndProcessId]);

    const idsToFind = new Set(directions);
    const objectsCanvas = Canvas.getObjects();

    objectsCanvas.forEach(obj => {
        const objId = obj.id?.replace('groupProcess', '');

        if (idsToFind.has(objId)) {
            obj.forEachObject(child => {
                if (child.type === 'rect') {
                    child.set({ stroke: colorOut });
                }
            });
        }
    });

    Canvas.requestRenderAll();
}

function ChangeProcessColorsByVisiblePolyline(selection) {
    const objectsCanvas = Canvas.getObjects();

    const selectedProcessIdsSet = new Set(selection?.flows?.processIds || []);
    const pdIdsSet = new Set(selection?.flows?.processDirectionIds || []);

    const paintItineraryBlack = isRelationsModeOn() && isProcessRelationsSelection(selection);
    const ColorLineMode = (getCssVariable('--sp-color-border-inverse') || '').trim() || '#000';

    const isProcessGroup = (g) => g === 'processInGroup' || g === 'processOutGroup' || g === 'processCustomGroup';

    // 1) Pintar itinerario (línea + nodos intermedios + nodos inicio/fin + flechas) color negro
    objectsCanvas.forEach(o => {
     
        if (!pdIdsSet.has(o.linkedEntityId)) return;

        // Guardar base para restaurar (objeto principal)
        if (o._baseStroke === undefined) o._baseStroke = o.stroke;
        if (o._baseFill === undefined) o._baseFill = o.fill;

        // Línea principal 
        if (o.type === 'polyline' && isProcessGroup(o.groupId) && o.visible) {
            o.set({ stroke: paintItineraryBlack ? ColorLineMode : (o._baseStroke ?? o.stroke) });
            return;
        }

        // Nodos intermedios 
        if (o.typeObject === 'controlCircle') {
            o.set({ stroke: paintItineraryBlack ? ColorLineMode : (o._baseStroke ?? o.stroke) });
            return;
        }

        // Nodo inicio/fin "simple"
        if (o.type === 'circle' && isProcessGroup(o.elementType)) {
            o.set({ stroke: paintItineraryBlack ? ColorLineMode : (o._baseStroke ?? o.stroke) });
            return;
        }

        // Flecha 
        if (o.type === 'group' && o.elementType === 'startOrEndArrow') {
            (o._objects || []).forEach(child => {
                if (child._baseStroke === undefined) child._baseStroke = child.stroke;
                child.set({ stroke: paintItineraryBlack ? ColorLineMode : (child._baseStroke ?? child.stroke) });
            });
            return;
        }
    });

    // 2) Lógica actual para colorear los PROCESOS
    const inboundIds = new Set();
    const outboundIds = new Set();
    const customboundIds = new Set();

    objectsCanvas
        .filter(d => d.type === 'polyline' && d.visible && isProcessGroup(d.groupId) && pdIdsSet.has(d.linkedEntityId))
        .forEach(direction => {
            if (direction.groupId === 'processInGroup') {
                inboundIds.add(direction.fromNode);
                inboundIds.add(direction.toNode);
            } else if (direction.groupId === 'processOutGroup') {
                outboundIds.add(direction.fromNode);
                outboundIds.add(direction.toNode);
            } else if (direction.groupId === 'processCustomGroup') {
                customboundIds.add(direction.fromNode);
                customboundIds.add(direction.toNode);
            }
        });

    objectsCanvas.forEach(obj => {
        if (obj.elementType !== 'groupProcess') return;

        const processId = obj.id?.replace('groupProcess', '');
        const baseColor = obj.stroke || ColorLineMode;

        // Guardar el color original del borde del rect 
        let rectRef = null;
        obj.forEachObject(child => {
            if (child.type === 'rect') {
                rectRef = child;
                if (child._baseStroke === undefined) child._baseStroke = child.stroke;
            }
        });

        // Si no hay rect, no hay nada que pintar
        if (!rectRef) return;

        // Por defecto, color del área
        const originalStroke = rectRef._baseStroke ?? rectRef.stroke;

        // Process principal
        const primaryProcessId =
            selection?.primaryProcessId ||
            selection?.flows?.primaryProcessId ||
            (selection?.flows?.processIds?.length ? selection.flows.processIds[0] : null);

        const forceBlackBorder = paintItineraryBlack && primaryProcessId && processId === primaryProcessId;

        let strokeColor = originalStroke;

        if (forceBlackBorder) {
            strokeColor = ColorLineMode;
        } else if (!paintItineraryBlack) {
         
            if (inboundIds.has(processId)) strokeColor = ColorProcessInDirections ?? originalStroke;
            else if (outboundIds.has(processId)) strokeColor = ColorProcessOutDirections ?? originalStroke;
            else if (customboundIds.has(processId)) strokeColor = ColorProcessCustomDirections ?? originalStroke;
        }


        rectRef.set({ stroke: strokeColor });

    });

    Canvas.requestRenderAll();
}



function ChangeProcessInColor(ResetColor = false) {
    var colorIn = ResetColor ? '#000000' : IntRouteActive ? ColorProcessInDirections : '#000000';
    const directions = modelProcessDirections
        .filter(x => x.InitProcessIsIn)
        .flatMap(x => [x.InitProcessId, x.EndProcessId]);

    const idsToFind = new Set(directions);
    const objectsCanvas = Canvas.getObjects();

    objectsCanvas.forEach(obj => {
        const objId = obj.id?.replace('groupProcess', '');

        if (idsToFind.has(objId)) {
            obj.forEachObject(child => {
                if (child.type === 'rect') {
                    child.set({ stroke: colorIn });
                }
            });
        }
    });

    Canvas.requestRenderAll();
}

function ChangeProcessCustomColor(ResetColor = false) {
    var colorIn = ResetColor ? '#000000' : CustomRouteActive ? ColorProcessCustomDirections : '#000000';
    const directions = modelProcessDirections
        .filter(x => !x.InitProcessIsIn && !x.InitProcessIsOut)
        .flatMap(x => [x.InitProcessId, x.EndProcessId]);

    const idsToFind = new Set(directions);
    const objectsCanvas = Canvas.getObjects();

    objectsCanvas.forEach(obj => {
        const objId = obj.id?.replace('groupProcess', '');

        if (idsToFind.has(objId)) {
            obj.forEachObject(child => {
                if (child.type === 'rect') {
                    child.set({ stroke: colorIn });
                }
            });
        }
    });

    Canvas.requestRenderAll();
}

function RemoveProcessDirections(isOut) {
    let objectsToRemove = Canvas.getObjects().filter(obj =>
        obj.groupId === 'processDirectionGroup' &&
        (isOut ? obj.initProcessIsOut === true : obj.initProcessIsIn === true)
    );

    if (Array.isArray(objectsToRemove) && objectsToRemove.length > 0) {
        objectsToRemove.forEach(obj => Canvas.remove(obj));
        Canvas.renderAll();
    }

    ChangeProcessOutColor();
    ChangeProcessInColor();
    ChangeProcessCustomColor();
}

function SelectAndEventElement(isSelectedEvented) {
    Canvas.getObjects().slice(1).forEach(p => {
        p.selectable = isSelectedEvented;
        p.evented = isSelectedEvented;
    })
}

function isInsideWorkArea(x, y) {
    return x >= 0 && x <= workAreaWidth && y >= 0 && y <= workAreaHeigth;
}

function isPointNearPolyline(polyline, pointer, tolerance = 6) {
    const points = polyline.get('points');
    for (let i = 0; i < points.length - 1; i++) {
        const p1 = points[i];
        const p2 = points[i + 1];
        if (isPointNearSegment(pointer, p1, p2, tolerance)) {
            return true;
        }
    }
    return false;
}

function isPointNearSegment(p, p1, p2, tolerance) {
    const dx = p2.x - p1.x;
    const dy = p2.y - p1.y;
    const lengthSquared = dx * dx + dy * dy;
    if (lengthSquared === 0) return distance(p, p1) < tolerance;

    const t = ((p.x - p1.x) * dx + (p.y - p1.y) * dy) / lengthSquared;
    if (t < 0 || t > 1) return false;

    const proj = {
        x: p1.x + t * dx,
        y: p1.y + t * dy
    };

    return distance(p, proj) < tolerance;
}

function distance(p1, p2) {
    return Math.sqrt((p1.x - p2.x) ** 2 + (p1.y - p2.y) ** 2);
}
function findObjectsInGroupArea(groupArea) {
    const groupId = groupArea.id.replace('groupArea', '');;

    return Canvas.getObjects().filter(obj => {
        return (obj.elementType === "groupStation" && obj.groupId === `groupStation${groupId}`) ||
            (obj.elementType === "groupEquipment" && obj.groupId === `groupEquipment${groupId}`) ||
            (obj.elementType === "groupShelf" && obj.groupId === `groupShelf${groupId}`);
    });
}

function ResetDrawObject() {
    deselectButton();
    Canvas.selection = true;
    Canvas.defaultCursor = 'default';
    Canvas.discardActiveObject();
}

function IsForbiddenMouse() {
    return Canvas.defaultCursor == "not-allowed";
}

// Disable the Save general button
function DisabledButtonSave(isDisabledMode) {
    document.getElementById("btnSaveDesigner").disabled = isDisabledMode ? true : false;
}

// Gets the number of objects to update or delete to show in the Save modal
function SaveMessageModal(isFromPostback = false) {
    currentMode = modes.save;
    modelLayoutDto.viewport = generateCanvasJson();

    if (modelLayoutDto.viewport != '{}' || acumulable != '{"Areas":[],"Objects":[], "Routes": [], "Equipments": [], "Stations": [], "Shelf": [], "Process": [], "ProcessDirections": []}') {

        layoutDto = fixPropertyNames(modelLayoutDto);
        const layoutDtoJson = JSON.stringify(layoutDto);

        dotnetLocalHelper.invokeMethodAsync('SaveLayoutDtoMessage', layoutDtoJson, acumulable, isFromPostback)
            .then(result => {
                if (result) {
                    selectActionAddCanvas('select');
                    SaveComponentCanvas();
                    DisabledButtonSave(true)
                }
            })
            .catch(err => {
                console.error('Error calling SaveLayoutDtoMessage:', err);
            });
    }
}


// Validates if there is content before closing the view
function BackButtonValidateMessageSave() {
    if (checkPendingChanges()) { // If either of the two arrays contains data, the following message is displayed
        dotnetLocalHelper.invokeMethodAsync('ShowModalBackButton'); // Event that shows the modal of the “Back” button
        clearRememberedView();
    }
    else {
        clearRememberedView();
        dotnetLocalHelper.invokeMethodAsync('ExitDesignerPage'); // Event that exits the page if there is nothing pending to be saved
    }
}

function checkPendingChanges() {
    const btn = document.getElementById("btnSaveDesigner");
    const saveEnabled = !!btn && btn.disabled === false;

    if (saveEnabled) {
        currentMode = modes.save; // Necessary for the generateCanvasJson method
        const viewport = generateCanvasJson(); // Gets the Viewport of unsaved elements
        return viewport !== '{}' ||
            acumulable !== '{"Areas":[],"Objects":[], "Routes": [], "Equipments": [], "Stations": [], "Shelf": [], "Process": [], "ProcessDirections": []}';
    }

    const prevMode = currentMode;
    try {
        currentMode = modes.save; // Necessary for the generateCanvasJson method
        const parse = (s) => {
            try {
                return JSON.parse(typeof s === 'string' ? (s || '{}') : JSON.stringify(s || {}));
            } catch { return {}; }
        };

        const vp = parse(generateCanvasJson()); // Gets the Viewport of unsaved elements
        const ac = parse(acumulable);

        const hasContent = (o) => {
            if (!o || typeof o !== 'object') return false;
            for (const [k, v] of Object.entries(o)) {
                if (k === 'Routes' || k === 'ProcessDirections') continue; // excluir
                if (Array.isArray(v)) { if (v.length) return true; }
                else if (v && typeof v === 'object') { if (hasContent(v)) return true; }
                else if (v != null) return true;
            }
            return false;
        };

        return hasContent(vp) || hasContent(ac);
    } finally {
        currentMode = prevMode; 
    }
}

// Back event that waits for the Save Event to finish executing before exiting
async function BackButtonSaveAndExit() {
    await SaveComponentCanvas();
}

window.showHtmlToast = function (message, duration = 3000) {
    const toast = document.getElementById("custom-toast");
    const text = document.getElementById("toast-text");

    if (toast && text) {
        text.innerText = message;
        toast.hidden = false;

        setTimeout(() => {
            toast.hidden = true;
        }, duration);
    }
}
window.showCopyToast = function (message, duration = 3000) {
    const toast = document.getElementById("copy-toast");
    const text = document.getElementById("copy-toast-text");

    if (toast && text) {
        text.innerText = message;
        toast.hidden = false;

        setTimeout(() => {
            toast.hidden = true;
        }, duration);
    }
}

window.showSelected = function (message) {
    const toast = document.getElementById("Selected-toast");
    const text = document.getElementById("Selected-toast-text");

    if (toast && text) {
        text.innerText = message;
        toast.hidden = false;
    }
}
window.hideSelectedToast = function () {
    const toast = document.getElementById("Selected-toast");
    if (toast) {
        toast.hidden = true;
    }
}
function fixPropertyNames(model) {
    let fixedModel = {};
    for (let key in model) {
        if (model.hasOwnProperty(key)) {
            let fixedKey = key.charAt(0).toUpperCase() + key.slice(1);
            if (typeof model[key] === 'object' && model[key] !== null && !Array.isArray(model[key])) {
                fixedModel[fixedKey] = fixPropertyNames(model[key]);
            }
            else {
                fixedModel[fixedKey] = model[key];
            }
        }
    }
    return fixedModel;
}

function fixPropertyNamesArray(model) {
    if (Array.isArray(model)) {
        return model.map(item => fixPropertyNames(item)); // Mantiene los arrays
    }

    let fixedModel = {};
    for (let key in model) {
        if (model.hasOwnProperty(key)) {
            let fixedKey = key.charAt(0).toUpperCase() + key.slice(1);
            if (typeof model[key] === 'object' && model[key] !== null) {
                fixedModel[fixedKey] = fixPropertyNames(model[key]); // Procesa objetos y arrays dentro
            } else {
                fixedModel[fixedKey] = model[key];
            }
        }
    }
    return fixedModel;
}

window.removeRoute = (element) => {
    modelRoutes = modelRoutes.filter(obj => obj.Id !== element.Id || obj.Id !== element.Id.replace('routeGroup', ''));
}

function deleteObjectInCanvas(element) {
    let model = JSON.parse(element);
    let objectToRemove = Canvas.getObjects().filter(obj => obj.id === 'groupArea' + model.Id || obj.groupId === 'groupStation' + model.Id || obj.groupId === 'groupProcess' + model.Id || obj.groupId === 'groupEquipment' + model.Id || obj.groupId === 'groupShelf' + model.Id);

    if (objectToRemove) {
        objectToRemove.forEach(objToRemove => Canvas.remove(objToRemove));
        Canvas.renderAll();
    }
}

function removeGroup(groupId) {
    const objectsToRemove = Canvas.getObjects().filter(obj => obj.groupId === groupId);

    if (Array.isArray(objectsToRemove) && objectsToRemove.length > 0) {
        objectsToRemove.forEach(obj => Canvas.remove(obj));
        Canvas.renderAll();
    }
}

function openDataObject(jsPosition) {

    const objAddCanvas = JSON.stringify(jsPosition);
    dotnetLocalHelper.invokeMethodAsync('EditDataObjectCanvas', objAddCanvas)
        .then(result => {
            console.log('Ok: ', result);
        })
        .catch(err => {
            console.error('Error: ', err);
        });
    currentMode = '';
    Canvas.discardActiveObject();
    deselectButton();
}


function openDataEditObject(jsPosition) {

    const objAddCanvas = JSON.stringify(jsPosition);
    dotnetLocalHelper.invokeMethodAsync('UpdateArea', objAddCanvas)
        .then(result => {
            console.log('Ok: ', result);
        })
        .catch(err => {
            console.error('Error: ', err);
        });
    currentMode = '';
    Canvas.discardActiveObject();
    deselectButton();
}

function openStationEditObject(jsPosition) {

    const objAddCanvas = JSON.stringify(jsPosition);
    dotnetLocalHelper.invokeMethodAsync('UpdateZoneFromJson', objAddCanvas)
        .then(result => {
            console.log('Ok: ', result);
        })
        .catch(err => {
            console.error('Error: ', err);
        });
    currentMode = '';
    Canvas.discardActiveObject();
    deselectButton();
}

function openShelfEditObject(jsPosition) {

    const objAddCanvas = JSON.stringify(jsPosition);
    dotnetLocalHelper.invokeMethodAsync('UpdateShelfFromJson', objAddCanvas)
        .then(result => {
            console.log('Ok: ', result);
        })
        .catch(err => {
            console.error('Error: ', err);
        });
    currentMode = '';
    Canvas.discardActiveObject();
    deselectButton();
}

function openProcessEditObject(jsPosition) {

    const objAddCanvas = JSON.stringify(jsPosition);
    dotnetLocalHelper.invokeMethodAsync('UpdateProcessFromJson', objAddCanvas)
        .then(result => {
            console.log('Ok: ', result);
        })
        .catch(err => {
            console.error('Error: ', err);
        });
    currentMode = '';
    Canvas.discardActiveObject();
    deselectButton();
}

function openDataEditRoute(jsRoute) {

    const routeAddCanvas = JSON.stringify(jsRoute);
    dotnetLocalHelper.invokeMethodAsync('UpdateRouteFromJson', routeAddCanvas)
        .then(result => {
            console.log('Ok: ', result);
        })
        .catch(err => {
            console.error('Error: ', err);
        });
    currentMode = '';
    Canvas.discardActiveObject();
    deselectButton();
}

function openDataEditEquipment(js) {
    const jsEquipment = JSON.stringify(js);
    dotnetLocalHelper.invokeMethodAsync('UpdateEquipmentFromJson', jsEquipment)
        .then(result => {
            console.log('Ok: ', result);
        })
        .catch(err => {
            console.error('Error: ', err);
        });
    currentMode = '';
    Canvas.discardActiveObject();
    deselectButton();
}

function openProcessDirectionEditObject(js) {
    const jsProcessDirection = JSON.stringify(js);
    dotnetLocalHelper.invokeMethodAsync('UpdateProcessDirectionFromJson', jsProcessDirection)
        .then(result => {
            console.log('Ok: ', result);
        })
        .catch(err => {
            console.error('Error: ', err);
        });
    currentMode = '';
    Canvas.discardActiveObject();
    deselectButton();
}

const MoveHand = () => {
    var vpt = Canvas.viewportTransform;
    if (Canvas.vptCoords.tl.x - firstMouseX + lastMouseX < 0)
        vpt[4] = 0
    else
        vpt[4] += firstMouseX - lastMouseX;
    if (Canvas.vptCoords.tl.y - firstMouseY + lastMouseY < 0)
        vpt[5] = 0
    else
        vpt[5] += firstMouseY - lastMouseY;

    Canvas.renderAll();
    reviseZoom();
    positionRulers(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
    updateScrollbarsValues(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
}

function activateHandMode() {
    if (!isPanning) {
        isPanning = true;
        Canvas.defaultCursor = 'grab';
        Canvas.hoverCursor = 'grab';
        enableHandMode();
    } else {
        resetCanvasCursor();
        disableHandMode();
        isPanning = false;
        setMainModeIcon('Select');
    }
}

function resetCanvasCursor() {
    if (!Canvas) return;
    Canvas.defaultCursor = 'default';
    Canvas.hoverCursor = 'move'; //'default'
    Canvas.upperCanvasEl.style.cursor = 'default';
}

function applyZoomWithLimits(offset, newZoom) {
    const clampedZoom = Math.min(Math.max(newZoom, minZoom), maxZoom);
    Canvas.zoomToPoint(offset, clampedZoom);
}

const enableHandMode = () => {
    Canvas.selection = false;

    Canvas.getObjects().forEach(obj => {
        if (obj.id !== 'background') {
            obj.selectable = false;
            obj.evented = true;
        }
    });
};

const disableHandMode = () => {
    Canvas.selection = true;

    Canvas.getObjects().forEach(obj => {
        if (obj.id !== 'background') {
            obj.selectable = true;
            obj.evented = true;
        }
    });
};

// Update data in RenderDesignerTabComponent
function callRecalculateRelatedElements() {
    dotnetLocalHelper.invokeMethodAsync('RenderDesignerTabComponent');
}


const getShelfColorByLocationType = (locationType) => {
    let fillColor = '#D9AFF5';
    let borderColor = '#D9AFF5';
    let textureKey = null;
    let barOpacity = 0.24;

    switch (locationType) {
        case 6: //Rack
            fillColor = getCssVariable('--sp-color-orange-200');
            borderColor = fillColor;
            textureKey = 'Rack';
            break;
        case 11: //DriveIn
            fillColor = getCssVariable('--sp-color-orange-200');
            borderColor = fillColor;
            textureKey = 'DriveIn';
            barOpacity = 1;
            break;
        case 12: //CaoticStorage
            fillColor = getCssVariable('--sp-color-orange-200');
            borderColor = fillColor;
            textureKey = 'CaoticStorage';
            barOpacity = 1;
            break;
        case 13: //AutomaticStorage
            fillColor = getCssVariable('--sp-color-orange-700');
            borderColor = fillColor; 
            textureKey = 'Automatic';
            break;
    }
    return {
        fill: texturePatterns[textureKey] || fillColor,
        stroke: borderColor,
        opacity: barOpacity
    }
};

function obtenerOrientacionDeStations(stations) {
    if (!stations || stations.length === 0) return 0;

    const st = stations.find(s =>
        s && (s.orientation !== undefined || s.Orientation !== undefined)
    );

    const orientation = st ? (st.orientation ?? st.Orientation) : 0;

    return [0, 1, 2].includes(orientation) ? orientation : 0;
}


function locateStationsWithoutCollision
(
    stations,
    element,
    sizeW,
    sizeH,
    paddingX,
    paddingY,
    stationSpacingX,
    stationSpacingY,
) {
    const orientationValue = obtenerOrientacionDeStations(stations);
 
    const splitName = (name) => {
        const match = (name || "").match(/^(.*?)(\d+)?$/);
        const prefix = (match?.[1] || "").trim();
        const num = parseInt(match?.[2] || "0", 10);
        return { prefix, num };
    };

    
    const sortedStations = [...stations].sort((a, b) => {
        const { prefix: preA, num: numA } = splitName(a.Name);
        const { prefix: preB, num: numB } = splitName(b.Name);

        if (preA < preB) return -1;
        if (preA > preB) return 1;
        return numA - numB; 
    });

  
    const estacionesExistentes = sortedStations.filter(st => st.X !== 0 || st.Y !== 0);

    let ocupados = estacionesExistentes.map(st => ({
        x1: st.X,
        y1: st.Y,
        x2: st.X + (st.Width || sizeW),
        y2: st.Y + (st.Height || sizeH)
    }));

    const startX = element.X + paddingX;
    const startY = element.Y + paddingY;

    let posIndex = 0;

    const getNextPosition = (width, height) => {
        let x, y;

        switch (orientationValue) {
            case 1: // Horizontal
                x = startX + posIndex * (width + stationSpacingX);
                y = startY;
                break;
            case 2: // Vertical
                x = startX;
                y = startY + posIndex * (height + stationSpacingY);
                break;
            default: // Grid (3 columnas)
                const cols = 3;
                const row = Math.floor(posIndex / cols);
                const col = posIndex % cols;
                x = startX + col * (width + stationSpacingX);
                y = startY + row * (height + stationSpacingY);
                break;
        }

        posIndex++;
        return { x, y };
    };

    const estacionesUbicadas = sortedStations.map(st => {
        const isNew = st.X === 0 && st.Y === 0;
        const width = st.Width || sizeW;
        const height = st.Height || sizeH;

        let posX = st.X;
        let posY = st.Y;

        if (isNew) {
            let intentos = 0;
            const maxIntentos = 100;

            // Busca una posición que no colisione
            do {
                const pos = getNextPosition(width, height);
                posX = pos.x;
                posY = pos.y;
                intentos++;
            } while (
                ocupados.some(box =>
                    posX < box.x2 &&
                    posX + width > box.x1 &&
                    posY < box.y2 &&
                    posY + height > box.y1
                ) && intentos < maxIntentos
            );

            ocupados.push({
                x1: posX,
                y1: posY,
                x2: posX + width,
                y2: posY + height
            });
        }

        return {
            ...st,
            X: posX,
            Y: posY,
            Width: width,
            Height: height
        };
    });

    return estacionesUbicadas;
}

function cleanGroupIdValue(groupId) {
    if (!groupId) return '';
    return groupId
        .replace('groupArea','')
        .replace('groupEquipment', '')
        .replace('groupStation', '')
        .replace('groupProcess', '')
        .replace('groupShelf', '');
}

function extractSelectedProps(o, customAreaId = null) {
    const areaId = customAreaId ?? o.areaId;
    if (o.elementType == 'groupProcess') {
        return {
            angle: o.angle,
            areaId: areaId,
            canvasObjectType: 'Process',
            height: o.height,
            id: generateUUID(),
            isIn: o.isIn,
            isOut: o.isOut,
            name: getNextAvailableCopyName(o.name, 'process'),
            percentageInitProcess: o.percentageInitProcess,
            processTypeId: generateUUID(),
            statusObject: 'Add',
            type: o.Type,
            width: o.width,
            x: o.left,
            y: o.top,
        }
    } else if (o.elementType == 'groupStation') {
        const baseName = o.name;
        return {
            angle: o.angle,
            areaId: areaId,
            canvasObjectType: 'Station',
            height: o.height,
            id: generateUUID(),
            isVertical: o.IsVertical ?? o.isVertical,
            locationType: o.LocationTypes,
            name: baseName ? getNextAvailableCopyName(baseName, 'station') : undefined,
            numCrossAisles: o.NumCrossAisles,
            numShelves: o.NumShelves,
            statusObject: 'Add',
            orientation: o.orientation,
            width: o.width,
            zoneType: o.zoneType
        }
    } else if (o.elementType == 'groupShelf') {
        return {
            angle: o.angle,
            areaId: areaId,
            canvasObjectType: o.tipe,
            height: o.height,
            id: generateUUID(),
            statusObject: 'Add',
            isVertical: o.IsVertical,
            locationTypes: o.LocationTypes,
            name: getNextAvailableCopyName(o.name, 'shelf'),
            numCrossAisles: o.NumCrossAisles,
            numShelves: o.NumShelves,
            width: o.width,
            zoneId: generateUUID(),
        }
    } else if (o.elementType == 'groupArea') {
        return {
            angle: o.angle,
            alternativeAreaId: o.alternativeAreaId,
            areaType: o.areaType,
            canvasObjectType: "Area",
            height: o.height,
            id: generateUUID(),
            statusObject: 'Add',
            width: o.width,
            x: o.newleft,
            y: o.newtop
        };
    }
    else if (o.elementType == 'groupStep') {
        return {
            id: generateUUID(),
            endProcess: o.EndProcess,
            initProcess: o.InitProcess,
            name: getNextAvailableCopyName(o.Name, 'steps'),
            order: o.Order
        };
    }
    else if (o.elementType == 'groupEquipment') {
        const newId = generateUUID();
        const areaId = customAreaId ?? o.areaId ?? o.AreaId;

        modelEquipments.push({
            Id: newId,
            IdOriginal: o.id,
            AreaId: areaId,
            NEquipment: o.nequipment,
            Name: getNextAvailableCopyName(o.name, 'equipment'),
            StatusObject: 'Add',
            TypeName: o.typeName
        });

        return {
            id: newId,
            idOriginal: o.id,
            areaId: areaId,
            elementType: o.elementType,
            name: getNextAvailableCopyName(o.name, 'equipment'),
            nequipment: o.nequipment,
            typeName: o.typeName,
            CanvasObjectType: o.CanvasObjectType,
            angle: o.angle
        };
    }
}

function extractOriginalSelectProps(object)
{
    //TODO: Revisar si requiere traducción
    if (object.elementType == 'groupProcess') {
        return {
            angle: object.angle,
            areaId: cleanGroupIdValue(object.groupId),
            canvasObjectType: 'Process',
            height: object.height,
            id: cleanGroupIdValue(object.id),
            isIn: object.isIn,
            isOut: object.isOut,
            name: object.name.replace(/\s*\(\d+%\)$/, ''),
            percentageInitProcess: object.percentageInitProcess,
            processTypeId: object.processTypeId,
            groupId: object.groupId,
            type: object.Type,
            width: object.width,
            x: 0,
            y: 0,
            left: 0,
            top: 0,
            statusObject: object.statusObject,
            idOriginal: object.idOriginal,
        }
    }
    else if (object.elementType == 'groupStation') {
        return {
            id: cleanGroupIdValue(object.id),
            angle: object.angle,
            canvasObjectType: 'Station',
            areaId: cleanGroupIdValue(object.groupId),
            width: object.width,
            isVertical: object.IsVertical ?? object.isVertical,
            height: object.height,
            locationType: object.LocationTypes,
            numCrossAisles: object.NumCrossAisles,
            numShelves: object.NumShelves,
            orientation: object.orientation,
            x: object.left,
            y: object.top,
            left: object.left,
            top: object.top,
            zoneType: object.zoneType,
            statusObject: object.statusObject,
            name: object._objects?.find(o => o.type === 'text')?.text || '',
            idOriginal: object.idOriginal,
        }
    }
    else if (object.elementType == 'groupShelf') {
        return {
            angle: object.angle,
            areaId: cleanGroupIdValue(object.groupId),
            canvasObjectType: object.tipe,
            height: object.height,
            id: cleanGroupIdValue(object.id),
            isVertical: object.IsVertical,
            locationTypes: object.LocationTypes,
            name: object.name,
            numCrossAisles: object.NumCrossAisles,
            numShelves: object.NumShelves,
            width: object.width,
            zoneId: object.ZoneId,
            x: object.left,
            y: object.top,
            left: object.left,
            top: object.top,
            statusObject: object.statusObject,
            idOriginal: object.idOriginal,
        }
    } 
    else if (object.elementType == 'groupArea') {
        return {
            angle: object.angle,
            areaType: object.areaType,
            alternativeAreaId: object.alternativeAreaId,
            canvasObjectType: "Area",
            x: object.left,
            y: object.top,
            left: object.left,
            top: object.top,
            width: object.width,
            height: object.height,
            statusObject: object.statusObject,
            id: cleanGroupIdValue(object.id),
            name: object.name,
            idOriginal: object.idOriginal,
        };
    }
   
    return {
        angle: object.angle,
        CanvasObjectType: object.CanvasObjectType,
        left: object.left,
        top: object.top,
        scaleX: object.scaleX,
        scaleY: object.scaleY,
        elementType: object.elementType
    };
}

function setMainModeIcon(actionType) {
    const icon = document.getElementById('modeIcon');
    const btn = document.getElementById('modeBtn');
    if (btn.hasAttribute('title')) btn.removeAttribute('title');

    icon.classList.remove('mlx-ico-cursor', 'mlx-ico-hand');

    const map = {
        select: { cls: 'mlx-ico-cursor' },
        hand: { cls: 'mlx-ico-hand' }
    };

    const cfg = map[actionType] || map.select;
    icon.classList.add(cfg.cls);
}
function showHoverOutline(target) {
    // Quita overlay previo
    if (hoverOverlay) {
        Canvas.remove(hoverOverlay);
        hoverOverlay = null;
    }
    if (!target) return;

    const group = (target.type === 'group') ? target : target.group;
    if (!group) return;

    const skipTypes = ['groupProcess', 'groupEquipment'];
    if (skipTypes.includes(group.elementType)) return;

    const bounds = group.getBoundingRect(true, true);

    hoverOverlay = new fabric.Rect({
        left: bounds.left,
        top: bounds.top,
        width: bounds.width,
        height: bounds.height,
        fill: 'rgba(0,0,0,0)',
        stroke: '#247ECC',
        strokeWidth: 2,
        rx: 5,
        ry: 5,
        selectable: false,
        evented: false,
        excludeFromExport: true,
        hoverOverlay: true,
        shadow: new fabric.Shadow({
            color: 'rgba(36, 126, 204, 0.5)',
            blur: 15,
            offsetX: 0,
            offsetY: 0
        })
    });

    Canvas.add(hoverOverlay);
    hoverOverlay.bringToFront();
    Canvas.requestRenderAll();
}

function hideHoverOutline() {
    if (hoverOverlay) {
        Canvas.remove(hoverOverlay);
        hoverOverlay = null;
        Canvas.requestRenderAll();
    }
}

// Lee los filtros activos desde window.canvasSelectionFilter
function getCurrentProcessFilterSets() {
    const selection = window.canvasSelectionFilter || {};
    const areasSelected = selection.areas || [];
    const flowsSelected = selection.flows || {};
    const processIdsSet = new Set(flowsSelected.processIds || []);
    const processTypesSet = new Set((selection.processTypeCodes || []).map(Number));
    return { areasSelected, processIdsSet, processTypesSet };
}

// Reaplica opacidad de TODOS los groupProcess según filtros vigentes
function applyProcessOpacityFromFilters() {
    const { areasSelected, processIdsSet, processTypesSet } = getCurrentProcessFilterSets();

    const hasTypeFilter = processTypesSet.size > 0;
    const hasIdFilter = processIdsSet.size > 0;
    const hasAreaFilter = (areasSelected?.length ?? 0) > 0;

    Canvas.getObjects()
        .filter(obj => obj?.elementType === 'groupProcess' && obj.visible === true)
        .forEach(obj => {
            const idByObj = obj.id?.replace('groupProcess', '');
            const areaByObj = obj.groupId?.replace('groupProcess', '');
            const objType = Number(obj.Type ?? obj.type ?? obj.processType);

            const matchesType = !hasTypeFilter || processTypesSet.has(objType);
            const matchesIdOrArea =
                (idByObj && processIdsSet.has(idByObj)) ||
                (areaByObj && areasSelected.includes(areaByObj));

            const isActive =
                matchesType && (
                    (!hasIdFilter && !hasAreaFilter) ||
                    matchesIdOrArea
                );

            obj.opacity = isActive ? opacityActive : opacityInactive;
        });

    Canvas.requestRenderAll();
}

// Enfoque temporal cuando hay selección (resalta seleccionados y atenúa los demás)
function applyProcessFocusOnSelection(selectedObjs) {
    const selected = Array.isArray(selectedObjs) ? selectedObjs : [selectedObjs];
    const selectedProcesses = selected.filter(o => o?.elementType === 'groupProcess');

    if (selectedProcesses.length === 0) {

        applyProcessOpacityFromFilters();
        return;
    }

    const selectedIds = new Set(selectedProcesses.map(o => o.id));

    Canvas.getObjects()
        .filter(obj => obj?.elementType === 'groupProcess' && obj.visible === true)
        .forEach(obj => {
            obj.opacity = selectedIds.has(obj.id) ? opacityActive : opacityInactive;
        });

    Canvas.requestRenderAll();
}
function autoPlaceArea(groupArea, areaChildGroups, isNewAtOrigin) {
    if (!Canvas) return;
    if (!isNewAtOrigin) return;

    // Tomamos solo las áreas ya dibujadas en el canvas
    const existingAreas = Canvas.getObjects().filter(o => o.elementType === 'groupArea');

    // Si no hay ninguna, dejamos esta en su posición actual
    if (!existingAreas || existingAreas.length === 0) return;

    const paddingY = 40; // espacio entre áreas

    // Buscamos el área que esté más abajo
    let bottomMostArea = existingAreas[0];
    let maxBottom = bottomMostArea.top + bottomMostArea.height * (bottomMostArea.scaleY || 1);

    existingAreas.forEach(a => {
        const bottom = a.top + a.height * (a.scaleY || 1);
        if (bottom > maxBottom) {
            maxBottom = bottom;
            bottomMostArea = a;
        }
    });

    // Alineamos todas las áreas en la misma "columna" X que la primera/última
    const targetLeft = bottomMostArea.left;
    const targetTop = maxBottom + paddingY;

    const deltaX = targetLeft - groupArea.left;
    const deltaY = targetTop - groupArea.top;

    // Movemos el área
    groupArea.set({
        left: groupArea.left + deltaX,
        top: groupArea.top + deltaY
    });
    groupArea.setCoords();

    // Movemos todos los grupos hijos (zones, shelves, processes, equipments)
    areaChildGroups.forEach(g => {
        g.set({
            left: g.left + deltaX,
            top: g.top + deltaY
        });
        g.setCoords();
    });
}

function resolveZonePastePlacement(objZone, area, selectedArea, offset, pasteCount) {
    let newLeft = 0;
    let newTop = 0;
    let modelArea = {};
    let destinationArea = null;
    let usedStairPlacementLocal = false;

    // Obtenemos coordenadas actuales del cursor
    const usePointerPlacement =
        lastPointerValid &&
        lastPointerCanvas.x !== null &&
        lastPointerCanvas.y !== null;

    const pointerX = lastPointerCanvas.x;
    const pointerY = lastPointerCanvas.y;

    let candidateArea = area;

    if (selectedArea && cleanGroupIdValue(selectedArea.id) !== cleanGroupIdValue(area.id)) {
        if (selectedArea.areaType !== area.areaType) {
            window.showSelected(L('The type of zone does not correspond to this area.'));
            return { cancel: true };
        }
        candidateArea = selectedArea;
    }

    // Calculamos la posición de la copia
    if (usePointerPlacement && candidateArea) {
        // Pegamos zona en escalera cuando no se tengan coordenadas en cursor
        newLeft = pointerX;
        newTop = pointerY;

        modelArea = extractOriginalSelectProps(candidateArea);
        destinationArea = candidateArea;
    } else {
        // Pegado en escalera
        newLeft = (objZone.left || 0) + offset * (pasteCount + 1);
        newTop = (objZone.top || 0) + offset * (pasteCount + 1);

        modelArea = extractOriginalSelectProps(candidateArea);
        destinationArea = candidateArea;

        usedStairPlacementLocal = true;
    }

    return {
        cancel: false,
        newLeft,
        newTop,
        modelArea,
        destinationArea,
        usedStairPlacement: usedStairPlacementLocal
    };
}

window.setCanvasRelationsMode = function (dotNetRef, isOn) {
    if (!dotNetRef) return;
    dotNetRef.invokeMethodAsync('OnCanvasRelationsModeChanged', isOn);
};

window.registerDesignerDotNetRef = function (dotNetRef) {
    window.dotNetRefDesigner = dotNetRef;
};

window.exitRelationsModeIfNeeded = function () {
    if (!window.dotNetRefDesigner) return;
    window.setCanvasRelationsMode(window.dotNetRefDesigner, false);
};

window.canvasRelationsState = {
    isOn: false,
    primaryAreaId: null
};

// Encender o apagar el modo relaciones
window.setRelationsState = function (isOn, primaryAreaId = null) {
    window.canvasRelationsState = window.canvasRelationsState || { isOn: false, primaryAreaId: null };

    const wasOn = !!window.canvasRelationsState.isOn;

    window.canvasRelationsState.isOn = !!isOn;
    window.canvasRelationsState.primaryAreaId = isOn ? primaryAreaId : null;

    // Venimos de modo relaciones y ahora se apaga
    if (wasOn && !isOn) {

        // Apagar modo relaciones en Blazor (restaurar tabs/filtros base)
        window.exitRelationsModeIfNeeded?.();

        // Limpiar el “detalle” seleccionado en UI
        if (window.dotNetRefDesigner) {
            window.dotNetRefDesigner.invokeMethodAsync('ClearCanvasSelectionState');
        }

        // Restaurar visual del canvas al filtro base
        if (window.baseCanvasSelection) {
            window.applyCanvasSelection(window.baseCanvasSelection);
        } else {        
            window.canvasSelectionFilter = null;
            window.forceProcessDirectionsBlack = false;
            Canvas?.requestRenderAll?.();
        }
    }
};

window.exitRelationsMode = function () {
    // 1) Apaga estado JS
    if (window.canvasRelationsState) {
        window.canvasRelationsState.isOn = false;
        window.canvasRelationsState.primaryAreaId = null;
    }

    // 2) Restaura interacción en todos los objetos
    if (Canvas) {
        Canvas.getObjects().forEach(o => {
            if (o.__origInteractivity) {
                o.selectable = o.__origInteractivity.selectable;
                o.evented = o.__origInteractivity.evented;
                o.hasControls = o.__origInteractivity.hasControls;
                o.hasBorders = o.__origInteractivity.hasBorders;
            }
        });
    }
};

function mergeById(targetArray, incomingArray, idKey = "Id") {
    if (!Array.isArray(incomingArray) || incomingArray.length === 0) return targetArray;

    if (!Array.isArray(targetArray)) targetArray = [];

    incomingArray.forEach(item => {
        const id = item?.[idKey];
        if (!id) return;

        const idx = targetArray.findIndex(x => x?.[idKey] === id);
        if (idx === -1) targetArray.push(item);
        else targetArray[idx] = { ...targetArray[idx], ...item };
    });

    return targetArray;
}

window.unregisterDesignerDotNetRef = function () {
    window.dotNetRefDesigner = null;
};

function isProcessDirectionObj(obj) {

    return obj && (
        obj.groupId === 'processOutGroup' ||
        obj.groupId === 'processInGroup' ||
        obj.groupId === 'processDirectionGroup' ||
        obj.groupId === 'processCustomGroup'
         
    );
}

function getProcessIdsFromProcessDirection(obj) {
    if (!obj) return { fromProcessId: null, toProcessId: null };

    // GUID directos
    const fromProcessId = obj.fromNode || obj.fromProcessId || (obj.data?.fromNode) || (obj.data?.fromProcessId) || null;
    const toProcessId = obj.toNode || obj.toProcessId || (obj.data?.toNode) || (obj.data?.toProcessId) || null;

    return { fromProcessId, toProcessId };
}

function findProcessObjById(processId) {
    if (!Canvas || !processId) return null;

    return Canvas.getObjects().find(o =>
        o.elementType === 'groupProcess' &&
        ((o.id || '').replace('groupProcess', '') === processId)
    ) || null;
}

function getAreaIdFromProcessObj(obj) {
    if (!obj?.groupId) return null;
    return (obj.groupId || '').replace('groupProcess', '');
}

function getAreaIdFromProcessId(processId) {
    const p = findProcessObjById(processId);
    return p ? getAreaIdFromProcessObj(p) : null;
}

function findProcessDirectionObjsTouchingProcess(processId) {
    if (!Canvas || !processId) return [];

    return Canvas.getObjects().filter(o => {
        if (!isProcessDirectionObj(o)) return false;
        if (!o.linkedEntityId) return false;

        const { fromProcessId, toProcessId } = getProcessIdsFromProcessDirection(o);
        return fromProcessId === processId || toProcessId === processId;
    });
}
function isRelationsTriggerObject(obj) {
    if (!obj) return false;
    return obj.elementType === 'groupArea' || obj.elementType === 'groupProcess';
}

window.selectProcessOnCanvasById = function (processId) {
    if (!Canvas || !processId) return;

    const obj = Canvas.getObjects().find(o =>
        o.elementType === 'groupProcess' &&
        ((o.id || '').replace('groupProcess', '') === processId)
    );

    if (!obj) return;

    Canvas.setActiveObject(obj);
    Canvas.requestRenderAll();
};
