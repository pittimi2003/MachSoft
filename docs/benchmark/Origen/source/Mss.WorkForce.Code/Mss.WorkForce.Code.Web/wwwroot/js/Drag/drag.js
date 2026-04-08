//METODOS PARA VISTA SIMULATION LOGIC
window.initDrag = function (dotNetHelper) {
    window.dotNetHelperRightNotify = dotNetHelper;
    
    // Limpiar eventos anteriores de botones .drag-remove sin causar errores
    document.querySelectorAll(".drag-remove").forEach(button => {
        const newButton = button.cloneNode(true);
        if (button.parentNode) {
            button.parentNode.replaceChild(newButton, button);
        }
    });

    //Limpiar listeners duplicados en drag items
    document.querySelectorAll(".drag-item-selector, .priority-element").forEach(el => {
        const clone = el.cloneNode(true);
        el.replaceWith(clone);
    });

    const draggableItems = document.querySelectorAll(".drag-item-selector, .priority-element");
    const containers = [document.getElementById("leftContainer"), document.getElementById("rightContainer")];

    // Drag start
    draggableItems.forEach(el => {
        el.addEventListener("dragstart", (e) => {
            const realTarget = e.target.closest(".priority-element, .drag-item-selector");

            if (!realTarget) return;

            const type = realTarget.dataset.type || "unknown";
            e.dataTransfer.setData("text/plain", realTarget.id);
            e.dataTransfer.setData("drag-type", type);
        });
    });

    // Drop en contenedores generales
    containers.forEach(container => {
        container.addEventListener("dragover", (e) => e.preventDefault());

        container.addEventListener("drop", (e) => {
            e.preventDefault();
            const draggedId = e.dataTransfer.getData("text/plain");
            const draggedEl = document.getElementById(draggedId);
            if (!draggedEl) return;

            const currentContainer = draggedEl.closest("#leftContainer, #rightContainer");
            const type = draggedEl.dataset.type;

            if (type === "priority") {
                // Buscar grupo de prioridad
                let group = container.querySelector('.drag-item-group[data-group="priority"]');

                if (!group) {
                    var source = document.querySelector('#leftContainer .drag-item-group[data-group="priority"]');

                    if (!source)
                        source = document.querySelector('#rightContainer .drag-item-group[data-group="priority"]');

                    const idforDivGrouped = currentContainer.id === "rightContainer" ? "left-" + source.dataset.id : "right-" + source.dataset.id;

                    group = document.createElement("div");
                    group.classList.add("drag-item-group", "drag-item-selector");
                    group.setAttribute("data-group", "priority");
                    group.setAttribute("draggable", "true");
                    group.setAttribute("id", idforDivGrouped);

                    group.setAttribute("data-order", source.dataset.order);
                    group.setAttribute("data-id", source.dataset.id);
                    group.setAttribute("data-code", source.dataset.code);
                    group.setAttribute("data-type", source.dataset.type);

                    // Contenedor para ícono + título
                    const titleWrapper = document.createElement("div");
                    titleWrapper.classList.add("group-title");

                    const dragIcon = document.createElement("span");
                    dragIcon.classList.add("mlx-ico-drag", "drag-icon");

                    const title = document.createElement("span");
                    title.textContent = "Priority";

                    titleWrapper.appendChild(dragIcon);
                    titleWrapper.appendChild(title);

                    group.appendChild(titleWrapper);
                    container.appendChild(group);
                }

                insertByDataOrder(group, draggedEl);
                removeEmptyPriorityGroup(currentContainer);
                window.enableRightContainerSorting();

            } else if (type === "grouped") {

                const isPriorityGroup = draggedEl.dataset.group === "priority";

                if (isPriorityGroup) {
                    // Buscar si ya existe un grupo en el contenedor destino
                    let targetGroup = container.querySelector('.drag-item-group[data-group="priority"]');

                    if (!targetGroup) {
                        // No existe grupo, mover el grupo completo como antes
                        const afterElement = getDragAfterElementWithContainer(container, e.clientY);
                        if (afterElement == null) {
                            container.appendChild(draggedEl);
                        } else {
                            container.insertBefore(draggedEl, afterElement);
                        }
                    } else {
                        const afterElement = getDragAfterElementWithContainer(container, e.clientY);
                        if (currentContainer === container) {
                            if (afterElement == null) {
                                container.appendChild(draggedEl);
                            } else {
                                container.insertBefore(draggedEl, afterElement);
                            }

                        } else {
                            // Ya existe grupo en el destino, fusionar hijos
                            const priorityChildren = draggedEl.querySelectorAll(".priority-element");

                            priorityChildren.forEach(child => {
                                insertByDataOrder(targetGroup, child);
                            });

                            // Eliminar el grupo original (draggedEl) si quedó vacío
                            draggedEl.remove();
                        }

                        
                    }

                    removeEmptyPriorityGroup(currentContainer);
                    window.enableRightContainerSorting();
                } else {
                    // Reordenamiento normal (por ejemplo: otros grupos no-priority)
                    if (currentContainer !== container) return;

                    const afterElement = getDragAfterElementWithContainer(container, e.clientY);
                    if (afterElement == null) {
                        container.appendChild(draggedEl);
                    } else {
                        container.insertBefore(draggedEl, afterElement);
                    }

                    window.enableRightContainerSorting();
                }
            }
            else {
                const afterElement = getDragAfterElementWithContainer(container, e.clientY);
                if (afterElement == null) {
                    container.appendChild(draggedEl);
                } else {
                    container.insertBefore(draggedEl, afterElement);
                }
                window.enableRightContainerSorting();


            }

            //seccion para acomodar iconos mlx-ico-add-border, mlx-ico-minus-border y drags
            if (type === "grouped") {
                // Aplicar a todos los hijos .priority-element dentro del grupo
                var priorityChildren = draggedEl.querySelectorAll(".priority-element");

                if (priorityChildren.length === 0) {
                    let group = container.querySelector('.drag-item-group[data-group="priority"]');
                    priorityChildren = group.querySelectorAll(".priority-element");
                }

                priorityChildren.forEach(child => {
                    const iconSpan = child.querySelector(".drag-remove span");
                    const label = child.querySelector(".drag-label");
                    const existingDragIcon = child.querySelector(".mlx-ico-drag");

                    if (iconSpan) {
                        if (container.id === "rightContainer") {
                            iconSpan.classList.remove("mlx-ico-add-border");
                            iconSpan.classList.add("mlx-ico-minus-border");

                        } else {
                            iconSpan.classList.remove("mlx-ico-minus-border");
                            iconSpan.classList.add("mlx-ico-add-border");

                            const dragIcon = child.querySelector(".mlx-ico-drag");
                            if (dragIcon) {
                                dragIcon.remove();
                            }
                        }
                    }
                });

            } else {
                // Actualizar ícono según destino
                const iconSpan = draggedEl.querySelector(".drag-remove span");
                const label = draggedEl.querySelector(".drag-label");
                const existingDragIcon = draggedEl.querySelector(".mlx-ico-drag");

                if (iconSpan) {
                    if (container.id === "rightContainer") {
                        iconSpan.classList.remove("mlx-ico-add-border");
                        iconSpan.classList.add("mlx-ico-minus-border");

                        if (label && !existingDragIcon && type != "priority") {
                            const dragIcon = document.createElement("span");
                            dragIcon.classList.add("mlx-ico-drag", "drag-icon");
                            label.insertAdjacentElement("beforebegin", dragIcon);
                        }

                    } else {
                        iconSpan.classList.remove("mlx-ico-minus-border");
                        iconSpan.classList.add("mlx-ico-add-border");

                        const dragIcon = draggedEl.querySelector(".mlx-ico-drag");
                        if (dragIcon) {
                            dragIcon.remove();
                        }
                    }
                }
            }            

            if (window.dotNetHelperRightNotify) {
                window.dotNetHelperRightNotify.invokeMethodAsync("NotifyRightContainerChanged");
            }

        });
    });

    document.querySelectorAll(".drag-remove").forEach(button => {
        button.addEventListener("click", function (e) {
            const draggedEl = this.closest(".drag-item, .drag-item-priority");
            if (!draggedEl) return;

            const currentContainer = draggedEl.closest("#leftContainer, #rightContainer");
        
            const isInRight = currentContainer?.id === "rightContainer";
            const targetContainer = document.getElementById(isInRight ? "leftContainer" : "rightContainer");
            const type = draggedEl.dataset.type;

            const dragIcon = document.createElement("span");
            if (!isInRight) {
                dragIcon.classList.add("mlx-ico-drag", "drag-icon");
            }

            if (!targetContainer) return;

            // Si va al lado derecho
            if (type === "priority") {
                // Buscar o crear grupo
                let group = targetContainer.querySelector('.drag-item-group[data-group="priority"]');

                if (!group) {
                    const innerItemGrouped = currentContainer.querySelector('.drag-item-group[data-type="grouped"]');

                    const idforDivGrouped = targetContainer.id === "rightContainer" ? "right-" + innerItemGrouped.dataset.id : "left-" + innerItemGrouped.dataset.id;                    

                    group = document.createElement("div");
                    group.classList.add("drag-item-group", "drag-item-selector");
                    group.setAttribute("data-group", "priority");
                    group.setAttribute("draggable", "true");
                    group.setAttribute("id", idforDivGrouped);
                    group.setAttribute("data-type", innerItemGrouped.dataset.type);
                    group.setAttribute("data-code", innerItemGrouped.dataset.code);
                    group.setAttribute("data-id", innerItemGrouped.dataset.id);
                    group.setAttribute("data-order", innerItemGrouped.dataset.order);

                    const titleWrapper = document.createElement("div");
                    titleWrapper.classList.add("group-title");

                    const dragIcon = document.createElement("span");
                    if (!isInRight) {
                        dragIcon.classList.add("mlx-ico-drag", "drag-icon");
                    }

                    const title = document.createElement("span");
                    title.textContent = "Priority";

                    if (!isInRight) {
                        titleWrapper.appendChild(dragIcon);
                    }

                    titleWrapper.appendChild(title);
                    group.appendChild(titleWrapper);
                    targetContainer.appendChild(group);
                }

                insertByDataOrder(group, draggedEl);
                removeEmptyPriorityGroup(currentContainer);
                window.enableRightContainerSorting();


            } else {
                targetContainer.appendChild(draggedEl);
                window.enableRightContainerSorting();

            }

            // Actualizar ícono según destino
            const iconSpan = draggedEl.querySelector(".drag-remove span");
            const label = draggedEl.querySelector(".drag-label");
            const existingDragIcon = draggedEl.querySelector(".mlx-ico-drag");

            if (iconSpan) {
                if (targetContainer.id === "rightContainer") {
                    iconSpan.classList.remove("mlx-ico-add-border");
                    iconSpan.classList.add("mlx-ico-minus-border");

                    if (label && !existingDragIcon && type != "priority") {
                        const dragIcon = document.createElement("span");
                        dragIcon.classList.add("mlx-ico-drag", "drag-icon");
                        label.insertAdjacentElement("beforebegin", dragIcon);
                    }

                } else {
                    iconSpan.classList.remove("mlx-ico-minus-border");
                    iconSpan.classList.add("mlx-ico-add-border");

                    const dragIcon = draggedEl.querySelector(".mlx-ico-drag");
                    if (dragIcon) {
                        dragIcon.remove();
                    }
                }
            }

            if (window.dotNetHelperRightNotify) {
                window.dotNetHelperRightNotify.invokeMethodAsync("NotifyRightContainerChanged");
            }

        });
    });
};

//funcion para mantener el orden original en priority
function insertByDataOrder(group, draggedEl) {
    const newOrder = parseInt(draggedEl.dataset.order || "999", 10);

    // Evitar duplicados
    if (group.contains(draggedEl)) return;

    const items = Array.from(group.querySelectorAll(".drag-item-priority"));

    let inserted = false;
    for (let i = 0; i < items.length; i++) {
        const currentOrder = parseInt(items[i].dataset.order || "999", 10);
        if (newOrder < currentOrder) {
            group.insertBefore(draggedEl, items[i]);
            inserted = true;
            break;
        }
    }

    if (!inserted) {
        group.appendChild(draggedEl);
    }
}

function removeEmptyPriorityGroup(container) {
    const group = container.querySelector('.drag-item-group[data-group="priority"]');
    if (!group) return;

    const items = group.querySelectorAll('.drag-item-priority');
    if (items.length === 0) {
        group.remove();
    }
}


window.enableRightContainerSorting = function () {
    const rightContainer = document.getElementById("rightContainer");
    if (!rightContainer) return;

    let draggedElement = null;

    const sortableItems = rightContainer.querySelectorAll(".drag-item-selector");

    sortableItems.forEach(item => {
        if (item.classList.contains("priority-element")) return;

        item.setAttribute("draggable", "true");

        item.addEventListener("dragstart", (e) => {
            draggedElement = item;
            e.dataTransfer.effectAllowed = "move";
        });

        item.addEventListener("dragover", (e) => {
            e.preventDefault();
            e.dataTransfer.dropEffect = "move";
        });

        item.addEventListener("drop", (e) => {
            e.preventDefault();

            if (!draggedElement || draggedElement === item) return;

            const bounding = item.getBoundingClientRect();
            const offset = e.clientY - bounding.top;

            const parent = item.parentNode;

            if (offset < bounding.height / 2) {
                parent.insertBefore(draggedElement, item);
            } else {
                if (item.nextSibling) {
                    parent.insertBefore(draggedElement, item.nextSibling);
                } else {
                    parent.appendChild(draggedElement); // por si es el último
                }
            }

            draggedElement = null;
        });

        item.addEventListener("dragend", () => {
            draggedElement = null;
        });
    });
};


window.getPrioritiesState = function () {
    const result = {
        processItems: [],
        priorityItems: []
    };

    const groupedIds = new Set();

    // 1. Contenedor derecho
    const fieldItems = document.querySelectorAll('#rightContainer .drag-item[data-type="field"], #rightContainer .drag-item-group[data-type="grouped"]');
    fieldItems.forEach((el, index) => {

        const id = el.dataset.id;
        // Si es grouped, lo anotamos como ya agregado
        if (el.dataset.type === "grouped") {
            groupedIds.add(id);
        }

        result.processItems.push({
            id: el.dataset.id,
            code: el.dataset.code,
            priority: index,
            isActive: true
        });
    });

    // 2. Contenedor izquierdo
    const fieldItems2 = document.querySelectorAll('#leftContainer .drag-item[data-type="field"], #leftContainer .drag-item-group[data-type="grouped"]');
    fieldItems2.forEach((el, index) => {

        const id = el.dataset.id;
        if (el.dataset.type === "grouped" && groupedIds.has(id)) {
            return;
        }

        result.processItems.push({
            id: el.dataset.id,
            code: el.dataset.code,
            priority: index,
            isActive: false
        });
    });    

    // 3. Contenedor derecho Prioridades (order priorities dentro del grupo)
    const priorityGroup = document.querySelector('#rightContainer .drag-item-group[data-group="priority"]');
    if (priorityGroup) {
        const priorityItems = priorityGroup.querySelectorAll('.drag-item-priority');
        priorityItems.forEach((el, index) => {
            result.priorityItems.push({
                id: el.dataset.id,
                code: el.dataset.code,
                priority: parseInt(el.dataset.order || "0", 10),
                isActive: true
            });
        });
    }

    // 4. Contenedor izquierdo Prioridades (order priorities dentro del grupo)
    const priorityGroupLeft = document.querySelector('#leftContainer .drag-item-group[data-group="priority"]');
    if (priorityGroupLeft) {
        const priorityItems = priorityGroupLeft.querySelectorAll('.drag-item-priority');
        priorityItems.forEach((el, index) => {
            result.priorityItems.push({
                id: el.dataset.id,
                code: el.dataset.code,
                priority: parseInt(el.dataset.order || "0", 10),
                isActive: false
            });
        });
    }

    return result;
}

function getDragAfterElementWithContainer(container, y) {
    const draggableElements = [...container.querySelectorAll('.drag-item-selector:not(.dragging)')];

    return draggableElements.reduce((closest, child) => {
        const box = child.getBoundingClientRect();
        const offset = y - box.top - box.height / 2;
        if (offset < 0 && offset > closest.offset) {
            return { offset: offset, element: child };
        } else {
            return closest;
        }
    }, { offset: Number.NEGATIVE_INFINITY }).element;
}


//funciones que utiliza el componente MlxProgressBar que no usa mucho JS
window.getElementWidth = (el) => el.offsetWidth;
window.getElementLeft = (el) => el.getBoundingClientRect().left;


//funciones que utiliza el componente MlxProgressBarJS, JS puro para evitar delay
window.initProgressDragJS = function (trackId, min, max, dotnetRef) {
    const track = document.getElementById(trackId);
    if (!track) return;

    const thumb = track.querySelector(".progress-thumb");
    const fill = track.querySelector(".progress-fill");

    let isDragging = false;

    function getRelativePercent(clientX) {
        const rect = track.getBoundingClientRect();
        const x = clientX - rect.left;
        const percent = Math.max(0, Math.min(x / rect.width, 1));
        return percent;
    }

    function updateVisual(percent) {
        const value = Math.round(min + (max - min) * percent);
        fill.style.width = `${percent * 100}%`;
        thumb.style.left = `${percent * 100}%`;
        thumb.innerText = `${value}%`;
    }

    function onMouseMove(e) {
        if (!isDragging) return;
        const percent = getRelativePercent(e.clientX);
        updateVisual(percent);
    }

    function onMouseUp(e) {
        if (!isDragging) return;
        isDragging = false;
        const percent = getRelativePercent(e.clientX);
        const value = Math.round(min + (max - min) * percent);
        dotnetRef.invokeMethodAsync("UpdateValueFromJS", value);
        window.removeEventListener("mousemove", onMouseMove);
        window.removeEventListener("mouseup", onMouseUp);
    }

    function onMouseDown(e) {
        isDragging = true;
        window.addEventListener("mousemove", onMouseMove);
        window.addEventListener("mouseup", onMouseUp);
        const percent = getRelativePercent(e.clientX);
        updateVisual(percent);
    }

    track.addEventListener("mousedown", onMouseDown);
};