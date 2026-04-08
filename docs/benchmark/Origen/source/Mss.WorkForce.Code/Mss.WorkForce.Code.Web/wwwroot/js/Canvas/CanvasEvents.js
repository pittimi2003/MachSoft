/**
 * Handles all canvas-related events such as mouse interactions and object modifications.
 * Includes mouse down, move, up, selection, and other Fabric.js event bindings.
 */



//mouse wheel => Manejo de zoom por medio de scroll de mouse
window.enableMouseZoom = () => {
    Canvas.on("mouse:wheel", (event) => {
        const delta = event.e.deltaY;
        const offsetX = event.e.offsetX;
        const offsetY = event.e.offsetY;
        let originalZoom = Canvas.getZoom();
        let newZoom = delta > 0
            ? originalZoom / zoomRatio
            : originalZoom * zoomRatio;

        if (newZoom < 0.01) return;

        Canvas.zoomToPoint({ x: offsetX, y: offsetY }, newZoom);

  

       /* reviseZoom();*/
        scaleRulers(Canvas.getZoom());
        positionRulers(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
        updateScrollbarsValues(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
        updateZoomDisplay();
        enableDisableZoomButtons();

        event.e.preventDefault();
        event.e.stopPropagation();
    });
};

window.ActivateActionsObjectAggregation = () => {

    Canvas.on('mouse:down', (event) => {
        objectSelection = null;

        if (Canvas.getActiveObjects().length == 0) {
            applyProcessOpacityFromFilters();
        }

        const now = Date.now();
        const evt = event.e;
        const pointerDown = Canvas.getPointer(evt);
        lastPointerCanvas.x = pointerDown.x;
        lastPointerCanvas.y = pointerDown.y;
        lastPointerValid = true;
        const target = event.target || null;
        const clickedBackground = !target || target.id === 'background';
        mouseDownOnBackground = clickedBackground;

        if (isPanning) {
            Canvas.upperCanvasEl.style.cursor = 'grabbing';
            isPanModeEnabled = true;
            Canvas.selection = false;
            Canvas.lastPosX = evt.clientX;
            Canvas.lastPosY = evt.clientY;
            panStart = { x: evt.clientX, y: evt.clientY };
            return;
        }

        // Doble clic para texto
        if (currentMode === modes.addText && (now - lastClickTime) < doubleClickDelay) {
            return;
        }
        lastClickTime = now;

        isDraggingState = true;

        //manejamos menú en derecho y seguimos con creación en izquierdo.
        if (event.button === 2 || event.button === 3) {       
            deselectButton();
            return;
        }

        // Coordenadas iniciales
        Canvas.selection = true;
        const canvasRect = Canvas.getElement().getBoundingClientRect();
        const pointer = Canvas.getPointer(evt);
        firstMouseX = Math.floor(evt.clientX - canvasRect.left);
        firstMouseY = Math.floor(evt.clientY - canvasRect.top);
        firstMouseXAbsolute = Math.floor(event.absolutePointer.x);
        firstMouseYAbsolute = Math.floor(event.absolutePointer.y);

        const x1 = pointer.x;
        const y1 = pointer.y;

        //Limita seleccion de elementos fuera del limite
        //if (currentMode !== modes.zoomToSelection && !isInsideWorkArea(x1, y1)) {
        //    console.warn("The start of the object is outside the working area.");
        //    ResetDrawObject();
        //    return;
        //}

        // --- Switch por modo ---
        switch (currentMode) {
            case modes.addArea:
            case modes.addDock:
            case modes.addAisle:
            case modes.addBuffer:
            case modes.addShelf:
            case modes.addStage: {
                Canvas.selection = false;
                let objectAreaCanvas = {
                    Id: null,
                    CanvasObjectType: currentMode,
                    X: Math.floor(x1),
                    Y: Math.floor(y1)
                };
                openDataObject(objectAreaCanvas);
                break;
            }

            case modes.addStorage: {
                const objectCanvas = {
                    Id: null,
                    CanvasObjectType: currentMode,
                    Left: Math.floor(x1),
                    Top: Math.floor(y1)
                }
                break;
            }

            case modes.zoomToSelection:
                Canvas.selection = true;
                break;

            case modes.addText:
                Canvas.selection = false;
                if (!firstTimeWriting) {
                    deselectButton();
                    const lastObject = Canvas.getObjects().at(-1);
                    if (lastObject) lastObject.exitEditing();
                }
                firstTimeWriting = false;
                break;

            case modes.addLineStraight:
                Canvas.selection = false;

                drawElementLine(x1, y1, x1, y1, x1, y1, 'Add');
                break;

            case modes.addSquare:
                Canvas.selection = true;

                break;

            default:

                break;
        }
    });

    Canvas.on('mouse:move', (event) => {
        const e = event.e;

        // Escuchamos las coordenadas del cursor
        const pointerMove = Canvas.getPointer(e);
        lastPointerCanvas.x = pointerMove.x;
        lastPointerCanvas.y = pointerMove.y;
        lastPointerValid = true;
        // Actualiza track de mouse 
        const canvasRect = Canvas.getElement().getBoundingClientRect();
        lastMouseX = firstMouseX;
        lastMouseY = firstMouseY;
        firstMouseX = Math.floor(e.clientX - canvasRect.left);
        firstMouseY = Math.floor(e.clientY - canvasRect.top);

        // --- PANNING activo ---
        if (isPanModeEnabled && isPanning) {
            const dx = e.clientX - panStart.x;
            const dy = e.clientY - panStart.y;
            Canvas.relativePan({ x: dx, y: dy });
            panStart = { x: e.clientX, y: e.clientY };

            const tl = Canvas.vptCoords.tl;
            positionRulers(tl.x, tl.y);
            updateScrollbarsValues(tl.x, tl.y);
            reviseZoom();
            return;
        }


        if (Canvas.getActiveObjects().length > 1) return;
        if (!isDraggingState) {
            return;
        }
        // Habilitar botón Save si arrastras ciertos elementos 
        if (Canvas.getActiveObjects().length > 0) {
            const ao = Canvas.getActiveObjects()[0];
            if (ao.typeObject === 'controlCircle') {
                DisabledButtonSave(false);
            }
            if (['groupArea', 'Text', 'LineStraight', 'Square'].includes(ao.elementType)) {
                DisabledButtonSave(false);
            }
        }

        // --- Estirar línea mientras se arrastra ---
        if (currentMode === modes.addLineStraight && typeof line !== 'undefined' && line) {
            const pointer = Canvas.getPointer(e);
            line.set({ x2: pointer.x, y2: pointer.y });
            Canvas.renderAll();
            updateZoomDisplay();
        }

        // Limpieza de tooltip si cambia el hover
        if (!currentHoveredObject) return;
        const target = e.target;
        if (target !== currentHoveredObject) {
            clearTimeout(tooltipTimer);
            tooltip.style.display = 'none';
            currentHoveredObject = null;
        }
    });

    Canvas.on('mouse:up', (event) => {

        isPanModeEnabled = false;
        isDraggingState = false;
        Canvas.selection = true;


        // Actualiza últimos absolutos
        const canvasRect = Canvas.getElement().getBoundingClientRect();
        lastMouseX = Math.floor(event.e.clientX - canvasRect.left);
        lastMouseY = Math.floor(event.e.clientY - canvasRect.top);
        lastMouseXAbsolute = Math.floor(event.absolutePointer.x);
        lastMouseYAbsolute = Math.floor(event.absolutePointer.y);

        if (IsForbiddenMouse()) {
            Canvas.selection = true;
            return;
        }

        if (currentMode === '') {
            const actives = Canvas.getActiveObjects();
            if (actives.length === 1 && actives[0] === Canvas.getObjects()[0]) {
                Canvas.discardActiveObject();
            }
        }

        if (Canvas.getActiveObjects().length > 0) {
            const ao = Canvas.getActiveObjects()[0];
            document.getElementById('btnTash').disabled = (ao.typeObject === 'controlCircle');

            if (['groupArea', 'Text', 'LineStraight', 'Square', 'groupStation', 'groupShelf', 'groupProcess', 'groupEquipment']
                .includes(ao.elementType) && ao.statusObject !== 'Add') {
                document.getElementById('btnCopy').disabled = false;
            }
        } else {
            document.getElementById('btnTash').disabled = true;
            document.getElementById('btnCopy').disabled = true;
        }

        // Crear elementos que dependen de mouse up (Text, Square, etc.)
        if (!Canvas.getActiveObject() && currentMode !== modes.zoomToSelection) {
            addElementType(currentMode); 
        }

        // Zoom a selección 
        if (currentMode === modes.zoomToSelection) {
            if ((Math.abs(firstMouseXAbsolute - lastMouseXAbsolute) > 10) &&
                (Math.abs(firstMouseYAbsolute - lastMouseYAbsolute) > 10)) {

                var minX = Math.min(firstMouseXAbsolute, lastMouseXAbsolute);
                var maxX = Math.max(firstMouseXAbsolute, lastMouseXAbsolute);
                var minY = Math.min(firstMouseYAbsolute, lastMouseYAbsolute);
                var maxY = Math.max(firstMouseYAbsolute, lastMouseYAbsolute);

                var width = maxX - minX;
                var height = maxY - minY;

                var scaleX = Canvas.width / width;
                var scaleY = Canvas.height / height;
                var scale = Math.min(scaleX, scaleY);

                var offsetX = -(minX * scale) + (Canvas.width / 2) - (width * scale / 2);
                var offsetY = -(minY * scale) + (Canvas.height / 2) - (height * scale / 2);

                Canvas.setViewportTransform([scale, 0, 0, scale, offsetX, offsetY]);
                scaleRulers(Canvas.getZoom());
                positionRulers(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
                updateScrollbarsValues(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
            }
        }

        // restaurar filtro base de AREA
        if (mouseDownOnBackground) {
            const active = Canvas.getActiveObject();

            // Si no quedó nada seleccionado y tenemos un filtro base, lo reaplicamos
            if (!active && window.dotNetRefDesigner) {
                window.exitRelationsMode();
                window.setCanvasRelationsMode(window.dotNetRefDesigner, false);
                window.dotNetRefDesigner.invokeMethodAsync(
                    'ClearCanvasSelectionState'
                );
            }
            mouseDownOnBackground = false;
        }

        Canvas.selection = true;
    });

    Canvas.on('mouse:dblclick', (event) => {
        if (Canvas.getActiveObject() !== undefined && Canvas.getActiveObject() !== null && Canvas.getActiveObjects().length < 2) {
            var activeObject = Canvas.getActiveObject();

            if (activeObject.elementType !== 'Text' && activeObject.elementType !== 'Square' && activeObject.elementType !== 'LineStraight') {
                let objectId = null;
                let width = 0, height = 0;

                if (activeObject.elementType === 'groupArea') {
                    if (activeObject.type === 'group') {
                        let rect = activeObject._objects.find(obj => obj.type === 'rect');
                        if (rect) {
                            objectId = rect.id;
                            width = Math.floor(activeObject.width * activeObject.scaleX) || 0;
                            height = Math.floor(activeObject.height * activeObject.scaleY) || 0;
                        }
                    } else {
                        objectId = activeObject.id;
                    }

                    let objectAreaCanvas = {
                        Id: objectId,
                        CanvasObjectType: activeObject.elementType,
                        X: Math.floor(activeObject.left),
                        Y: Math.floor(activeObject.top),
                        Width: Math.floor(width),
                        Height: Math.floor(height),
                    }

                    openDataEditObject(objectAreaCanvas);
                }
                else if (activeObject.elementType === 'groupEquipment') {
                    let objectEquipment = modelEquipments.find(x => x.Id === activeObject.id);
                    openDataEditEquipment(objectEquipment);
                }
                else if (activeObject.elementType === 'groupStation') {
                    let objectStation = {
                        Id: activeObject.id.replace('groupStation', ''),
                        Name: activeObject.name,
                        CanvasObjectType: activeObject.elementType,
                        X: Math.floor(activeObject.left),
                        Y: Math.floor(activeObject.top),
                        Width: Math.floor(activeObject.width),
                        Height: Math.floor(activeObject.height),
                        locationTypes: activeObject.locationTypes ?? [],
                        zoneType: activeObject.zoneType,
                        Orientation: activeObject.orientation,
                        StatusObject: activeObject.statusObject
                    }

                    openStationEditObject(objectStation);
                }
                else if (activeObject.elementType === 'groupProcess') {
                    let objectProcess = {
                        Id: activeObject.id.replace('groupProcess', ''),
                        CanvasObjectType: activeObject.elementType,
                        X: Math.floor(activeObject.left),
                        Y: Math.floor(activeObject.top),
                        Width: Math.floor(activeObject.width),
                        Height: Math.floor(activeObject.height),
                        ProcessTypeId: activeObject.processTypeId
                    }

                    openProcessEditObject(objectProcess);
                }

                else if (activeObject.elementType === 'groupShelf') {
                    let objectStation = {
                        Id: activeObject.id.replace('groupShelf', ''),
                        CanvasObjectType: activeObject.elementType,
                        X: Math.floor(activeObject.left),
                        Y: Math.floor(activeObject.top),
                        Width: Math.floor(activeObject.width),
                        Height: Math.floor(activeObject.height),
                        LocationTypes: activeObject.LocationTypes,
                        /*NumCrossAisles: activeObject.NumCrossAisles,*/
                        NumShelves: activeObject.NumShelves
                    }

                    openShelfEditObject(objectStation);
                }

                else if ((activeObject.groupId === 'processDirectionGroup' || activeObject.groupId === 'processInGroup' || activeObject.groupId === 'processOutGroup' || activeObject.groupId === 'processCustomGroup') && activeObject.typeObject != 'controlCircle') {

                    if (activeObject.id != undefined) {
                        var processDirection = modelProcessDirections.find(obj => obj.Id === activeObject.id);
                        const jsonprocessDirection = JSON.stringify(processDirection);
                        openProcessDirectionEditObject(JSON.parse(jsonprocessDirection));
                    }
                }

                if (activeObject.groupId === 'routeGroup' && activeObject.typeObject != 'controlCircle') {
                    if (activeObject.id != undefined) {
                        var route = modelRoutes.find(obj => obj.Id === activeObject.id);
                        const jsonRoute = JSON.stringify(route);
                        openDataEditRoute(JSON.parse(jsonRoute));
                    }
                }
            }
        }
    });



    Canvas.on('mouse:down:before', (event) => {
        if (Canvas.getActiveObject() !== undefined && Canvas.getActiveObject() !== null) {
            var currentElement = Canvas.getActiveObject();

            if (currentElement.elementType === 'Text') {
                if (currentMode == modes.addText) {
                    firstTimeWriting = true;
                    currentMode = '';
                    Canvas.discardActiveObject();
                    deselectButton();
                }
                if (currentElement.text === '') {
                    currentElement.exitEditing();
                    DisabledButtonSave(true);
                    Canvas.remove(currentElement);
                }
            }
        }

        updateSaveButtonState(); // Updates the status of the button
    });

    function updateSaveButtonState() {
        // Check if there are objects on the canvas with status Add, Update and type allowed
        const canvasObjects = Canvas.getObjects();
        const allowed = ['LineStraight', 'Text', 'Square', 'groupArea', 'groupStation', 'circleControl', 'processOutGroup', 'processInGroup', 'processCustomGroup', 'startOrEndArrow', 'groupShelf', 'groupProcess', 'routeGroup'];
        const hasPermissiveCanvasObject = canvasObjects.some(obj => {
            if (!['Add', 'Update'].includes(obj.statusObject)) return false;

            const matches =
                allowed.includes(obj.elementType) ||
                allowed.includes(obj.groupId);

            return matches;
        });
        // Check if in cumulative there is at least one object with StatusObject === 'Delete'.
        let hasDeleteInAcumulable = false;

        try {
            const acumulableJson = typeof acumulable === 'string' ? JSON.parse(acumulable) : acumulable;

            const allCollections = [
                ...(acumulableJson.Areas || []),
                ...(acumulableJson.Objects || []),
                ...(acumulableJson.Routes || []),
                ...(acumulableJson.Equipments || []),
                ...(acumulableJson.Stations || []),
                ...(acumulableJson.Shelf || []),
                ...(acumulableJson.Process || []),
                ...(acumulableJson.ProcessDirections || [])
            ];

            hasDeleteInAcumulable = allCollections.some(item => item.StatusObject === 'Delete');
        } catch (e) {
            console.error("Error parsing cumulative JSON:", e);
        }

        // Enable button if there is any permissive object or any delete in cumulative
        const shouldEnable = hasPermissiveCanvasObject || hasDeleteInAcumulable;

        DisabledButtonSave(!shouldEnable);
    }

    Canvas.on('mouse:over', (e) => {
        onCanvas = true;

        const target = e.target;
        if (!target || !allowedTypes.includes(target.elementType)) return;

        showHoverOutline(target);

        currentHoveredObject = target;

        clearTimeout(tooltipTimer);
        tooltipTimer = setTimeout(() => {
            const nombre = target.name || (target.data && target.data.name) || 'Unnamed';
            //TODO: Revisar si requiere traducción
            tooltip.innerText = nombre;
            tooltip.style.left = e.e.clientX + 8 + 'px';
            tooltip.style.top = e.e.clientY + 5 + 'px';
            tooltip.style.display = 'block';
        }, waitingTimeTooltip);
    });

    Canvas.on("mouse:out", (e) => {
        onCanvas = false;
        if (isDraggingState) {
            isDraggingState = false;
        }

        clearTimeout(tooltipTimer);
        tooltip.style.display = 'none';
        currentHoveredObject = null;
    });

    Canvas.on("mouse:out", hideHoverOutline);

    ["mouse:down", "selection:created", "selection:cleared", "object:moving", "object:scaling"]
        .forEach(ev => Canvas.on(ev, hideHoverOutline));
    Canvas.on('text:editing:exited', () => {
        deselectButton();
        if (Canvas.getActiveObject() !== undefined && Canvas.getActiveObject() !== null) {
            var currentElement = Canvas.getActiveObject();

            if (currentElement.elementType === 'Text') {
                DisabledButtonSave(false);
                currentElement.width = currentElement.calcTextWidth();
                currentElement.height = currentElement.calcTextHeight();
            }
        }
    });

    //Function that keeps objects within the work area when dragging them out of the work area
    Canvas.on("object:modified", function (e) {
        let existArea = false;
        const activeObjects = Canvas.getActiveObjects();
        DisabledButtonSave(false);
        activeObjects.forEach(function (object) {
            if (object.statusObject !== "Add") {
                object.set("statusObject", "Update");
                if (object.elementType === 'groupArea') {
                    existArea = true;
                    let objRelatedArea = findObjectsInGroupArea(object);
                    activeObjects.push(...objRelatedArea);
                }
            }
        });
        updateSaveButtonState();

        if (activeObjects.length > 1) {
            const group = e.target;
          /*  adjustObjectPosition(group);*/

        } else {
            activeObjects.forEach((object) => {
                object.isModified = true;
                object.angle = Math.round((object.angle || 0 + 360) % 360);
               /* adjustObjectPosition(object);*/
            });
        }

        if (existArea) {
            updateAllPosicionRoute();
            removeGroup('processDirectionGroup');
            ChangeProcessColorsByVisiblePolyline();
        }


        Canvas.renderAll();

        function getAbsSineAngle(angle) {
            return Math.abs(Math.sin((angle * Math.PI) / 180));
        }

        function getAbsCosineAngle(angle) {
            return Math.abs(Math.cos((angle * Math.PI) / 180));
        }

        function getContainerBoxWidth(width, height, angle) {
            return width * getAbsCosineAngle(angle) + height * getAbsSineAngle(angle);
        }

        function getContainerBoxHeight(width, height, angle) {
            return width * getAbsSineAngle(angle) + height * getAbsCosineAngle(angle);
        }

        function getContainerBoxLeft(left, width, height, angle) {
            if (angle % 360 < 90) return left - height * getAbsSineAngle(angle);
            if (angle % 360 < 180) return left - getContainerBoxWidth(width, height, angle);
            if (angle % 360 < 270) return left - width * getAbsCosineAngle(angle);
            return left;
        }

        function getContainerBoxTop(top, width, height, angle) {
            if (angle % 360 < 90) return top;
            if (angle % 360 < 180) return top - height * getAbsCosineAngle(angle);
            if (angle % 360 < 270) return top - getContainerBoxHeight(width, height, angle);
            return top - width * getAbsSineAngle(angle);
        }
    });
    Canvas.on('object:moving', function (e) {
        const movingObject = e.target;
        const tolerance = 5;

        const allObjects = Canvas.getObjects().filter(obj =>
            obj !== movingObject &&
            ['groupStation', 'groupShelf', 'groupArea'].includes(obj.elementType)
        );

        let alignedX = null;
        let alignedY = null;

        allObjects.forEach(obj => {
            const centerX_obj = obj.left + obj.width / 2 * obj.scaleX;
            const centerY_obj = obj.top + obj.height / 2 * obj.scaleY;
            const centerX_mov = movingObject.left + movingObject.width / 2 * movingObject.scaleX;
            const centerY_mov = movingObject.top + movingObject.height / 2 * movingObject.scaleY;

            const dx = Math.abs(centerX_obj - centerX_mov);
            const dy = Math.abs(centerY_obj - centerY_mov);

            if (dx < tolerance) alignedX = centerX_obj;
            if (dy < tolerance) alignedY = centerY_obj;
        });

        if (alignedX !== null) {
            movingObject.left = alignedX - (movingObject.width / 2 * movingObject.scaleX);

            const sameTypeObjectsX = allObjects.filter(obj => obj.elementType === movingObject.elementType);
            const bounds = getBoundsFromAlignedObjects([...sameTypeObjectsX, movingObject], 'x', alignedX);
            showVerticalGuide(alignedX, bounds.minX, bounds.maxX);
        } else {
            hideVerticalGuide();
        }

        if (alignedY !== null) {
            movingObject.top = alignedY - (movingObject.height / 2 * movingObject.scaleY);

            const sameTypeObjectsY = allObjects.filter(obj => obj.elementType === movingObject.elementType);
            const bounds = getBoundsFromAlignedObjects([...sameTypeObjectsY, movingObject], 'y', alignedY);
            showHorizontalGuide(alignedY, bounds.minY, bounds.maxY);
        } else {
            hideHorizontalGuide();
        }

        const sameGroupObjects = allObjects.filter(obj =>
            obj.elementType === movingObject.elementType
        );

        if (sameGroupObjects.length > 0) {
            showEvenSpacingGuides(movingObject, sameGroupObjects);
        }

        movingObject.setCoords();
    });

    Canvas.on("object:modified", function (e) {
        hideVerticalGuide();
        hideHorizontalGuide();
        clearSpacingGuides();
    });
    Canvas.on('object:removed', (e) => {
        const removed = e.target;
        if (!removed) return;

        const isZone = removed.elementType === 'groupStation' || removed.elementType === 'groupShelf';
        if (!isZone || !removed.groupId) return;

        const areaId = removed.groupId.replace(/^(groupStation|groupShelf)/, '');
        resizeGroupAreaFromStations(areaId);
    });
    Canvas.selectionKey = 'ctrlKey';
    Canvas.on('selection:created', (e) => {
        const activeObject = Canvas.getActiveObject();   
        if (!activeObject) return;
        if (e?.e?.ctrlKey) {           
            return;
        }
        handleCanvasSelection(e);
        if (activeObject.elementType === 'groupProcess') {
            notifySelectedProcessFromActiveObject(activeObject);
        } else if (activeObject.elementType === 'groupArea') {
            notifySelectedAreaFromActiveObject(activeObject);
        }   
        if (Canvas.getActiveObjects().length > 1) {
            activeObject.lockMovementX = true;
            activeObject.lockMovementY = true;
            activeObject.hasControls = false;
            activeObject.hasBorders = false;
            cleanSelectionHierarchy(activeObject);
            Canvas.requestRenderAll();
        }
    });

    Canvas.on('selection:updated', (e) => {
        const activeObject = Canvas.getActiveObject();
        if (!activeObject) return;
        if (e?.e?.ctrlKey && activeObject?.type === 'activeSelection') {
            activeObject.lockMovementX = true;
            activeObject.lockMovementY = true;
            activeObject.hasControls = false;
            activeObject.hasBorders = false;
            cleanSelectionHierarchy(activeObject);
            return
        };
        handleCanvasSelection(e);
        if (activeObject.elementType === 'groupProcess') {
            notifySelectedProcessFromActiveObject(activeObject);
        } else if (activeObject.elementType === 'groupArea') {
            notifySelectedAreaFromActiveObject(activeObject);
        }    
        if (activeObject?.type === 'activeSelection') {
           
            activeObject.lockMovementX = true;
            activeObject.lockMovementY = true;
            activeObject.hasControls = false;
            activeObject.hasBorders = false;
            cleanSelectionHierarchy(activeObject);

        }
        //Activar para movimiento de process
        //else if (activeObject) {
        //    activeObject.lockMovementX = false;
        //    activeObject.lockMovementY = false;
        //    activeObject.hasControls = true;
        //}
    });

    function cleanSelectionHierarchy(activeSelection) {
        const selectedObjects = activeSelection._objects;
        if (!selectedObjects || selectedObjects.length <= 1) return;

        //Filtrar las áreas y procesos seleccionados
        const selectedAreas = selectedObjects.filter(obj => obj.elementType === 'groupArea');
        const selectedProcesses = selectedObjects.filter(obj => obj.elementType === 'groupProcess');

        if (selectedAreas.length === 0 && selectedProcesses.length === 0) return;

        const selectedAreaIds = selectedAreas.map(area => area.id);
        const selectedProcessIds = selectedProcesses.map(proc => proc.id);

        // 2Buscar hijos redundantes, rutas y processDirections dependientes
        const redundantChildren = selectedObjects.filter(obj => {
            // Ignorar las propias áreas y procesos seleccionados
            if (obj.elementType === 'groupArea') return false;
            // 1-. Hijos normales (zonas, procesos, estaciones) ligados a áreas
            const isDirectChild = selectedAreaIds.some(areaId => {
                const guid = areaId.replace('groupArea', '');
                return obj.groupId?.includes(guid);
            });
            if (isDirectChild) return true;
            //2-. Rutas (routeGroup) conectadas a áreas seleccionadas
            if (obj.groupId === 'routeGroup') {
                const linkedToArea = selectedAreaIds.some(areaId =>
                    obj.fromNode === areaId || obj.toNode === areaId
                );
                if (linkedToArea) return true;
            }
            //3-. ProcessDirections conectados a procesos seleccionados o procesos dentro de un área seleccionada
            if (obj.groupId === 'processInGroup' || obj.groupId === 'processOutGroup' || obj.groupId === 'processCustomGroup') {
                //3.1 Verificar si está conectado a procesos seleccionados directamente
                const cleanProcessIds = selectedProcessIds.map(id => id.replace('groupProcess', ''));
                const linkedToProcess = cleanProcessIds.some(procId =>
                    obj.fromNode === procId || obj.toNode === procId
                );
                if (linkedToProcess) return true;
                //3.2 Verificar si está conectado a procesos que pertenecen a un área seleccionada
                const linkedToAreaProcess = selectedAreaIds.some(areaId => {
                    const guid = areaId.replace('groupArea', '');
                    //Buscar todos los procesos que pertenecen a esa área
                    const processesInArea = Canvas.getObjects().filter(p =>
                        p.elementType === 'groupProcess' && p.groupId?.includes(guid)
                    );
                    //Tomar los IDs de esos procesos (sin el prefijo)
                    const processIdsInArea = processesInArea.map(p => p.id.replace('groupProcess', ''));
                    //Verificar si el processDirection conecta con alguno de esos procesos
                    return processIdsInArea.some(procId =>
                        obj.fromNode === procId || obj.toNode === procId
                    );
                });

                if (linkedToAreaProcess) return true;
            }
            return false;
        });
        if (redundantChildren.length === 0) return;
        const cleanSelection = selectedObjects.filter(obj => !redundantChildren.includes(obj));
        setTimeout(() => {
            if (activeSelection.type === 'activeSelection') {
                redundantChildren.forEach(child => {
                    activeSelection.removeWithUpdate(child);
                });
                if (activeSelection._objects.length === 1) {
                    const singleObj = activeSelection._objects[0];
                    Canvas.discardActiveObject();
                    Canvas.setActiveObject(singleObj);
                }
            } else {
                Canvas.discardActiveObject();

                if (cleanSelection.length > 1) {
                    const multi = new fabric.ActiveSelection(cleanSelection, { canvas: Canvas });
                    Canvas.setActiveObject(multi);
                } else {
                    Canvas.setActiveObject(cleanSelection[0]);
                }
            }
            Canvas.requestRenderAll();
        }, 0);
    }

    Canvas.upperCanvasEl.addEventListener("contextmenu", (e) => {
        e.preventDefault();
        handleRightClick(e);
    });
    // Mostrar el menú en click derecho
    Canvas.upperCanvasEl.addEventListener("contextmenu", (e) => {
        e.preventDefault();
        handleRightClick(e);
    });

    function handleRightClick(e) {
        const selected = Canvas.getActiveObjects();
        if (!selected || selected.length === 0) return hideMenu();

        const isValid = selected.every(obj =>
            obj.elementType === "groupStation" || obj.elementType === "groupShelf"
        );

        if (!isValid) return hideMenu();

        const menu = document.getElementById("contextMenuAlignObjects");
        menu.style.left = `${e.clientX}px`;
        menu.style.top = `${e.clientY}px`;
        menu.style.display = "block";
    }

    document.getElementById("contextMenuAlignObjects")
        .addEventListener("click", function (e) {
            const item = e.target.closest(".menu-item");
            if (!item) return;

            const action = item.dataset.action;
            if (action) {
                alignObjects(action);
            }
        });

    // Ocultar menú en click fuera
    document.addEventListener("click", () => hideMenu());
    const menu = document.getElementById("contextMenuAlignObjects");
    function hideMenu() {
       
        menu.style.display = "none";
    }

    /*************************************************************
     * MENÚ CONTEXTUAL PERSONALIZADO
     *************************************************************/
    const contextMenu = document.getElementById("contextMenu");
    let contextData = { point: null, targetObject: null, connector: null };

    if (typeof window.selectActionAddCanvas === 'function' && !window.__selectWrapperInstalled) {
        const _origSelect = window.selectActionAddCanvas;
        window.currentTool = 'select';
        window.selectActionAddCanvas = function (tool) {
            window.currentTool = tool;
            return _origSelect(tool);
        };
        window.__selectWrapperInstalled = true;
    }
    function getCurrentTool() {
        return window.currentTool || 'select';
    }

    document.addEventListener("click", function (e) {
        contextMenu.style.display = "none";
    });

    document.addEventListener('keydown', async (event) => {

        if (event.key === 'Escape') {
            ResetCopyPasteObject();
            Canvas.discardActiveObject();
            Canvas.requestRenderAll();
            if (document.activeElement && typeof document.activeElement.blur === 'function') {
                document.activeElement.blur();
            }
            selectActionAddCanvas('select');
        }

        if (event.repeat) return;

        const key = event.key.toLowerCase();
        const isMod = event.ctrlKey || event.metaKey; 
        const t = event.target;
        const inTextContext = isEditableElement(t) || isInModal(t);
        const isCtrlOrMeta = event.ctrlKey || event.metaKey; 
        const isShift = event.shiftKey;
        const isAlt = event.altKey;

        if (isMod && key === 'c' && !isShift && !isCopying) {
            if (inTextContext) return; 
            event.preventDefault();
            isCopying = true;

            CopyObject();

            setTimeout(() => {
                isCopying = false;
            }, 200);
        }

        if (isMod && key === 'v' && !isPastingShortcut) {
            if (inTextContext) return; 
            event.preventDefault();
            isPastingShortcut = true;

            try {
                await PasteObject();
            } catch (error) {
                console.error('Error en el pegado con atajo:', error);
            } finally {
                setTimeout(() => { isPastingShortcut = false; }, 600);
            }
        }

        if (!event.ctrlKey && !event.altKey && !event.metaKey && event.code === 'Space') {
            if (inTextContext) return;      // no interferir con inputs/modales
            if (event.repeat) return;       // evita reentradas
            event.preventDefault();

            if (!_handHoldActive) {
                _toolBeforeHold = getCurrentTool();
                selectActionAddCanvas('hand');
                _handHoldActive = true;
            }
           
        }

        if (!event.ctrlKey && !event.altKey && !event.metaKey && key === 'h') {
            if (inTextContext) return; // evitar en un input/modales
            event.preventDefault();
            selectActionAddCanvas('hand');
        }

        if (!event.ctrlKey && !event.altKey && !event.metaKey && key === 'v') {
            if (inTextContext) return; // evitar en un input/modales
            event.preventDefault();
            selectActionAddCanvas('select');
        }

        if (event.ctrlKey && (key === '+' || key === '=')) {
            event.preventDefault();
            ZoomIn();
        }

        if (event.ctrlKey && key === '-') {
            event.preventDefault();
            ZoomOut();
        }

        if (event.ctrlKey && key === '0') {
            event.preventDefault();
            zoomToSelectionChange(100);
        }

        if (event.ctrlKey && key === '1') {
            event.preventDefault();
            zoomToSelectionChange(0);
        }

        //if (event.ctrlKey && key === '2') {
        //    event.preventDefault();
        //    zoomToSelectionChange(1);
        //}
        //Alineación
        //if (isCtrlOrMeta && isShift && ALIGN_KEYS[key]) {
        //    if (inTextContext) return;
        //    event.preventDefault();
        //    alignObjects(ALIGN_KEYS[key]);
        //}
        //if (isCtrlOrMeta && isAlt && !isShift && key === 't') {
        //    if (inTextContext) return;
        //    event.preventDefault();
        //    alignObjects('top');
        //}

        const isInputFocused = document.activeElement.tagName === 'INPUT' || document.activeElement.tagName === 'TEXTAREA';
        if (!isInputFocused && event.key === 'Delete') {
            if (!isInputFocused && event.key === 'Delete' && document.getElementById('btnTash').disabled === false) {

                dotnetLocalHelper.invokeMethodAsync('OnClickDeleteButton')
                    .then(result => {
                        console.log('OnClickDeleteButton result:', result);
                    })
                    .catch(err => {
                        console.error('Error calling OnClickDeleteButton:', err);
                    });
            }

        }
    });

    const canvasEl = Canvas.upperCanvasEl || Canvas.getElement();
    canvasEl.addEventListener('mouseleave', () => {
        // Limpiamos coordenadas de cursor
        lastPointerCanvas.x = null;
        lastPointerCanvas.y = null;
        lastPointerValid = false;
    });

    document.addEventListener('keyup', (event) => {
        if (event.code === 'Space') {
           
            if (_handHoldActive) {
                event.preventDefault();
                selectActionAddCanvas(_toolBeforeHold || 'select');
                _handHoldActive = false;
                _toolBeforeHold = null;
            }
        }
    });

    window.addEventListener('blur', () => {
        if (_handHoldActive) {
            selectActionAddCanvas(_toolBeforeHold || 'select');
            _handHoldActive = false;
            _toolBeforeHold = null;
        }
    });

    document.getElementById("menuCreate").addEventListener("click", function (e) {
        if (contextData.connector) {
            contextData.connector.insertIntermediatePoint(contextData.point);
        }
        DisabledButtonSave(false);
        contextMenu.style.display = "none";
    });

    document.getElementById("menuDelete").addEventListener("click", function (e) {
        const connector = contextData.connector;
        const circle = contextData.targetObject;
        DisabledButtonSave(false);
      
        if (connector && circle) {
            const index = connector.controlCircles.indexOf(circle);
            if (index !== -1) {
                const puntoEliminado = connector.points[index + 1];
                if (!puntoEliminado) {
                    console.error('Punto a eliminar no encontrado en el índice especificado.');
                    return;
                }
                const puntoId = puntoEliminado.id;

                connector.points.splice(index + 1, 1);
                const route = routesViewport.find(r => r.polyline?.id === connector.polyline?.id);
                if (route && puntoId) {
                    route.points = route.points.filter(p => p.id !== puntoId);
                }

                Canvas.remove(circle);
                connector.drawIntermediateCircles();
                connector.polyline.set({ points: connector.points });

                if (connector.bidirectional && connector.endControlFrom?.type === 'group') {
                    connector.updateArrowControl(connector.endControlFrom, connector.points[1], connector.points[0]);
                }
                connector.updateArrowControl(
                    connector.endControlTo,
                    connector.points[connector.points.length - 2],
                    connector.points[connector.points.length - 1]
                );

                Canvas.requestRenderAll();
            }
        }

        contextMenu.style.display = "none";
    });

    Canvas.upperCanvasEl.addEventListener("contextmenu", function (e) {
        e.preventDefault();
        const pointer = Canvas.getPointer(e);
        let target = Canvas.findTarget(e, true);
        contextData.point = { x: pointer.x, y: pointer.y };
        contextData.targetObject = target;

        const menuCreate = document.getElementById("menuCreate");
        const menuDelete = document.getElementById("menuDelete");

        if (target && target.type === "circle" && target.typeObject === "controlCircle") {
            menuCreate.style.display = "none";
            menuDelete.style.display = "block";
            contextData.connector = target.connector;
        } else {
            // Corrección: si no es polyline, buscar manualmente la más cercana
            const objects = Canvas.getObjects().filter(obj => obj.type === 'polyline');
            for (let obj of objects) {
                if (isPointNearPolyline(obj, pointer)) {
                    target = obj;
                    contextData.targetObject = target;
                    contextData.connector = routesViewport.find(c => c.polyline === obj);
                    menuCreate.style.display = "block";
                    menuDelete.style.display = "none";
                    break;
                }
            }

            if (!target || target.type !== "polyline") {
                contextMenu.style.display = "none";
                return;
            }
        }

        contextMenu.style.left = e.pageX + "px";
        contextMenu.style.top = e.pageY + "px";
        contextMenu.style.display = "block";
    });

    const zoomInput = document.getElementById('zoomPercentageText');

    zoomInput.addEventListener('keydown', (event) => {
        if (event.key === 'Enter') {
            let rawValue = zoomInput.value.replace('%', '').trim();
            let value = parseInt(rawValue);

            if (isNaN(value) || value < 0) {
                value = minZoom * 100;
            } else if (value == 0) {
                value = 1;
            } else if (value > 100000) {
                value = 100000;
            }

            const newZoom = value / 100;
            Canvas.setZoom(newZoom);

            /*reviseZoom();*/
            scaleRulers(newZoom);
            updateScrollbarsValues(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
            updateZoomDisplay();
            zoomInput.blur();
        }
    });

    zoomInput.addEventListener('blur', () => {
        let value = zoomInput.value.replace('%', '').trim();
        if (!isNaN(value) && value !== '') {
            zoomInput.value = `${parseInt(value)}%`;
        }
    });

    zoomInput.addEventListener('focus', () => {
        zoomInput.value = zoomInput.value.replace('%', '').trim();
    });

    zoomInput.addEventListener('keydown', (event) => {
        const invalidKeys = ['e', 'E', '+', '-', '.', ',', '%'];

        if (invalidKeys.includes(event.key)) {
            event.preventDefault();
        }

        const allowedKeys = [
            'Backspace', 'Tab', 'ArrowLeft', 'ArrowRight', 'Delete', 'Enter'
        ];

        if (!/^\d$/.test(event.key) && !allowedKeys.includes(event.key)) {
            event.preventDefault();
        }
    });

    function handleCanvasSelection(e) {
        if (!e || !e.selected || e.selected.length !== 1) return;
        if (e?.e?.ctrlKey) return;
        const obj = e.selected[0];
        if (!obj) return;

        if (obj.groupId === 'routeGroup' || obj.groupId === 'processInGroup' || obj.groupId === 'processOutGroup' || obj.groupId === 'processCustomGroup') {
            return;
        }

        if (obj.elementType === 'groupArea') {
            const areaId = (obj.id || '').replace('groupArea', '');
            const selection = window.buildSelectionFromArea(areaId);

            if (selection) {
                const routeIds = selection.flows?.routeIds || [];
                if (routeIds.length > 0) {
                    // Modo “detalle”: área + rutas relacionadas
                    window.setRelationsState(true, areaId);
                    window.applyCanvasSelection(selection);
                    window.setCanvasRelationsMode(window.dotNetRefDesigner, true);
                } else {
                    window.setRelationsState(false);
                }
            }
            return;
        }

        //  PROCESS 
        if (obj.elementType === 'groupProcess') {
            const processId = (obj.id || '').replace('groupProcess', '');
            const selection = window.buildSelectionFromProcess(processId);

            const pdIds = selection?.flows?.processDirectionIds || [];
            const hasPD = pdIds.length > 0;

            if (selection && hasPD) {
                // Modo “detalle”: área + Itinerarios relacionadas
                const primaryAreaId = selection.primaryAreaId || getAreaIdFromProcessObj(obj);
                window.setRelationsState(true, primaryAreaId);
                window.applyCanvasSelection(selection);
                window.setCanvasRelationsMode(window.dotNetRefDesigner, true);
            } else {
                window.setRelationsState(false);
            }
            return;
        }

        window.setRelationsState(false);
    }
    Canvas.on('selection:created', handleCanvasSelection);
    Canvas.on('selection:updated', handleCanvasSelection);


}
function DeleteObject() {
    currentMode = modes.delete;
    jsonDelete = generateCanvasJson();
    addToAcumulable(jsonDelete);

    const activeObjects = Canvas.getActiveObjects();

    let removedAreaIds = new Set();
    let removedProcessIds = new Set();

    activeObjects.forEach(function (object) {

        if (object.elementType === 'groupProcess') {
            removedProcessIds.add(object.id);
        }

        if (object.elementType === 'groupEquipment') {
            modelEquipments = modelEquipments.filter(eq => eq.Id !== object.id);
        }

        if (object.elementType === 'groupArea') {
            let areaId = object.id.replace('groupArea', '');
            removedAreaIds.add(areaId);


            let objectsToRemove = Canvas.getObjects().filter(obj => obj.groupId === `groupStation${areaId}` || obj.groupId === `groupShelf${areaId}` || obj.groupId === `groupProcess${areaId}` ||
                modelEquipments.some(e => e.AreaId === areaId && e.Id === obj.id)
            );
            objectsToRemove.forEach(objToRemove => Canvas.remove(objToRemove));


            modelRoutes = modelRoutes.filter(r =>
                r.InboundAreaId !== areaId && r.OutboundAreaId !== areaId
            );
        }

        if (object.typeObject === 'controlCircle') return;

        if (object.groupId === 'routeGroup') {
            ClearAllDirectionsByType('routeGroup');
        }

        if (object.groupId == 'processInGroup' || object.groupId == 'processInGroup' || object.groupId == 'processCustomGroup')
        {
            const itineraryRemove = Canvas.getObjects().filter(x => x.linkedEntityId == object.linkedEntityId)
            itineraryRemove.forEach(line => Canvas.remove(line));
        }

        Canvas.remove(object);
    });


    const linesToRemove = Canvas.getObjects().filter(o =>
        o.elementType === 'processDirection' &&
        (
            removedProcessIds.has(o.sourceId) ||
            removedProcessIds.has(o.targetId) ||
            removedAreaIds.has(o.sourceId) ||
            removedAreaIds.has(o.targetId)
        )
    );
    linesToRemove.forEach(line => Canvas.remove(line));

    document.getElementById("btnTash").disabled = true;
    ResetCopyPasteObject();


    if (removedAreaIds.size > 0 || removedProcessIds.size > 0) {
        DrawingAllRoutes();
        DrawingAllProcessOutDirections();
        DrawingAllProcessInDirections();
       
        if (window.canvasSelectionFilter) {
            applyCanvasSelection(window.canvasSelectionFilter);
        }
    }

    ChangeProcessColorsByVisiblePolyline();
    DisabledButtonSave(true);
    Canvas.discardActiveObject();
    SaveComponentCanvas();
};

const CopyObject = () => {

    const activeObjects = Canvas.getActiveObjects();
    if (!activeObjects || activeObjects.length === 0) return;

    const allowedTypes = ['groupArea', 'Text', 'LineStraight', 'Square', 'groupStation', 'groupShelf', 'groupProcess', 'groupEquipment'];
    const allAllowed = activeObjects.every(obj => allowedTypes.includes(obj.elementType) && obj.statusObject != 'Add');
    if (!allAllowed) return;
    if (activeObjects.length > 1) {
        return;
    }

    document.getElementById("btnCopy").disabled = true;
    document.getElementById("btnPaste").disabled = false;
    Canvas.selection = false;
    pasteCount = 0;
    copiedObjects = [];

    let copiedStation = false;
    let copiedShelf = false;
    let copiedProcess = false;

    activeObjects.forEach(obj => {
        const idObject = generateUUID();
        if (obj.elementType === 'groupStation') copiedStation = true;
        else if (obj.elementType === 'groupShelf') copiedShelf = true;
        else if (obj.elementType === 'groupProcess') copiedProcess = true;
        else if (obj.elementType === 'groupEquipment') copiedProcess = true;
        obj.clone(clone => {
            if (obj.elementType) {
                clone.idOriginal = cleanGroupIdValue(obj.id);
                clone.id = obj.elementType + idObject;
                clone.elementType = obj.elementType;
                clone.areaType = obj.areaType;
                clone.alternativeAreaId = obj.alternativeAreaId;
                clone.groupId = obj.groupId;     
                clone.name = obj.name;
            }
            copiedObjects.push(clone);
        });
    });
    if (typeof window.showCopyToast === 'function') {
    } if (copiedStation || copiedShelf || copiedProcess || copiedProcess) {
        window.showCopyToast(L('Click on the area where you want to pasate the element.'));
    }
};


const cloneObjectAsync = (obj) => {
    return new Promise((resolve) => {
        obj.clone(clone => {
            resolve(clone);
        });
    });
};

const PasteObject = async () => {
    if (!Canvas || isPasting) return;
    isPasting = true;

    try {
        Canvas.selection = true;

        if (!copiedObjects || copiedObjects.length === 0) {
            document.getElementById("btnCopy").disabled = true;
            document.getElementById("btnPaste").disabled = true;
            isPasting = false;
            return;
        }

        objectSelection = Canvas.getActiveObjects().length > 0 ? Canvas.getActiveObjects()[0].id : null;

        const offset = 20;
        const drawCopyArea = [];
        const drawCopyProcess = [];
        const drawCopyEquipments = [];
        const drawCopyStations = [];
        const drawCopyShelf = [];
        const drawCopySteps = [];
        const selectObject = [];

        const groupsMap = {
            groupProcess: drawCopyProcess,
            groupEquipment: drawCopyEquipments,
            groupStation: drawCopyStations,
            groupShelf: drawCopyShelf,
            groupStep: drawCopySteps
        };

        let usedStairPlacement = false;
        for (const original of copiedObjects) {
            
            if (original.elementType === 'groupArea') {
                usedStairPlacement = true;
                const newLeft = (original.left || 0) + offset * (pasteCount + 1);
                const newTop = (original.top || 0) + offset * (pasteCount + 1);

                const cloneArea = { ...extractSelectedProps(original), x: newLeft, y: newTop, left: newLeft, top: newTop };
                const textObj = original.name;
                cloneArea.name = getNextAvailableCopyName(textObj, 'area');

                drawCopyArea.push(cloneArea);
                selectObject.push({ tab: 'Area', id: cloneArea.id });

                const relatedId = original.idOriginal;
                Canvas.getObjects().forEach(o => {
                    if (!o.groupId) return;
                    const cleaned = cleanGroupIdValue(o.groupId);
                    if (cleaned !== relatedId) return;

                    const targetArray = groupsMap[o.elementType];
                    if (!targetArray) return;

                    const newLeft = (o.left || 0) + offset * (pasteCount + 1);
                    const newTop = (o.top || 0) + offset * (pasteCount + 1);

                    const objProps = {
                        ...extractSelectedProps(o, cloneArea.id),
                        statusObject: 'Add',
                        idOriginal: cleanGroupIdValue(o.id),
                        left: newLeft,
                        top: newTop,
                        x: newLeft,
                        y: newTop,
                    };
                    targetArray.push(objProps);

                    if (o.elementType === 'groupProcess' && o.steps?.length > 0) {
                        o.steps.forEach(step => {
                            const stepProps = {
                                id: generateUUID(),
                                endProcess: step.EndProcess,
                                initProcess: step.InitProcess,
                                name: getNextAvailableCopyName(step.Name, 'steps'),
                                order: step.Order,
                                statusObject: 'Add',
                                idOriginal: step.Id,
                                processId: cleanGroupIdValue(objProps.id),
                            };
                            const stepArray = groupsMap['groupStep'];
                            if (stepArray) stepArray.push(stepProps);
                        });
                    }
                });
                continue;
            }

            if (original.elementType === 'groupShelf') {
                const selectedArea = Canvas.getActiveObjects().find(o => o.elementType === 'groupArea');
                const objZone = Canvas.getObjects().find(o => cleanGroupIdValue(o.id) === original.idOriginal);
                if (!objZone) continue;

                const area = Canvas.getObjects().find(o =>
                    o.elementType === 'groupArea' && cleanGroupIdValue(o.id) === cleanGroupIdValue(objZone.groupId)
                );
                if (!area) continue;

                const placement = resolveZonePastePlacement(objZone, area, selectedArea, offset, pasteCount);
                if (placement.cancel) continue;

                const { newLeft, newTop, modelArea, destinationArea, usedStairPlacement: usedStairLocal } = placement;
                if (usedStairLocal) usedStairPlacement = true;

                // Pegado en misma área
                if (destinationArea && cleanGroupIdValue(destinationArea.id) === cleanGroupIdValue(area.id)) {
                    drawCopyShelf.push(extractOriginalSelectProps(objZone));
                }

                if (modelArea) drawCopyArea.push(modelArea);

                const cloneShelf = {
                    ...extractSelectedProps(objZone, cleanGroupIdValue(destinationArea.id)),
                    statusObject: 'Add',
                    idOriginal: cleanGroupIdValue(objZone.id),
                    x: newLeft,
                    y: newTop,
                    left: newLeft,
                    top: newTop,
                    groupId: original.groupId || null,
                };

                drawCopyShelf.push(cloneShelf);

                const relatedId = cleanGroupIdValue(destinationArea.id);

                Canvas.getObjects().forEach(o => {
                    if (!o.groupId) return;
                    const cleaned = o.groupId
                        .replace('groupProcess', '')
                        .replace('groupStation', '')
                        .replace('groupEquipment', '')
                        .replace('groupShelf', '');
                    if (cleaned !== relatedId) return;
                    if (o.elementType === 'groupShelf' && cleanGroupIdValue(o.id) === cleanGroupIdValue(objZone.id)) return;

                    const targetArray = groupsMap[o.elementType];
                    if (!targetArray) return;
                    targetArray.push(extractOriginalSelectProps(o));

                    if (o.elementType === 'groupProcess' && o.steps?.length > 0) {
                        o.steps.forEach(step => {
                            const stepProps = {
                                id: step.Id,
                                endProcess: step.EndProcess,
                                initProcess: step.InitProcess,
                                name: step.Name,
                                order: step.Order,
                                statusObject: 'Add',
                                idOriginal: step.Id,
                                processId: cleanGroupIdValue(o.id),
                            };
                            const stepArray = groupsMap['groupStep'];
                            if (stepArray) stepArray.push(stepProps);
                        });
                    }
                });
                continue;
            }

            if (original.elementType === 'groupStation') {
                const selectedArea = Canvas.getActiveObjects().find(o => o.elementType === 'groupArea');
                const objZone = Canvas.getObjects().find(o => cleanGroupIdValue(o.id) === original.idOriginal);
                if (!objZone) continue;

                const area = Canvas.getObjects().find(o =>
                    o.elementType === 'groupArea' && cleanGroupIdValue(o.id) === cleanGroupIdValue(objZone.groupId)
                );
                if (!area) continue;

                const placement = resolveZonePastePlacement(objZone, area, selectedArea, offset, pasteCount);
                if (placement.cancel) continue;

                const { newLeft, newTop, modelArea, destinationArea, usedStairPlacement: usedStairLocal } = placement;
                if (usedStairLocal) usedStairPlacement = true;

                // Pegado en misma área
                if (destinationArea && cleanGroupIdValue(destinationArea.id) === cleanGroupIdValue(area.id)) {
                    drawCopyStations.push(extractOriginalSelectProps(objZone));
                }

                if (modelArea) drawCopyArea.push(modelArea);

                const cloneZone = {
                    ...extractSelectedProps(objZone, cleanGroupIdValue(destinationArea.id)),
                    statusObject: 'Add',
                    idOriginal: cleanGroupIdValue(objZone.id),
                    x: newLeft,
                    y: newTop,
                    left: newLeft,
                    top: newTop,
                };
                drawCopyStations.push(cloneZone);

                const relatedId = cleanGroupIdValue(destinationArea.id);
                Canvas.getObjects().forEach(o => {
                    if (!o.groupId) return;
                    const cleaned = o.groupId
                        .replace('groupProcess', '')
                        .replace('groupStation', '')
                        .replace('groupEquipment', '')
                        .replace('groupShelf', '');
                    if (cleaned !== relatedId) return;
                    if (o.elementType === 'groupStation' && cleanGroupIdValue(o.id) === cleanGroupIdValue(objZone.id)) return;

                    const targetArray = groupsMap[o.elementType];
                    if (!targetArray) return;
                    targetArray.push(extractOriginalSelectProps(o));

                    if (o.elementType === 'groupProcess' && o.steps?.length > 0) {
                        o.steps.forEach(step => {
                            const stepProps = {
                                id: step.Id,
                                endProcess: step.EndProcess,
                                initProcess: step.InitProcess,
                                name: step.Name,
                                order: step.Order,
                                statusObject: 'Add',
                                idOriginal: step.Id,
                                processId: cleanGroupIdValue(o.id),
                            };
                            const stepArray = groupsMap['groupStep'];
                            if (stepArray) stepArray.push(stepProps);
                        });
                    }
                });
                continue;
            }

            if (original.elementType === 'groupProcess') {
                const selectedArea = Canvas.getActiveObjects().find(o => o.elementType === 'groupArea');
                const objProcess = Canvas.getObjects().find(o => cleanGroupIdValue(o.id) === original.idOriginal);
                if (!objProcess) continue;

                const area = Canvas.getObjects().find(o =>
                    o.elementType === 'groupArea' && cleanGroupIdValue(o.id) === cleanGroupIdValue(objProcess.groupId)
                );
                if (!area) continue;

                const destinationArea = (selectedArea && selectedArea.id !== area.id) ? selectedArea : area;
                const modelArea = extractOriginalSelectProps(destinationArea);

                if (modelArea) drawCopyArea.push(modelArea);

                groupsMap['groupProcess'] = groupsMap['groupProcess'] || [];
                groupsMap['groupStep'] = groupsMap['groupStep'] || [];

                const relatedId = cleanGroupIdValue(destinationArea.id);
                Canvas.getObjects().forEach(o => {
                    if (!o.groupId) return;
                    const cleaned = o.groupId
                        .replace('groupProcess', '')
                        .replace('groupStation', '')
                        .replace('groupEquipment', '')
                        .replace('groupShelf', '');
                    if (cleaned !== relatedId) return;

                    if (o.elementType === 'groupProcess') {
                        groupsMap['groupProcess'].push(extractOriginalSelectProps(o));
                        if (o.steps?.length > 0) {
                            o.steps.forEach(step => {
                                groupsMap['groupStep'].push({
                                    id: step.Id,
                                    endProcess: step.EndProcess,
                                    initProcess: step.InitProcess,
                                    name: step.Name,
                                    order: step.Order,
                                    statusObject: 'StoredInBase',
                                    idOriginal: step.Id,
                                    processId: cleanGroupIdValue(o.id),
                                });
                            });
                        }

                        return;
                    }

                    const targetArray = groupsMap[o.elementType];
                    if (targetArray) targetArray.push(extractOriginalSelectProps(o));
                });

                const cloneProcess = {
                    ...extractSelectedProps(objProcess, cleanGroupIdValue(destinationArea.id)),
                    statusObject: 'Add',
                    idOriginal: cleanGroupIdValue(objProcess.id),
                    x: 0,
                    y: 0,
                    left: 0,
                    top: 0,
                    groupId: original.groupId || null,
                };

                groupsMap['groupProcess'].push(cloneProcess);
                selectObject.push({ tab: 'Area', id: cloneProcess.id });

                if (objProcess.steps?.length > 0) {
                    objProcess.steps.forEach(step => {
                        groupsMap['groupStep'].push({
                            id: generateUUID(),
                            endProcess: step.EndProcess,
                            initProcess: step.InitProcess,
                            name: getNextAvailableCopyName(step.Name, 'steps'),
                            order: step.Order,
                            statusObject: 'Add',
                            idOriginal: step.Id,
                            processId: cleanGroupIdValue(cloneProcess.id),
                        });
                    });
                }

                continue;
            }

            if (original.elementType === 'groupEquipment') {
                const selectedArea = Canvas.getActiveObjects().find(o => o.elementType === 'groupArea');
                if (selectedArea && selectedArea.id !== cleanGroupIdValue(original.groupId)) {
                    const objEquipment = Canvas.getObjects().find(o => o.id == original.idOriginal);
                   
                        const modelArea = extractOriginalSelectProps(selectedArea);
                        if (modelArea) drawCopyArea.push(modelArea);
                        extractSelectedProps(objEquipment, cleanGroupIdValue(selectedArea.id));

                        const relatedId = cleanGroupIdValue(selectedArea.id);
                        Canvas.getObjects().forEach(o => {
                            if (!o.groupId) return;
                            const cleaned = o.groupId
                                .replace('groupProcess', '')
                                .replace('groupStation', '')
                                .replace('groupEquipment', '')
                                .replace('groupShelf', '');
                            if (cleaned !== relatedId) return;
                            if (o.elementType === 'groupEquipment' && cleanGroupIdValue(o.id) === cleanGroupIdValue(objEquipment.id)) return;

                            const targetArray = groupsMap[o.elementType];
                            if (!targetArray) return;
                            targetArray.push(extractOriginalSelectProps(o));

                            if (o.elementType === 'groupProcess' && o.steps?.length > 0) {
                                o.steps.forEach(step => {
                                    const x = {
                                        id: step.Id,
                                        endProcess: step.EndProcess,
                                        initProcess: step.InitProcess,
                                        name: step.Name,
                                        order: step.Order,
                                        statusObject: 'Add',
                                        idOriginal: step.Id,
                                        processId: cleanGroupIdValue(o.id),
                                    };
                                    const stepArray = groupsMap['groupStep'];
                                    if (stepArray) stepArray.push(stepProps);
                                });
                            }
                        });
                    
                } else {
                    window.showSelected(L('You must select an area to continue.'));
                }
                continue;
            }

            // ====================== Clonación general para otros objetos ======================
            const clone = await cloneObjectAsync(original);
            usedStairPlacement = true;
            const offsetLeft = (clone.left || 0) + offset * (pasteCount + 1);
            const offsetTop = (clone.top || 0) + offset * (pasteCount + 1);

            clone.set({
                id: generateUUID(),
                idOriginal: original.idOriginal,
                left: offsetLeft,
                top: offsetTop,
                x: offsetLeft,
                y: offsetTop,
                statusObject: 'Add',
                evented: true,
                selectable: true,
                elementType: original.elementType || null,
                groupId: original.groupId || null,
                areaId: !!original.groupId ? cleanGroupIdValue(original.groupId) : null
            });

            if (clone.text || clone.name) {
                const originalName = clone.text || clone.name;
                const typeHint = clone.elementType?.replace('group', '').toLowerCase();
                const newName = getNextAvailableCopyName(originalName, typeHint);
                if (clone.text) clone.text = newName;
                if (clone.name) clone.name = newName;
            }

            Canvas.add(clone);
            Canvas.renderAll();

            if (clone.elementType === 'groupEquipment') {
                modelEquipments.push({
                    Id: clone.id,
                    AreaId: clone.areaId,
                    NEquipment: clone.nequipment,
                    Name: clone.name,
                    StatusObject: 'Add',
                    TypeName: clone.typeName
                });
            }
        }

        drawCopyArea.forEach(area => {
            drawAreaOnCanvas(area, drawCopyProcess, drawCopyStations, drawCopyShelf, [], [], drawCopySteps);
        });

        if (usedStairPlacement) {
            pasteCount = (pasteCount || 0) + 1;
        }

        setTimeout(() => {
            const jsonSelectObject = JSON.stringify(selectObject);
            SaveComponentCanvas(jsonSelectObject);
        }, 100);

    } catch (error) {
        console.error('Error en PasteObject:', error);
        window.showSelected(L('An error occurred while pasting objects.'));
    } finally {
        isPasting = false;
    }
};

window.SaveOnPostback = function () {
    SaveMessageModal(true);
}

const generateCanvasJson = () => {
    const canvasObjects = currentMode == modes.save ? Canvas.getObjects() : Canvas.getActiveObjects();
    const ignoreStatusObject = currentMode == modes.save ? 'StoredInBase' : currentMode == modes.delete ? 'Add' : '';

    const areas = [];
    const objects = [];
    const routes = [];
    const storage = [];
    const equipments = [];
    const stations = [];
    const shelf = [];
    const process = [];
    const processDirection = [];
    const steps = [];

    canvasObjects
        .filter(obj => obj.id !== 'background')
        .forEach(obj => {

            if (obj.elementType && obj.elementType == 'groupStorage') {
                const rect = obj._objects.find(o => o.type === 'rect');
                if (rect && rect.statusObject != ignoreStatusObject) {
                    storage.push({
                        Id: obj.id || null,
                        CanvasObjectType: rect.elementType || 'Unknown',
                        Left: Math.floor(obj.left) || null,
                        Top: Math.floor(obj.top) || null,
                        Height: obj.height || 0,
                        Width: obj.width || 0,
                        X: Math.floor(obj.x1) || 0,
                        Y: Math.floor(obj.y1) || 0,
                        X2: Math.floor(obj.x2) || null,
                        Y2: Math.floor(obj.y2) || null,
                        StatusObject: currentMode == modes.save ? (obj.statusObject || null) : 'Delete',
                        LayoutId: null,
                    });
                }
            }

            else if (obj.elementType && obj.elementType == 'groupArea') {
                //const rect = obj._objects.find(o => o.type === 'rect');
                const text = obj._objects.find(o => o.type === 'text');
                if (obj.statusObject != ignoreStatusObject) {
                    areas.push({
                        AreaType: obj.areaType,
                        Id: obj.id.replace('groupArea', '') || null,
                        CanvasObjectType: 'Area',
                        Name: obj.name,
                        X: Math.floor(obj.left) || 0,
                        Y: Math.floor(obj.top) || 0,
                        Height: Math.floor(obj.height * obj.scaleY) || 0,
                        Width: Math.floor(obj.width * obj.scaleX) || 0,
                        Angle: Math.floor(obj.angle) || null,
                        StatusObject: currentMode == modes.save ? (obj.statusObject || null) : 'Delete',
                        LayoutId: null,
                        AlternativeAreaId: obj.alternativeAreaId,
                    });
                }
            }

            else if (obj.groupId === 'routeGroup') {
                const route = modelRoutes.find(x => x.Id === obj.id);
                if (!route) return;
                if (currentMode == modes.delete) {
                    route.StatusObject = 'Delete';
                    routes.push(route);
                    removeRoute(route);
                    return;
                }
                if (currentMode == modes.save) {
                    if (route.StatusObject === 'StoredInBase') {
                        return;
                    }
                    route.StatusObject = 'Edit';
                }
                const routeFromMemory = routesViewport.find(r => r.polyline && r.polyline.id === obj.id);
                if (routeFromMemory) {
                    const pointsArray = routeFromMemory.points.map(p => ({
                        id: p.id,
                        x: Math.floor(p.x),
                        y: Math.floor(p.y)
                    }));
                    route.ViewPort = JSON.stringify(pointsArray);
                }

                routes.push(route);
            }


            else if (obj.statusObject != ignoreStatusObject && obj.groupId && obj.groupId.startsWith('groupEquipment')) {
                const rect = obj._objects.find(o => o.type === 'rect');

                equipments.push({
                    Id: obj.id || null,
                    AreaId: obj.groupId.replace('groupEquipment', ''),
                    CanvasObjectType: obj.elementType || 'groupEquipment',
                    Name: obj.name || 'Equipment',
                    X: Math.floor(obj.x1) || 0,
                    Y: Math.floor(obj.y1) || 0,
                    X2: Math.floor(obj.x2) || null,
                    Y2: Math.floor(obj.y2) || null,
                    Left: Math.floor(obj.left) || null,
                    Top: Math.floor(obj.top) || null,
                    Width: obj.scaleX ? Math.floor(obj.width * obj.scaleX) : Math.floor(obj.width),
                    Height: obj.scaleY ? Math.floor(obj.height * obj.scaleY) : Math.floor(obj.height),
                    Angle: obj.angle || null,
                    Text: obj.text || null,
                    StatusObject: currentMode == modes.save ? (obj.statusObject || null) : 'Delete',
                    LayoutId: null,
                    IdOriginal: obj.idOriginal || null,
                    NEquipment: obj.nequipment
                });
            }

            else if (obj.statusObject != ignoreStatusObject && obj.groupId && obj.groupId.startsWith('groupStation')) {
                const rect = obj._objects.find(o => o.type === 'rect');
                const text = obj._objects.find(o => o.type === 'text');

                const scaledWidth = Math.floor(obj.width * obj.scaleX) || 0;
                const scaledHeight = Math.floor(obj.height * obj.scaleY) || 0;

                stations.push({
                    AreaId: obj.groupId.replace('groupStation', ''),
                    Id: obj.id.replace('groupStation', ''),
                    CanvasObjectType: 'Station',
                    Name: text ? text.text : '',
                    X: Math.floor(obj.left) || 0,
                    Y: Math.floor(obj.top) || 0,
                    Width: scaledWidth,
                    Height: scaledHeight,
                    Angle: Math.floor(rect.angle) || null,
                    StatusObject: currentMode == modes.save ? (obj.statusObject || null) : 'Delete',
                    LayoutId: null,
                    ZoneType: obj.zoneType,
                    Orientation: obj.orientation,
                    Angle: obj.angle,
                    IdOriginal: obj.idOriginal || null,
                });
            }

            else if (obj.statusObject != ignoreStatusObject && obj.groupId && obj.groupId.startsWith('groupShelf')) {
                const rect = obj._objects.find(o => o.type === 'rect');

                const npasX = typeof obj.quantity === 'number' ? obj.quantity : 0;
                const bHeight = typeof obj.barHeight === 'number' ? obj.barHeight : Math.floor(obj.height * (obj.scaleY || 1));

                shelf.push({
                    AreaId: obj.groupId.replace('groupShelf', ''),  // ID del área
                    Id: obj.id.replace('groupShelf', ''),       // ID de la shelf
                    ZoneId: obj.ZoneId,
                    Name: obj.name,
                    CanvasObjectType: 'Shelf',
                    X: Math.floor(obj.left) || 0,
                    Y: Math.floor(obj.top) || 0,
                    Width: Math.floor(obj.width * (obj.scaleX || 1)) || 0,
                    Height: Math.floor(obj.height * (obj.scaleY || 1)) || 0,
                    Angle: rect ? Math.floor(rect.angle) : 0,
                    LayoutId: null,
                    ZoneType: obj.ZoneType || null,
                    QuantityHeigth: npasX,
                    BarHeight: bHeight,
                   /* NumCrossAisles: obj.NumCrossAisles,*/
                    NumShelves: obj.NumShelves,
                    IsVertical: obj.IsVertical,
                    LocationTypes: obj.LocationTypes,
                    PositionX: Math.floor(obj.left) || 0,
                    PositionY: Math.floor(obj.top) || 0,
                    Width: Math.floor(obj.width * (obj.scaleX || 1)) || 0,
                    Height: Math.floor(obj.height * (obj.scaleY || 1)) || 0,
                    StatusObject: currentMode == modes.save ? (obj.statusObject || null) : 'Delete',
                    ZoneType: obj.zoneType,
                    IdOriginal: obj.idOriginal || null,
                });
            }

            else if (obj.groupId && obj.groupId.startsWith('groupProcess')) {

                if (currentMode == modes.save && obj.statusObject === "StoredInBase") {
                    return;
                }
                process.push({
                    AreaId: obj.groupId.replace('groupProcess', ''),
                    Id: obj.id.replace('groupProcess', ''),
                    CanvasObjectType: 'Process',
                    Name: obj.name.replace(/\s*\(.*?\)\s*$/, ""),
                    X: Math.floor(obj.left) || 0,
                    Y: Math.floor(obj.top) || 0,
                    Height: Math.floor(obj.height) || 0,
                    Width: Math.floor(obj.width) || 0,
                    Angle: 0,
                    StatusObject: currentMode == modes.save ? (obj.statusObject || null) : 'Delete',
                    LayoutId: null,
                    ProcessTypeId: obj.processTypeId,
                    IdOriginal: obj.idOriginal || null
                });

                if (Array.isArray(obj.steps)) {
                    obj.steps.forEach((step, index) => {
                        steps.push({
                            Id: step.Id || null,
                            ProcessId: obj.id.replace('groupProcess', ''),
                            Name: step.Name.replace(/\s*\(.*?\)\s*$/, ""),
                            TimeMin: step.TimeMin || 0,
                            StatusObject: step.StatusObject || "StoredInBase",
                            Sequence: index + 1 
                        });
                    });
                }
            }


            else if (obj.groupId === 'processInGroup' || obj.groupId === 'processOutGroup' || obj.groupId === 'processCustomGroup') {
                const direction = modelProcessDirections.find(x => x.Id === obj.id);
                if (!direction) return;

                if (currentMode == modes.delete) {
                    direction.StatusObject = 'Delete';
                    processDirection.push(direction);
                    modelProcessDirections = modelProcessDirections.filter(x => x.Id !== obj.id.replace('processDirectionGroup', ''));
                    return;
                }
                if (currentMode == modes.save) {
                    if (direction.StatusObject === 'StoredInBase') {
                        return;
                    }
                    direction.StatusObject = direction.StatusObject || 'Edit';
                }

                const processFromMemory = routesViewport.find(r => r.polyline && r.polyline.id === obj.id);
                if (processFromMemory) {
                    const pointsArray = processFromMemory.points.map(p => ({
                        id: p.id,
                        x: Math.floor(p.x),
                        y: Math.floor(p.y)
                    }));
                    direction.ViewPort = JSON.stringify(pointsArray);
                }

                processDirection.push(direction);
            }

            else {
                if (obj.statusObject != ignoreStatusObject) {
                    objects.push({
                        Id: obj.id || null,
                        CanvasObjectType: obj.elementType || 'Unknown',
                        Name: obj.elementType || 'Unknown',
                        X: Math.floor(obj.x1) || 0,
                        Y: Math.floor(obj.y1) || 0,
                        X2: Math.floor(obj.x2) || null,
                        Y2: Math.floor(obj.y2) || null,
                        Left: Math.floor(obj.left) || null,
                        Top: Math.floor(obj.top) || null,
                        Width: obj.scaleX ? Math.floor(obj.width * obj.scaleX) : Math.floor(obj.width),
                        Height: obj.scaleY ? Math.floor(obj.height * obj.scaleY) : Math.floor(obj.height),
                        Angle: obj.angle || null,
                        Text: obj.text || null,
                        StatusObject: currentMode == modes.save ? (obj.statusObject || null) : 'Delete',
                        LayoutId: null,
                    });
                }
            }
        });

    const resultJson = {};

    if (areas.length > 0)
        resultJson.Areas = areas;

    if (objects.length > 0)
        resultJson.Objects = objects;

    if (routes.length > 0)
        resultJson.Routes = routes;

    if (equipments.length > 0)
        resultJson.Equipments = equipments;

    if (stations.length > 0)
        resultJson.Stations = stations;

    if (shelf.length > 0)
        resultJson.Shelf = shelf;

    if (process.length > 0)
        resultJson.Process = process;

    if (processDirection.length > 0)
        resultJson.ProcessDirections = processDirection;

    if (steps.length > 0)
        resultJson.Steps = steps;

    return JSON.stringify(resultJson);
};

function SaveComponentCanvas(objectSelectionJson = '') {
    if (!Canvas)
        return;

    currentMode = modes.save;
    Canvas.discardActiveObject();
    Canvas.requestRenderAll();
    modelLayoutDto.viewport = generateCanvasJson();
    if (modelLayoutDto.viewport != '{}' || acumulable != '{"Areas":[],"Objects":[], "Routes": [], "Equipments": [], "Stations": [], "Shelf": [], "Process": [], "ProcessDirections": []}')
        updateLayoutFromJs(modelLayoutDto, objectSelectionJson);
    routesViewport = [];

}

function updateLayoutFromJs(layoutDto, objectSelectionJson = '') {
    layoutDto = fixPropertyNames(layoutDto);
    const layoutDtoJson = JSON.stringify(layoutDto);
    let functionUpdate = tabSlected == '' ? 'UpdateCanvasNotRefresh' : 'UpdateCanvas';
    dotnetLocalHelper.invokeMethodAsync(functionUpdate, layoutDtoJson, acumulable, objectSelectionJson)
        .then(result => {
            console.log('UpdateCanvasNotRefresh result:', result);
            callRecalculateRelatedElements();
            selectObjectPreviusSave();
        })
        .catch(err => {
            console.error('Error calling UpdateCanvasNotRefresh:', err);
        });

    modelLayoutDto.viewport = "";
    jsonDelete = '[]';
    acumulable = '{"Areas":[],"Objects":[], "Routes": [], "Equipments": [], "Stations": [], "Shelf": [], "Process": [], "ProcessDirections": []}';
    completeSaveFunctionality();
}

function completeSaveFunctionality() {
    Canvas.getObjects().forEach(obj => {
        if (obj.statusObject === 'Add' || obj.statusObject === 'Update') {
            obj.statusObject = 'StoredInBase';
            obj.idOriginal = undefined;
        }
    });
}

function selectObjectPreviusSave() {

    if (objectSelection != null) {
        if (typeof Canvas !== 'undefined') {
            const selected = Canvas.getObjects().find(o => o.id === objectSelection);
            if (selected) {
                Canvas.setActiveObject(selected);
                Canvas.requestRenderAll();
                objectSelection = null;
            }
        }
    }
}

// 2) Función que crea el botón y lo engancha
window.applyCanvasSelection = function (selection) {
    try {
        if (selection === undefined) return;

        tabSlected = '';
        const waitForCanvas = (callback, interval = 50, maxAttempts = 100) => {
            let attempts = 0;

            const check = () => {
                if (Canvas) {
                    callback();
                } else if (attempts < maxAttempts) {
                    attempts++;
                    setTimeout(check, interval);
                } else {
                    console.error("Canvas not initialized after waiting.");
                }
            };
            check();
        };

        waitForCanvas(() => {
            
            if (!Canvas) return;
            const areasSelected = selection.areas || [];
            const flowsSelected = selection.flows || [];
            const processIdsSet = new Set(flowsSelected.processIds);
            const processTypesSet = new Set((selection.processTypeCodes || []).map(Number));
            IntRouteActive = false;
            OutRouteActive = false;

            window.canvasSelectionFilter = selection;
            Canvas.getObjects().forEach(obj => {
                obj.visible = false;
                obj.opacity = 1;
            });

            // 1. Siempre visibles estos tipos
            const alwaysVisibleTypes = ['groupStation', 'groupShelf', 'goupAutomatic', 'groupCaotic', 'groupRack', 'groupDriveIn', 'Square', 'Text', 'LineStraight'];
            Canvas.getObjects().forEach(obj => {
                if (alwaysVisibleTypes.includes(obj.elementType)) {
                    obj.visible = true;
                    obj.opacity = opacityActive;
                }
            });

            // 2. Mostrar solo las áreas seleccionadas
            if ((areasSelected && areasSelected.length > 0) || (flowsSelected.areaIds && flowsSelected.areaIds.length > 0)) {
                Canvas.getObjects().forEach(obj => {
                    if (obj.elementType == 'groupArea') {
                        obj.visible = areasSelected.includes(obj.id.replace('groupArea', '')) || flowsSelected.areaIds.includes(obj.id.replace('groupArea', ''));
                    }
                    if (obj.elementType == "groupEquipment") {
                        obj.visible = areasSelected.includes(obj.groupId.replace('groupEquipment', '')) || flowsSelected.areaIds.includes(obj.groupId.replace('groupEquipment', ''));
                    }
                    if (obj.elementType == "groupStation" || obj.elementType == "groupCaotic" || obj.elementType == "groupRack" || obj.elementType == "goupAutomatic" || obj.elementType == "groupShelf") {

                        let isVisible = areasSelected.includes(obj.groupId.replace('groupStation', '')) || areasSelected.includes(obj.id.replace('groupStation', '')) || areasSelected.includes(obj.groupId.replace('groupCaotic', ''))
                            || areasSelected.includes(obj.groupId.replace('groupRack', '')) || areasSelected.includes(obj.groupId.replace('goupAutomatic', ''))
                            || areasSelected.includes(obj.groupId.replace('groupShelf', ''))

                            || flowsSelected.areaIds.includes(obj.groupId.replace('groupStation', '')) || flowsSelected.areaIds.includes(obj.id.replace('groupStation', '')) || flowsSelected.areaIds.includes(obj.groupId.replace('groupCaotic', ''))
                            || flowsSelected.areaIds.includes(obj.groupId.replace('groupRack', '')) || flowsSelected.areaIds.includes(obj.groupId.replace('goupAutomatic', ''))
                            || flowsSelected.areaIds.includes(obj.groupId.replace('groupShelf', ''));

                        obj.opacity = isVisible ? opacityActive : opacityInactive;
                    }

                    if (obj.elementType === 'groupProcess') {
                        const idByObj = obj.id?.replace('groupProcess', '');
                        const areaByObj = obj.groupId?.replace('groupProcess', '');
                        const objType = Number(obj.Type ?? obj.type ?? obj.processType);

                        const hasTypeFilter = processTypesSet.size > 0;
                        const hasIdFilter = processIdsSet.size > 0;
                        const hasAreaFilter = (areasSelected?.length ?? 0) > 0;

                        const matchesType = !hasTypeFilter || processTypesSet.has(objType);
                        const matchesIdOrArea =
                            (idByObj && processIdsSet.has(idByObj)) ||
                            (areaByObj && areasSelected.includes(areaByObj));

                        const isActive =
                            matchesType && (
                                (!hasIdFilter && !hasAreaFilter) ||
                                matchesIdOrArea
                            );
                        obj.visible = true;
                        obj.opacity = isActive ? opacityActive : opacityInactive;
                    }

                    if (['Square', 'Text', 'LineStraight'].includes(obj.elementType)) {

                        let isLinkedToVisibleArea = false;

                        if (obj.groupId) {
                            const areaId = obj.groupId.replace('groupStation', '')
                                .replace('groupShelf', '')
                                .replace('groupEquipment', '')
                                .replace('groupProcess', '');

                            isLinkedToVisibleArea = areasSelected.includes(areaId) || flowsSelected.areaIds.includes(areaId);
                        }

                        obj.opacity = isLinkedToVisibleArea ? opacityActive : opacityInactive;
                    }
                });
            }

            const processDirectionIdsSet = new Set(flowsSelected.processDirectionIds || []);
            const routeIdsSet = new Set(flowsSelected.routeIds || []); 
            const areaIdsSet = new Set(areasSelected || []);       

            IntRouteActive = flowsSelected.inP;
            OutRouteActive = flowsSelected.outP;
            CustomRouteActive = (!flowsSelected.inP && flowsSelected.outP);
            Canvas.getObjects().forEach(obj => {
                if (processDirectionIdsSet.has(obj.linkedEntityId)) {
                    tabSlected = 'processDirection';
                    obj.visible = true;
                }
                if (routeIdsSet.size > 0 && routeIdsSet.has(obj.linkedEntityId)) {
                    tabSlected = 'routes';
                    obj.visible = true;
                }
                if (routeIdsSet.size === 0 && areaIdsSet.size > 0) {
                    if (areaIdsSet.has(obj.linkedEntityId)) {
                        tabSlected = 'routes';
                        obj.visible = true;
                    }
                }
            });

            window.forceProcessDirectionsBlack = isRelationsModeOn() && isProcessRelationsSelection(selection);
            ChangeProcessColorsByVisiblePolyline(selection);
            applyProcessOpacityFromFilters();
            //  Bloquear acciones en áreas relacionadas
            const rs = window.canvasRelationsState || { isOn: false, primaryAreaId: null };
            const relationsOn = !!rs.isOn;
            const primaryAreaId = rs.primaryAreaId;

            Canvas.getObjects().forEach(obj => {
                backupInteractivity(obj);

                // Restablecer acciones
                if (!relationsOn || !primaryAreaId) {
                    restoreInteractivity(obj);
                    return;
                }

                const objAreaId = getAreaIdFromObj(obj);

                // Área principal y sus elementos: interactivos
                const isPrimary =
                    (obj.elementType === 'groupArea' && objAreaId === primaryAreaId) ||
                    (objAreaId === primaryAreaId);

                if (isPrimary) {
                    obj.selectable = true;
                    obj.evented = true;
                    return;
                }

                // Permitir rutas y itinerarios
                if (isRouteObj(obj)) {
                    obj.selectable = true;
                    obj.evented = true;
                    obj.hasControls = false; 
                    obj.hasBorders = false;   
                    return;
                }
              
                obj.selectable = false;
                obj.evented = false;
                obj.hasControls = false;
                obj.hasBorders = false;
            });

            Canvas.requestRenderAll();
        });
     
    } catch (e) {
        console.error("Error applying canvas selection", e);
    }
};

function toggleAllAccordions() {
    const accordions = document.querySelectorAll('.action-event-open-all-checking');
    const anyOpen = Array.from(accordions).some(acc => acc.classList.contains('show'));

    accordions.forEach(acc => {
        const collapse = new bootstrap.Collapse(acc, {
            toggle: false
        });

        if (anyOpen) {
            collapse.hide(); // Hide all if any are open
        } else {
            collapse.show(); // Show all if closed
        }
    });
}

window.discardCanvasSelection = () => {
    if (Canvas) {
        Canvas.discardActiveObject();  
        Canvas.requestRenderAll();     
    }
};

function resizeGroupAreaFromStations(areaId) {
    const groupArea = Canvas.getObjects().find(obj => obj.id === 'groupArea' + areaId);
    if (!groupArea) return;

    const stations = Canvas.getObjects().filter(obj =>
        obj.groupId === 'groupStation' + areaId ||
        obj.groupId === 'groupShelf' + areaId
    );

    if (stations.length === 0) return;

    let minX = Infinity, minY = Infinity;
    stations.forEach(obj => {
        if (obj.left < minX) minX = obj.left;
        if (obj.top < minY) minY = obj.top;
    });

    let maxX = -Infinity, maxY = -Infinity;
    stations.forEach(obj => {
        const right = obj.left + obj.width * (obj.scaleX || 1);
        const bottom = obj.top + obj.height * (obj.scaleY || 1);
        if (right > maxX) maxX = right;
        if (bottom > maxY) maxY = bottom;
    });

    //Márgen
    const marginTop = 45;   
    const marginRight = 8;
    const marginLeft = 8;
    const marginBottomBase = 8;


    const hasProcesses = Canvas.getObjects().some(obj =>
        obj.elementType === 'groupProcess' && obj.groupId === 'groupProcess' + areaId
    );
    const processHeight = 5;  
    const processGap = 1;   
    const marginBottomFinal = hasProcesses
        ? marginBottomBase + processHeight + processGap
        : marginBottomBase;

    const element = {
        Id: areaId,
        X: minX - marginLeft,
        Y: minY - marginTop,
        Width: (maxX - minX) + marginLeft + marginRight,
        Height: (maxY - minY) + marginTop + marginBottomFinal
    };
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
        const originalFontSize = 10; // Tamaño fijo original
        const maxTextWidth = element.Width - 20; // Espacio máximo permitido
        const { displayedText, fullText } = getTruncatedText(text.fullText || text.text, maxTextWidth, originalFontSize);

        text.fullText = fullText;
        text.set({
            text: displayedText,
            fontSize: originalFontSize,
            originX: 'center',
            originY: 'top',
            selectable: false,
            evented: false,
            left: element.Width / 2,
            top: 20
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

    groupArea.setCoords();
    groupArea.prevLeft = groupArea.left;
    groupArea.prevTop = groupArea.top;
    if (groupArea.statusObject !== 'Add') groupArea.statusObject = 'Update';

    // Reacomodar procesos, equipos y rutas si los hay
    const processes = Canvas.getObjects().filter(obj =>
        obj.elementType === 'groupProcess' && obj.groupId === 'groupProcess' + areaId
    );

    const spacing = 4;
    if (processes.length > 0) {
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
        obj.elementType === 'groupEquipment' && obj.groupId === 'groupEquipment' + areaId
    );

    if (equipments.length > 0) {
        const EQUIP_W = 40;     
        const GAP_X = 8;      
        const rightEdgeLeft = element.X + element.Width - EQUIP_W;
        const equipTop = element.Y - EQUIP_W * 0.8;

      
        const orderKey = 'groupEquipment' + areaId;
        const orderList = (window.__equipOrderByArea && window.__equipOrderByArea[orderKey]) || [];

        
        const big = 1e9;
        const sorted = equipments.slice().sort((a, b) => {
            const ia = orderList.indexOf(a.id); const ib = orderList.indexOf(b.id);
            return (ia === -1 ? big : ia) - (ib === -1 ? big : ib);
        });

     
        sorted.forEach((g, i) => {
            const newLeft = Math.round(rightEdgeLeft - i * (EQUIP_W + GAP_X));
            g.set({ left: newLeft, top: equipTop });
            g.setCoords();
        });
    }

    updateAllPosicionRoute();

    Canvas.getObjects().forEach(obj => {
        if (obj.elementType === 'processDirection' || obj.type === 'circle') obj.setCoords();
    });

    Canvas.calcOffset();
    Canvas.requestRenderAll();
}

function resizeStationGroup(group, stationElement, areaElement) {

    const wasScaled = group.scaleX !== 1 || group.scaleY !== 1;
    if (!wasScaled) return;

    const scaleX = group.scaleX;
    const scaleY = group.scaleY;

    const MIN_WIDTH = 50;
    const MIN_HEIGHT = 20;

    const newWidth = group.width * scaleX;
    const newHeight = group.height * scaleY;


    const text = group._objects?.[1];
    let minTextWidth = MIN_WIDTH;

    if (text) {

        minTextWidth = text.width * (text.scaleX || 1) + 20; 
    }

    // Verificamos si el nuevo tamaño es válido
    if (newWidth < minTextWidth || newHeight < MIN_HEIGHT || scaleX < 0 || scaleY < 0) {

        group.set({
            scaleX: 1,
            scaleY: 1,
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


    const rect = group._objects?.[0];
    if (rect) {
        rect.set({
            width: newWidth,
            height: newHeight,
            left: 0,
            top: 0
        });
        rect.setCoords();
    }


    if (text) {
        text.set({
            left: newWidth / 2,
            top: newHeight / 2,
            originX: 'center',
            originY: 'center'
        });
        text.setCoords();
    }

    const prevLeft = group.left;
    const prevTop = group.top;

    group._calcBounds();
    group._updateObjectsCoords();
    group.set({
        left: prevLeft,
        top: prevTop
    });
    group.setCoords();


    stationElement.Width = newWidth;
    stationElement.Height = newHeight;


    if (areaElement.StatusObject !== 'Add') {
        areaElement.StatusObject = 'Update';
    }
}

let verticalGuide = null;
let verticalGuideLeft = null;
let verticalGuideRight = null;
let horizontalGuide = null;
let horizontalGuideTop = null;
let horizontalGuideBottom = null;

function showVerticalGuide(x, minX, maxX) {
    const vb = getVisibleBounds();



    // Línea izquierda
    if (!verticalGuideLeft) {
        verticalGuideLeft = new fabric.Line([minX, vb.minY, minX, vb.maxY], {
            stroke: '#247ECC',
            strokeDashArray: [4, 4],
            selectable: false,
            evented: false,
            excludeFromExport: true,
            name: 'alignmentGuide'
        });
        Canvas.add(verticalGuideLeft);
    } else {
        verticalGuideLeft.set({ x1: minX, x2: minX, y1: vb.minY, y2: vb.maxY }).setCoords();
    }

    // Línea derecha
    if (!verticalGuideRight) {
        verticalGuideRight = new fabric.Line([maxX, vb.minY, maxX, vb.maxY], {
            stroke: '#247ECC',
            strokeDashArray: [4, 4],
            selectable: false,
            evented: false,
            excludeFromExport: true,
            name: 'alignmentGuide'
        });
        Canvas.add(verticalGuideRight);
    } else {
        verticalGuideRight.set({ x1: maxX, x2: maxX, y1: vb.minY, y2: vb.maxY }).setCoords();
    }
}


function showHorizontalGuide(y, minY, maxY) {
    const vb = getVisibleBounds();


    // Línea superior
    if (!horizontalGuideTop) {
        horizontalGuideTop = new fabric.Line([vb.minX, minY, vb.maxX, minY], {
            stroke: '#247ECC',
            strokeDashArray: [4, 4],
            selectable: false,
            evented: false,
            excludeFromExport: true,
            name: 'alignmentGuide'
        });
        Canvas.add(horizontalGuideTop);
    } else {
        horizontalGuideTop.set({ x1: vb.minX, x2: vb.maxX, y1: minY, y2: minY }).setCoords();
    }

    // Línea inferior
    if (!horizontalGuideBottom) {
        horizontalGuideBottom = new fabric.Line([vb.minX, maxY, vb.maxX, maxY], {
            stroke: '#247ECC',
            strokeDashArray: [4, 4],
            selectable: false,
            evented: false,
            excludeFromExport: true,
            name: 'alignmentGuide'
        });
        Canvas.add(horizontalGuideBottom);
    } else {
        horizontalGuideBottom.set({ x1: vb.minX, x2: vb.maxX, y1: maxY, y2: maxY }).setCoords();
    }
}

function hideVerticalGuide() {
    [verticalGuide, verticalGuideLeft, verticalGuideRight].forEach(g => {
        if (g) Canvas.remove(g);
    });
    verticalGuide = verticalGuideLeft = verticalGuideRight = null;
    Canvas.requestRenderAll();
}

function hideHorizontalGuide() {
    [horizontalGuide, horizontalGuideTop, horizontalGuideBottom].forEach(g => {
        if (g) Canvas.remove(g);
    });
    horizontalGuide = horizontalGuideTop = horizontalGuideBottom = null;
    Canvas.requestRenderAll();
}

function getVisibleBounds() {
    const vpt = Canvas.viewportTransform || fabric.iMatrix.concat();
    const inv = fabric.util.invertTransform(vpt);


    const tl = fabric.util.transformPoint(new fabric.Point(0, 0), inv);
    const br = fabric.util.transformPoint(
        new fabric.Point(Canvas.getWidth(), Canvas.getHeight()), inv
    );

    const overscan = 1000; 
    return {
        minX: Math.min(tl.x, br.x) - overscan,
        maxX: Math.max(tl.x, br.x) + overscan,
        minY: Math.min(tl.y, br.y) - overscan,
        maxY: Math.max(tl.y, br.y) + overscan
    };
}

function refreshGuides() {
    const vb = getVisibleBounds();
    if (verticalGuide) verticalGuide.set({ y1: vb.minY, y2: vb.maxY }).setCoords();
    if (horizontalGuide) horizontalGuide.set({ x1: vb.minX, x2: vb.maxX }).setCoords();

    if (verticalGuide) verticalGuide.bringToFront();
    if (horizontalGuide) horizontalGuide.bringToFront();
    Canvas.requestRenderAll();
}

function isEditableElement(el) {
    if (!el) return false;
    const tag = el.tagName;
    if ((tag === 'INPUT' || tag === 'TEXTAREA') && !el.readOnly && !el.disabled) return true;
    if (el.isContentEditable) return true;
    return false;
}

function isInModal(el) {
    if (!el || !el.closest) return false;
    return !!el.closest('[role="dialog"], .modal, .drawer, .dx-overlay-wrapper, .dx-popup-wrapper, .ant-modal, .MuiDialog-root, .swal2-container');
}

function alignObjects(direction) {
    const selected = Canvas.getActiveObjects();
    if (!selected || selected.length === 0) return;

    const originalSelection = [...selected];

    const grouped = {};
    selected.forEach(obj => {
        if (obj.elementType === "groupStation" || obj.elementType === "groupShelf") {
            if (!grouped[obj.groupId]) grouped[obj.groupId] = [];
            grouped[obj.groupId].push(obj);
        }
    });

    const marginTop = 45;
    const marginRight = 8;
    const marginLeft = 8;
    const marginBottomBase = 8;

    const stationSpacingX = 20;
    const stationSpacingY = 20;

    Canvas.discardActiveObject();

    Object.keys(grouped).forEach(groupId => {
        const area = Canvas.getObjects().find(
            o => cleanGroupIdValue(o.id) === cleanGroupIdValue(groupId) &&
                o.elementType === 'groupArea'
        );
        if (!area) return;

        grouped[groupId].forEach(obj => {
            const abs = obj.getBoundingRect(true);

            switch (direction) {
                case "left":
                    obj.set({
                        left: area.left + marginLeft,
                        statusObject: 'Update'
                    });
                    break;
                case "right":
                    obj.set({
                        left: area.left + area.width - abs.width - marginRight,
                        statusObject: 'Update'
                    });
                    break;
                case "top":
                    obj.set({
                        top: area.top + marginTop,
                        statusObject: 'Update'
                    });
                    break;
                case "bottom":
                    obj.set({
                        top: area.top + area.height - abs.height - marginBottomBase,
                        statusObject: 'Update'
                    });
                    break;
                case "horizontalCenter":
                    obj.set({
                        left: area.left + (area.width - abs.width) / 2,
                        statusObject: 'Update'
                    });
                    break;
                case "verticalCenter":
                    obj.set({
                        top: area.top + (area.height - abs.height) / 2,
                        statusObject: 'Update'
                    });
                    break;
                case "distributeHorizontal":
                    const sorted = [...grouped[groupId]].sort((a, b) => a.left - b.left);
                    let currentX = area.left + marginLeft;
                    const startY = area.top + marginTop;

                    sorted.forEach(obj => {
                        const abs = obj.getBoundingRect(true);
                        obj.set({
                            left: currentX,
                            top: startY,
                            statusObject: 'Update'
                        });
                        obj.setCoords();
                        currentX += abs.width + stationSpacingX;
                    });
                    break;
                case "distributeVertical":
                    const sortedV = [...grouped[groupId]].sort((a, b) => a.top - b.top);
                    let currentY = area.top + marginTop;
                    const startX = area.left + marginLeft;

                    sortedV.forEach(obj => {
                        const abs = obj.getBoundingRect(true);
                        obj.set({
                            top: currentY,
                            left: startX,
                            statusObject: 'Update'
                        });
                        obj.setCoords();
                        currentY += abs.height + stationSpacingY;
                    });
                    break;
            }
            obj.setCoords();
        });
        resizeGroupAreaFromStations(cleanGroupIdValue(area.id));
    });

    Canvas.requestRenderAll();

    if (originalSelection.length === 1) {
        Canvas.setActiveObject(originalSelection[0]);
    } else if (originalSelection.length > 1) {
        const sel = new fabric.ActiveSelection(originalSelection, { canvas: Canvas });
        Canvas.setActiveObject(sel);
    }
    DisabledButtonSave(false);
    Canvas.requestRenderAll();
}

function showEvenSpacingGuides(movingObject, allObjects, tolerance = 5) {
    clearSpacingGuides();

    const SCALE = (obj) => ({
        left: obj.left,
        right: obj.left + obj.width * obj.scaleX,
        top: obj.top,
        bottom: obj.top + obj.height * obj.scaleY
    });

    const spacingThreshold = 300;

    {
        const rowAligned = allObjects.filter(obj => {
            const { top, bottom } = SCALE(obj);
            const { top: mTop, bottom: mBottom } = SCALE(movingObject);
            return Math.abs(top - mTop) <= tolerance || Math.abs(bottom - mBottom) <= tolerance;
        });

        const candidates = [...rowAligned, movingObject];
        candidates.sort((a, b) => a.left - b.left);

        const spacings = [];
        for (let i = 1; i < candidates.length; i++) {
            const prev = SCALE(candidates[i - 1]);
            const curr = SCALE(candidates[i]);
            const spacing = curr.left - prev.right;
            spacings.push({ spacing, from: candidates[i - 1], to: candidates[i] });
        }

        const commonSpacing = getMostCommonSpacing(spacings, tolerance);
        spacings.forEach(({ spacing, from, to }) => {
            if (Math.abs(spacing - commonSpacing) <= tolerance && spacing < spacingThreshold) {
                const y = Math.max(SCALE(from).bottom, SCALE(to).bottom) + 10;
                const x1 = SCALE(from).right;
                const x2 = SCALE(to).left;
                const centerX = x1 + (x2 - x1) / 2;

                Canvas.add(new fabric.Line(
                    [x1, y, x2, y],
                    {
                        stroke: '#F28888',
                        top: y + 5.5,
                        strokeWidth: 1,
                        strokeDashArray: [4, 4],
                        selectable: false,
                        evented: false,
                        excludeFromExport: true,
                        name: 'spacingGuide'
                    }
                ));

                const verticalHeight = 6; 
                const verticalStyle = {
                    stroke: 'gray',
                    strokeWidth: 1,
                    strokeDashArray: [4, 4], 
                    selectable: false,
                    evented: false,
                    excludeFromExport: true,
                    name: 'spacingGuide'
                };

                Canvas.add(new fabric.Line(
                    [x1, y - verticalHeight, x1, y + verticalHeight],
                    verticalStyle
                ));

                Canvas.add(new fabric.Line(
                    [x2, y - verticalHeight, x2, y + verticalHeight],
                    verticalStyle
                ));

                const arrowLeft = new fabric.Triangle({
                    left: x1 + 3,
                    top: y + 6,
                    originX: 'center',
                    originY: 'center',
                    angle: -90,
                    width: 6,
                    height: 5,
                    fill: '#F26363',
                    selectable: false,
                    evented: false,
                    excludeFromExport: true,
                    name: 'spacingGuide'
                });

                const arrowRight = new fabric.Triangle({
                    left: x2 - 3,
                    top: y + 6,
                    originX: 'center',
                    originY: 'center',
                    angle: 90,
                    width: 6,
                    height: 5,
                    fill: '#F26363',
                    selectable: false,
                    evented: false,
                    excludeFromExport: true,
                    name: 'spacingGuide'
                });

                Canvas.add(arrowLeft);
                Canvas.add(arrowRight);

                if (to === movingObject) {
                    movingObject.left = SCALE(from).right + commonSpacing;
                    movingObject.setCoords();
                } else if (from === movingObject) {
                    movingObject.left = SCALE(to).left - movingObject.width * movingObject.scaleX - commonSpacing;
                    movingObject.setCoords();
                }
            }
        });

        const idx = candidates.indexOf(movingObject);
        if (idx === 0 && candidates.length > 1) {
            const to = candidates[1];
            const toLeft = to.left;
            const expectedLeft = toLeft - movingObject.width * movingObject.scaleX - commonSpacing;
            const actualSpacing = toLeft - (movingObject.left + movingObject.width * movingObject.scaleX);
            if (Math.abs(actualSpacing - commonSpacing) <= tolerance) {
                movingObject.left = expectedLeft;
                movingObject.setCoords();
            }
        }
    }

    {
        const colAligned = allObjects.filter(obj => {
            const { left, right } = SCALE(obj);
            const { left: mLeft, right: mRight } = SCALE(movingObject);
            return Math.abs(left - mLeft) <= tolerance || Math.abs(right - mRight) <= tolerance;
        });

        const candidates = [...colAligned, movingObject];
        candidates.sort((a, b) => a.top - b.top);

        const spacings = [];
        for (let i = 1; i < candidates.length; i++) {
            const prev = SCALE(candidates[i - 1]);
            const curr = SCALE(candidates[i]);
            const spacing = curr.top - prev.bottom;
            spacings.push({ spacing, from: candidates[i - 1], to: candidates[i] });
        }

        const commonSpacing = getMostCommonSpacing(spacings, tolerance);
        spacings.forEach(({ spacing, from, to }) => {
            if (Math.abs(spacing - commonSpacing) <= tolerance && spacing < spacingThreshold) {
                const x = Math.max(SCALE(from).right, SCALE(to).right) + 10;
                const y1 = SCALE(from).bottom;
                const y2 = SCALE(to).top;
                const centerY = y1 + (y2 - y1) / 2;

                Canvas.add(new fabric.Line(
                    [x, y1, x, y2],
                    {
                        stroke: '#F28888',
                        left: x + 6.5,
                        strokeWidth: 1,
                        strokeDashArray: [4, 4], 
                        selectable: false,
                        evented: false,
                        excludeFromExport: true,
                        name: 'spacingGuide'
                    }
                ));

                const horizontalWidth = 6; 
                const horizontalStyle = {
                    stroke: 'grey',
                    strokeWidth: 1,
                    strokeDashArray: [4, 4], 
                    selectable: false,
                    evented: false,
                    excludeFromExport: true,
                    name: 'spacingGuide'
                };

                Canvas.add(new fabric.Line(
                    [x - horizontalWidth, y1, x + horizontalWidth, y1],
                    horizontalStyle
                ));

                Canvas.add(new fabric.Line(
                    [x - horizontalWidth, y2, x + horizontalWidth, y2],
                    horizontalStyle
                ));

                const arrowTop = new fabric.Triangle({
                    left: x + 7,
                    top: y1 + 2,
                    originX: 'center',
                    originY: 'center',
                    angle: 0,                   
                    width: 6,
                    height: 5,
                    fill: '#F26363',
                    selectable: false,
                    evented: false,
                    excludeFromExport: true,
                    name: 'spacingGuide'
                });

                const arrowBottom = new fabric.Triangle({
                    left: x + 7,
                    top: y2 - 2,
                    originX: 'center',
                    originY: 'center',
                    angle: 180,                  
                    width: 6,
                    height: 5,
                    fill: '#F26363',
                    selectable: false,
                    evented: false,
                    excludeFromExport: true,
                    name: 'spacingGuide'
                });

                Canvas.add(arrowTop);
                Canvas.add(arrowBottom);

                if (to === movingObject) {
                    movingObject.top = SCALE(from).bottom + commonSpacing;
                    movingObject.setCoords();
                } else if (from === movingObject) {
                    movingObject.top = SCALE(to).top - movingObject.height * movingObject.scaleY - commonSpacing;
                    movingObject.setCoords();
                }
            }
        });
    }
}

function getMostCommonSpacing(spacings, tolerance) {
    for (let i = 0; i < spacings.length; i++) {
        const base = spacings[i].spacing;
        const count = spacings.filter(s => Math.abs(s.spacing - base) <= tolerance).length;
        if (count >= 2) return base;
    }
    return null;
}

function clearSpacingGuides() {
    const guides = Canvas.getObjects().filter(obj => obj.name === 'spacingGuide');
    guides.forEach(guide => Canvas.remove(guide));
}

function getBoundsFromAlignedObjects(objects, axis, value, tolerance = 5) {
    const filtered = objects.filter(obj => {
        const centerX = obj.left + obj.width / 2 * obj.scaleX;
        const centerY = obj.top + obj.height / 2 * obj.scaleY;
        return axis === 'x'
            ? Math.abs(centerX - value) <= tolerance
            : Math.abs(centerY - value) <= tolerance;
    });

    const minX = Math.min(...filtered.map(o => o.left));
    const maxX = Math.max(...filtered.map(o => o.left + o.width * o.scaleX));
    const minY = Math.min(...filtered.map(o => o.top));
    const maxY = Math.max(...filtered.map(o => o.top + o.height * o.scaleY));

    return { minX, maxX, minY, maxY };
}

/**
 * Retorna el texto a mostrar en el canvas, truncado visualmente si es necesario.
 * Mantiene el texto original en la propiedad `fullText` para no perder datos.
 *
 * @param {string} fullText 
 * @param {number} maxWidth 
 * @param {number} fontSize 
 * @returns {{ displayedText: string, fullText: string }}
 */
function getTruncatedText(fullText, maxWidth, fontSize = 10) {
    const ctx = document.createElement("canvas").getContext("2d");
    ctx.font = `${fontSize}px Segoe UI`;

    const truncate = (txt, maxWidth) => {
        let truncated = txt;
        while (truncated.length > 0 && ctx.measureText(truncated + "...").width > maxWidth) {
            truncated = truncated.slice(0, -1);
        }
        return truncated + (truncated.length < txt.length ? "..." : "");
    };


    if (ctx.measureText(fullText).width <= maxWidth) {
        return { displayedText: fullText, fullText };
    }

    return { displayedText: truncate(fullText, maxWidth), fullText };
}


function getAreaMetrics(area) {
    return {
        X: area.X ?? area.left ?? 0,
        Y: area.Y ?? area.top ?? 0,
        Width: area.Width ?? area.width ?? 0,
        Height: area.Height ?? area.height ?? 0,
        Key: ((area.Id ?? area.id ?? '') + '').replace('groupArea', '')
    };
}

function relayoutAreaEquipments(area) {
    const EQUIP_W = 40;  
    const GAP_X = 8;    

    const X = area.X ?? area.left ?? 0;
    const W = area.Width ?? area.width ?? 0;
    const Key = ((area.Id ?? area.id ?? '') + '').replace('groupArea', '');
    const groupId = 'groupEquipment' + Key;

    const equipments = Canvas.getObjects().filter(o =>
        o.elementType === 'groupEquipment' && o.groupId === groupId
    );
    if (!equipments.length) return;
    const byType = {};
    for (const g of equipments) {
        const t = ((g.typeName || g.type || '') + '').toLowerCase();
        if (!t) continue;
        const l = g.left ?? 0;
        if (!byType[t]) byType[t] = { maxLeft: l, items: [] };
        byType[t].maxLeft = Math.max(byType[t].maxLeft, l); 
        byType[t].items.push(g);
    }
    const typeOrder = Object.keys(byType).sort((a, b) => byType[b].maxLeft - byType[a].maxLeft);
    const rightEdgeLeft = X + W - EQUIP_W;
    for (const g of equipments) {
        const t = ((g.typeName || g.type || '') + '').toLowerCase();
        const col = Math.max(0, typeOrder.indexOf(t));
        const newLeft = Math.round(rightEdgeLeft - col * (EQUIP_W + GAP_X));
        g.set({ left: newLeft });
        g.setCoords();
    }
    Canvas.requestRenderAll();
}

function normalizeHandState(reason = '') {
    if (currentMode !== modes?.zoomToSelection) currentMode = '';

    isPanning = false;
    isPanModeEnabled = false;

    Canvas.selection = true;
    Canvas.defaultCursor = 'grab';
    Canvas.hoverCursor = 'grab';
    if (Canvas?.upperCanvasEl) Canvas.upperCanvasEl.style.cursor = 'grab';

    // Asegura que el fondo conserve su id y reciba eventos
    const bg = Canvas.getObjects().find(o => o?.id === 'background');
    if (bg) {
        bg.id = 'background';
        bg.elementType = bg.elementType || 'background';
        bg.selectable = false;
        bg.evented = true;
    }

    Canvas.requestRenderAll();
}

window.wf = window.wf || {};

// Calcula Width/Height desde componente
window.wf.calcShelfWHOnSave = function (dto, constants) {
    const BAR_THICKNESS = constants?.BAR_THICKNESS ?? 20;
    const BAR_SPACING = constants?.BAR_SPACING ?? 5;

    const numBars = Math.max(1, dto.numShelves ?? 1);

    const isVertical = (typeof dto.isVertical === "boolean")
        ? dto.isVertical
        : (dto.Orientation === 2);

    // bases: detectar fallback (0 -> 60) 
    const wRaw = Number(dto.width) || 0;
    const hRaw = Number(dto.height) || 0;
    const usedFallbackDim = (wRaw <= 0) || (hRaw <= 0);

    const baseW = (wRaw > 0 ? wRaw : 60);
    const baseH = (hRaw > 0 ? hRaw : 60);

    // posición: detectar fallback (0/undefined) 
    const leftFromDto = (dto.left ?? dto.x);
    const topFromDto = (dto.top ?? dto.y);
    const leftRaw = Number(leftFromDto ?? 0);
    const topRaw = Number(topFromDto ?? 0);

    // Si no venían en dto (undefined) o vienen ambos en 0, tratamos como “sin pos confiable”
    const usedFallbackPos = (leftFromDto === undefined && topFromDto === undefined) ||
        (leftRaw === 0 && topRaw === 0);

    const oldLeft = leftRaw;
    const oldTop = topRaw;
    const oldBottom = oldTop + baseH;

    // Diferenciamos coordenadas verticas o horizontales
    const expectedVerticalW = numBars * BAR_THICKNESS + (numBars + 1) * BAR_SPACING; // V: ancho crece
    const expectedHorizontalH = numBars * BAR_THICKNESS + (numBars + 1) * BAR_SPACING; // H: alto crece
    const EPS = BAR_THICKNESS + BAR_SPACING;

    const coordsLookVertical = !usedFallbackDim && Math.abs(baseW - expectedVerticalW) <= EPS;
    const coordsLookHorizontal = !usedFallbackDim && Math.abs(baseH - expectedHorizontalH) <= EPS;


    let contW, contH;

    if (isVertical) {
        if (coordsLookHorizontal) {
            // H -> V: acostar
            contW = baseH;
            contH = baseW;
        } else {
            // Vertical NORMAL 
            const barH = Math.max(BAR_THICKNESS, baseH - 2 * BAR_SPACING);
            contW = expectedVerticalW;
            contH = barH + 2 * BAR_SPACING;
        }
    } else {
        if (coordsLookVertical) {
            // V -> H: parar
            contW = baseH;
            contH = baseW;
        } else {
            // Horizontal NORMAL 
            const barW = Math.max(BAR_THICKNESS, baseW - 2 * BAR_SPACING);
            contW = barW + 2 * BAR_SPACING;
            contH = expectedHorizontalH;
        }
    }

    // Decidimos si vienen coordenadas en 0 ne nacimiento
    const skipPivot = usedFallbackPos || usedFallbackDim;

    const newLeft = oldLeft;
    const newTop = skipPivot ? oldTop : (oldBottom - contH); 

    const numCrossAisles = (numBars <= 1) ? 0 : (numBars - 1);

    return {
        width: Math.max(1, Math.round(contW)),
        height: Math.max(1, Math.round(contH)),
        left: Math.round(newLeft),
        top: Math.round(newTop),
        numShelves: numBars,
        numCrossAisles
    };
};
// === Endpoints en espacio de CANVAS (sin zoom/pan) ===
function getLineEndpointsCanvas(line) {
    const M = line.calcTransformMatrix();
    const p1 = fabric.util.transformPoint(new fabric.Point(line.x1, line.y1), M);
    const p2 = fabric.util.transformPoint(new fabric.Point(line.x2, line.y2), M);
    return { p1, p2 };
}
// Bake manteniendo posición y origin center
function bakeLineScaleKeepPosition(line) {
    const { p1, p2 } = getLineEndpointsCanvas(line);

    const cx = (p1.x + p2.x) / 2;
    const cy = (p1.y + p2.y) / 2;

    const nx1 = p1.x - cx, ny1 = p1.y - cy;
    const nx2 = p2.x - cx, ny2 = p2.y - cy;

    requestAnimationFrame(() => {
        line.set({
            originX: 'center',
            originY: 'center',
            x1: nx1, y1: ny1,
            x2: nx2, y2: ny2,
            scaleX: 1, scaleY: 1,
            angle:0         
        });
        line.setPositionByOrigin(new fabric.Point(cx, cy), 'center', 'center');

        line.setCoords();
        line.canvas?.requestRenderAll();
    });
}

function attachLineTransformHandlers(lineObj) {

    lineObj.on('moving', function () {
        DisabledButtonSave(false);
        if (this._angleBadge?.visible) {
            positionAngleBadge(this, this._angleBadge);
        }
        this.canvas?.requestRenderAll();
    });

    lineObj.on('scaling', function () {
        if (this._angleBadge?.visible) {
            positionAngleBadge(this, this._angleBadge);
        }
        this.canvas?.requestRenderAll();
    });

    lineObj.on('rotating', function () {
        const badge = ensureAngleBadge(this);
        const deg = getVisualAngleDegrees(this);
        const near = nearZeroOrNinety(deg);

        if (near !== null) {
            badge.set({ text: `${near}°`, visible: true });
            positionAngleBadge(this, badge);
        } else {
            badge.visible = false;
        }
        this.canvas?.requestRenderAll();
    });

    lineObj.on('modified', function (e) {
        const obj = this;

        const sx = (obj.scaleX ?? 1);
        const sy = (obj.scaleY ?? 1);
        const scaled =
            (e && e.transform && e.transform.action === 'scale') ||
            sx !== 1 || sy !== 1;

        if (scaled) {
            bakeLineScaleKeepPosition(obj);
        }

        // Actualizar valores lógicos para guardar
        const { p1, p2 } = getLineEndpointsCanvas(obj); 
        const minX = Math.min(p1.x, p2.x), minY = Math.min(p1.y, p2.y);
        const maxX = Math.max(p1.x, p2.x), maxY = Math.max(p1.y, p2.y);

        obj.X  = p1.x; obj.Y  = p1.y;
        obj.X2 = p2.x; obj.Y2 = p2.y;
        obj.Left   = minX;
        obj.Top    = minY;
        obj.Width  = maxX - minX;
        obj.Height = maxY - minY;
        obj.Angle  = obj.angle || 0;

        if (obj.statusObject !== 'Add') obj.statusObject = 'Update';
        DisabledButtonSave(false);

        if (obj._angleBadge) obj._angleBadge.visible = false;
        obj.setCoords();
        obj.canvas?.requestRenderAll();
    });
}

// Normalizar coordenadas al crear linea nueva
function normalizeNewLineUsingCurrentCenter(line) {
    if (line._normalizedOnce) return; 
    const cx = line.left;   
    const cy = line.top;

    line.set({
        originX: 'center',
        originY: 'center',
        x1: line.x1 - cx,
        y1: line.y1 - cy,
        x2: line.x2 - cx,
        y2: line.y2 - cy,
        scaleX: 1,
        scaleY: 1
    });

    line.setPositionByOrigin(new fabric.Point(cx, cy), 'center', 'center');
    line._normalizedOnce = true;
    line.setCoords();
    line.canvas?.requestRenderAll();
}

// ===== Mostrar grados de inclinacion =====
function getVisualAngleDegrees(line) {
    // puntos locales (del objeto)
    const p1 = new fabric.Point(line.x1, line.y1);
    const p2 = new fabric.Point(line.x2, line.y2);

    // transformar a canvas con la matriz del objeto
    const m = line.calcTransformMatrix();                 
    const P1 = fabric.util.transformPoint(p1, m);
    const P2 = fabric.util.transformPoint(p2, m);

    let deg = Math.atan2(P2.y - P1.y, P2.x - P1.x) * 180 / Math.PI;
    deg = ((deg % 180) + 180) % 180;
    return deg;
}

function nearZeroOrNinety(deg, tol = SNAP_TOL_DEG) {
    const d0 = Math.min(Math.abs(deg - 0), Math.abs(180 - deg)); 
    const d90 = Math.abs(deg - 90);
    if (d0 <= tol) return 0;
    if (d90 <= tol) return 90;
    return null;
}

function ensureAngleBadge(line) {
    if (line._angleBadge && !line._angleBadge._disposed) return line._angleBadge;

    const badge = new fabric.Text('0°', {
        fontSize: 14,
        fontStyle: 'bold',
        fill: 'black',
        backgroundColor: '#fff',
        padding: 4,
        selectable: false,
        evented: false,
        excludeFromExport: true,
        visible: false,
        originX: 'center',
        originY: 'center',
        statusObject: 'StoredInBase',
    });

    line._angleBadge = badge;
    line.canvas?.add(badge);
    return badge;
}

function positionAngleBadge(line, badge) {
    const center = line.getCenterPoint();
    const angRad = (line.angle ?? 0) * Math.PI / 180;
    const nx = -Math.sin(angRad), ny = Math.cos(angRad);
    badge.set({ left: center.x + nx * BADGE_OFFSET, top: center.y + ny * BADGE_OFFSET, angle: 0 });
    badge.setCoords();
}

// Nuevo método recibe tipos y Busca los IDs de área
window.applyCanvasSelectionByAreaTypes = function (selection) {
    try {
        if (!selection) return;

        const waitForCanvas = (callback, interval = 50, maxAttempts = 100) => {
            let attempts = 0;

            const check = () => {
                if (typeof Canvas !== 'undefined' && Canvas) {
                    callback();
                } else if (attempts < maxAttempts) {
                    attempts++;
                    setTimeout(check, interval);
                } else {
                    console.error("Canvas not initialized after waiting (applyCanvasSelectionByAreaTypes).");
                }
            };

            check();
        };

        waitForCanvas(() => {
            if (!Canvas) return;

            // 1) AREA TYPES -> AREA IDS
            const areaTypesFilter = (selection.areaTypeCodes || [])
                .map(x => x != null ? String(x).toLowerCase() : "");

            const areaIdsFromAreaTypes = [];

            if (areaTypesFilter.length !== 0) {
                Canvas.getObjects().forEach(obj => {
                    if (obj.elementType === 'groupArea') {
                        const rawType = obj.areaType ?? obj.AreaType ?? obj.type;
                        const objType = (rawType !== undefined && rawType !== null ? rawType : "")
                            .toString()
                            .toLowerCase();

                        const matchesType =
                            areaTypesFilter.length === 0 ||
                            areaTypesFilter.includes(objType);

                        if (matchesType) {
                            const areaId = obj.id.replace('groupArea', '');
                            areaIdsFromAreaTypes.push(areaId);
                        }
                    }
                });
            }
            // 2) PROCESS TYPE CODES -> AREA IDS
            const processTypeCodes = (selection.processTypeCodes || []).map(Number);
            const processTypesSet = new Set(processTypeCodes);
            const areaIdsFromProcessTypes = new Set();

            if (processTypesSet.size > 0) {
                Canvas.getObjects().forEach(obj => {
                    if (obj.elementType === 'groupProcess') {
                        const rawProcType = obj.Type ?? obj.type ?? obj.processType;
                        if (rawProcType === undefined || rawProcType === null) return;

                        const procType = Number(rawProcType);
                        if (!Number.isNaN(procType) && processTypesSet.has(procType)) {
                            const areaByObj = obj.groupId
                                ? obj.groupId.replace('groupProcess', '')
                                : null;

                            if (areaByObj) {
                                areaIdsFromProcessTypes.add(areaByObj);
                            }
                        }
                    }
                });
            }
            // 3) ROUTE IDS -> AREA IDS (fromNode / toNode)
            const rawRouteIds =
                (selection.flows && (selection.flows.routeIds || selection.flows.RouteIds)) ||
                selection.routeIds ||
                selection.RouteIds ||
                [];

            const routeIdsSet = new Set(
                rawRouteIds
                    .filter(r => r != null)
                    .map(r => String(r))
            );

            const areaIdsFromRoutes = new Set();

            if (routeIdsSet.size > 0) {
                Canvas.getObjects().forEach(obj => {
                    const linkedId =
                        obj.linkedEntityId !== undefined && obj.linkedEntityId !== null
                            ? String(obj.linkedEntityId)
                            : null;

                    // Filtramos rutas
                    if (!linkedId || !routeIdsSet.has(linkedId)) return;

                    const fromNode = obj.fromNode ?? obj.FromNode;
                    const toNode = obj.toNode ?? obj.ToNode;

                    const extractAreaId = (node) => {
                        if (!node) return null;
                        let s = String(node);
                        s = s.replace('groupArea', '');
                        return s;
                    };

                    const startAreaId = extractAreaId(fromNode);
                    const endAreaId = extractAreaId(toNode);

                    if (startAreaId) areaIdsFromRoutes.add(startAreaId);
                    if (endAreaId) areaIdsFromRoutes.add(endAreaId);

                    areaIdsFromRoutes.add(linkedId);
                });
            }
            // 4) AREAS FINALES PARA applyCanvasSelection
            const mergedAreas = new Set();

            if (Array.isArray(selection.areas)) {
                selection.areas.forEach(a => {
                    if (a != null) mergedAreas.add(String(a));
                });
            }

            areaIdsFromAreaTypes.forEach(a => mergedAreas.add(String(a)));
            areaIdsFromProcessTypes.forEach(a => mergedAreas.add(String(a)));
            areaIdsFromRoutes.forEach(a => mergedAreas.add(String(a)));

            const finalSelection = {
                areas: Array.from(mergedAreas),
                flows: selection.flows || {},
                processTypeCodes: selection.processTypeCodes || []
            };

            window.applyCanvasSelection(finalSelection);
        });
    } catch (e) {
        console.error("Error in applyCanvasSelectionByAreaTypes", e);
    }
};

window.buildSelectionFromArea = function (areaId) {
    if (!areaId || !Canvas) return null;


    const connectedAreas = new Set([areaId]);
    const routeIds = new Set();

    const isRoute = (obj) =>
        obj.groupId === 'routeGroup' ||
        obj.elementType === 'groupRoute' ||
        obj.elementType === 'routePolyline';

    const getAreaIdsFromRoute = (obj) => {
        const fromNode = obj.fromNode || '';
        const toNode = obj.toNode || '';

        const fromAreaId = fromNode.startsWith('groupArea')
            ? fromNode.replace('groupArea', '')
            : null;

        const toAreaId = toNode.startsWith('groupArea')
            ? toNode.replace('groupArea', '')
            : null;

        return { fromAreaId, toAreaId };
    };

    // Solo tomamos rutas que toquen al área seleccionada 1 a 1
    Canvas.getObjects().forEach(obj => {
        if (!isRoute(obj) || !obj.linkedEntityId) return;

        const { fromAreaId, toAreaId } = getAreaIdsFromRoute(obj);

        const touchesSelectedArea =
            (fromAreaId === areaId) ||
            (toAreaId === areaId);

        if (!touchesSelectedArea) return;

        // Agregamos la ruta
        routeIds.add(obj.linkedEntityId);

        // Agregamos el área vecina, si existe
        if (fromAreaId && fromAreaId !== areaId) {
            connectedAreas.add(fromAreaId);
        }
        if (toAreaId && toAreaId !== areaId) {
            connectedAreas.add(toAreaId);
        }
    });

    return {
        // Área seleccionada + solo áreas de 1er nivel conectadas
        primaryAreaId: areaId, 
        areas: Array.from(connectedAreas),

        flows: {
            areaIds: Array.from(connectedAreas),   
            processIds: [],
            processDirectionIds: [],
            routeIds: Array.from(routeIds),        
            inP: false,
            outP: false
        },
        processTypeCodes: []
    };
};

function backupInteractivity(obj) {
    if (obj.__origInteractivity) return;
    obj.__origInteractivity = {
        selectable: obj.selectable !== false,
        evented: obj.evented !== false,
        hasControls: !!obj.hasControls,
        hasBorders: !!obj.hasBorders
    };
}

function restoreInteractivity(obj) {
    const o = obj.__origInteractivity;
    if (!o) return;
    obj.selectable = o.selectable;
    obj.evented = o.evented;
    obj.hasControls = o.hasControls;
    obj.hasBorders = o.hasBorders;
}

function getAreaIdFromObj(obj) {
    if (obj.elementType === 'groupArea' && obj.id) {
        return obj.id.replace('groupArea', '');
    }
    if (obj.groupId) {
        return obj.groupId
            .replace('groupStation', '')
            .replace('groupShelf', '')
            .replace('groupEquipment', '')
            .replace('groupProcess', '')
            .replace('groupRack', '')
            .replace('groupDriveIn', '')
            .replace('groupCaotic', '')
            .replace('goupAutomatic', '');
    }
    return null;
}

function isRouteObj(obj) {
    return obj.groupId === 'routeGroup' ||
        obj.elementType === 'groupRoute' ||
        obj.elementType === 'routePolyline' ||
        obj.groupId === 'processInGroup' ||
        obj.groupId === 'processOutGroup' ||
        obj.groupId === 'processCustomGroup';
}
function rebuildRoutesViewportFromCanvas() {
    routesViewport = [];

    const objs = Canvas.getObjects();
    const polylines = objs.filter(o =>
    (o.groupId === 'routeGroup' ||
        o.groupId === 'processInGroup' ||
        o.groupId === 'processOutGroup' ||
        o.groupId === 'processCustomGroup')
    );

    polylines.forEach(poly => {
        if (poly.connector) {
           
            if (!routesViewport.includes(poly.connector)) {
                routesViewport.push(poly.connector);
            }
        }
    });
}

function notifySelectedAreaFromActiveObject(activeObject) {
    if (!activeObject || activeObject.elementType !== 'groupArea') return;

    const areaId = (activeObject.id || '').replace('groupArea', '');
    if (window.dotNetRefDesigner) {
        window.dotNetRefDesigner.invokeMethodAsync('OnCanvasAreaSelectionChanged', areaId);
    }
}

window.selectAreaOnCanvasById = function (areaId) {
    if (!Canvas || !areaId) return;

    const obj = Canvas.getObjects().find(o =>
        o.elementType === 'groupArea' &&
        ((o.id || '').replace('groupArea', '') === areaId)
    );

    if (!obj) return;

    Canvas.setActiveObject(obj);
    Canvas.requestRenderAll();
};

window.buildSelectionFromProcess = function (processId) {
    if (!processId || !Canvas) return null;

    const connectedAreas = new Set();
    const connectedProcessIds = new Set([processId]);
    const processDirectionIds = new Set();

    // Área del proceso seleccionado
    const primaryProcessObj = findProcessObjById(processId);
    const primaryAreaId = primaryProcessObj ? getAreaIdFromProcessObj(primaryProcessObj) : null;
    if (primaryAreaId) connectedAreas.add(primaryAreaId);

    // PDs 1er nivel que toquen al proceso
    const pdObjs = findProcessDirectionObjsTouchingProcess(processId);

    pdObjs.forEach(pd => {
        processDirectionIds.add(pd.linkedEntityId);

        const { fromProcessId, toProcessId } = getProcessIdsFromProcessDirection(pd);

        // procesos conectados
        if (fromProcessId) connectedProcessIds.add(fromProcessId);
        if (toProcessId) connectedProcessIds.add(toProcessId);

        // áreas de esos procesos
        const a1 = getAreaIdFromProcessId(fromProcessId);
        const a2 = getAreaIdFromProcessId(toProcessId);
        if (a1) connectedAreas.add(a1);
        if (a2) connectedAreas.add(a2);
    });

    const hasPD = processDirectionIds.size > 0;

    return {
        primaryAreaId: primaryAreaId,
        areas: Array.from(connectedAreas),

        flows: {
            areaIds: Array.from(connectedAreas),
            processIds: hasPD ? Array.from(connectedProcessIds) : [],
            processDirectionIds: hasPD ? Array.from(processDirectionIds) : [],
            inP: false,
            outP: false
        },
        processTypeCodes: []
    };
};

function isRelationsModeOn() {
    const rs = window.canvasRelationsState || { isOn: false };
    return !!rs.isOn;
}

function isProcessRelationsSelection(selection) {

    const pdIds = selection?.flows?.processDirectionIds || [];
    return pdIds.length > 0;
}

function getProcessIdFromObj(obj) {
    if (!obj) return null;

    if (obj.elementType === 'groupProcess' && obj.id) {
        return (obj.id || '').replace('groupProcess', '');
    }

    if (obj.elementType === 'groupProcess' && obj.linkedEntityId) {
        return obj.linkedEntityId;
    }

    return null;
}

function notifySelectedProcessFromActiveObject(activeObject) {
    const processId = getProcessIdFromObj(activeObject);
    if (!processId) return;

    if (window.dotNetRefDesigner) {
        window.dotNetRefDesigner.invokeMethodAsync('OnCanvasProcessSelectionChanged', processId);
    }
}
