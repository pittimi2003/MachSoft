"use strict";
var ruler = function (options) {
  this.api = this.builder();
  this.api.constructRulers.call(this, options);
};

ruler.prototype.builder = function () {
  var VERTICAL = 1,
    HORIZONTAL = 2;

  var options,
    rulerz = {},
    theRulerDOM = document.createElement('div'),
    corners = [],
    defaultOptions = {
      rulerHeight: 15,
      fontFamily: 'arial',
      fontSize: '8px',
      strokeStyle: 'gray',
      sides: ['top', 'left'],
      cornerSides: ['TL'],
      lineWidth: 1,
      rulerLengthHor: 1000,
      rulerLengthVer: 1000

    };



  var constructRuler = function (container, alignment) {
    var canvas,
      dimension = alignment === 'left' || alignment === 'right' ? VERTICAL : HORIZONTAL,
      rulerStyle = dimension === VERTICAL ? 'rul_ruler_Vertical' : 'rul_ruler_Horizontal',
      element = document.createElement('canvas');


    ruler.prototype.utils.addClasss(element, ['rul_ruler', rulerStyle, 'rul_align_' + alignment]);
    canvas = container.appendChild(element);
    rulerz[alignment] = ruler.prototype.rulerConstructor(canvas, options, dimension);
    switch (alignment) {
      case 'top':
        rulerz[alignment].drawRulerTop(options.rulerLengthHor || 0, options.rulerHeight, alignment);
        break;
      case 'left':
        rulerz[alignment].drawRulerLeft(options.rulerLengthVer || 0, options.rulerHeight, alignment);
        break;
    }
    rulerz[alignment].canvas.style.left = 0;
    rulerz[alignment].canvas.style.top = 0;
  };

  var constructCorner = (function () {
    function cornerDraw(container, side) {
      var corner = document.createElement('div'),
        cornerStyle = 'rul_corner' + side.toUpperCase();

      ruler.prototype.utils.addClasss(corner, ['rul_corner', cornerStyle]);
      corner.style.width = ruler.prototype.utils.pixelize(options.rulerHeight + 1);
      corner.style.height = ruler.prototype.utils.pixelize(options.rulerHeight);
      return container.appendChild(corner);

    }

    function mousedown(e) {
      e.stopPropagation();
    }

    return function (container, cornerSides) {
      cornerSides.forEach(function (side) {
        var corner = cornerDraw(container, side);
        corner.addEventListener('mousedown', mousedown);
        corner.destroy = function () {
          corner.removeEventListener('mousedown', mousedown);
          corner.parentNode.removeChild(corner);
        };

        corners.push(corner);
      })
    }

  })();

  var constructRulers = function (curOptions) {
    theRulerDOM = ruler.prototype.utils.addClasss(theRulerDOM, 'rul_wrapper');
    options = ruler.prototype.utils.extend(defaultOptions, curOptions);
    theRulerDOM = options.container.insertBefore(theRulerDOM, options.container.firstChild);
    options.sides.forEach(function (side) {
      constructRuler(theRulerDOM, side);
    });
    constructCorner(theRulerDOM, options.cornerSides);


  };

  var forEachRuler = function (cb) {
    var index = 0;
    for (var rul in rulerz) {
      if (rulerz.hasOwnProperty(rul)) {
        cb(rulerz[rul], index++);
      }
    }
  };


  var setScale = function (newScale) {
    forEachRuler(function (rul) {
      rul.context.clearRect(0, 0, rul.canvas.width, rul.canvas.height);
      rul.context.beginPath();
      rul.setScale(newScale);
      rul.context.stroke();
    });

  };

  var setPos = function (newPosH, newPosV) {
    forEachRuler(function (rul) {
      rul.context.clearRect(0, 0, rul.canvas.width, rul.canvas.height);
      rul.context.beginPath();
      rul.setPosH(newPosH);
      rul.setPosV(newPosV)
      rul.context.stroke();
    });

  };


  var toggleRulerVisibility = function (val) {
    var state = val ? 'block' : 'none';
    theRulerDOM.style.display = state;
  };


  var destroy = function () {
    forEachRuler(function (ruler) {
      ruler.destroy();
    });
    corners.forEach(function (corner) {
      corner.destroy();
    });
    theRulerDOM.parentNode.removeChild(theRulerDOM);
  };

  return {
    VERTICAL: VERTICAL,
    HORIZONTAL: HORIZONTAL,
    setScale: setScale,
    setPos: setPos,
    constructRulers: constructRulers,
    toggleRulerVisibility: toggleRulerVisibility,
    destroy: destroy
  }
};

ruler.prototype.rulerConstructor = function (_canvas, options, rulDimension) {

  var canvas = _canvas,
    context = canvas.getContext('2d'),
    rulThickness = 0,
    rulLength = 0,
    rulScale = 1,
    dimension = rulDimension || 2,
    orgPosH = 0,
    orgPosV = 0;

  var getLength = function () {
    return rulLength;
  };

  var getThickness = function () {
    return rulThickness;
  };

  var getScale = function () {
    return rulScale;
  };

  var getPosH = function () {
    return orgPosH;
  };

  var getPosV = function () {
    return orgPosV;
  };

  var setScale = function (newScale) {
    rulScale = parseFloat(newScale);
    drawPointsTop();
    drawPointsLeft();
    return rulScale;
  };

  var setPosH = function (newPosH) {
    orgPosH = parseFloat(newPosH);
    drawPointsTop();
    return orgPosH;
  };

  var setPosV = function ( newPosV) {
    orgPosV = parseFloat(newPosV);
    drawPointsLeft();
    return orgPosV;
  };


  var drawRulerTop = function (_rulerLength, _rulerThickness, _rulerScale) {
    canvas.width = _rulerLength + _rulerThickness;
    rulLength = _rulerLength;
    rulThickness = canvas.height = _rulerThickness;
    rulScale = _rulerScale || rulScale;
    context.strokeStyle = options.strokeStyle;
    context.font = options.fontSize + ' ' + options.fontFamily;
    context.lineWidth = options.lineWidth;
    context.beginPath();
    drawPointsTop(_rulerThickness);
    context.stroke();
  };

  var drawRulerLeft = function (_rulerLength, _rulerThickness, _rulerScale) {
    canvas.height = _rulerLength + _rulerThickness;
    rulLength = _rulerLength;
    rulThickness = canvas.width = _rulerThickness;
    rulScale = _rulerScale || rulScale;
    context.strokeStyle = options.strokeStyle;
    context.font = options.fontSize + ' ' + options.fontFamily;
    context.lineWidth = options.lineWidth;
    context.beginPath();
    drawPointsLeft(_rulerThickness);
    context.stroke();
  };



  var drawPointsTop = function (_rullThickness) {
    var pointLength = 0,
      label = '',
      delta = 0,
      draw = false,
      lineLengthMax = 0,
      lineLengthMin = rulThickness / 1.5,
      lastTick = -1,
      smallNumber = Math.max(Math.round(10 / rulScale), 1),
      bigNumber = 5 * smallNumber,
      scaleNumber = Math.max(1, rulScale);

    for (var pos = orgPosH - rulThickness; pos <= (rulLength * scaleNumber + orgPosH); pos += 1) {
      delta = (pos - orgPosH) / rulScale / scaleNumber + orgPosH;
      draw = false;
      label = '';

      if (Math.abs(delta % bigNumber) <= 1 / rulScale) {
        pointLength = lineLengthMax;
        label = Math.round(delta / bigNumber) * bigNumber;
        if (lastTick != Math.round(delta / bigNumber) * bigNumber) {
          draw = true;
          lastTick = Math.round(delta / bigNumber) * bigNumber;
        }
      }
      else if (Math.abs(delta % smallNumber) <= 1 / rulScale) {
        pointLength = lineLengthMin;
        if (lastTick != Math.round(delta / smallNumber) * smallNumber) {
          draw = true;
          lastTick = Math.round(delta / smallNumber) * smallNumber;
        }
      }
      if (draw) {
        context.moveTo((pos - orgPosH) / scaleNumber + rulThickness, rulThickness);
        context.lineWidth = 1;
        context.lineTo((pos - orgPosH) / scaleNumber + rulThickness, pointLength + 0.5);
        context.fillText(label, (pos - orgPosH) / scaleNumber + rulThickness + 1.5, (rulThickness) * 1 / 3);
      }
    }
  };

  var drawPointsLeft = function (_rullThickness) {
    var pointLength = 0,
      label = '',
      delta = 0,
      draw = false,
      lineLengthMax = 0,
      lineLengthMin = rulThickness / 1.5,
      lastTick = -1,
      smallNumber = Math.max(Math.round(10 / rulScale),1),
      bigNumber = 5 * smallNumber,
      scaleNumber = Math.max(1, rulScale);


    for (var pos = orgPosV - rulThickness; pos <= (rulLength * scaleNumber + orgPosV); pos += 1) {
      delta = (pos - orgPosV) / rulScale / scaleNumber + orgPosV;
      draw = false;
      label = '';

      if (Math.abs(delta % bigNumber) <= 1 / rulScale) {
        pointLength = lineLengthMax;
        label = Math.round(delta / bigNumber) * bigNumber;
        if (lastTick != Math.round(delta / bigNumber) * bigNumber) {
          draw = true;
          lastTick = Math.round(delta / bigNumber) * bigNumber;
        }
      }
      else if (Math.abs(delta % smallNumber) <= 1 / rulScale) {
        pointLength = lineLengthMin;
        if (lastTick != Math.round(delta/smallNumber) * smallNumber) {
          draw = true;
          lastTick = Math.round(delta / smallNumber) * smallNumber;
        }
      }
      if (draw) {
        context.moveTo(rulThickness, (pos - orgPosV) / scaleNumber + rulThickness);
        context.lineWidth = 1;
        context.lineTo(pointLength + 0.5, (pos - orgPosV) / scaleNumber + rulThickness);
        context.fillText(label, 0.5, (pos - orgPosV) / scaleNumber - 3 + rulThickness);
      }
    }
  };

  var destroy = function () {
    this.clearListeners && this.clearListeners();
  };

  return {
    getLength: getLength,
    getThickness: getThickness,
    getScale: getScale,
    getPosH: getPosH,
    getPosV: getPosV,
    setScale: setScale,
    setPosH: setPosH,
    setPosV: setPosV,
    dimension: dimension,
    orgPosH: orgPosH,
    rogPosV: orgPosV,
    canvas: canvas,
    context: context,
    drawRulerTop: drawRulerTop,
    drawRulerLeft: drawRulerLeft,
    drawPointsTop: drawPointsTop,
    drawPointsLeft: drawPointsLeft,
    destroy: destroy
  }
};

ruler.prototype.utils = {
  extend: function extend() {
    for (var i = 1; i < arguments.length; i++)
      for (var key in arguments[i])
        if (arguments[i].hasOwnProperty(key))
          arguments[0][key] = arguments[i][key];
    return arguments[0];
  },
  pixelize: function (val) {
    return val + 'px';
  },
  prependChild: function (container, element) {
    return container.insertBefore(element, container.firstChild);
  },
  addClasss: function (element, classNames) {
    if (!(classNames instanceof Array)) {
      classNames = [classNames];
    }

    classNames.forEach(function (name) {
      element.className += ' ' + name;
    });

    return element;

  },
  removeClasss: function (element, classNames) {
    var curCalsss = element.className;
    if (!(classNames instanceof Array)) {
      classNames = [classNames];
    }

    classNames.forEach(function (name) {
      curCalsss = curCalsss.replace(name, '');
    });
    element.className = curCalsss;
    return element;

  }
};