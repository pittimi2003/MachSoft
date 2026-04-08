/**
 * Contains all logic for rendering and drawing objects onto the canvas.
 * Includes custom functions to create shapes, stations, areas, and any visual components.
 */

// Padding interno (en píxeles) que dejará el área por cada lado al envolver estaciones
const AREA_PADDING = 10;
//TODO: Revisar si requiere traducción
//TODO: En este archivo hay muchas propiedades "Name", preguntar si es necesaria la traduccion.

window.isVisibleOnSelection = function (elementType, id, idArea = undefined) {
    if (!window.canvasSelectionFilter) return true;
    
    const f = window.canvasSelectionFilter;
    if (elementType === 'groupArea') return f.areas.includes(id) || f.flows.areaIds.includes(id);
    if (elementType === 'groupProcess') return f.flows.processIds.includes(id) || f.areas.includes(idArea) || f.flows.areaIds.includes(idArea);
    return true;
};

const addElementType = (type) => {

    var addCorrectly = false;
    var minXAbsolute = Math.min(firstMouseXAbsolute, lastMouseXAbsolute);
    var minYAbsolute = Math.min(firstMouseYAbsolute, lastMouseYAbsolute);
    var maxXAbsolute = Math.max(firstMouseXAbsolute, lastMouseXAbsolute);
    var maxYAbsolute = Math.max(firstMouseYAbsolute, lastMouseYAbsolute);

    if (Canvas.getActiveObjects().length == 0) {
        if (!addCorrectly) {
            switch (type) {
                case modes.addText:
                    drawTextBox(lastMouseXAbsolute, lastMouseYAbsolute, 'Add');
                    addCorrectly = true;
                    DisabledButtonSave(true);
                    break;
                case modes.addSquare:
                    drawSquare(minXAbsolute, minYAbsolute, maxXAbsolute, maxYAbsolute, 'Add');
                    addCorrectly = true;
                    currentMode = '';
                    Canvas.discardActiveObject();
                    DisabledButtonSave(false);
                    deselectButton();
                    break;
                case modes.addLineStraight:
                    addCorrectly = true;
                    DisabledButtonSave(false);
                    if (line) normalizeNewLineUsingCurrentCenter(line);
                    deselectButton();
                    Canvas.selection = true;
                    Canvas.bringToFront(line);
                    Canvas.defaultCursor = 'default';
                    line = null;
                    Canvas.discardActiveObject();
                    break;
                default:
                    addCorrectly = false;
                    break;
            }
        }
    }
}

const selectActionAddCanvas = (actionType) => {
    Canvas.defaultCursor = "auto";
    const button = document.getElementById("btn" + actionType);
    Canvas.renderAll();

    setMainModeIcon(actionType);
    if (actionType == 'select') {
        currentMode = '';
        isPanning = true;
        activateHandMode();
        if (activeButton)
            DesactivateButton(activeButton);
        return;
    }

    if (button == null && actionType === 'zoomToSelection') {
        SelectAndEventElement(false);
        currentMode = modes.zoomToSelection;
        SetZoomToSelection();
        return;
    }

    if (button && actionType === 'hand') {
        activateHandMode();
        currentMode = modes[actionType];

        return;
    } else {
        isPanning = true;
        activateHandMode();
    }

    if (activeButton && activeButton !== button) {
        DesactivateButton(activeButton);
    }

    if (button.classList.contains("mlx-float-menu-barTool-item-active")) {
        DesactivateButton(button);
        SelectAndEventElement(true);
    }
    else {

        button.classList.add('mlx-float-menu-barTool-item-active');
        activeButton = button;
        Canvas.isDrawingMode = true;

        const actionsWithDrawing = [
            'addText', 'addSquare', 'addLineStraight', 'addArea', 'addStorage', 'addStage', 'addDock', 'addAisle', 'addBuffer', 'addShelf', 'addStage'
        ];

        if (actionsWithDrawing.includes(actionType)) {
            currentMode = modes[actionType];
            AddingDragPainting();
        } else {
            currentMode = modes[actionType];
        }

        SelectAndEventElement(false);
    }
};

const AddingText = () => {
    firstTimeWriting = true;
    Canvas.defaultCursor = "text";
    SetNullDrawingBrushAndMode();
}

const drawTextBox = (x, y, statusObject, text = '', width = 50, height = 10, scaleX = 1, scaleY = 1, fontSize = 12, id = null, angle) => {
    //  Generar ID si no se proporciona
    const textboxId = id || generateUUID();
    var textbox = new fabric.Textbox(text, {
        id: textboxId,
        left: x,
        top: y,
        width: width,
        height: height,
        fontFamily: 'Segoe UI',
        fontStyle: 'normal',
        fontWeight: 600,
        fontSize: fontSize,
        textAlign: "left",
        elementType: 'Text',
        hasControls: true,
        selectable: true,
        lockScalingFlip: true,
        scaleX: scaleX,
        scaleY: scaleY,
        statusObject: statusObject,
        borderColor: '#247ECC',
        angle: angle ?? 0
    });
    textbox.baseFontSize = fontSize;
    textbox.on('modified', () => {

        const newW = Math.max(10, textbox.width * (textbox.scaleX || 1));
        const newH = Math.max(10, textbox.height * (textbox.scaleY || 1));

        textbox.set({
            width: newW,
            height: newH,
            scaleX: 1,
            scaleY: 1,
            fontSize: textbox.baseFontSize
        });
        if (textbox.statusObject === 'StoredInBase') {
            textbox.statusObject = 'Update';
        }
        textbox.setCoords();
        Canvas.requestRenderAll();
    });
    if (text == '') {
        Canvas.add(textbox).setActiveObject(textbox);
        textbox.enterEditing();
        textbox.hiddenTextarea.focus();
        Canvas.renderAll();
    } else {
        Canvas.add(textbox);
    }
}

const drawElementRect = (minX, minY, maxX, maxY, elementType, color, borderColor, borderWidth, opacity, id = '', width = 0, height = 0, statusObject, angle) => {
    if (typeof maxX === 'undefined' || typeof maxY === 'undefined') {
        maxX = minX + 1;
        maxY = minY + 1;
    }

    var rect = new fabric.Rect({
        left: minX,
        top: minY,
        width: width === 0 ? maxX - minX : width,
        height: height === 0 ? maxY - minY : height,
        angle: angle ?? 0,
        fill: color,
        id: id === '' ? generateUUID() : id,
        name: '',
        description: '',
        elementType: elementType,
        opacity: opacity,
        stationID: '',
        stroke: borderColor,
        strokeWidth: borderWidth,
        selectable: true,
        hasControls: true,
        statusObject: statusObject,
        rx: 5,
        ry: 5,
        borderColor: '#247ECC'
    });

    if (elementType === 'Square') {
        rect.set({
            perPixelTargetFind: true, 
            fill: 'rgba(0,0,0,0)',    
            strokeWidth: borderWidth,
        });
        Canvas.add(rect);
        Canvas.requestRenderAll();
    } else {
        return rect;
    }

}

const drawElementLine = (minX, minY, maxX, maxY, left, top, statusObject, id = null, angle) => {
    const lineId = id || generateUUID();
    line = new fabric.Line([minX, minY, maxX, maxY], {
        id: lineId,
        stroke: '#2E4052',
        left: left,
        top: top,
        strokeWidth: 2,
        elementType: 'LineStraight',
        selectable: true,
        statusObject: statusObject,
        borderColor: '#247ECC',
        angle: angle ?? 0,
        originX: 'center',
        originY: 'center',
    });
    Canvas.add(line);
    attachLineTransformHandlers(line);
}

function DrawTextRectArea(rect, text, id, color, positionText, fontSize) {
    const rectLeft = rect.left || 0;
    const rectTop = rect.top || 0;
    const rectWidth = rect.width * (rect.scaleX || 1);
    const rectHeight = rect.height * (rect.scaleY || 1);

    // fallback font size
    const finalFontSize = fontSize || 12;

    let textLeft;
    let textTop;

    if (positionText === 'Center') {
        textLeft = rectLeft + rectWidth / 2;
        textTop = rectTop + rectHeight / 2;
    } else if (positionText === 'CenterLower') {
        textLeft = rectLeft + rectWidth / 2;
        textTop = rectTop + rectHeight - 13.5;
    } else if (positionText === 'LeftLower') {
        textLeft = rectLeft + 1;
        textTop = rectTop + rectHeight - 13.5;
    } else if (positionText === 'MiddleLeft') {
        textLeft = rectLeft + 5;
        textTop = rectTop + rectHeight / 2;
    } else if (positionText === 'TopCenter') {
        textLeft = rectLeft + rectWidth / 2;
        textTop = rectTop + 20;
    } else if (positionText === 'TopLeft') {
        textLeft = rectLeft;
        textTop = rectTop;
    }


    const maxTextWidth = rectWidth - 20;
    const { displayedText, fullText } = getTruncatedText(text, maxTextWidth, finalFontSize);


    const fabricText = new fabric.Text(displayedText, {
        id: 'text' + id,
        left: textLeft,
        top: textTop,
        fontSize: finalFontSize,
        fontFamily: 'Segoe UI',
        fontStyle: 'normal',
        fontWeight: 600,
        selectable: false,
        fill: color,
        originX: 'center',
        originY: 'center'
    });

    fabricText.fullText = fullText;
    return fabricText;
}

function DrawTextRect(rect, text, id, color, positionText, fontSize) {
    const rectLeft = rect.left || 0;
    const rectTop = rect.top || 0;
    const rectWidth = rect.width * (rect.scaleX || 1);
    const rectHeight = rect.height * (rect.scaleY || 1);

    // fallback font size
    const finalFontSize = fontSize || 12;

    let textLeft;
    let textTop;

    if (positionText === 'Center') {
        textLeft = rectLeft + rectWidth / 2;
        textTop = rectTop + rectHeight / 2;
    } else if (positionText === 'CenterLower') {
        textLeft = rectLeft + rectWidth / 2;
        textTop = rectTop + rectHeight - 13.5;
    } else if (positionText === 'LeftLower') {
        textLeft = rectLeft + 1;
        textTop = rectTop + rectHeight - 13.5;
    } else if (positionText === 'MiddleLeft') {
        textLeft = rectLeft + 5;
        textTop = rectTop + rectHeight / 2;
    } else if (positionText === 'TopCenter') {
        textLeft = rectLeft + rectWidth / 2;
        textTop = rectTop + 20;
    } else if (positionText === 'TopLeft') {
        textLeft = rectLeft;
        textTop = rectTop;
    }

    const fabricText = new fabric.Text(text, {
        id: 'text' + id,
        left: textLeft,
        top: textTop,
        fontSize: finalFontSize,
        fontFamily: 'Segoe UI',
        fontStyle: 'normal',
        fontWeight: 600,
        selectable: false,
        fill: color,
        originX: 'center',
        originY: 'center'
    });

    return fabricText;
}


const drawSquare = (minX, minY, maxX, maxY, statusObject, id = '', width = 0, height = 0, angle) => {
    drawElementRect(minX, minY, maxX, maxY, 'Square', colorSquare, borderSquare, 2, 0.9, id, width, height, statusObject, angle);
}

const drawRectangleGroup = (element, color, borderColor, textPosition = 'TopCenter', customFontSize = 10) => {
    const width = (element.Width && element.Width > 0) ? element.Width : 150;
    const height = (element.Height && element.Height > 0) ? element.Height : 100;
    const statusObject = element.StatusObject === 'Add' ? 'Add' :element.StatusObject === 'Update' ? 'Update' :'StoredInBase';

    const rect = drawElementRect(
        0,
        0,
        width,
        height,
        element.CanvasObjectType,
        color,
        borderColor,
        2,
        opacityZones,
        element.Id,
        width,
        height,
        statusObject
    );

    rect.set({
        left: 0,
        top: 0,
        originX: 'left',
        originY: 'top'
    });

    const text = DrawTextRectArea(rect, element.Name, element.Id, textColorArea, textPosition, customFontSize);
    const nameGroup = 'group' + element.CanvasObjectType;
    const idGroup = nameGroup + (element.AreaId === undefined ? element.Id : element.AreaId);
    const group = new fabric.Group([rect, text], {
        id: nameGroup + element.Id,
        groupId: idGroup,
        elementType: nameGroup,
        areaType: element.AreaType,
        name: element.Name,
        left: element.X,
        top: element.Y,
        width: element.Width,
        height: element.Height,
        originX: 'left',
        originY: 'top',
        lockRotation: true,
        hasControls: false,
        scalable: false,
        lockMovementX: false,
        lockMovementY: false,
        statusObject: statusObject,
        zoneType: element.ZoneType || null,
        orientation: element.Orientation,
        angle: element.Angle,
        selectable: true,
        hoverCursor: 'default',
        borderColor: 'black',
        alternativeAreaId: element.AlternativeAreaId,
        borderScaleFactor: 2
    });


    return group;
};

const drawRectangleGroupStations = (element, color, borderColor, textPosition = 'TopCenter', fillOpacity, customFontSize = 10) => {
    const width = (element.Width && element.Width > 0) ? element.Width : 150;
    const height = (element.Height && element.Height > 0) ? element.Height : 100;
    const rect = drawElementRect(
        0,
        0,
        width,
        height,
        element.CanvasObjectType,
        color,
        borderColor,
        2,
        fillOpacity,
        element.Id,
        width,
        height,
        'StoredInBase'
    );

    rect.set({
        left: 0,
        top: 0,
        originX: 'left',
        originY: 'top'
    });

    const text = DrawTextRect(rect, element.Name, element.Id, textColorArea, textPosition, customFontSize);
    const nameGroup = 'group' + element.CanvasObjectType;
    const idGroup = (nameGroup === 'groupStation' && element.ZoneType !== 0 ? 'groupStation' : 'groupShelf') + (element.AreaId === undefined ? element.Id : element.AreaId);
    const group = new fabric.Group([rect, text], {
        id: (nameGroup === 'groupStation' && element.ZoneType !== 0 ? 'groupStation' : 'groupShelf') + element.Id,
        groupId: idGroup,
        elementType: nameGroup === 'groupStation' && element.ZoneType !== 0 ? 'groupStation' : 'groupShelf',
        areaType: element.AreaType,
        name: element.Name,
        left: element.X,
        top: element.Y,
        width: element.Width,
        height: element.Height,
        originX: 'left',
        originY: 'top',
        lockRotation: true,
        hasControls: false,
        scalable: false,
        lockMovementX: false,
        lockMovementY: false,
        statusObject: 'StoredInBase',
        zoneType: element.ZoneType || null,
        orientation: element.Orientation,
        angle: element.Angle !== undefined ? element.Angle : 0,
        selectable: true,
        hoverCursor: 'move',
        borderColor: '#247ECC'  
    });


    return group;
};


/**
 * drawStation:
 *   - Para cada station en “stations”, si station.X/Y = 0 → la coloca en el siguiente hueco de la rejilla (grid).
 *   - Inyecta luego en el propio objeto station { X, Y, Width, Height } si antes eran ceros.
 *   - Crea un fabric.Group( rect + texto ) único para esa estación.
 *   - Cada grupo añade listeners:
 *       • 'modified' → recalcula el área contenedora.
 *       • 'scaling'  → mantiene el texto con tamaño fijo (fuera de la escala del grupo).
 *   - Si station.ZoneType === 1 (Dock), el fondo lleva un patrón de franjas.
 */

const getZoneColorByZoneType = (ZoneType) => {
    let fillColor = '#C3CEDC';
    let borderColor = '#C3CEDC';
    let textureKey = null;
    let barOpacity = 0.24;

    switch (ZoneType) {
        case 1: //Rack
            fillColor = getCssVariable('--sp-color-smoke-300');
            borderColor = fillColor;
            textureKey = 'Dock';
            barOpacity = 1;
            break; 
    }
    return {
        fill: texturePatterns[textureKey] || fillColor,
        stroke: borderColor,
        opacity: barOpacity
    }
};

const drawStation = (element, stations, color, borderColor) => {


    const paddingX = 10;
    const paddingY = 40;
    const stationSpacingX = 20;
    const stationSpacingY = 20;
    const sizeW = sizeStationWidth;
    const sizeH = sizeStationHeight;
    const finalSatations = locateStationsWithoutCollision(
        stations,
        element,
        sizeW,
        sizeH,
        paddingX,
        paddingY,
        stationSpacingX,
        stationSpacingY
    );

    const stationGroups = finalSatations.map((station, index) => {

        const isNew = station.X === 0 && station.Y === 0;

        let width = station.Width || sizeW;
        let height = station.Height || sizeH;

        let posX = station.X;
        let posY = station.Y;

        if (isNew) {
            const libre = buscarPosicionLibre(width, height);
            posX = libre.x;
            posY = libre.y;

            // Marcar como ocupado para las siguientes nuevas
            ocupados.push({
                x1: posX,
                y1: posY,
                x2: posX + width,
                y2: posY + height
            });
        }

        const stationElement = {
            ...station,
            X: posX,
            Y: posY,
            Width: width,
            Height: height
        };
        if (station.X === 0 || station.Y === 0) {
            stationElement.Width = sizeW;
            stationElement.Height = sizeH;
        }

        let fillOption = color;  
        let fillOpacity = opacityZones;
        /* 7-b) Patrón de relleno especial si ZoneType === 1 (dock) */
        if (station.ZoneType === 1) {
            const { fill, stroke, opacity } = getZoneColorByZoneType(station.ZoneType);
                fillOption = fill;    
                borderColor = stroke;
                fillOpacity = opacity;
        } else if (station.LocationTypes == 12 || station.LocationTypes == 13)
        {
            const { fill, stroke, opacity } = getShelfColorByLocationType(station.LocationTypes);
            fillOption = fill;       // Pattern con fondo + textura
            borderColor = stroke;
            fillOpacity = opacity;
        }
        let group = drawRectangleGroupStations(stationElement, fillOption, borderColor, 'Center', fillOpacity);
        /* Crea el fabric.Group de la estación */
        group.set({
            statusObject: stationElement.StatusObject == '' ? 'StoredInBase' : stationElement.StatusObject,
            selectable: true,
            hasControls: true,
            lockMovementX: false,
            lockMovementY: false,
            lockScalingX: false,
            lockScalingY: false,
            hoverCursor: 'default',
            LocationTypes: station.LocationTypes,
            elementType: group.elementType,
            ZoneId: station.ZoneId,
            NumShelves: station.NumShelves,
            IsVertical: station.IsVertical,
            NumCrossAisles: station.NumCrossAisles,
            ZoneType: station.ZoneType,
            tipe: station.Tipe,
            idOriginal: station.IdOriginal,
        });
        /* =========================================================
         Handler MODIFIED → resize / mover estación
         ========================================================= */
        group.on('modified', function () {
            //Cambio de Tamaño Zonas
            resizeStationGroup(group, stationElement, element);
            //Cambio de tamaño de area y movimiento de Zonas
            resizeGroupAreaFromStations(element.Id);
    });
        group.on('selected', function () {
            group.set({
                stroke: 'blue',
                strokeWidth: 4
            });
            Canvas.requestRenderAll();
        });

        group.on('deselected', function () {
            group.set({
                stroke: borderColor,
                strokeWidth: 2
            });
            Canvas.requestRenderAll();
        });
        return group;
    });
    return stationGroups;
};

const drawArea = (element, color, borderColor, stations, process, shelfs = [], steps) => {
    let stationGroups = [];

    const allGroups = [];
    const isNewAtOrigin = (element.X === 0 && element.Y === 0);

    if (shelfs && shelfs.length > 0) {
        shelfs.forEach(sh => {
            const subType = (sh.SubType ?? sh.CanvasObjectType ?? '').toLowerCase();
            const tempElement = { ...element };
            if (subType === 'chaoticstorage' || subType === 'automaticstorage') {
                const fakeStation = {
                    ...sh,
                    CanvasObjectType: 'station',
                    Tipe: sh.CanvasObjectType ?? '',
                    LocationTypes: sh.LocationTypes,
                    ZoneType: sh.zoneType,
                    ZoneId: sh.ZoneId,
                    IsVertical: sh.IsVertical,
                    NumShelves: sh.NumShelves,
                };
                const source = [...(stations || []), fakeStation];
                allGroups.push(...drawStation(tempElement, source, color, borderColor));
            } else {
                const shelfArray = Array.isArray(sh) ? sh : [sh];
                allGroups.push(...drawShelf(tempElement, shelfArray, borderColor, color));
            }
        });
    } else if (stations && stations.length > 0) {
        if (stations[0].ZoneType !== 4) {
            stationGroups = drawStation(element, stations, color, borderColor);
            allGroups.push(...stationGroups);
        }
    }
    if (allGroups.length > 0) {
        let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;

        allGroups.forEach(group => {
            const left = group.left;
            const top = group.top;
            const right = left + group.width * (group.scaleX || 1);
            const bottom = top + group.height * (group.scaleY || 1);

            minX = Math.min(minX, left);
            minY = Math.min(minY, top);
            maxX = Math.max(maxX, right);
            maxY = Math.max(maxY, bottom);
        });

        //Márgen
        const marginTop = 45;
        const marginRight = 8;
        const marginLeft = 8;
        const marginBottomBase = 8;


        const hasProcesses = Array.isArray(process) && process.length > 0;
        const processHeight = 5;
        const processGap = 1;
        const marginBottom = hasProcesses
            ? marginBottomBase + processHeight + processGap
            : marginBottomBase;


        element.X = minX - marginLeft;
        element.Y = minY - marginTop;
        element.Width = (maxX - minX) + marginLeft + marginRight;
        element.Height = (maxY - minY) + marginTop + marginBottom;
    }

    const groupArea = drawRectangleGroup(element, color, borderColor, 'TopCenter');
    groupArea.visible = window.isVisibleOnSelection('groupArea', element.Id);
    if (updateData) groupArea.statusObject = "Update";

    // ***Acomodamos el área si viene en (0,0)***

    Canvas.add(groupArea);
    autoPlaceArea(groupArea, allGroups, isNewAtOrigin);
    // 4. Agregamos los grupos generados (stations o shelves)
    allGroups.forEach(group => Canvas.add(group));

    // 5. Equipamientos y procesos
    modelEquipments
        .filter(obj => obj.AreaId === groupArea.id.replace('groupArea', ''), )
        .forEach(obj => drawEquipment(groupArea, obj, borderColor));

    drawProcess(groupArea, process, steps, borderColor);

    // 6. Eventos de movimiento y escalado del área
    let prevWidth = groupArea.width;
    let prevHeight = groupArea.height;

    groupArea.on('mousedown', () => {
        groupArea.prevLeft = groupArea.left;
        groupArea.prevTop = groupArea.top;
    });

    groupArea.on('moving', () => {
       
        if (groupArea.prevLeft === undefined || groupArea.prevTop === undefined) {
            groupArea.prevLeft = groupArea.left;
            groupArea.prevTop = groupArea.top;
            return; 
        }

        const deltaX = groupArea.left - groupArea.prevLeft;
        const deltaY = groupArea.top - groupArea.prevTop;

        moveEquipmentsWithArea(groupArea, deltaX, deltaY);

        const elementsInArea = Canvas.getObjects().filter(obj =>
            ( obj.elementType === 'groupStation' ||obj.elementType === 'groupShelf' ||obj.elementType === 'groupProcess' ||obj.elementType === 'groupEquipment') &&
            (obj.groupId === 'groupStation' + element.Id ||obj.groupId === 'groupShelf' + element.Id ||obj.groupId === 'groupProcess' + element.Id ||obj.groupId === 'groupEquipment' + element.Id
            )
        );

        elementsInArea.forEach(el => {
            if (el.statusObject === "StoredInBase") {
                el.set({ statusObject: "Update" });
            }

            el.setCoords();
        });

        groupArea.prevLeft = groupArea.left;
        groupArea.prevTop = groupArea.top;
    });

    groupArea.on('scaling', () => {
        const scaleX = groupArea.scaleX || 1;
        const scaleY = groupArea.scaleY || 1;

        scaleEquipmentsWithArea(groupArea, scaleX, scaleY, prevWidth, prevHeight);

        prevWidth = groupArea.width * scaleX;
        prevHeight = groupArea.height * scaleY;
    });

    Canvas.requestRenderAll();
};

const drawEquipment = (element, equipment, borderColor) => {
    const typename = equipment.typename ?? equipment.TypeName;
    const nequipment = equipment.nequipment ?? equipment.NEquipment;
    const typeKey = typename != 'Carretilla' && typename != 'RF' ? 'other' : (typename || '').toString().trim().toLowerCase();

    const iconByType = {
        carretilla: 'equipment.svg',
        rf: 'rf.svg',
        other: 'equipmentNA.svg',
    };
    const iconName = iconByType[typeKey];
    const equipmentWidth = 40, equipmentHeight = 40, padding = 3;

    const buildFallbackGlyph = () => {
        const FALLBACK_SCALE = 0.75;
        const radius = ((Math.min(equipmentWidth, equipmentHeight) - padding * 2) / 2) * FALLBACK_SCALE;

        const circ = new fabric.Circle({
            radius,
            fill: '#FFFFFF',
            stroke: '#344859',
            strokeWidth: 2,
            originX: 'center',
            originY: 'center',
            left: 0,
            top: 0,
            selectable: false
        });

        const q = new fabric.Text('?', {
            fontSize: Math.round(radius * 0.9),
            fontWeight: 'bold',
            fill: '#334155',
            originX: 'center',
            originY: 'center',
            left: 0,
            top: 0,
            selectable: false
        });

        return new fabric.Group([circ, q], {
            originX: 'center',
            originY: 'center',
            left: 0,
            top: 0,
            selectable: false
        });
    };

    const finalizeWithIcon = (iconChild) => {
        if (iconChild.type !== 'group' && iconChild.type !== 'image') {
            iconChild.set({ originX: 'center', originY: 'center', left: equipmentWidth / 2, top: equipmentHeight / 2 });
        }

        iconChild.set({
            originX: 'center',
            originY: 'center',
            left: equipmentWidth / 2,
            top: equipmentHeight / 2
        });
        iconChild.setCoords();

        const rect = new fabric.Rect({
            width: equipmentWidth,
            height: equipmentHeight,
            fill: 'white',
            stroke: borderColor,
            strokeWidth: 2,
            rx: 5,
            ry: 5,
            selectable: false
        });

        const circleSize = equipmentWidth * 0.4;
        const circle = new fabric.Circle({
            radius: circleSize / 2,
            fill: '#2E4052',
            left: equipmentWidth,
            top: 0,
            originX: 'center',
            originY: 'center',
            selectable: false
        });

        const text = new fabric.Text(nequipment.toString(), {
            fontSize: circleSize * 0.7,
            fill: 'white',
            fontWeight: 'bold',
            originX: 'center',
            originY: 'center',
            left: circle.left,
            top: circle.top
        });

        const rawAreaId = (element.Id ?? element.id ?? '').toString();
        const areaKey = rawAreaId.replace('groupArea', '');
        const groupId = 'groupEquipment' + areaKey;

        const placed = Canvas.getObjects().filter(o =>
            o.elementType === 'groupEquipment' && o.groupId === groupId
        );

        const typeOrder = [];
        for (const o of placed) {
            const t = ((o.typeName ?? o.type ?? '') + '').toLowerCase();
            if (t && !typeOrder.includes(t)) typeOrder.push(t);
        }

        const countsByType = {};
        for (const o of placed) {
            const t = ((o.typeName ?? o.type ?? '') + '').toLowerCase();
            countsByType[t] = (countsByType[t] ?? 0) + 1;
        }

        let offset = 0;
        for (const t of typeOrder) {
            if (t === typeKey) break;
            offset += countsByType[t] ?? 0;
        }

        const sameTypeCount = countsByType[typeKey] ?? 0;
        const columnIndex = offset + sameTypeCount;
        const gapX = 8;
        const rightEdgeLeft = element.left + element.width - equipmentWidth;
        const leftPos = Math.round(rightEdgeLeft - columnIndex * (equipmentWidth + gapX));
        const topPos = element.top - equipmentHeight * 0.8;

        const group = new fabric.Group([rect, iconChild, circle, text], {
            id: equipment.id ?? equipment.Id,
            left: leftPos, top: topPos,
            name: equipment.name ?? equipment.Name,
            nequipment: nequipment,
            groupId: groupId,
            elementType: 'groupEquipment',
            typeName: typename,
            selectable: true,
            visible: equipment.StatusObject === 'Add' ||
                window.isVisibleOnSelection('groupArea', areaKey),
            hasControls: false,
            lockMovementX: true, lockMovementY: true,
            lockScalingX: true, lockScalingY: true,
            lockRotation: true,
            hoverCursor: 'pointer',
            idOriginal: equipment.IdOriginal,
            statusObject: equipment.StatusObject ?? 'StoredInBase',
            borderColor: '#247ECC'
        });

        group.on('selected', () => { group.set({ stroke: 'blue' }); Canvas.requestRenderAll(); });
        group.on('deselected', () => { group.set({ stroke: borderColor }); Canvas.requestRenderAll(); });

        Canvas.add(group);
        Canvas.requestRenderAll();
    };

    if (iconName) {
        const img = new Image();
        img.src = routeSvgEquipment + iconName;
        img.onload = () => {
            fabric.Image.fromURL(img.src, (fabricImage) => {
                const scaleX = (equipmentWidth - padding * 2) / fabricImage.width;
                const scaleY = (equipmentHeight - padding * 2) / fabricImage.height;
                fabricImage.set({
                    selectable: false, originX: 'center', originY: 'center',
                    left: equipmentWidth / 2, top: equipmentHeight / 2
                });
                fabricImage.scale(Math.min(scaleX, scaleY));
                finalizeWithIcon(fabricImage);
            });
        };
        img.onerror = () => finalizeWithIcon(buildFallbackGlyph());
    } else {
        finalizeWithIcon(buildFallbackGlyph());
    }
};

const drawShelf = (element, shelves, color, borderColor) => {
    const stationGroups = [];

    const paddingX = 10;
    const paddingY = 40;
    const stationSpacingX = 20;
    const stationSpacingY = 20;
    const sizeW = 60;
    const sizeH = 60;
    const BAR_THICKNESS = 20;
    const BAR_SPACING = 5;
    const INNER_MARGIN_X = 25;
    const INNER_MARGIN_Y = 40;
    const marginInside = 30;

    const orientationValue = obtenerOrientacionDeStations(shelves);
    let columns, totalRows, calculatedWidth, calculatedHeight;

    switch (orientationValue) {
        case 1:
            columns = shelves.length;
            totalRows = 1;
            calculatedWidth = columns * sizeW + (columns - 1) * stationSpacingX + paddingX * 2;
            calculatedHeight = sizeH + paddingY * 2;
            break;
        case 2:
            columns = 1;
            totalRows = shelves.length;
            calculatedWidth = sizeW + paddingX * 2;
            calculatedHeight = totalRows * sizeH + (totalRows - 1) * stationSpacingY + paddingY * 2;
            break;
        default:
            columns = 3;
            totalRows = Math.ceil(shelves.length / columns);
            calculatedWidth = columns * sizeW + (columns - 1) * stationSpacingX + paddingX * 2;
            calculatedHeight = totalRows * sizeH + (totalRows - 1) * stationSpacingY + paddingY * 2;
            break;
    }

    if (!element.Width || !element.Height) {
        element.Width = calculatedWidth;
        element.Height = calculatedHeight;
    }

    shelves.forEach((station, index) => {
        station.StatusObject = updateData ? "Update" : station.StatusObject;
        const isCopy = (station.IdOriginal && station.IdOriginal !== station.Id);
        const hasDbSize = (station.Width > 0 && station.Height > 0);

        let numBars = Math.max(1, station.NumShelves || 1);

        const col = index % columns;
        const row = Math.floor(index / columns);

        const startX = station.X !== 0 ? station.X : element.X + paddingX + col * (sizeW + stationSpacingX);
        const startY = station.Y !== 0 ? station.Y : element.Y + paddingY + row * (sizeH + stationSpacingY);

        const baseW = station.Width > 0 ? station.Width : sizeW;
        const baseH = station.Height > 0 ? station.Height : sizeH;

        let barW, barH;
        if (station.IsVertical) {
            barW = BAR_THICKNESS;
            barH = Math.max(BAR_THICKNESS, baseH - 2 * BAR_SPACING);
        } else {
            barW = Math.max(BAR_THICKNESS, baseW - 2 * BAR_SPACING);
            barH = BAR_THICKNESS;
        }

        let contW, contH;
        if (station.IsVertical) {
            contW = numBars * barW + (numBars - 1) * BAR_SPACING;
            contH = barH;
        } else {
            contW = barW;
            contH = numBars * barH + (numBars - 1) * BAR_SPACING;
        }

        const finalWidth = Math.max(baseW, contW + INNER_MARGIN_X * 2);
        const finalHeight = Math.max(baseH, contH + INNER_MARGIN_Y * 2);

        station.X = startX;
        station.Y = startY;
        station.Width = finalWidth;
        station.Height = finalHeight;

        const { fill: barColor, stroke: barBorderColor, opacity: barOpacity } = getShelfColorByLocationType(station.LocationTypes);

        let numPasillos = 0; 
        let bars = [];
        let aisles = [];
        if (numBars <= 1) {
            numPasillos = 0; 
        } else {
            numPasillos = numBars - 1; 
        }
        for (let i = 0; i < numBars; i++) {
           
            const barX = station.IsVertical
                ? BAR_SPACING + i * (barW + BAR_SPACING)
                : BAR_SPACING;
            const barY = station.IsVertical
                ? BAR_SPACING
                : BAR_SPACING + i * (barH + BAR_SPACING);

            const background = new fabric.Rect({
                left: barX,
                top: barY,
                originX: "left",
                originY: "top",
                width: barW,
                height: barH,
                fill: barColor,
                stroke: null,
                opacity: barOpacity,
                selectable: false
            });

            const border = new fabric.Rect({
                left: barX,
                top: barY,
                originX: "left",
                originY: "top",
                width: barW,
                height: barH,
                fill: "transparent",
                stroke: barBorderColor,
                strokeWidth: 2,
                selectable: false,
                rx: 2,
                ry: 2,
            });

            bars.push(background, border);

           
            if (i < numBars - 1) {
                if (station.IsVertical) {
                  
                    const aisleX = barX + barW;
                    const aisle = new fabric.Rect({
                        left: aisleX,
                        top: barY,
                        originX: "left",
                        originY: "top",
                        width: BAR_SPACING,
                        height: barH,
                        fill: "rgba(200,200,200,0.2)", 
                        stroke: "transparent",
                        selectable: false
                    });
                    aisles.push(aisle);
                } else {
            
                    const aisleY = barY + barH;
                    const aisle = new fabric.Rect({
                        left: barX,
                        top: aisleY,
                        originX: "left",
                        originY: "top",
                        width: barW,
                        height: BAR_SPACING,
                        fill: "rgba(200,200,200,0.2)",
                        stroke: "transparent",
                        selectable: false
                    });
                    aisles.push(aisle);
                }
            }
        }


        const containerRect = new fabric.Rect({
            left: 0,
            top: 0,
            originX: "left",
            originY: "top",
            width: contW,
            height: contH,
            fill: "transparent",
            stroke: "transparent",
            selectable: true,
            hasControls: true,
            lockRotation: true,
            lockScalingFlip: true
        });
        //modificacion de grupo para corregir reduccioon en copias
        let group; 

        if (isCopy || hasDbSize) {
            group = new fabric.Group([containerRect, ...bars], {
                id: "groupShelf" + station.Id,
                elementType: "groupShelf",
                name: station.Name,
                ZoneId: station.ZoneId,
                groupId: "groupShelf" + element.Id,
                maxContainers: station.MaxContainers,
                originX: "left",
                originY: "top",
                left: station.X,
                top: station.Y,
                NumCrossAisles: numPasillos,
                originalNumCrossAisles: station.NumCrossAisles,
                NumShelves: numBars,
                statusObject: station.StatusObject == '' ? 'StoredInBase' : station.StatusObject,
                IsVertical: station.IsVertical,
                LocationTypes: station.LocationTypes,
                ZoneType: station.ZoneType,
                tipe: "Rack",
                selectable: true,
                hasControls: true,
                lockMovementX: false,
                lockMovementY: false,
                lockScalingX: false,
                lockScalingY: false,
                lockRotation: true,
                hoverCursor: "Cursor",
                idOriginal: station.IdOriginal,
                borderColor: '#247ECC',
                width: baseW,
                height: baseH,
            });
        } else {
            group = new fabric.Group([containerRect, ...bars], {
                id: "groupShelf" + station.Id,
                elementType: "groupShelf",
                name: station.Name,
                ZoneId: station.ZoneId,
                groupId: "groupShelf" + element.Id,
                maxContainers: station.MaxContainers,
                originX: "left",
                originY: "top",
                left: station.X,
                top: station.Y,
                NumCrossAisles: numPasillos,
                originalNumCrossAisles: station.NumCrossAisles,
                NumShelves: numBars,
                statusObject: station.StatusObject == '' ? 'StoredInBase' : station.StatusObject,
                IsVertical: station.IsVertical,
                LocationTypes: station.LocationTypes,
                ZoneType: station.ZoneType,
                tipe: "Rack",
                selectable: true,
                hasControls: true,
                lockMovementX: false,
                lockMovementY: false,
                lockScalingX: false,
                lockScalingY: false,
                lockRotation: true,
                hoverCursor: "Cursor",
                idOriginal: station.IdOriginal,
                borderColor: '#247ECC'
            });
        }


        group.on("selected", () => {
            group.set({ stroke: "blue", strokeWidth: 4 });
            Canvas.requestRenderAll();
        });

        group.on("deselected", () => {
            group.set({ stroke: borderColor, strokeWidth: 1 });
            Canvas.requestRenderAll();
        });

        group.on("modified", () => {
            updateData = true;
            const savedLeft = group.left;
            const savedTop = group.top;
            const wasScaled = group.scaleX !== 1 || group.scaleY !== 1;

            if (wasScaled) {
                const MIN_W = 0, MIN_H = 0;
                const newW = group.width * group.scaleX;
                const newH = group.height * group.scaleY;

                if (newW < MIN_W || newH < MIN_H || group.scaleX < 0 || group.scaleY < 0) {
                    group.set({
                        scaleX: 1, scaleY: 1,
                        width: group._prevWidth || group.width,
                        height: group._prevHeight || group.height,
                        left: group._prevLeft || group.left,
                        top: group._prevTop || group.top
                    });
                    group.setCoords();
                    Canvas.requestRenderAll();
                    return;
                }

                group.set({ scaleX: 1, scaleY: 1 });

                if (station.IsVertical) {
                    barW = BAR_THICKNESS;
                    barH = Math.max(BAR_THICKNESS, newH - 2 * BAR_SPACING);
                    numBars = Math.max(1,
                        Math.round((newW - BAR_SPACING) / (BAR_THICKNESS + BAR_SPACING)));
                } else {
                    barH = BAR_THICKNESS;
                    barW = Math.max(BAR_THICKNESS, newW - 2 * BAR_SPACING);
                    numBars = Math.max(1,
                        Math.round((newH - BAR_SPACING) / (BAR_THICKNESS + BAR_SPACING)));
                }

                station.NumShelves = numBars;
                group.NumShelves = numBars;
                let numPasillos = (numBars <= 1) ? 0 : numBars - 1;
                station.NumCrossAisles = numPasillos;
                group.NumCrossAisles = numPasillos;
                station.Width = newW;
                station.Height = newH;

                const contW = station.IsVertical
                    ? numBars * barW + (numBars + 1) * BAR_SPACING
                    : barW + 2 * BAR_SPACING;
                const contH = station.IsVertical
                    ? barH + 2 * BAR_SPACING
                    : numBars * barH + (numBars + 1) * BAR_SPACING;

                containerRect.set({ width: contW, height: contH, left: 0, top: 0 });

                bars.forEach(b => group.removeWithUpdate(b));
                const freshBars = [];
                for (let i = 0; i < numBars; i++) {
                    const barX = station.IsVertical
                        ? BAR_SPACING + i * (barW + BAR_SPACING)
                        : BAR_SPACING;
                    const barY = station.IsVertical
                        ? BAR_SPACING
                        : BAR_SPACING + i * (barH + BAR_SPACING);

                    // Fondo con opacidad
                    const barBackground = new fabric.Rect({
                        left: barX,
                        top: barY,
                        originX: "left",
                        originY: "top",
                        width: barW,
                        height: barH,
                        fill: barColor,
                        stroke: null,
                        opacity: barOpacity,
                        selectable: false
                    });

                    // Borde sólido sin opacidad
                    const barBorder = new fabric.Rect({
                        left: barX,
                        top: barY,
                        originX: "left",
                        originY: "top",
                        width: barW,
                        height: barH,
                        fill: "transparent",
                        stroke: barBorderColor,
                        strokeWidth: 2,
                        selectable: false,
                        rx: 2,
                        ry: 2,
                    });
                    freshBars.push(barBackground, barBorder);
                    group.addWithUpdate(barBackground);
                    group.addWithUpdate(barBorder);
                }

                bars = freshBars;

                containerRect.set({ left: 0, top: 0 });
                const firstBar = freshBars[0];
                const deltaX = BAR_SPACING - firstBar.left;
                const deltaY = BAR_SPACING - firstBar.top;
                freshBars.forEach(b => {
                    b.left += deltaX;
                    b.top += deltaY;
                });

                group._calcBounds();
                group._updateObjectsCoords();
                group.set({ left: savedLeft, top: savedTop });
                group.setCoords();
            }

            station.PositionX = Math.floor(group.left);
            station.PositionY = Math.floor(group.top);

            const grpRight = group.left + group.width;
            const grpBottom = group.top + group.height;
            let changedArea = false;

            if (grpRight + paddingX > element.X + element.Width) {
                element.Width = grpRight + paddingX - element.X;
                changedArea = true;
            }
            if (grpBottom + paddingY > element.Y + element.Height) {
                element.Height = grpBottom + paddingY - element.Y;
                changedArea = true;
            }
            if (group.left - paddingX < element.X) {
                const delta = element.X - (group.left - paddingX);
                element.X = group.left - paddingX;
                element.Width += delta;
                changedArea = true;
            }
            if (group.top - paddingY < element.Y) {
                const delta = element.Y - (group.top - paddingY);
                element.Y = group.top - paddingY;
                element.Height += delta;
                changedArea = true;
            }
            if (changedArea && element.StatusObject !== "Add") {
                element.StatusObject = "Update";
            }

            {
                const allObjects = Canvas.getObjects();
                const children = allObjects.filter(o =>
                    o.groupId === "groupShelf" + element.Id ||
                    o.groupId === "groupStation" + element.Id);
                if (children.length) {
                    let minX = Infinity, minY = Infinity;
                    let maxX = -Infinity, maxY = -Infinity;
                    children.forEach(obj => {
                        const w = obj.width * (obj.scaleX || 1);
                        const h = obj.height * (obj.scaleY || 1);
                        const l = obj.left;
                        const t = obj.top;
                        minX = Math.min(minX, l);
                        minY = Math.min(minY, t);
                        maxX = Math.max(maxX, l + w);
                        maxY = Math.max(maxY, t + h);
                    });

                    element.X = minX - paddingX - marginInside;
                    element.Y = minY - paddingY - marginInside;
                    element.Width = (maxX - minX) + (paddingX + marginInside) * 2;
                    element.Height = (maxY - minY) + (paddingY + marginInside) * 2;
                }
            }

            //   Actualiza visualmente groupArea ----------------- */
            const groupArea = Canvas.getObjects().find(obj =>
                obj.id === 'groupArea' + element.Id
            );


            const stations = Canvas.getObjects().filter(obj =>
                obj.groupId === 'groupStation' + element.Id ||
                obj.groupId === 'groupShelf' + element.Id
            );

            if (stations.length > 0) {
       
                let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
                stations.forEach(obj => {
                    const left = obj.left, top = obj.top;
                    const right = left + obj.width * (obj.scaleX || 1);
                    const bottom = top + obj.height * (obj.scaleY || 1);
                    if (left < minX) minX = left;
                    if (top < minY) minY = top;
                    if (right > maxX) maxX = right;
                    if (bottom > maxY) maxY = bottom;
                });

       
                const marginTop = 50;
                const marginRight = 8;
                const marginLeft = 8;
                const marginBottomBase = 8;

     
                const hasProcesses = Canvas.getObjects().some(o =>
                    o.elementType === 'groupProcess' && o.groupId === ('groupProcess' + element.Id)
                );
                const processHeight = 5;  
                const processGap = 1;
                const marginBottomFinal = hasProcesses
                    ? marginBottomBase + processHeight + processGap
                    : marginBottomBase;

          
                element.X = minX - marginLeft;
                element.Y = minY - marginTop;

                element.Width = (maxX - element.X) + marginRight;       
                element.Height = (maxY - element.Y) + marginBottomFinal;  
            }

            if (groupArea) {
                const rect = groupArea._objects?.[0];
                const text = groupArea._objects?.[1];
                if (rect) {
                    rect.set({
                        width: element.Width,
                        height: element.Height,
                        left: 0,
                        top: 0,
                        originX: 'left',
                        originY: 'top'
                    });
                    rect.setCoords();
                }
                if (text) {
                    const originalFontSize = 10; 
                    const maxTextWidth = element.Width - 20; 
                    const { displayedText, fullText } = getTruncatedText(text.fullText || text.text, maxTextWidth, originalFontSize);       
                    text.fullText = fullText;     
                    text.set({
                        text: displayedText,
                        fontSize: originalFontSize,
                        left: element.Width / 2,
                        top: 25,           
                        originX: 'center',
                        originY: 'top'
                    });
                    text.setCoords();
                }
                groupArea._calcBounds();
                groupArea._updateObjectsCoords();
                groupArea.set({
                    left: element.X,
                    top: element.Y,
                    originX: 'left',
                    originY: 'top'
                });
                groupArea.left = element.X;
                groupArea.top = element.Y;

                //  Esto fuerza el refresco interno de coordenadas
                groupArea.setCoords();
                Canvas.renderAll();

                //  Forzar guardado en coordenadas internas
                groupArea.prevLeft = groupArea.left;
                groupArea.prevTop = groupArea.top;
                if (groupArea.statusObject !== 'Add') {
                    groupArea.statusObject = 'Update';
                }
                groupArea.setCoords();
                const processes = Canvas.getObjects().filter(obj =>
                    obj.elementType === 'groupProcess' &&
                    obj.groupId === 'groupProcess' + element.Id
                );
                //   Reposiciona procesos, equipos, rutas ------------ */
                const spacing = 4;
                if (processes.length > 0) {
                    // Ordenamos los procesos de izquierda a derecha por su propiedad "left"
                    processes.sort((a, b) => a.left - b.left);
                    const totalWidth = processes.reduce((sum, p) => sum + p.width, 0) + (processes.length - 1) * spacing;
                    let currentLeft = element.X + element.Width - totalWidth - spacing + 12;
                    processes.forEach(proc => {
                        proc.set({
                            left: currentLeft,
                            top: element.Y + element.Height - proc.height / 2 + 12,
                            originX: 'left',
                            originY: 'center'
                        });
                        proc.setCoords();
                        currentLeft += proc.width + spacing;
                    });
                }
                const equipments = Canvas.getObjects().filter(obj =>
                    obj.elementType === 'groupEquipment' &&
                    obj.groupId === 'groupEquipment' + element.Id
                );
                const equipSpacing = 8;
                const equipWidthFixed = 45; // Ajusta al tamaño fijo que usaste
                let equipLeftStart = element.X + element.Width - (equipments.length * (equipWidthFixed + equipSpacing)) + 20;
                const equipTop = element.Y - equipWidthFixed * 0.8;
                equipments.forEach(eq => {
                    eq.set({
                        left: equipLeftStart,
                        top: equipTop
                    });
                    eq.setCoords();
                    equipLeftStart += equipWidthFixed + equipSpacing;
                });

                updateAllPosicionRoute();
                Canvas.getObjects().forEach(obj => {
                    if (obj.elementType === 'processDirection' || obj.type === 'circle') {
                        obj.setCoords();
                    }
                });
                Canvas.calcOffset();
                Canvas.requestRenderAll();
            }
        });

        stationGroups.push(group);
    });

    if (stationGroups.length > 0) {
        let minX = Infinity, minY = Infinity, maxX = -Infinity, maxY = -Infinity;
        stationGroups.forEach(group => {
            const left = group.left;
            const top = group.top;
            const right = group.left + group.width * (group.scaleX || 1);
            const bottom = group.top + group.height * (group.scaleY || 1);
            minX = Math.min(minX, left);
            minY = Math.min(minY, top);
            maxX = Math.max(maxX, right);
            maxY = Math.max(maxY, bottom);
        });

        const margin = 50;
        element.X = minX - margin;
        element.Y = minY - margin;
        element.Width = (maxX - minX) + margin * 2;
        element.Height = (maxY - minY) + margin * 2;
    }

    return stationGroups;
};


// Draw the processes of an area, with their initial and final steps.
const drawProcess = (area, processes, allSteps = [], borderColor) => {
    const processHeight = 40;     // Altura fija del proceso
    const stepHeight = 10;        // Altura fija de cada paso
    const fontSize = 10;
    const spacing = 15;
    let currentLeft = area.left + area.width;

    processes.forEach((process) => {
        const nameComplete = process.name ?? `${process.Name}${(+process.PercentageInitProcess || 0) > 0 ? ` (${process.PercentageInitProcess}%)` : ''}`;
        const processId = process.id == undefined ? process.Id : process.id.replace("groupProcess", "");

        // Obtain Init and End steps
        const processSteps = allSteps.filter(s => s.ProcessId === processId);
        const stepsInit = processSteps.filter(s => s.InitProcess).sort((a, b) => a.Order - b.Order);
        const stepsEnd = processSteps.filter(s => s.EndProcess).sort((a, b) => a.Order - b.Order);
        
        // Create visual elements for Init and End steps
        const stepsInitVisual = drawStepInit(stepsInit, fontSize, stepHeight, spacing);
        const stepsEndVisual = drawStepEnd(stepsEnd, fontSize, stepHeight, spacing);

        let totalStepsInitWidth = stepsInitVisual.reduce((sum, g) => sum + g.width + spacing, 0); // Position visual element

        // Adjust relative position of each object
        stepsInitVisual.reduce((offset, g) => {
            g.left = offset;
            return offset + g.width + spacing;
        }, 0);

        const processText = new fabric.Text(nameComplete, {
            fontSize,
            fill: "#2E4052",
            fontFamily: 'Segoe UI',
            originX: 'left',
            originY: 'center',
            left: totalStepsInitWidth,
            top: 0
        });

        stepsEndVisual.reduce((offset, g) => {
            g.left = totalStepsInitWidth + processText.width + spacing + offset;
            return offset + g.width + spacing;
        }, 0);

        // Grouping content (StepsInit, ProcessText and StepsEnd)
        const contentGroup = new fabric.Group(
            [...stepsInitVisual, processText, ...stepsEndVisual],
            {
                originX: 'center',
                originY: 'center',
                left: 0,
                top: 0
            }
        );
        const contentWidth = contentGroup.width;

        // Process Oval
        const processRect = new fabric.Rect({
            width: contentWidth + 15,
            height: processHeight - 20,
            fill: colorProcess,
            stroke: borderColor,
            strokeWidth: 2,
            rx: 4,
            ry: 4,
            originX: 'center',
            originY: 'center'
        });

        // Group oval Process and content (Text and Steps)
        const processGroup = new fabric.Group([processRect, contentGroup, processText], {
            id: 'groupProcess' + processId,
            left: currentLeft - (contentWidth + 10),
            top: area.top + area.height - processHeight / 2 + 20,
            originX: 'left',
            originY: 'center',
            selectable: true,
            groupId: 'groupProcess' + area.id.replace('groupArea', ''),
            visible: window.isVisibleOnSelection('groupProcess', processId, area.id.replace('groupArea', '')),
            elementType: 'groupProcess',
            name: nameComplete,
            hasControls: false,
            lockMovementX: true,
            lockMovementY: true,
            lockScalingX: true,
            lockScalingY: true,
            lockRotation: true,
            hoverCursor: 'pointer',
            statusObject: process.StatusObject ?? 'StoredInBase',
            processTypeId: process.ProcessTypeId,
            idOriginal: process.IdOriginal,
            Type: process.Type,
            steps: processSteps,
            borderColor: '#247ECC',
            borderScaleFactor: 1.5,
            stroke: borderColor,
        });

        processGroup.on('selected', function () {
            processGroup.opacity = 1;
            Canvas.getObjects().filter(o => o.visible == true && o.id != processGroup.id && o.elementType === 'groupProcess').forEach(obj => {
                obj.opacity = 0.24;
            });
            Canvas.requestRenderAll();
        });

        Canvas.add(processGroup);
        currentLeft = currentLeft - (contentWidth + 10 + spacing); // Adjust the process spacing
    });

    Canvas.requestRenderAll();
};

// Draw Init steps (before the text of the process)
const drawStepInit = (steps, fontSize, stepHeight, spacing) => {
    const stepInit = [];

    steps.forEach((step) => {
        // Step Text
        const text = new fabric.Text(step.Name, {
            fontSize,
            fill: "#FFFFFF",
            fontFamily: 'Segoe UI',
            originX: 'center',
            originY: 'center'
        });

        //Step Rect
        const rect = new fabric.Rect({
            width: text.width + 7,
            height: stepHeight,
            fill: "#2E4052",
            rx: 5,
            ry: 5,
            originX: 'center',
            originY: 'center'
        });

        // Step Group (Rect and Text)
        const group = new fabric.Group([rect, text], {
            originX: 'left',
            originY: 'center',
            top: 0,
            elementType: 'groupStep',
            id: step.Id,
        });

        stepInit.push(group);
    });

    return stepInit;
};

// Draws End steps
const drawStepEnd = (steps, fontSize, stepHeight, spacing) => {
    const stepEnd = [];

    steps.forEach((step) => {
        // Step Text
        const text = new fabric.Text(step.Name, {
            fontSize,
            fill: "#FFFFFF",
            fontFamily: 'Segoe UI',
            originX: 'center',
            originY: 'center'
        });

        //Step Rect
        const rect = new fabric.Rect({
            width: text.width + 7,
            height: stepHeight,
            fill: "#2E4052",
            rx: 5,
            ry: 5,
            originX: 'center',
            originY: 'center'
        });

        // Step Group (Rect and Text)
        const group = new fabric.Group([rect, text], {
            originX: 'left',
            originY: 'center',
            top: 0,
            elementType: 'groupStep',
            id: step.Id,
        });

        stepEnd.push(group);
    });

    return stepEnd;
};

window.drawAreaOnCanvas = (element, process, stations, shelf, changedProcessDirection, routes, steps) => {
    process = fixPropertyNamesArray(process);
    stations = fixPropertyNamesArray(stations);
    shelf = fixPropertyNamesArray(shelf);
    element = fixPropertyNamesArray(element);
    steps = fixPropertyNamesArray(steps);

    if (Array.isArray(changedProcessDirection) && changedProcessDirection.length > 0) {
        changedProcessDirection = fixPropertyNamesArray(changedProcessDirection);
        changedProcessDirection.forEach(function (element) {
            var index = modelProcessDirections.findIndex(obj => obj.Id === element.Id);
            if (index >= 0)
                modelProcessDirections[index] = { ...modelProcessDirections[index], ...element };
        });
    }

    if (Array.isArray(routes) && routes.length > 0) {
        routes = fixPropertyNamesArray(routes);
        routes.forEach(function (element) {
            var index = modelRoutes.findIndex(obj => obj.Id === element.Id);
            if (index >= 0)
                modelRoutes[index] = { ...modelRoutes[index], ...element };
        });
    }

    let areaJson = {
        "Areas": [element],
        "Objects": [],
        "Routes": [],
        "Equipments": [],
        "Stations": stations,
        "Process": process,
        "Shelf": shelf,
        "ProcessDirections": [],
        "Steps": steps,
        "Flows": []
    };
    if (areaJson != null) {
        deleteObjectInCanvas(JSON.stringify(element));
        let areaJsonString = JSON.stringify(areaJson);
        StartDrawCanvas(areaJsonString);
        //DrawingAllRoutes();

        //removeGroup('processDirectionGroup');

        //if (OutRouteActive)
        //    DrawingAllProcessOutDirections();

        //if (IntRouteActive)
        //    DrawingAllProcessInDirections();

        //if (CustomRouteActive)
        //    DrawingAllProcessCustomDirections();

        if (window.canvasSelectionFilter) {
            applyCanvasSelection(window.canvasSelectionFilter);
        }
    }
}

window.drawRouteOnCanvas = (element) => {
    element = fixPropertyNames(element);
    var index = modelRoutes.findIndex(obj => obj.Id === (element.Id || element.id));

    if (index === -1) {
        modelRoutes.push(element);
    } else {
        modelRoutes[index] = { ...modelRoutes[index], ...element };
    }

    const hasElementsToConnect = Canvas.getObjects().some(obj =>
        obj.elementType === 'groupProcess' || obj.elementType === 'groupStation'
    );

    if (hasElementsToConnect) {
        DrawingAllRoutes();
        dotnetLocalHelper.invokeMethodAsync('RefreshCanvasSelection');
    }
}

window.drawProcessDirectionOnCanvas = (element) => {
    element = fixPropertyNames(element);
    var index = modelProcessDirections.findIndex(obj => obj.Id === element.Id);
    var oldProcessDirection = index !== -1 ? { ...modelProcessDirections[index] } : null;

    removeGroup('processDirectionGroup');
    const objectsCanvas = Canvas.getObjects();

    objectsCanvas.forEach(obj => {
        if (obj.elementType === 'groupProcess') {
            obj.forEachObject(child => {
                if (child.type === 'rect') {
                    child.set({ stroke: 'black' });
                }
            });
        }
    });

    if (index === -1) {
        modelProcessDirections.push(element);
    } else {
        modelProcessDirections[index] = { ...modelProcessDirections[index], ...element };
    }

    if (element.InitProcessIsOut === true) {
        DrawingAllProcessOutDirections();
    }

    if (element.InitProcessIsIn === true) {
        DrawingAllProcessInDirections();
    }

    if (element.InitProcessIsIn === false && element.InitProcessIsOut === false) {
        DrawingAllProcessCustomDirections();
    }


    if (oldProcessDirection) {
        if (oldProcessDirection.InitProcessIsOut === true && oldProcessDirection.InitProcessIsOut !== element.InitProcessIsOut) {
            DrawingAllProcessOutDirections();
        }

        if (oldProcessDirection.InitProcessIsIn === true && oldProcessDirection.InitProcessIsIn !== element.InitProcessIsIn) {
            DrawingAllProcessInDirections();
        }

        if (
            oldProcessDirection.InitProcessIsIn === false &&
            oldProcessDirection.InitProcessIsOut === false &&
            (oldProcessDirection.InitProcessIsIn !== element.InitProcessIsIn ||
                oldProcessDirection.InitProcessIsOut !== element.InitProcessIsOut)
        ) {
            DrawingAllProcessCustomDirections();
        }
    }

    dotnetLocalHelper.invokeMethodAsync('RefreshCanvasSelection');
}




window.drawEquipmentOnCanvas = (equipment, area) => {

    equipment = fixPropertyNames(equipment);
    area = fixPropertyNames(area);
    var index = modelEquipments.findIndex(obj => obj.Id === equipment.Id);

    if (index === -1) {
        modelEquipments.push(equipment);
    } else {
        modelEquipments[index] = { ...modelEquipments[index], ...equipment };
    }
    
    let objectsToRemove = Canvas.getObjects().filter(obj => obj.groupId == 'groupEquipment' + area.Id);
    objectsToRemove.forEach(objToRemove => Canvas.remove(objToRemove));

    modelEquipments
        .filter(obj => obj.AreaId === area.Id)
        .forEach(obj => {
            if (area.AreaType == 0) { //Dock
                drawEquipment(area, obj, borderDock);
            } else if (area.AreaType == 1) { //Aisle
                drawEquipment(area, obj, borderStorageArea);
            }
            else if (area.AreaType == 2) { //Buffer 
                drawEquipment(area, obj, borderBuffer);
            }
            else if (area.AreaType == 3) { //Stage
                drawEquipment(area, obj, borderStage);
            }
        });
}

window.drawProcessOnCanvas = (processes, area, procesesDirection, steps) => {
    processes = fixPropertyNamesArray(processes);
    modelProcessDirections = fixPropertyNamesArray(procesesDirection);
    area = fixPropertyNames(area);
    steps = fixPropertyNamesArray(steps);

    let objectsToRemove = Canvas.getObjects().filter(X => X.groupId === 'groupProcess' + area.Id);
    let areaReference = Canvas.getObjects().find(X => X.id === 'groupArea' + area.Id);
    if (objectsToRemove)
        objectsToRemove.forEach(objToRemove => Canvas.remove(objToRemove));

    drawProcess(areaReference, processes, steps);
    removeGroup('processDirectionGroup');
    if (modelProcessDirections.some(x => x.InitProcessIsOut === true)) {
        DrawingAllProcessOutDirections();
    }

    if (modelProcessDirections.some(x => x.InitProcessIsIn === true)) {
        DrawingAllProcessInDirections();
    }

    if (modelProcessDirections.some(x => x.InitProcessIsIn === false && x.InitProcessIsOut === false)) {
        DrawingAllProcessCustomDirections();
    }

    dotnetLocalHelper.invokeMethodAsync('RefreshCanvasSelection');
}

window.drawStationOnCanvas = (stations, area, process, steps) => {

    stations = fixPropertyNamesArray(stations);
    area = fixPropertyNames(area);
    process = fixPropertyNamesArray(process);
    steps = fixPropertyNamesArray(steps);

    let objectJson = {
        "Areas": [area],
        "Objects": [],
        "Routes": [],
        "Equipments": [],
        "Stations": stations,
        "Process": process,
        "Shelf": [],
        "ProcessDirections": [],
        "Steps": steps,
        "Flows": []
    };

    if (objectJson != null) {
        deleteObjectInCanvas(JSON.stringify(area));
        let objectJsonString = JSON.stringify(objectJson);
        StartDrawCanvas(objectJsonString);
    }
}

window.drawObjectsOnCanvas = (modelJson) => {

    if (modelJson != '') {
        let model = JSON.parse(modelJson);
        if (model != null)
        {
            if (model.Areas) {
                var area = fixPropertyNames(model.Areas[0]);

                if (area) {
                    deleteObjectInCanvas(JSON.stringify(area));
                    StartDrawCanvas(modelJson);
                }
            }
        }
    }
}

window.drawShelfOnCanvas = (shelves, area, process, steps) => {

    shelf = fixPropertyNamesArray(shelves);
    area = fixPropertyNames(area);
    process = fixPropertyNamesArray(process);
    steps = fixPropertyNamesArray(steps);

    let objectJson = {
        "Areas": [area],
        "Objects": [],
        "Routes": [],
        "Equipments": [],
        "Stations": [],
        "Process": process,
        "Shelf": shelf,
        "ProcessDirections": [],
        "Steps": steps,
        "Flows": []
    };

    if (objectJson != null) {
        deleteObjectInCanvas(JSON.stringify(area));
        let objectJsonString = JSON.stringify(objectJson);
        StartDrawCanvas(objectJsonString);
    }
}

window.StartDrawCanvas = (modelJson) => {
    try {
        modelEquipments = [];
        modelProcessDirections = [];
        if (modelJson != '') {
            let model = JSON.parse(modelJson);
            updateData = false;
            if (model != null) {
                if (Array.isArray(model.Equipments) && model.Equipments.length > 0) {
                    model.Equipments.forEach(equipment => {
                        var index = modelEquipments.findIndex(obj => obj.Id === equipment.Id);
                        if (index === -1) {
                            modelEquipments.push(equipment);
                        } else {
                            modelEquipments[index] = { ...modelEquipments[index], ...equipment };
                        }
                    });
                }


                if (Array.isArray(model.Areas)) {
                    model.Areas.forEach(function (element) {
                        switch (element.CanvasObjectType) {
                            case 'Area':

                                let process = model.Process.filter(x => x.AreaId === element.Id)
                                let steps = model.Steps.filter(x => process.some(p => p.Id === x.ProcessId));
                                var stations = model.Stations.filter(x => x.AreaId === element.Id)
                                var shelf = model.Shelf.filter(x => x.AreaId === element.Id);

                                if (element.AreaType == 0) { //Dock
                                    drawArea(element, colorDock, borderDock, stations, process, shelf, steps);
                                } else if (element.AreaType == 1) { //Aisle
                                    drawArea(element, colorStorageArea, borderStorageArea, stations, process, shelf, steps);
                                }
                                else if (element.AreaType == 2) { //Buffer
                                    drawArea(element, colorBuffer, borderBuffer, stations, process, shelf, steps);
                                }

                                else if (element.AreaType == 3) { //Stage
                                    drawArea(element, colorStage, borderStage, stations, process, shelf, steps);
                                }
                                break;
                        }
                    });
                    initializeCopyNameTracker();
                }

                if (Array.isArray(model.Routes) && model.Routes.length > 0) {
                    modelRoutes = mergeById(modelRoutes, model.Routes, "Id");
                }

                // Redibujar rutas SIEMPRE desde el cache global (si existen)
                if (Array.isArray(modelRoutes) && modelRoutes.length > 0) {
                    DrawingAllRoutes();
                }

                if (Array.isArray(model.ProcessDirections) && model.ProcessDirections.length > 0) {

                    // Merge incremental Itinerarios
                    modelProcessDirections = mergeById(modelProcessDirections, model.ProcessDirections, "Id");
                    DrawingAllProcessOutDirections();
                    DrawingAllProcessInDirections();
                    DrawingAllProcessCustomDirections();
                } else {

                    DrawingAllProcessOutDirections();
                    DrawingAllProcessInDirections();
                    DrawingAllProcessCustomDirections();
                }

                if (updateData) {
                    SaveComponentCanvas();
                }

                if (window.canvasSelectionFilter) {
                    applyCanvasSelection(window.canvasSelectionFilter);
                }

                if (Array.isArray(model.Objects)) {
                    model.Objects.forEach(function (element) {
                        switch (element.CanvasObjectType) {
                            case 'Square':
                                drawSquare(element.Left, element.Top, 0, 0, 'StoredInBase', element.Id, element.Width, element.Height, element.Angle);
                                break;
                            case 'Text':
                                drawTextBox(
                                    element.Left,
                                    element.Top,
                                    'StoredInBase',
                                    element.Text,
                                    element.Width,
                                    element.Height,
                                    1, //scaleX,
                                    1, //scaleY,
                                    16, //fontSize
                                    element.Id,
                                    element.Angle
                                );
                                break;
                            case 'LineStraight':
                                drawElementLine(element.X, element.Y, element.X2, element.Y2, element.Left, element.Top, 'StoredInBase', element.Id, element.Angle);
                                break;
                        }
                    });
                }

                Canvas.requestRenderAll();
            }
        }
    } catch (e) {
        console.error("Error StartDrawCanvas", e);
    }
}
