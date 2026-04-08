/**
 * Initializes and configures the Fabric.js canvas instance.
 * This file contains only the basic canvas setup such as size, background, zoom, and rendering settings.
 */


///=========== Parametros que definen el comportamiento del canvas
const idCanvas = 'canvas'; //id dado al canvas
const activeGridOverlay = true; // Indica si se visualiza la imagen tipo grid en el canvas
const activeMouseWheel = true; // Indica si se podra aplicar zoom con el scroll del mouse
const activeRulers = true; //Indica si el cambas debe mostrar reglas
let offsetX = 0,
    offsetY = 0;
var inDesigner = false; //La variable determina si el js se esta ejecutando desde el modulo designer
//=============================================//

///=========== Variables para la construcción y manipulación del canvas
let Canvas = null;
var rulers;
let minZoom = 0.1;
let zoomP = 0;
let maxZoom = 100;
let zoomRatio = 1.25;
const SNAP_TOL_DEG = 2;   // Solo mostrar badge si está dentro de ±4° de 0 o 90
const BADGE_OFFSET = 16; // Separación del centro de la línea
// Scroll bars
let rightScrollBar;
let bottomScrollBar;
// Ratio for each scroll bar
let visualRatioRigthScrollBar = 0;
let visualRatioBottomScrollBar = 0;
//=============== Particular actions affecting the content of the canvas
let acumulable = '{"Areas":[],"Objects":[], "Routes": [], "Equipments": [], "Stations": [], "Shelf": [], "Process": [], "ProcessDirections": []}';
let activeButton = null;
let areasVisible = true;
let copiedObjects = [];
let copyNameTracker = {
    area: {},
    process: {},
    station: {},
    steps: {}
    //equipment: {}
};
let currentMode;
let CustomRouteActive = false;
const doubleClickDelay = 300;
let dotnetLocalHelper = null;
let firstMouseX = 0;
let firstMouseXAbsolute = 0;
let firstMouseY = 0;
let firstMouseYAbsolute = 0;
var firstTimeWriting = true;
let IntRouteActive = false;
let isCopying = false;
let isDraggingState = false;
let isPanModeEnabled = false;
let isPanning = false;
let isPasting = false;
let isPastingShortcut = false;
let jsonDelete = '[]';
let lastClickTime = 0;
let lastMouseX = 0;
let lastMouseXAbsolute = 0;
let lastMouseY = 0;
let lastMouseYAbsolute = 0;
let line;
let modelEquipments = [];
let modelLayoutDto = null;
let modelProcessDirections = [];
let modelRoutes = [];
const modes = {
    hand: 'hand',
    addText: 'addText',
    addSquare: 'addSquare',
    addLineStraight: 'addLineStraight',
    zoomToSelection: 'zoomToSelection',
    addArea: 'addArea',
    addStorage: 'addStorage',
    addStage: 'addStage',
    addDock: 'addDock',
    addAisle: 'addAisle',
    addBuffer: 'addBuffer',
    addShelf: 'addShelf',
    save: 'save',
    delete: 'delete',
    select: 'select'
};
let objectSelection = null;
let onCanvas = false;
let OutRouteActive = false;
let pasteCount = 0;
let updateData = false;
let tabSlected = '';
const ALIGN_KEYS = {
    l: 'left',
    c: 'horizontalCenter',
    r: 'right',
    m: 'verticalCenter',
    b: 'bottom',
};
let _handHoldActive = false;
let _toolBeforeHold = null;
let hoverOverlay = null;
//=============================================//
//================== Propiedaes de los objetos
let borderDock = '#C3CEDC';
let borderProcess = '#2E4052';
let borderSquare = '#2E4052';
let borderBuffer = '#D2A1E0';
let borderStorageArea = '#FFDAA6';
let borderStage = '#5B9FDA';

let cellSizeStorage = 50;
let colorDock = '#C3CEDC';
let colorProcess = '#ffffff';
let colorSquare = 'transparent';
let colorBuffer = '#D2A1E0';
let colorStorageArea = '#FFDAA6';
let colorStage = '#5B9FDA';

let ColorProcessInDirections = '#68CAA7';
let ColorProcessOutDirections = '#247ECC';
let ColorProcessCustomDirections = '#AF2E79';

let opacityActive = 1;
let opacityInactive = 0.24;
let opacityZones = 0.24;

let routeSvgEquipment = 'img/ico/'
let textColorArea = '#000000';

//===============================================
var workAreaWidth;
var workAreaHeigth;

let canvasGroup;
const texturePatterns = {};

const sizeStationHeight = 50;
const sizeStationWidth = 100;

let lastPointerCanvas = { x: null, y: null };
let lastPointerValid = false;
let selectedAreaOverlay = null;
//============== Tooltip ============================
let tooltipTimer = null;
let currentHoveredObject = null;
const waitingTimeTooltip = 2000;
const tooltip = document.createElement('div');
const allowedTypes = ['groupArea', 'groupProcess', 'groupEquipment', 'groupStation','groupShelf'];

document.addEventListener("DOMContentLoaded", () => {
    tooltip.id = 'canvas-tooltip';
    tooltip.style.position = 'absolute';

    const rootStyles = getComputedStyle(document.documentElement);
    const colorText = rootStyles.getPropertyValue('--sp-color-content-default').trim();
    const borderColor = rootStyles.getPropertyValue('--sp-color-border-inverse').trim();

    tooltip.style.background = '#fff';
    tooltip.style.color = colorText;
    tooltip.style.border = `1px solid ${borderColor}`;
    tooltip.style.padding = '6px 10px';
    tooltip.style.borderRadius = '2px';
    tooltip.style.fontSize = '12px';
    tooltip.style.display = 'none';
    tooltip.style.pointerEvents = 'none';

    document.body.appendChild(tooltip);
    if (document.body) {
        document.body.appendChild(tooltip);
    } else {
        console.error("document.body no está disponible todavía.");
    }
});

//===================================================
//Imagen que define el area de trabajo
let workArea = 'data:image/png;base64,iVBORw0KGgoAAAANSUhEUgAABwgAAAUUCAYAAADLGEZPAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAC58SURBVHhe7NkBAQAACMMg+5e+QQY1uAEAAAAAAAAZghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQDfnh0IAAAAMAzyp/5BVhoBAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgRBACAAAAAABAiCAEAAAAAACAEEEIAAAAAAAAIYIQAAAAAAAAQgQhAAAAAAAAhAhCAAAAAAAACBGEAAAAAAAAECIIAQAAAAAAIEQQAgAAAAAAQIggBAAAAAAAgBBBCAAAAAAAACGCEAAAAAAAAEIEIQAAAAAAAIQIQgAAAAAAAAgRhAAAAAAAABAiCAEAAAAAACBEEAIAAAAAAECIIAQAAAAAAIAQQQgAAAAAAAAhghAAAAAAAABCBCEAAAAAAACECEIAAAAAAAAIEYQAAAAAAAAQIggBAAAAAAAgYzuGdwP1PVBlCQAAAABJRU5ErkJggg=='


window.canvasSelectionFilter = {
    areas: [],
    flows: {
        areaIds: [],
        processIds: [],
        inP: false,
        outP: false
    },
    routes: [],
    processes: []

};


///=========== Constantes con funciones de renderizado
const calculateCanvasWidth = () => {
    const windowWidth = $(window).width();
    const rootFontSize = parseFloat(getComputedStyle(document.documentElement).fontSize);

    const horizontalMargin = 3.5 * rootFontSize; // Margen horizontal en rem
    const fixedPadding = 55; // Ajuste fijo en px

    return windowWidth - horizontalMargin - fixedPadding;
};

const calculateCanvasHeight = () => {
    const windowHeight = $(window).height();
    const rootFontSize = parseFloat(getComputedStyle(document.documentElement).fontSize);
    const fixedPadding = 93; // Ajuste fijo en px
    const horizontalMargin = 5 * rootFontSize;

    return (windowHeight - horizontalMargin - fixedPadding);
}

//Genera el area de trabajo

window.StartWorkArea = () => {
    CreateWorkArea();
    GetTextureZone();
    console.log("StartWorkArea");

    jsonDelete = '[]';
    acumulable = '{"Areas":[],"Objects":[], "Routes": [], "Equipments": [], "Stations": [], "Shelf": [], "Process": [], "ProcessDirections": []}';

    canvasGroup = new fabric.Group(Canvas.getObjects()[0]);
    canvasGroup.elementType = "group";
    canvasGroup.hasControls = false;
    canvasGroup.selectable = false;
    canvasGroup.evented = false;
    canvasGroup.id = 'background';

    const floatMenu = document.getElementById('mlx-float-menu-id');

    Canvas.remove(Canvas.getObjects()[0]);
    Canvas.add(canvasGroup);
    Canvas.renderAll();
    Canvas.subTargetCheck = true;
    Canvas.preserveObjectStacking = true; // (opcional) no eleva el seleccionado al frente
    Canvas.targetFindTolerance = 5;  
    SetInitialZoom();
    syncRulersWithCanvas();
    updateZoomDisplay();
    enableDisableZoomButtons();

    makeDivDraggable(floatMenu);

    if (floatMenu) {
        const top = localStorage.getItem(floatMenu.id + "-top");
        const left = localStorage.getItem(floatMenu.id + "-left");
        if (top && left) {
            floatMenu.style.top = top;
            floatMenu.style.left = left;
        }
        // Aseguramos menu dentro de tamaño real de ventana
        ensureMenuVisible(floatMenu);

        // Re-encierra en cada resize (con debounce)
        const onResize = debounce(() => ensureMenuVisible(floatMenu), 120);
        window.addEventListener('resize', onResize);
    }

};

const createPattern = (url, key, fillColor, scale = 0.3) => {
    const img = new Image();
    img.src = url;

    img.onload = () => {
        const canvas = document.createElement('canvas');
        canvas.width = img.width * scale;
        canvas.height = img.height * scale;

        const ctx = canvas.getContext('2d');
        ctx.imageSmoothingEnabled = false;

        ctx.fillStyle = fillColor;
        ctx.globalAlpha = 0.25;
        ctx.fillRect(0, 0, canvas.width, canvas.height);

        ctx.globalAlpha = 1;
        ctx.drawImage(img, 0, 0, canvas.width, canvas.height);

        texturePatterns[key] = new fabric.Pattern({
            source: canvas,
            repeat: 'repeat'
        });

        if (Canvas?.requestRenderAll) Canvas.requestRenderAll();
    };

    img.onerror = (err) => {
        console.error(`Error loading image: ${url}`, err);
    };
};



const GetTextureZone = () => {
    if (Object.keys(texturePatterns).length === 0) {
        createPattern('img/designer/pattern-WF-Caotic.svg', 'CaoticStorage', getCssVariable('--sp-color-orange-200'));
        createPattern('img/designer/pattern-dots-orange.svg', 'DriveIn', getCssVariable('--sp-color-orange-200'));
        createPattern('img/designer/pattern-1B.svg', 'Dock', getCssVariable('--sp-color-smoke-300'));
       /* createPattern('img/designer/pattern-plus-orange.svg', 'Automatic', getCssVariable('--sp-color-orange-200'));*/
        //createPattern('img/ico/ChaoticDots.png', 'Chaotic', getCssVariable('--sp-color-orange-200'));
        
    }
};

const CreateWorkArea = () => {
    Canvas.setBackgroundColor('white', Canvas.renderAll.bind(Canvas));

    //let objectImage = new Image();
    //objectImage.src = workArea;
    //var image = new fabric.Image(objectImage);
    //image.left = 0;
    //image.top = 0;
    //image.angle = 0;
    //image.scaleX = workAreaWidth / image.width;
    //image.scaleY = workAreaHeigth / image.height;
    //image.id = 'background';
    //image.selectable = false;
    //image.isModified = false;

    //Canvas.add(image);
}

//Add scroll canvas
const useScrollBarRight = (position) => {

    var maxValue = (workAreaHeigth - calculateCanvasHeight() / Canvas.getZoom() * (1 - visualRatioRigthScrollBar));

    Canvas.absolutePan({
        x: Canvas.vptCoords.tl.x * Canvas.getZoom(),
        y: Math.round(Math.max(position / 100 * maxValue * Canvas.getZoom(), 0))
    });

    Canvas.renderAll();

    updateZoomDisplay();
    positionRulers(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
    //Descomentar para uso de unidades de medida 
    /*positionRulers((Canvas.vptCoords.tl.x / typeUnit), (Canvas.vptCoords.tl.y / typeUnit));*/
}

const useScrollBarBottom = (position) => {

    var maxValue = (workAreaWidth - calculateCanvasWidth() / Canvas.getZoom() * (1 - visualRatioBottomScrollBar));

    Canvas.absolutePan({
        x: Math.round(Math.max(position / 100 * maxValue * Canvas.getZoom(), 0)),
        y: Canvas.vptCoords.tl.y * Canvas.getZoom(),
    });

    Canvas.renderAll();

    updateZoomDisplay();
    positionRulers(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
    //Descomentar para uso de unidades de medida 
    /*positionRulers((Canvas.vptCoords.tl.x / typeUnit), (Canvas.vptCoords.tl.y / typeUnit));*/

}

const updateScrollbarsValues = (x, y) => {

    let maxValueRight = calculateRightScrollBarMaxValue(workAreaHeigth, calculateCanvasHeight(), Canvas.getZoom(), visualRatioRigthScrollBar);
    let maxValueBottom = calculateBottomScrollBarMaxValue(workAreaWidth, calculateCanvasWidth(), Canvas.getZoom(), visualRatioBottomScrollBar);

    // Control the lock in scrollBars when there is not enough zoom
    if (Math.round(workAreaHeigth * Canvas.getZoom()) <= Math.round(calculateCanvasHeight() * (1 - visualRatioRigthScrollBar))) {
        rightScrollBar.value = 50;
        rightScrollBar.disabled = true;
    }
    else {
        rightScrollBar.value = Math.max(Math.min(100 * y / maxValueRight, 100), 0);
        rightScrollBar.disabled = false;
    }

    if (Math.round(workAreaWidth * Canvas.getZoom()) <= Math.round(calculateCanvasWidth() * (1 - visualRatioBottomScrollBar))) {
        bottomScrollBar.value = 50;
        bottomScrollBar.disabled = true;
    }
    else {
        bottomScrollBar.value = Math.max(Math.min(100 * x / maxValueBottom, 100), 0);
        bottomScrollBar.disabled = false;
    }

    updateRightScrollBarThumb(Math.max(Math.min(calculateCanvasHeight() / workAreaHeigth / Canvas.getZoom() * 100, 100), 5) + "%")
    updateBottomScrollBarThumb(Math.max(Math.min(calculateCanvasWidth() / workAreaWidth / Canvas.getZoom() * 100, 100), 5) + "%")
}

//Obtiene el zoom minimo que se le puede aplicar al canvas
const calculateMinZoom = (preferredZoom = null) => {
    if (typeof preferredZoom === 'number' && isFinite(preferredZoom) && preferredZoom > 0) {
        minZoom = preferredZoom;         
        return minZoom;
    }

    if (Canvas.getObjects().length > 0) {
        const minZoomWidth = calculateCanvasWidth() / workAreaWidth;
        const minZoomHeight = calculateCanvasHeight() / workAreaHeigth;
        const rawMinZoom = Math.min(minZoomHeight, minZoomWidth);
        zoomP = Math.ceil(rawMinZoom * 10) / 10;
        return zoomP;
    } else {
        minZoom = 1;
    }
    return minZoom;
}

//Asigna zoom a las reglas y redimenciona bacground den canvas
const scaleRulers = (zoom) => {

    //Descomentar para aplicar imagen como background en canvas 
    //if (gridAuto) {
    //    gridSize = Math.max(Math.round(10 / Canvas.getZoom()), 1) / 2
    //    if (gridOn) {
    //        Canvas.setOverlayColor('', Canvas.renderAll.bind(Canvas));
    //        Canvas.setOverlayColor({
    //            source: getStaticFilePath('/images/grid.svg'), repeat: 'repeat', patternTransform: [gridSize, 0, 0, gridSize, 0, 0] // con 1 -> cada 10 píxeles. mínio 0.2 -> 2 píxeles
    //        }, Canvas.renderAll.bind(Canvas));
    //    }
    //}

    if (activeRulers)
        rulers.api.setScale(zoom)
}

//Función para no salirse del lienzo sea cual sea el zoom
const reviseZoom = () => {
    if (!isPanModeEnabled) {
        if (Canvas.getObjects().length > 0) {
            let fixedX = Canvas.vptCoords.tl.x;
            let fixedY = Canvas.vptCoords.tl.y;

            if (Canvas.vptCoords.tl.x + calculateCanvasWidth() / Canvas.getZoom() > workAreaWidth)
                fixedX = workAreaWidth - calculateCanvasWidth() / Canvas.getZoom();

            if (Canvas.vptCoords.tl.y + calculateCanvasHeight() / Canvas.getZoom() > workAreaHeigth)
                fixedY = workAreaHeigth - calculateCanvasHeight() / Canvas.getZoom();

            Canvas.absolutePan({
                x: Math.floor(Math.max(fixedX, 0) * Canvas.getZoom()),
                y: Math.floor(Math.max(fixedY, 0) * Canvas.getZoom())
            });
        }
    }
    // Siempre actualiza UI
    updateZoomDisplay();
    enableDisableZoomButtons();
};

const SetZoom = () => {
    Canvas.setViewportTransform([zoomP, 0, 0, zoomP, 0, 0]);
    scaleRulers(Canvas.getZoom());
    positionRulers(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
    updateScrollbarsValues(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);

    updateZoomDisplay();
    enableDisableZoomButtons();
    Canvas.requestRenderAll();        // adegura de repintar antes   
}

const ZoomIn = () => {
    const zoomInBtn = document.getElementById('zoomInBtn');
    const zoomOutBtn = document.getElementById('zoomOutBtn');

    let originalZoom = Canvas.getZoom();
    let newZoom = Math.min(originalZoom + 0.10, maxZoom); // Controls the zoom increment when clicking on the zoom button
    newZoom = Math.round(newZoom * 100) / 100; // Round to 2 decimal places to avoid accuracy problems.

    if (newZoom !== originalZoom) {
        Canvas.setZoom(newZoom);
        /*reviseZoom();*/ // Check visual status related to zoom
        scaleRulers(newZoom); // Scale the rules according to the new zoom
        Canvas.requestRenderAll(); // Render the entire canvas again
        updateScrollbarsValues(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
        updateZoomDisplay();
    }

    // Enable or disable buttons according to new zoom state
    zoomInBtn.disabled = newZoom >= maxZoom;
    zoomOutBtn.disabled = newZoom <= minZoom;
};

const ZoomOut = () => {
    const zoomInBtn = document.getElementById('zoomInBtn');
    const zoomOutBtn = document.getElementById('zoomOutBtn');

    let originalZoom = Canvas.getZoom();
    let newZoom = originalZoom - 0.10;

    if (newZoom < 0.01) {
        newZoom = 0.01;
    }

    newZoom = Math.round(newZoom * 100) / 100;
    if (newZoom !== originalZoom) {
        Canvas.setZoom(newZoom);
       /* reviseZoom();*/
        scaleRulers(newZoom);
        Canvas.requestRenderAll();
        updateScrollbarsValues(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);
        updateZoomDisplay();
    }

    zoomInBtn.disabled = newZoom >= maxZoom;
    zoomOutBtn.disabled = newZoom <= 0.01;
};


const zoomTo100 = () => {
    const targetZoom = 1;
    const center = new fabric.Point(Canvas.getWidth() / 2, Canvas.getHeight() / 2);
    Canvas.zoomToPoint(center, targetZoom);

    const vpt = Canvas.viewportTransform;
    const z = Canvas.getZoom();
    const tlWorldX = -vpt[4] / z;
    const tlWorldY = -vpt[5] / z;
    scaleRulers(z);
    updateScrollbarsValues(tlWorldX, tlWorldY);
    updateZoomDisplay();

    Canvas.requestRenderAll();
};

const zoomToSelectionChange = (type) => {
    currentMode = '';
    SelectAndEventElement(true);
    ResetDrawObject();
    if (type == 0) {
        SetZoom();
        $('#textSelectZooom').text(L("Zoom to fit"));
    } else if (type == 1) {
        selectActionAddCanvas('zoomToSelection');
        $('#textSelectZooom').text(L("Zoom selection"));
    } else if (type == 100) {
        $('#textSelectZooom').text(L("Zoom to 100%"));
        zoomTo100();
    }
}

function SetInitialZoom() {

    const vpt = getRememberedView();
    const lastZoom = vpt ? vpt[0] : null; 

    if (vpt) {

        calculateMinZoom(lastZoom);
        const scale = vpt[0]; 
        const clamped = Math.max(scale, minZoom);
        vpt[0] = clamped; 
        vpt[3] = clamped; 

        Canvas.setViewportTransform(vpt);
        syncRulersWithCanvas();
    } else {

        calculateMinZoom(lastZoom);
        Canvas.setViewportTransform([zoomP, 0, 0, zoomP, 0, 0]);
        syncRulersWithCanvas();
    }

    scaleRulers(Canvas.getZoom());
    const bounds = Canvas.vptCoords || Canvas.calcViewportBoundaries();
    const tl = (bounds && bounds.tl) ? bounds.tl : { x: 0, y: 0 };
    updateScrollbarsValues(tl.x, tl.y);

    Canvas.requestRenderAll();
}

//============================================//

//Actualiza el valor de la posición actual del canvas
const positionRulers = (posH, posV) => {
    rulers.api.setPos(posH, posV);

}

//Pasa el cursor a su estado por default
const clearCursor = () => {
    Canvas.defaultCursor = "auto";
};

window.SetSizeWorkArea = (model) => {
    if (model != null) {
        modelLayoutDto = model;
        workAreaWidth = model.width;
        workAreaHeigth = model.height;
    }
}

//Si el canvas no se comporta como se espera después de cambiar de pestaña, podrías reinicializarlo forzando una nueva instancia de fabric.Canvas.
window.reinitializeCanvas = (isReset, isResetChanges) => {
    inDesigner = true;
    isPanModeEnabled = false;
    isPanning = false;
    if (isResetChanges) {
        ResetCopyPasteObject();
        copyNameTracker = {
            area: {},
            process: {},
            station: {},
            steps: {},
            //equipment: {}
        };
    }


    if (Canvas != null) {
        const ctx = Canvas.getContext('2d');
        ctx.clearRect(0, 0, calculateCanvasWidth(), calculateCanvasHeight());
        fabric.Object.prototype.objectCaching = false;
        Canvas.enableRetinaScaling = false;
    }

    return new Promise((resolve, reject) => {

        const attemptInitialization = (remainingRetries) => {
            const canvasElement = document.getElementById(idCanvas);
            if (canvasElement) {
                InitCanvas(isReset);
                if (activeMouseWheel)
                    enableMouseZoom();

                ActivateActionsObjectAggregation();
                console.log(`Canvas con ID '${idCanvas}' inicializado.`);
                resolve();
            } else if (remainingRetries > 0) {
                console.warn(`Canvas con ID '${idCanvas}' no encontrado. Reintentando...`);
                setTimeout(() => attemptInitialization(remainingRetries - 1), 100);
            } else {
                console.error(`Canvas con ID '${idCanvas}' no encontrado después de varios intentos.`);
                reject(`Canvas con ID '${idCanvas}' no encontrado.`);
            }
        };

        attemptInitialization(10);
    });
};

window.clearCanvas = function () {
    // Initializes the elements contained in the canvas.
    if (Canvas) {
        try {
            // Verifies if Canvas is a valid instance of Fabric.js
            if (Canvas instanceof fabric.Canvas) {
                // Clears all objects on the canvas
                Canvas.clear();

                // Try to free up resources if possible
                if (typeof Canvas.dispose === 'function') {
                    Canvas.dispose();
                }
            }
        } catch (error) {
            console.error("Error when cleaning the canvas with dispose:", error);
        } finally {
            // Removes the reference to the Canvas object
            Canvas = null;
        }

        // Cleans the visual content of the HTML canvas
        const canvasElement = document.getElementById(idCanvas);
        if (canvasElement) {
            const ctx = canvasElement.getContext('2d');
            ctx.clearRect(0, 0, canvasElement.width, canvasElement.height);
        }
    }

    // Cleans the layout model
    modelLayoutDto = null;

    console.log("Canvas and model cleaned correctly.");
};

//Inicializa el canvas con las propiedades de arranque
function InitCanvas(isReset) {
    const ribbon = document.getElementById("ribbon-menu");
    ribbon.width = calculateCanvasWidth() + "px";

    const container = document.getElementById(idCanvas);
    container.style.width = calculateCanvasWidth() + "px";
    container.style.height = calculateCanvasHeight() + "px";
    initialPositionFloatMenu(idCanvas);

    Canvas = new fabric.Canvas(idCanvas, {
        width: container.offsetWidth,
        height: container.offsetHeight,
        selection: true,
        imageSmoothingEnabled: false,
        fireMiddleClick: true,
        fireRightClick: true,
        stopContextMenu: true,
        sides: ['top', 'left'],
        backgroundColor: '#BFBFBF'
    });

    if (!isReset) { // Skip building scrollbars in case of a canvas reset
        if (activeRulers) {

            rulers = new ruler({
                container: document.getElementById("rulerContainer"),
                rulerHeight: 20, // thickness of ruler
                fontFamily: 'arial',// font for points
                fontSize: '7px',
                strokeStyle: 'black',
                lineWidth: 1,
                enableMouseTracking: false,
                enableToolTip: false,
                sides: ['top', 'left'],
                rulerLengthHor: calculateCanvasWidth(),
                rulerLengthVer: calculateCanvasHeight(),
            });

            rulers.api.setScale(1);
            rulers.api.toggleRulerVisibility(true);
        }

        // Create the right scroll to move vertically inside the canvas
        rightScrollBar = createRightScrollBar(
            document.getElementById("rightScrollBarContainer"),
            calculateCanvasHeight(),
            useScrollBarRight
        );

        rightScrollBar.value = 50;
        rightScrollBar.disabled = true;


        // Create the bottom scroll to move horizontally inside the canvas
        bottomScrollBar = createBottomScrollBar(
            document.getElementById("bottomScrollBarContainer"),
            useScrollBarBottom
        );

        bottomScrollBar.value = 50;
        bottomScrollBar.disabled = true;
    }
    StartWorkArea();
}

//La función entra en caso de que la pantalla cambie de tamaño, toma los nuevos valores de la pantalla
function canvasResize() {
    const ribbon = document.getElementById("ribbon-menu");
    ribbon.width = calculateCanvasWidth() + "px";
    const container = document.getElementById(Canvas.getElement().id);
    container.style.width = calculateCanvasWidth() + "px";
    container.style.height = calculateCanvasHeight() + "px";
    Canvas.setDimensions({ width: container.offsetWidth, height: container.offsetHeight });

    calculateMinZoom();
    if (Canvas.getZoom() < minZoom)
        Canvas.setZoom(minZoom);

    if (activeRulers) {
        rulers.api.destroy();
        rulers = new ruler({
            container: document.getElementById("rulerContainer"),
            rulerHeight: 20, // thickness of ruler
            fontFamily: 'arial',// font for points
            fontSize: '7px',
            strokeStyle: 'black',
            lineWidth: 1,
            enableMouseTracking: false,
            enableToolTip: false,
            sides: ['top', 'left'],
            rulerLengthHor: calculateCanvasWidth(),
            rulerLengthVer: calculateCanvasHeight()
        });
        scaleRulers(Canvas.getZoom())
        rulers.api.toggleRulerVisibility(true);
    }

    reviseZoom();
    positionRulers(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);

    // Resize right scrollbar when cavas resize
    resizeRightScrollBar(calculateCanvasHeight());
    // Update scrollbars position values when viewport changes
    updateScrollbarsValues(Canvas.vptCoords.tl.x, Canvas.vptCoords.tl.y);


}

function resizeCanvasAndFloatingMenu() {
    if (inDesigner) {
        canvasResize();
        initialPositionFloatMenu(idCanvas);
    }
}

window.onresize = resizeCanvasAndFloatingMenu;

let LAST_VIEW_VPT = null;
function rememberView() {
    if (Canvas?.viewportTransform) {
        LAST_VIEW_VPT = Canvas.viewportTransform.slice();
        localStorage.setItem(`${idCanvas}:lastVPT`, JSON.stringify(LAST_VIEW_VPT));
    }
}
function getRememberedView() {

    if (LAST_VIEW_VPT) return LAST_VIEW_VPT;
    try {
        const raw = localStorage.getItem(`${idCanvas}:lastVPT`);
        if (!raw) return null;
        const v = JSON.parse(raw);
        if (Array.isArray(v) && v.length === 6) return v;
    } catch { }
    return null;
}
function syncRulersWithCanvas() {
    if (!Canvas || !Canvas.viewportTransform) return;

    const vpt = Canvas.viewportTransform;
    const z = Canvas.getZoom ? Canvas.getZoom() : vpt[0] || 1;

    if (rulers?.api?.setScale) rulers.api.setScale(z);
    const worldTLx = -vpt[4] / z;  
    const worldTLy = -vpt[5] / z;  
    positionRulers(worldTLx, worldTLy);

}
function clearRememberedView() {
    try {
        localStorage.removeItem(`${idCanvas}:lastVPT`);
    } catch { }
    LAST_VIEW_VPT = null;
}

window.addEventListener('pagehide', clearRememberedView);
function debounce(fn, wait = 120) {
    let t;
    return (...args) => {
        clearTimeout(t);
        t = setTimeout(() => fn.apply(null, args), wait);
    };
}

// Funcion para detectar tamaño de ventana y mostrar menu
function ensureMenuVisible(el, margin = 70) {
    if (!el) return;

    const vw = window.innerWidth;
    const vh = window.innerHeight;
    const w = el.offsetWidth || 0;
    const h = el.offsetHeight || 0;


    let top = parseFloat(el.style.top) || 0;
    let left = parseFloat(el.style.left) || 0;


    const maxLeft = Math.max(margin, vw - w - margin);
    const maxTop = Math.max(margin, vh - h - margin);

    left = Math.min(Math.max(margin, left), maxLeft);
    top = Math.min(Math.max(margin, top), maxTop);

    el.style.left = `${left}px`;
    el.style.top = `${top}px`;


    localStorage.setItem(el.id + "-left", el.style.left);
    localStorage.setItem(el.id + "-top", el.style.top);
}