const createRightScrollBar = (rightScrollBarContainer, length, func) => {

	let scrollBar = createScrollBar({
		scrollBarId: "rightScrollBar",
		scrollBarMinValue: 0,
		scrollBarDefaultValue: 0,
		scrollBarWidth: length,
		scrollBarStep: 5,
		callbackFunc: () => {
			func(scrollBar.value);
		}
	});

	scrollBar.className = "form-range";
	scrollBar.classList.add("right-scroll-bar");
	scrollBar.style.transform = "rotate(90deg) translateY(" + length / 2 + "px) translateX(50%)";

	rightScrollBarContainer.appendChild(scrollBar);
	rightScrollBarContainer.style.padding = "none";

	return scrollBar;
}

const createBottomScrollBar = (bottomScrollBarContainer, func) => {

	let scrollBar = createScrollBar({
		scrollBarId: "bottomScrollBar",
		scrollBarMinValue: 0,
		scrollBarDefaultValue: 0,
		scrollBarStep: 5,
		callbackFunc: () => {
			func(scrollBar.value);
		}
	});

	scrollBar.className = "form-range";
	scrollBar.classList.add("bottom-scroll-bar");

	bottomScrollBarContainer.appendChild(scrollBar);
	rightScrollBarContainer.style.padding = "none";

	return scrollBar;
}

const createScrollBar = ({ scrollBarId, scrollBarMinValue, scrollBarDefaultValue, scrollBarMaxValue, scrollBarStep, scrollBarWidth, scrollBarHeight, callbackFunc } = {}) => {

	var input = document.createElement("input");
	input.type = "range";

	if (scrollBarMinValue !== undefined) {
		input.min = scrollBarMinValue + "";

	}

	if (scrollBarMaxValue !== undefined) {
		input.max = scrollBarMaxValue + "";

	}

	if (scrollBarDefaultValue !== undefined) {
		input.defaultValue = scrollBarDefaultValue + "";

	}

	if (scrollBarStep !== undefined) {
		input.step = scrollBarStep +  "";
	}

	if (callbackFunc !== undefined) {
		input.addEventListener("input", callbackFunc);
	}

	if (scrollBarId !== undefined) {
		input.id = scrollBarId + "";
	}
	
	if (scrollBarWidth !== undefined) {
		input.style.width = scrollBarWidth + "px";
	}

	if (scrollBarHeight !== undefined) {
		input.style.height = scrollBarHeight + "";
	}

	return input;
}


const updateScrollBarProperty = (property, value) => {
	document.documentElement.style.setProperty(property, value);
}

const updateRightScrollBarThumb = (value) => {
	updateScrollBarProperty("--scroll-bar-right-width", value);
}

const updateBottomScrollBarThumb = (value) => {
	updateScrollBarProperty("--scroll-bar-bottom-width", value);
}

const resizeRightScrollBar = (canvasHeight) => {
	var scrollBarInput = document.getElementById("rightScrollBar");
	scrollBarInput.style.width = canvasHeight + "px";
	scrollBarInput.style.transform = "rotate(90deg) translateY(" + canvasHeight / 2 + "px) translateX(50%)";
}

const calculateRightScrollBarMaxValue = (mapAreaHeight, canvasHeight, zoomLevel, visualRatioRigthScrollBar) => {
	return mapAreaHeight - canvasHeight / zoomLevel * (1 - visualRatioRigthScrollBar);
};

const calculateBottomScrollBarMaxValue = (mapAreaWidth, canvasWidth, zoomLevel, visualRatioBottomScrollBar) => {
	return mapAreaWidth - canvasWidth / zoomLevel * (1 - visualRatioBottomScrollBar);
};

//calculateBottomScrollBarMaxValue(drawingAreaWidth, zoomLevel, visualRatio);
//let maxValueRight = canvas.getObjects()[0].height - calculateCanvasHeight() / canvas.getZoom() * (1 - visualRatioRigthScrollBar);
//let maxValueBottom = canvas.getObjects()[0].width - calculateCanvasWidth() / canvas.getZoom() * (1 - visualRatioBottomScrollBar);