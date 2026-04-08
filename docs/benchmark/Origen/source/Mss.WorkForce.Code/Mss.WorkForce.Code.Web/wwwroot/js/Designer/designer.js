window.floatMenuActions = () => {

    /* -------------Float Menu Move Actions------------*/
    const container = document.getElementById("mlx-float-menu-id"),
        circle = document.getElementById("button-move"),
        body = document.getElementById("canvas");

    let isDragging = false,
        offsetX = 0,
        offsetY = 0;

    // Start cursor movement
    circle.addEventListener("mousedown", (event) => {
        isDragging = true;

        // Calculate initial position
        const rect = container.getBoundingClientRect();
        offsetX = event.clientX - rect.left;
        offsetY = event.clientY - rect.top;

        // Prevent Default actions
        event.preventDefault();
    });

    // Stop cursor movement
    document.addEventListener("mouseup", () => {
        /*isDragging = false;*/
        if (isDragging) {

            localStorage.setItem(container.id + "-top", container.style.top);
            localStorage.setItem(container.id + "-left", container.style.left);
        }

        isDragging = false;
    });

    // Movement cursor event to FloatingMenu
    document.addEventListener("mousemove", (event) => {
        if (!isDragging) return;

        // Calculate new position
        let newLeft = event.clientX - offsetX,
            newTop = event.clientY - offsetY;

        // Limits of container body
        const bodyRect = body.getBoundingClientRect(),
            containerWidth = container.offsetWidth,
            containerHeight = container.offsetHeight;

        calculatePositionFloatMenu(container, newLeft, newTop, bodyRect, containerWidth, containerHeight);
    });

    const savedTop = localStorage.getItem(container.id + "-top");
    const savedLeft = localStorage.getItem(container.id + "-left");

    if (savedTop && savedLeft) {
        container.style.top = savedTop;
        container.style.left = savedLeft;
    }
    /* ------------------------------*/

    // Scroll event, apply in FloatingMenu
    document.getElementById('backgroundPage').addEventListener('scroll', () => {
        //Calculate new position
        const containerRect = container.getBoundingClientRect(),
            bodyRect = body.getBoundingClientRect();

        const containerWidth = container.offsetWidth,
            containerHeight = container.offsetHeight;

        let newLeft = containerRect.left,
            newTop = containerRect.top;

        calculatePositionFloatMenu(container, newLeft, newTop, bodyRect, containerWidth, containerHeight);
    });
};

function makeDivDraggable(div) {
    let isDragging = false;
    let startX, startY;
    let offsetX, offsetY;

    div.addEventListener('mousedown', function (e) {
        startX = e.clientX;
        startY = e.clientY;

        offsetX = e.clientX - div.offsetLeft;
        offsetY = e.clientY - div.offsetTop;

        function onMouseMove(e) {
            const dx = e.clientX - startX;
            const dy = e.clientY - startY;

            if (!isDragging && (Math.abs(dx) > 3 || Math.abs(dy) > 3)) {
                isDragging = true;
            }

            if (isDragging) {
                const newLeft = e.clientX - offsetX;
                const newTop = e.clientY - offsetY;


                const canvasWrapper = document.getElementById('canvasWrapper');
                const wrapperRect = canvasWrapper.getBoundingClientRect();
                const divRect = div.getBoundingClientRect();


                const minLeft = wrapperRect.left;
                const maxLeft = wrapperRect.right - div.offsetWidth;

                const minTop = wrapperRect.top;
                const maxTop = wrapperRect.bottom - div.offsetHeight;


                if (newLeft >= minLeft && newLeft <= maxLeft) {
                    div.style.left = newLeft + 'px';
                }

                if (newTop >= minTop && newTop <= maxTop) {
                    div.style.top = newTop + 'px';
                }
            }
        }

        function onMouseUp(e) {
            document.removeEventListener('mousemove', onMouseMove);
            document.removeEventListener('mouseup', onMouseUp);

            if (isDragging) {
                div.dataset.dragged = 'true';


                localStorage.setItem(div.id + "-top", div.style.top);//conservar posicion
                localStorage.setItem(div.id + "-left", div.style.left);
            } else {
                div.dataset.dragged = 'false';
            }

            isDragging = false;
            div.style.transition = '';
        }

        document.addEventListener('mousemove', onMouseMove);
        document.addEventListener('mouseup', onMouseUp);
    });
}


/*Calculate the position to FloatMenu with Canvas container*/
function calculatePositionFloatMenu(container, newLeft, newTop, bodyRect, containerWidth, containerHeight) {

    /*Tolerable minimum and maximum values*/
    const minLeft = bodyRect.left,
        maxLeft = bodyRect.right - containerWidth,
        minTop = bodyRect.top,
        maxTop = bodyRect.bottom - containerHeight;

    // Rule to size in body container
    if (newLeft < minLeft) newLeft = minLeft;
    if (newLeft > maxLeft) newLeft = maxLeft;
    if (newTop < minTop) newTop = minTop;
    if (newTop > maxTop) newTop = maxTop;

    var leftPercentage = newLeft,
        topPercentage = newTop;

    leftPercentage = leftPercentage >= 0 ? leftPercentage : 2; /*If result is lower or equal to 0, default asignate 2*/

    // Adjust position to body container
    container.style.left = `${leftPercentage}px`;
    container.style.top = `${topPercentage}px`;
}

function initialPositionFloatMenu(idCanvas) {
    const menuFloat = document.getElementById("mlx-float-menu-id"),
    canvas = document.getElementById(idCanvas);
    if (menuFloat && canvas) {
        const dimensionCanvas = canvas.getBoundingClientRect();

        if (menuFloat.attributes.hidden) { /*Validate load or reload in view*/
            menuFloat.attributes.removeNamedItem('hidden'); /*Show FloatingMenu*/
        }
        var menuFloatTotalTop = dimensionCanvas.top + 10, /*Add top spacing between border and floating menu*/
            menuFloatTotalLeft = dimensionCanvas.left + dimensionCanvas.width - menuFloat.offsetWidth - 10; /*Add right spacing between border and floating menu*/

        menuFloat.style.top = `${menuFloatTotalTop}px`;
        menuFloat.style.left = `${menuFloatTotalLeft}px`;
    }
};

/*------------------SideBar Tab Action switch container active-------------------*/
function showTab(tabName) {
    // Hidden elements
    document.querySelectorAll('.mlx-right-sideBar-body-content-width-tab').forEach(bodyContent => {
        bodyContent.style.display = 'none';
    });

    // Remove 'activate' class
    document.querySelectorAll('.mlx-right-sideBar-header-tab').forEach(headerTab => {
        headerTab.classList.remove('active');
    });

    // Mostrar el contenido correspondiente
    document.getElementById(tabName).style.display = 'flex';

    // Agregar la clase 'active' a la pestaña seleccionada
    event.target.classList.add('active');
}
/*---------------------------------------------------------------------*/

/*---------------------------- BeforeUnload Event ----------------------------*/
function registerBeforeUnload() {
    window.addEventListener("beforeunload", onBeforeUnload);
}

function unregisterBeforeUnload() {
    window.removeEventListener("beforeunload", onBeforeUnload);
}

function onBeforeUnload(e) {
    const hasChanges = checkPendingChanges();

    if (!hasChanges) return;

    e.preventDefault();
    e.returnValue = "";
    return "";
}
/*----------------------------------------------------------------------------*/