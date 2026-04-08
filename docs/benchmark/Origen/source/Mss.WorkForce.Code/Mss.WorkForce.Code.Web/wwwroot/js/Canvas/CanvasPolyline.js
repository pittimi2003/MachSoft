

let routesViewport = [];
// Umbral para snap (en píxeles)

const SNAP_THRESHOLD = 15;
// Distancia mínima para que se aplique el snap (si el segmento es muy corto, se deja sin ajustar)
const SNAP_MIN_DISTANCE = 5;
let typeGroup = '';

let ColorLine = getCssVariable('--sp-color-border-inverse');
let ColorFillCircle = getCssVariable('--sp-color-bg-default');
//let ColorSelected = getCssVariable('--gl-color-smoke-900');
function DrawingAllRoutes() {

    typeGroup = 'routeGroup';
    const objectsCanvas = Canvas.getObjects();

    routesViewport.forEach(connector => {
        if (typeof connector.removeFromCanvas === 'function') {
            connector.removeFromCanvas();
        }
    });
    routesViewport.length = 0; 
    

    modelRoutes.forEach(direction => {
        let objCanvasInit = null;
        let objCanvasEnd = null;

        objectsCanvas.forEach(obj => {
            const cleanId = obj.id?.replace('groupArea', '');

            if (cleanId === direction.OutboundAreaId) {
                objCanvasInit = obj;
            }

            if (cleanId === direction.InboundAreaId) {
                objCanvasEnd = obj;
            }
        });

        if (objCanvasInit && objCanvasEnd) {
            let puntos = [];

            if (direction.ViewPort) {
                try {
                    const parsed = JSON.parse(direction.ViewPort);
                    if (Array.isArray(parsed)) {
                        puntos = parsed.map(p => ({ id: p.id || generateUUID(), x: p.x, y: p.y }));
                    }
                } catch (err) {
                    console.error('Error al parsear ViewPort:', err);
                }
            }

            if (objCanvasInit && objCanvasEnd) {
                const connector = new Connector(
                    objCanvasInit,
                    objCanvasEnd,
                    direction,
                    puntos.length > 0 ? [...puntos] : null
                );
                direction.StatusObject = 'StoredInBase';
                connector.statusObject = 'StoredInBase';
                connector.polyline.statusObject = 'StoredInBase';
            }
        } 
    });

}

function DrawingAllProcessInDirections() {
    typeGroup = 'processInGroup';
    const objectsCanvas = Canvas.getObjects();

    ClearAllDirectionsByType(typeGroup);

    modelProcessDirections
        .filter(direction => direction.InitProcessIsIn)
        .forEach(direction => {
            let objCanvasInit = null;
            let objCanvasEnd = null;

            objectsCanvas.forEach(obj => {
                if (obj.elementType === 'groupProcess') {
                    if (obj.id == 'groupProcess' + direction.InitProcessId) objCanvasInit = obj;
                    if (obj.id == 'groupProcess' + direction.EndProcessId) objCanvasEnd = obj;
                }
            });

            if (objCanvasInit && objCanvasEnd) {
                let puntos = [];
                if (direction.ViewPort) {
                    try {
                        const parsed = JSON.parse(direction.ViewPort);
                        if (Array.isArray(parsed)) {
                            puntos = parsed.map(p => ({ id: p.id || generateUUID(), x: p.x, y: p.y }));
                        }
                    } catch (err) {
                        console.error('Error al parsear ViewPort:', err);
                    }
                }
                //Marcamos StatusObject
                if (objCanvasInit && objCanvasEnd) {
                    const connector = new Connector(
                        objCanvasInit,
                        objCanvasEnd,
                        direction,
                        puntos.length > 0 ? [...puntos] : null
                    );
                    direction.StatusObject = 'StoredInBase';
                    connector.statusObject = 'StoredInBase';
                    connector.polyline.statusObject = 'StoredInBase';
                }
            } 
        });
}

function DrawingAllProcessOutDirections() {
    typeGroup = 'processOutGroup';
    const objectsCanvas = Canvas.getObjects();

    ClearAllDirectionsByType(typeGroup);

    modelProcessDirections
        .filter(direction => direction.InitProcessIsOut)
        .forEach(direction => {
            let objCanvasInit = null;
            let objCanvasEnd = null;

            objectsCanvas.forEach(obj => {
                if (obj.elementType === 'groupProcess') {
                    if (obj.id == 'groupProcess' + direction.InitProcessId) objCanvasInit = obj;
                    if (obj.id == 'groupProcess' + direction.EndProcessId) objCanvasEnd = obj;
                }
            });

            if (objCanvasInit && objCanvasEnd) {
                let puntos = [];
                if (direction.ViewPort) {
                    try {
                        const parsed = JSON.parse(direction.ViewPort);
                        if (Array.isArray(parsed)) {
                            puntos = parsed.map(p => ({ id: p.id || generateUUID(), x: p.x, y: p.y }));
                        }
                    } catch (err) {
                        console.error('Error al parsear ViewPort:', err);
                    }
                }
                //Marcamos StatusObject
                if (objCanvasInit && objCanvasEnd) {
                    const connector = new Connector(
                        objCanvasInit,
                        objCanvasEnd,
                        direction,
                        puntos.length > 0 ? [...puntos] : null
                    );
                    direction.StatusObject = 'StoredInBase';
                    connector.statusObject = 'StoredInBase';
                    connector.polyline.statusObject = 'StoredInBase';
                }
            } 
        });
}

function DrawingAllProcessCustomDirections() {
    typeGroup = 'processCustomGroup';
    const objectsCanvas = Canvas.getObjects();

    ClearAllDirectionsByType(typeGroup);

    modelProcessDirections
        .filter(direction => !direction.InitProcessIsOut && !direction.InitProcessIsIn)
        .forEach(direction => {
            let objCanvasInit = null;
            let objCanvasEnd = null;
            CustomRouteActive = true;

            objectsCanvas.forEach(obj => {
                if (obj.elementType === 'groupProcess') {
                    if (obj.id == 'groupProcess' + direction.InitProcessId) objCanvasInit = obj;
                    if (obj.id == 'groupProcess' + direction.EndProcessId) objCanvasEnd = obj;
                }
            });

            if (objCanvasInit && objCanvasEnd) {
                let puntos = [];
                if (direction.ViewPort) {
                    try {
                        const parsed = JSON.parse(direction.ViewPort);
                        if (Array.isArray(parsed)) {
                            puntos = parsed.map(p => ({ id: p.id || generateUUID(), x: p.x, y: p.y }));
                        }
                    } catch (err) {
                        console.error('Error al parsear ViewPort:', err);
                    }
                }
                //Marcamos StatusObject
                if (objCanvasInit && objCanvasEnd) {
                    const connector = new Connector(
                        objCanvasInit,
                        objCanvasEnd,
                        direction,
                        puntos.length > 0 ? [...puntos] : null
                    );
                    direction.StatusObject = 'StoredInBase';
                    connector.statusObject = 'StoredInBase';
                    connector.polyline.statusObject = 'StoredInBase';
                }
            }
        });
}


function ClearAllDirectionsByType(typeGroupName) {
    
    const toRemove = routesViewport.filter(connector => connector.polyline?.groupId === typeGroupName);

    toRemove.forEach(connector => {
        if (typeof connector.removeFromCanvas === 'function') {
            connector.removeFromCanvas();
        }
    });

    for (let i = routesViewport.length - 1; i >= 0; i--) {
        if (routesViewport[i].polyline?.groupId === typeGroupName) {
            routesViewport.splice(i, 1);
        }
    }

    Canvas.requestRenderAll();
}

/*************************************************************
  * CLASE CONNECTOR CON ENDPOINTS MOVIBLES, BEND POINTS EDITABLES Y SNAP
  *************************************************************/
class Connector {
    constructor(fromNode, toNode, direction, puntosCustom = null) {
        this.fromNode = fromNode;
        this.toNode = toNode;
        var id = direction.Id;
        this.bidirectional = direction.Bidirectional;

        // Determinar grupo y color basado en tipo de dirección
        let groupId = 'routeGroup';
        this.groupId = groupId;
        let ColorLine = getCssVariable('--gl-color-smoke-700');
        let ColorSelected = getCssVariable('--gl-color-smoke-900');

        if (direction.InitProcessIsIn) {
            groupId = 'processInGroup';
            ColorLine = getCssVariable('--gl-color-green-400');
            ColorSelected = getCssVariable('--gl-color-green-500');
        } else if (direction.InitProcessIsOut) {
            groupId = 'processOutGroup';
            ColorLine = getCssVariable('--gl-color-blue-400');
            ColorSelected = getCssVariable('--gl-color-blue-500');
        } else if (CustomRouteActive) {
            groupId = 'processCustomGroup';
            ColorLine = getCssVariable('--gl-color-pink-600');
            ColorSelected = getCssVariable('--gl-color-pink-700');
        }

        let ColorFillCircle = getCssVariable('--sp-color-bg-default');

        this.endPointFrom = getConnectionPoint(this.fromNode, getCenterAnchor(this.toNode));
        this.endPointTo = getConnectionPoint(this.toNode, getCenterAnchor(this.fromNode));

        if (puntosCustom && Array.isArray(puntosCustom) && puntosCustom.length >= 2) {
            this.points = puntosCustom.map(p => ({
                x: p.x,
                y: p.y,
                id: p.id || generateUUID()
            }));

            this.endPointFrom = { x: this.points[0].x, y: this.points[0].y };
            this.endPointTo = { x: this.points[this.points.length - 1].x, y: this.points[this.points.length - 1].y };
            this.fromBaseData = true;
        } else {
            const midX = (this.endPointFrom.x + this.endPointTo.x) / 2;
            this.points = [
                { x: this.endPointFrom.x, y: this.endPointFrom.y, id: generateUUID() },
                { x: midX, y: this.endPointFrom.y, id: generateUUID() },
                { x: midX, y: this.endPointTo.y, id: generateUUID() },
                { x: this.endPointTo.x, y: this.endPointTo.y, id: generateUUID() }
            ];
            this.fromBaseData = false;
        }

        this.polyline = new fabric.Polyline(this.points, {
            id: id,
            stroke: ColorLine,
            strokeWidth: 2,
            fill: '',
            selectable: true,
            evented: true,
            hasControls: false,
            hasBorders: false,
            lockMovementX: true,
            lockMovementY: true,
            lockScalingX: true,
            lockScalingY: true,
            lockRotation: true,
            objectCaching: false,
            perPixelTargetFind: true,
            groupId: groupId,
            linkedEntityId: id,
            visible: false,
            fromNode: this.fromNode.id.replace('groupProcess', ''), 
            toNode: this.toNode.id.replace('groupProcess', ''),
        });

        // Event selected to polyline and header rows
        this.polyline.on('selected', () => {
            if (window.canvasRelationsState?.isOn) return;
            this.polyline.set({ stroke: ColorSelected });
            this.endControlTo._objects.forEach(line => line.set({ stroke: ColorSelected })); // Change color of endControlTo

            // Change color of endControlFrom if bidirectional
            if (this.bidirectional) {
                this.endControlFrom._objects.forEach(line => line.set({ stroke: ColorSelected }));
            } else {
                this.endControlFrom.set({ stroke: ColorSelected });
            }

            Canvas.requestRenderAll();
        });

        // Event Deselected to polyline and header rows
        this.polyline.on('deselected', () => {
            if (window.canvasRelationsState?.isOn) return;
            this.polyline.set({ stroke: ColorLine });
            this.endControlTo._objects.forEach(line => line.set({ stroke: ColorLine })); // Returns original color of endControlTo

            // Returns original color from endControlFrom
            if (this.bidirectional) {
                this.endControlFrom._objects.forEach(line => line.set({ stroke: ColorLine }));
            } else {
                this.endControlFrom.set({ stroke: ColorLine });
            }

            Canvas.requestRenderAll();
        });
        Canvas.add(this.polyline);

        // Control FROM
        if (this.bidirectional) {
            this.endControlFrom = createArrowGroup(this.points[1], this.points[0], ColorLine, this.fromNode, this.toNode, this.groupId, id);
        } else {
            this.endControlFrom = new fabric.Circle({
                left: this.points[0].x,
                top: this.points[0].y,
                radius: 6,
                fill: ColorFillCircle,
                elementType: groupId,
                stroke: ColorLine,
                strokeWidth: 2,
                originX: 'center',
                originY: 'center',
                groupId: groupId,
                selectable: true,
                evented: true,
                hasControls: false,
                hasBorders: false,
                lockRotation: true,
                objectCaching: false,
                perPixelTargetFind: true,
                linkedEntityId: id,
                visible: false,
                hoverCursor: 'move', 
            });
        }

        Canvas.add(this.endControlFrom);

        // Control TO (siempre flecha)
        this.endControlTo = createArrowGroup(this.points[this.points.length - 2], this.points[this.points.length - 1], ColorLine, this.fromNode, this.toNode, groupId, id);
        Canvas.add(this.endControlTo);

        // Movimiento FROM
        this.endControlFrom.on('moving', () => {
            const cp = this.endControlFrom.getCenterPoint();
            this.points[0].x = cp.x;
            this.points[0].y = cp.y;
            this.endPointFrom = this.points[0];

            const route = routesViewport.find(r => r.polyline && r.polyline.id === this.polyline.id);
            if (route) {
                route.points[0].x = cp.x;
                route.points[0].y = cp.y;
            }

            this.polyline.set({ points: this.points });
            Canvas.requestRenderAll();
        });

        this.endControlFrom.on('modified', () => {
            this.update();
        });

        // Movimiento TO
        this.endControlTo.on('moving', () => {
            const cp = this.endControlTo.getCenterPoint();
            const lastIdx = this.points.length - 1;
            this.points[lastIdx].x = cp.x;
            this.points[lastIdx].y = cp.y;
            this.endPointTo = this.points[lastIdx];

            const route = routesViewport.find(r => r.polyline && r.polyline.id === this.polyline.id);
            if (route) {
                route.points[lastIdx].x = cp.x;
                route.points[lastIdx].y = cp.y;
            }

            this.polyline.set({ points: this.points });
            Canvas.requestRenderAll();
        });

        this.endControlTo.on('modified', () => {
            this.update();
        });

        this.controlCircles = [];

        routesViewport.push(this);
      

        this.update();
        this.fromBaseData = false;
    }


    update() {
        if (!this.fromBaseData) {
            this.endPointFrom = constrainPointToRectBoundary(this.fromNode, this.endPointFrom);
            this.endPointTo = constrainPointToRectBoundary(this.toNode, this.endPointTo);
        }

        const ruta = routesViewport.find(r => r.polyline === this.polyline);
        if (!ruta) return;

        if (this.points[0]?.id) {
            this.points[0].x = this.endPointFrom.x;
            this.points[0].y = this.endPointFrom.y;
        } else {
            this.points[0] = { id: generateUUID(), ...this.endPointFrom };
        }

        if (this.points[this.points.length - 1]?.id) {
            this.points[this.points.length - 1].x = this.endPointTo.x;
            this.points[this.points.length - 1].y = this.endPointTo.y;
        } else {
            this.points[this.points.length - 1] = { id: generateUUID(), ...this.endPointTo };
        }

        this.points.forEach(p => {
            const punto = ruta.points.find(x => x.id === p.id);
            if (punto) {
                punto.x = p.x;
                punto.y = p.y;
            }
        });

        const skip = getArrowSkipIndicesFor(this);
        const THRESH = typeof SNAP_THRESHOLD !== 'undefined' ? SNAP_THRESHOLD : 12;
        snapPoints(this.points, THRESH, { skipIndices: skip });
        this.polyline.set({ points: this.points });
        this.endControlFrom.set({
            left: this.points[0].x,
            top: this.points[0].y
        });
        this.endControlTo.set({
            left: this.points[this.points.length - 1].x,
            top: this.points[this.points.length - 1].y
        });


        if (this.bidirectional && this.endControlFrom.type === 'group') {
            this.updateArrowControl(this.endControlFrom, this.points[1], this.points[0]);
        }
        this.updateArrowControl(this.endControlTo, this.points[this.points.length - 2], this.points[this.points.length - 1]);


        this.removeCircles();
        this.drawIntermediateCircles();

        Canvas.requestRenderAll();
        if (this.statusObject === 'StoredInBase') {
            this.statusObject = 'Update';
            let modelObj = modelRoutes.find(r => r.Id === this.polyline.id);
            if (!modelObj) {
                modelObj = modelProcessDirections.find(p => p.Id === this.polyline.id);
            }
            if (modelObj) modelObj.StatusObject = 'Update';
            this.polyline.statusObject = 'Update';
        }

    }


    drawIntermediateCircles() {

        let ColorLine = getCssVariable('--sp-color-border-inverse');
        let ColorFillCircle = getCssVariable('--sp-color-bg-default');
       
        this.removeCircles();

        const route = routesViewport.find(r => r.polyline === this.polyline);

        if (route) {
            for (let i = 1; i < route.points.length - 1; i++) {

                if (route.endControlFrom.groupId == 'processInGroup') {
                    ColorLine = getCssVariable('--gl-color-green-400');
                } else if (route.endControlFrom.groupId == 'processOutGroup') {
                    ColorLine = getCssVariable('--gl-color-blue-400');
                } else if (route.endControlFrom.groupId == 'processCustomGroup') {
                    ColorLine = getCssVariable('--gl-color-pink-600');
                }

                const pt = route.points[i];

                const circle = new fabric.Circle({
                    left: pt.x,
                    top: pt.y,
                    radius: 6,
                    fill: ColorFillCircle,
                    stroke: ColorLine,
                    strokeWidth: 2,
                    originX: 'center',
                    originY: 'center',
                    groupId: typeGroup,
                    selectable: true,
                    evented: true,
                    hasControls: false,
                    hasBorders: false,
                    lockRotation: true,
                    objectCaching: false,
                    elementType: 'circleControl',
                    perPixelTargetFind: true,
                    groupId: this.groupId,
                    linkedEntityId: this.polyline.id,
                    visible: this.polyline.visible,
                    hoverCursor: 'move',    
                });

                circle.typeObject = 'controlCircle';
                circle.connector = this;

                circle.on('moving', function () {
                    const cp = circle.getCenterPoint();

                    let draggingFill = '';

                    if (circle.groupId == "processInGroup") {
                        draggingFill = getCssVariable('--gl-color-blue-500');
                    } else if (circle.groupId == "processOutGroup") {
                        draggingFill = getCssVariable('--gl-color-green-500');
                    } else if (circle.groupId == "processOutGroup") {
                        draggingFill = getCssVariable('--gl-color-pink-700');
                    } else {
                        draggingFill = getCssVariable('--gl-color-smoke-900');
                    }

                    circle.set({ fill: draggingFill });
                    
                    circle.set({ left: cp.x, top: cp.y });

                    const index = circle.connector.controlCircles.indexOf(circle);

                    if (index !== -1) {
                        const route = routesViewport.find(r => r.polyline === circle.connector.polyline);
                        if (route) {
                            const punto = route.points[index + 1];
                            if (punto) {
                                punto.x = cp.x;
                                punto.y = cp.y;
                            }
                        }

                        const puntoConnector = circle.connector.points[index + 1];
                        if (puntoConnector) {
                            puntoConnector.x = cp.x;
                            puntoConnector.y = cp.y;
                        }

                        circle.connector.polyline.set({ points: circle.connector.points });
                        Canvas.requestRenderAll();
                    }
                });

                circle.on('modified', function () {
                    const normalFill = getCssVariable('--sp-color-bg-default');
                    circle.set({ fill: normalFill });
                    circle.connector.update();
                });

                //circle.on('modified', function () {
                //    circle.connector.update();
                //});

                Canvas.add(circle);
                Canvas.bringToFront(circle);
                this.controlCircles.push(circle);
            }
        }
    }


    insertIntermediatePoint(pos) {
        const idx = findSegmentIndexNearest(this.points, pos);
        const newPoint = { id: generateUUID(), x: pos.x, y: pos.y };

        this.points.splice(idx + 1, 0, newPoint);

        this.removeCircles();
        this.controlCircles = [];
        this.update();
    }


    removeFromCanvas() {
        if (this.polyline) {
            Canvas.remove(this.polyline);
        }

        if (this.endControlFrom) {
            Canvas.remove(this.endControlFrom);
        }

        if (this.endControlTo) {
            Canvas.remove(this.endControlTo);
        }

        if (this.controlCircles && this.controlCircles.length) {
            this.controlCircles.forEach(c => {
                if (Canvas.contains(c)) {
                    Canvas.remove(c);
                }
            });
        }

        this.controlCircles = [];
        Canvas.requestRenderAll();
    }

    removeCircles() {
        if (this.controlCircles) {
            this.controlCircles.forEach(circle => {
                Canvas.remove(circle); // Eliminar de la lista de objetos en canvas
            });
            this.controlCircles = []; // Limpiar el array de círculos intermedios
        }
    }

    updateArrowControl(control, fromPoint, toPoint) {
        if (!control) return;

        control.left = toPoint.x;
        control.top = toPoint.y;

        const angleRad = Math.atan2(toPoint.y - fromPoint.y, toPoint.x - fromPoint.x);
        const angleDeg = angleRad * (180 / Math.PI);

        control.angle = angleDeg;

        control.setCoords();
    }

}

function updateAllPosicionRoute() {
    routesViewport.forEach(route => route.update());
}

function createArrowGroup(fromPoint, toPoint, color, fromNode, toNode, groupId, id) {
    const angleRad = Math.atan2(toPoint.y - fromPoint.y, toPoint.x - fromPoint.x);
    const angleDeg = angleRad * (180 / Math.PI);

    const headLength = 10;
    const headWidth = 8;

    const line1 = new fabric.Line([0, 0, -headLength, -headWidth / 2], {
        stroke: color,
        strokeWidth: 2,
        selectable: false,
        evented: false
    });

    const line2 = new fabric.Line([0, 0, -headLength, headWidth / 2], {
        stroke: color,
        strokeWidth: 2,
        selectable: false,
        evented: false
    });

    const arrowGroup = new fabric.Group([line1, line2], {
        originX: 'center',
        originY: 'center',
        selectable: true,
        evented: true,
        elementType: 'startOrEndArrow',
        hasControls: false,
        hasBorders: false,
        lockRotation: true,
        objectCaching: false,
        perPixelTargetFind: true,
        angle: angleDeg,
        groupId: groupId,
        linkedEntityId: id,
        visible: false,
        hoverCursor: 'move',
    });

    return arrowGroup;
}

function getConnectionPoint(node, targetPoint) {
    const points = getAnchorPoints(node);
    let best = points[0];
    let minDist = Infinity;
    points.forEach(pt => {
        const d = Math.hypot(pt.x - targetPoint.x, pt.y - targetPoint.y);
        if (d < minDist) { minDist = d; best = pt; }
    });
    return best;
}
function getAnchorPoints(node) {
    node.setCoords();
    const b = node.getBoundingRect({ absolute: true });
    return [
        { x: b.left + b.width / 2, y: b.top },                  // Top
        { x: b.left + b.width / 2, y: b.top + b.height },       // Bottom
        { x: b.left, y: b.top + b.height / 2 },                 // Left
        { x: b.left + b.width, y: b.top + b.height / 2 }        // Right
    ];
}
function getCenterAnchor(node) {
    return node.getCenterPoint();
}

/*************************************************************
  * FUNCIÓN: CONSTRAE UN PUNTO AL CONTORNO DEL NODO
  *************************************************************/
function constrainPointToRectBoundary(node, desired) {
    node.setCoords();
    const { left, top, width, height } = node.getBoundingRect(true);
    const cx = left + width / 2;
    const cy = top + height / 2;
    const dx = desired.x - cx;
    const dy = desired.y - cy;

    if (dx === 0 && dy === 0) return { x: cx, y: cy };

    const scale = Math.min((width / 2) / Math.abs(dx), (height / 2) / Math.abs(dy));

    return {
        x: cx + dx * scale,
        y: cy + dy * scale
    };
}


/*************************************************************
* FUNCIÓN: SNAP – AJUSTA LOS PUNTOS SI ESTÁN CERCA EN UN MISMO EJE
* Solo se aplica el snap si la diferencia es mayor a SNAP_MIN_DISTANCE.
*************************************************************/
function snapPoints(points, threshold, options = {}) {
    const min = typeof options.min === 'number' ? options.min : SNAP_MIN_DISTANCE;
    const skip = new Set(options.skipIndices || []);

    for (let i = 0; i < points.length - 1; i++) {
        const i1 = i;
        const i2 = i + 1;

        // si alguno de los dos puntos está excluido, salta este par
        if (skip.has(i1) || skip.has(i2)) continue;

        const dx = Math.abs(points[i2].x - points[i1].x);
        const dy = Math.abs(points[i2].y - points[i1].y);

        if (dx > min && dx <= threshold) {
            points[i2].x = points[i1].x;
        }
        if (dy > min && dy <= threshold) {
            points[i2].y = points[i1].y;
        }
    }
}

/*************************************************************
     * FUNCIONES PARA INSERTAR UN NUEVO PUNTO EN EL SEGMENTO MÁS CERCA
     *************************************************************/
function distanceFromPointToSegment(P, A, B) {
    const vx = B.x - A.x, vy = B.y - A.y;
    if (vx === 0 && vy === 0) return Math.hypot(P.x - A.x, P.y - A.y);
    const t = ((P.x - A.x) * vx + (P.y - A.y) * vy) / (vx * vx + vy * vy);
    let nearest;
    if (t < 0) nearest = A;
    else if (t > 1) nearest = B;
    else nearest = { x: A.x + t * vx, y: A.y + t * vy };
    return Math.hypot(P.x - nearest.x, P.y - nearest.y);
}

function findSegmentIndexNearest(points, pos) {
    let bestIndex = 0, bestDist = Infinity;
    for (let i = 0; i < points.length - 1; i++) {
        const d = distanceFromPointToSegment(pos, points[i], points[i + 1]);
        if (d < bestDist) { bestDist = d; bestIndex = i; }
    }
    return bestIndex;
}

/*************************************************************
     * FUNCIÓN PARA OBTENER COLOR VÍA VARIABLE
     *************************************************************/
function getCssVariable(variableName) {
    return getComputedStyle(document.documentElement).getPropertyValue(variableName).trim();
}

//  ** FUNCIÓN PARA EXCLUIR FLECHA EN EJE CERCA DE SNAP **
function getArrowSkipIndicesFor(connector) {
    const skip = [];
    const n = connector.points.length;
    if (!n) return skip;

    if (connector.endControlFrom && connector.endControlFrom.type === 'group') {
        skip.push(0);
    }

    if (connector.endControlTo) {
        skip.push(n - 1);
    }
    return skip;
}