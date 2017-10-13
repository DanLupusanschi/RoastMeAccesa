
(function($, undefined) {
  var slides, currentIndex, $container, $fragmentLinks;

  var events = {
   
    beforeChange: 'slide.beforeChange',
    change: 'slide.change',
    beforeInitialize: 'slide.beforeInit',
    initialize: 'slide.init'
  };

  var options = {};
  var $document = $(document);
  var $window = $(window);
  var stopPropagation = function(event) {
    event.stopPropagation();
  };

  var updateContainerState = function() {
    var oldIndex = $container.data('onSlide');
    $container.removeClass(options.classes.onPrefix + oldIndex);
    $container.addClass(options.classes.onPrefix + currentIndex);
    $container.data('onSlide', currentIndex);
  };

  var updateChildCurrent = function() {
    var $oldCurrent = $('.' + options.classes.current);
    var $oldParents = $oldCurrent.parentsUntil(options.selectors.container);
    var $newCurrent = slides[currentIndex];
    var $newParents = $newCurrent.parentsUntil(options.selectors.container);
    $oldParents.removeClass(options.classes.childCurrent);
    $newParents.addClass(options.classes.childCurrent);
  };

  var removeOldSlideStates = function() {
    var $all = $();
    $.each(slides, function(i, el) {
      $all = $all.add(el);
    });
    $all.removeClass([
      options.classes.before,
      options.classes.previous,
      options.classes.current,
      options.classes.next,
      options.classes.after
    ].join(' '));
  };

  var addNewSlideStates = function() {
    slides[currentIndex].addClass(options.classes.current);
    if (currentIndex > 0) {
      slides[currentIndex-1].addClass(options.classes.previous);
    }
    if (currentIndex + 1 < slides.length) {
      slides[currentIndex+1].addClass(options.classes.next);
    }
    if (currentIndex > 1) {
      $.each(slides.slice(0, currentIndex - 1), function(i, $slide) {
        $slide.addClass(options.classes.before);
      });
    }
    if (currentIndex + 2 < slides.length) {
      $.each(slides.slice(currentIndex+2), function(i, $slide) {
        $slide.addClass(options.classes.after);
      });
    }
  };

  var updateStates = function() {
    updateContainerState();
    updateChildCurrent();
    removeOldSlideStates();
    addNewSlideStates();

  };

  var initSlidesArray = function(elements) {
    if ($.isArray(elements)) {
      $.each(elements, function(i, element) {
        slides.push($(element));
      });
    }
    else {
      $(elements).each(function(i, element) {
        slides.push($(element));
      });
    }
  };

  var bindKeyEvents = function() {
    var editables = [
      'input',
      'textarea',
      'select',
      'button',
      'meter',
      'progress',
      '[contentEditable]'
    ].join(', ');

    $document.unbind('keydown.slide').bind('keydown.slide', function(event) {
      var isNext = event.which === options.keys.next;
      var isPrev = event.which === options.keys.previous;
      isNext = isNext || $.inArray(event.which, options.keys.next) > -1;
      isPrev = isPrev || $.inArray(event.which, options.keys.previous) > -1;

      if (isNext) {
        methods.next();
        event.preventDefault();
      }
      else if (isPrev) {
        methods.prev();
        event.preventDefault();
      }
    });

    $document.undelegate(editables, 'keydown.slide', stopPropagation);
    $document.delegate(editables, 'keydown.slide', stopPropagation);
  };

  var bindTouchEvents = function() {
    var startTouch;
    var direction = options.touch.swipeDirection;
    var tolerance = options.touch.swipeTolerance;
    var listenToHorizontal = ({ both: true, horizontal: true })[direction];
    var listenToVertical = ({ both: true, vertical: true })[direction];

    $container.unbind('touchstart.slide');
    $container.bind('touchstart.slide', function(event) {
      if (!startTouch) {
        startTouch = $.extend({}, event.originalEvent.targetTouches[0]);
      }
    });

    $container.unbind('touchmove.slide');
    $container.bind('touchmove.slide', function(event) {
      $.each(event.originalEvent.changedTouches, function(i, touch) {
        if (!startTouch || touch.identifier !== startTouch.identifier) {
          return true;
        }
        var xDistance = touch.screenX - startTouch.screenX;
        var yDistance = touch.screenY - startTouch.screenY;
        var leftToRight = xDistance > tolerance && listenToHorizontal;
        var rightToLeft = xDistance < -tolerance && listenToHorizontal;
        var topToBottom = yDistance > tolerance && listenToVertical;
        var bottomToTop = yDistance < -tolerance && listenToVertical;

        if (leftToRight || topToBottom) {
          $.slide('prev');
          startTouch = undefined;
        }
        else if (rightToLeft || bottomToTop) {
          $.slide('next');
          startTouch = undefined;
        }
        return false;
      });

      if (listenToVertical) {
        event.preventDefault();
      }
    });

    $container.unbind('touchend.slide');
    $container.bind('touchend.slide', function(event) {
      $.each(event.originalEvent.changedTouches, function(i, touch) {
        if (startTouch && touch.identifier === startTouch.identifier) {
          startTouch = undefined;
        }
      });
    });
  };

  var indexInBounds = function(index) {
    return typeof index === 'number' && index >=0 && index < slides.length;
  };

  var createBeforeInitEvent = function() {
    var event = $.Event(events.beforeInitialize);
    event.locks = 0;
    event.done = $.noop;
    event.lockInit = function() {
      ++event.locks;
    };
    event.releaseInit = function() {
      --event.locks;
      if (!event.locks) {
        event.done();
      }
    };
    return event;
  };

  var goByHash = function(str) {
    var id = str.substr(str.indexOf("#") + 1);

    $.each(slides, function(i, $slide) {
      if ($slide.attr('id') === id) {
        $.slide('go', i);
        return false;
      }
    });

    if (options.preventFragmentScroll) {
      $.slide('getContainer').scrollLeft(0).scrollTop(0);
    }
  };

  var assignSlideId = function(i, $slide) {
    var currentId = $slide.attr('id');
    var previouslyAssigned = $slide.data('slideAssignedId') === currentId;
    if (!currentId || previouslyAssigned) {
      $slide.attr('id', options.hashPrefix + i);
      $slide.data('slideAssignedId', options.hashPrefix + i);
    }
  };

  var removeContainerHashClass = function(id) {
    $container.removeClass(options.classes.onPrefix + id);
  };

  var addContainerHashClass = function(id) {
    $container.addClass(options.classes.onPrefix + id);
  };

  var setupHashBehaviors = function() {
    $fragmentLinks = $();
    $.each(slides, function(i, $slide) {
      var hash;

      assignSlideId(i, $slide);
      hash = '#' + $slide.attr('id');
      if (hash === window.location.hash) {
        setTimeout(function() {
          $.slide('go', i);
        }, 1);
      }
      $fragmentLinks = $fragmentLinks.add('a[href="' + hash + '"]');
    });

    if (slides.length) {
      addContainerHashClass($.slide('getSlide').attr('id'));
    };
  };

  var changeHash = function(from, to) {
    var hash = '#' + $.slide('getSlide', to).attr('id');
    var hashPath = window.location.href.replace(/#.*/, '') + hash;

    removeContainerHashClass($.slide('getSlide', from).attr('id'));
    addContainerHashClass($.slide('getSlide', to).attr('id'));
    if (Modernizr.history) {
      window.history.replaceState({}, "", hashPath);
    }
  };

  var methods = {

    init: function(opts) {
      var beforeInitEvent = createBeforeInitEvent();
      var overrides = opts;

      if (!$.isPlainObject(opts)) {
        overrides = arguments[1] || {};
        $.extend(true, overrides, {
          selectors: {
            slides: arguments[0]
          }
        });
      }

      options = $.extend(true, {}, $.slide.defaults, overrides);
      slides = [];
      currentIndex = 0;
      $container = $(options.selectors.container);

      $container.addClass(options.classes.loading);

      initSlidesArray(options.selectors.slides);
     
      beforeInitEvent.done = function() {
        
        slides = [];
        initSlidesArray(options.selectors.slides);
        setupHashBehaviors();
        bindKeyEvents();
        bindTouchEvents();
        $container.scrollLeft(0).scrollTop(0);

        if (slides.length) {
          updateStates();
        }

        $container.removeClass(options.classes.loading);
        $document.trigger(events.initialize);
      };

      $document.trigger(beforeInitEvent);
      if (!beforeInitEvent.locks) {
        beforeInitEvent.done();
      }
      window.setTimeout(function() {
        if (beforeInitEvent.locks) {
          if (window.console) {
            window.console.warn('Something locked slide initialization\
              without releasing it before the timeout. Proceeding with\
              initialization anyway.');
          }
          beforeInitEvent.done();
        }
      }, options.initLockTimeout);
    },

    go: function(indexOrId) {
      var beforeChangeEvent = $.Event(events.beforeChange);
      var index;

      if (indexInBounds(indexOrId)) {
        index = indexOrId;
      }
  
      else if (typeof indexOrId === 'string') {
        $.each(slides, function(i, $slide) {
          if ($slide.attr('id') === indexOrId) {
            index = i;
            return false;
          }
        });
      }
      if (typeof index === 'undefined') {
        return;
      }

      $document.trigger(beforeChangeEvent, [currentIndex, index]);
      if (!beforeChangeEvent.isDefaultPrevented()) {
        $document.trigger(events.change, [currentIndex, index]);
        changeHash(currentIndex, index);
        currentIndex = index;
        updateStates();
      }
    },

    next: function() {
      methods.go(currentIndex+1);
    },

    prev: function() {
      methods.go(currentIndex-1);
    },

    getSlide: function(index) {
      index = typeof index !== 'undefined' ? index : currentIndex;
      if (!indexInBounds(index)) {
        return null;
      }
      return slides[index];
    },

    getSlides: function() {
      return slides;
    },

    getTopLevelSlides: function() {
      var topLevelSlides = [];
      var slideSelector = options.selectors.slides;
      var subSelector = [slideSelector, slideSelector].join(' ');
      $.each(slides, function(i, $slide) {
        if (!$slide.is(subSelector)) {
          topLevelSlides.push($slide);
        }
      });
      return topLevelSlides;
    },

    getNestedSlides: function(index) {
      var targetIndex = index == null ? currentIndex : index;
      var $targetSlide = $.slide('getSlide', targetIndex);
      var $nesteds = $targetSlide.find(options.selectors.slides);
      var nesteds = $nesteds.get();
      return $.map(nesteds, function(slide, i) {
        return $(slide);
      });
    },

    getContainer: function() {
      return $container;
    },

    
    getOptions: function() {
      return options;
    },

    extend: function(name, method) {
      methods[name] = method;
    }
  };

  $.slide = function(method, arg) {
    var args = Array.prototype.slice.call(arguments, 1);
    if (methods[method]) {
      return methods[method].apply(this, args);
    }
    else {
      return methods.init(method, arg);
    }
  };

  $.slide.defaults = {
    classes: {
      after: 'slide-after',
      before: 'slide-before',
      childCurrent: 'slide-child-current',
      current: 'slide-current',
      loading: 'slide-loading',
      next: 'slide-next',
      onPrefix: 'on-slide-',
      previous: 'slide-previous'
    },

    selectors: {
      container: '.slide-container',
      slides: '.slide'
    },

    keys: {
      next: [13, 32, 34, 39, 40],
      previous: [8, 33, 37, 38]
    },

    touch: {
      swipeDirection: 'horizontal',
      swipeTolerance: 60
    },

    initLockTimeout: 10000,
    hashPrefix: 'slide-',
    preventFragmentScroll: true
  };

  $document.ready(function() {
    $('html').addClass('ready');
  });

  $window.bind('hashchange.slide', function(event) {
    if (event.originalEvent && event.originalEvent.newURL) {
      goByHash(event.originalEvent.newURL);
    }
    else {
      goByHash(window.location.hash);
    }
  });

  $window.bind('load.slide', function() {
    if (options.preventFragmentScroll) {
      $container.scrollLeft(0).scrollTop(0);
    }
  });
  
})(jQuery);

(function($, undefined) {
  var $document = $(document);
  var $window = $(window);
  var baseHeight, timer, rootSlides;

  var scaleslide = function() {
    var options = $.slide('getOptions');
    var $container = $.slide('getContainer');
    var baseHeight = options.baseHeight;

    if (!baseHeight) {
      baseHeight = $container.height();
    }

    $.each(rootSlides, function(i, $slide) {
      var slideHeight = $slide.innerHeight();
      var $scaler = $slide.find('.' + options.classes.scaleSlideWrapper);
      var shouldScale = $container.hasClass(options.classes.scale);
      var scale = shouldScale ? baseHeight / slideHeight : 1;

      if (scale === 1) {
        $scaler.css('transform', '');
      }
      else {
        $scaler.css('transform', 'scale(' + scale + ')');
        window.setTimeout(function() {
          $container.scrollTop(0)
        }, 1);
      }
    });
  };

  var populateRootSlides = function() {
    var options = $.slide('getOptions');
    var slideTest = $.map([
      options.classes.before,
      options.classes.previous,
      options.classes.current,
      options.classes.next,
      options.classes.after
    ], function(el, i) {
      return '.' + el;
    }).join(', ');

    rootSlides = [];
    $.each($.slide('getSlides'), function(i, $slide) {
      var $parentSlides = $slide.parentsUntil(
        options.selectors.container,
        slideTest
      );
      if (!$parentSlides.length) {
        rootSlides.push($slide);
      }
    });
  };

  var wrapRootSlideContent = function() {
    var options = $.slide('getOptions');
    var wrap = '<div class="' + options.classes.scaleSlideWrapper + '"/>';
    $.each(rootSlides, function(i, $slide) {
      $slide.children().wrapAll(wrap);
    });
  };

  var scaleOnResizeAndLoad = function() {
    var options = $.slide('getOptions');

    $window.unbind('resize.slidescale');
    $window.bind('resize.slidescale', function() {
      window.clearTimeout(timer);
      timer = window.setTimeout(scaleslide, options.scaleDebounce);
    });
    $.slide('enableScale');
    $window.unbind('load.slidescale');
    $window.bind('load.slidescale', scaleslide);
  };

  var bindKeyEvents = function() {
    var options = $.slide('getOptions');
    $document.unbind('keydown.slidescale');
    $document.bind('keydown.slidescale', function(event) {
      var isKey = event.which === options.keys.scale;
      isKey = isKey || $.inArray(event.which, options.keys.scale) > -1;
      if (isKey) {
        $.slide('toggleScale');
        event.preventDefault();
      }
    });
  };

  $.extend(true, $.slide.defaults, {
    classes: {
      scale: 'slide-scale',
      scaleSlideWrapper: 'slide-slide-scaler'
    },

    keys: {
      scale: 83
    },

    baseHeight: null,
    scaleDebounce: 200
  });

  $.slide('extend', 'disableScale', function() {
    $.slide('getContainer').removeClass($.slide('getOptions').classes.scale);
    scaleslide();
  });

  $.slide('extend', 'enableScale', function() {
    $.slide('getContainer').addClass($.slide('getOptions').classes.scale);
    scaleslide();
  });

  $.slide('extend', 'toggleScale', function() {
    var $container = $.slide('getContainer');
    var isScaled = $container.hasClass($.slide('getOptions').classes.scale);
    $.slide(isScaled? 'disableScale' : 'enableScale');
  });

  $document.bind('slide.init', function() {
    populateRootSlides();
    wrapRootSlideContent();
    scaleOnResizeAndLoad();
    bindKeyEvents();
  });
})(jQuery, 'slide', this);

(function($, undefined) {
  var $document = $(document);
  var rootCounter;

  var updateCurrent = function(event, from, to) {
    var options = $.slide('getOptions');
    var currentSlideNumber = to + 1;
    if (!options.countNested) {
      currentSlideNumber = $.slide('getSlide', to).data('rootSlide');
    }
    $(options.selectors.statusCurrent).text(currentSlideNumber);
  };

  var markRootSlides = function() {
    var options = $.slide('getOptions');
    var slideTest = $.map([
      options.classes.before,
      options.classes.previous,
      options.classes.current,
      options.classes.next,
      options.classes.after
    ], function(el, i) {
      return '.' + el;
    }).join(', ');

    rootCounter = 0;
    $.each($.slide('getSlides'), function(i, $slide) {
      var $parentSlides = $slide.parentsUntil(
        options.selectors.container,
        slideTest
      );

      if ($parentSlides.length) {
        $slide.data('rootSlide', $parentSlides.last().data('rootSlide'));
      }
      else {
        ++rootCounter;
        $slide.data('rootSlide', rootCounter);
      }
    });
  };

  var setInitialSlideNumber = function() {
    var slides = $.slide('getSlides');
    var $currentSlide = $.slide('getSlide');
    var index;

    $.each(slides, function(i, $slide) {
      if ($slide === $currentSlide) {
        index = i;
        return false;
      }
    });
    updateCurrent(null, index, index);
  };

  var setTotalSlideNumber = function() {
    var options = $.slide('getOptions');
    var slides = $.slide('getSlides');

    if (options.countNested) {
      $(options.selectors.statusTotal).text(slides.length);
    }
    else {
      $(options.selectors.statusTotal).text(rootCounter);
    }
  };

  $.extend(true, $.slide.defaults, {
    selectors: {
      statusCurrent: '.slide-status-current',
      statusTotal: '.slide-status-total'
    },

    countNested: true
  });

  $document.bind('slide.init', function() {
    markRootSlides();
    setInitialSlideNumber();
    setTotalSlideNumber();
  });
  $document.bind('slide.change', updateCurrent);
})(jQuery, 'slide');

(function($, undefined) {
  var $document = $(document);

  var updateButtons = function(event, from, to) {
    var options = $.slide('getOptions');
    var lastIndex = $.slide('getSlides').length - 1;
    var $prevSlide = $.slide('getSlide', to - 1);
    var $nextSlide = $.slide('getSlide', to + 1);
    var hrefBase = window.location.href.replace(/#.*/, '');
    var prevId = $prevSlide ? $prevSlide.attr('id') : undefined;
    var nextId = $nextSlide ? $nextSlide.attr('id') : undefined;
    var $prevButton = $(options.selectors.previousLink);
    var $nextButton = $(options.selectors.nextLink);

    $prevButton.toggleClass(options.classes.navDisabled, to === 0);
    $prevButton.attr('aria-disabled', to === 0);
    $prevButton.attr('href', hrefBase + '#' + (prevId ? prevId : ''));
    $nextButton.toggleClass(options.classes.navDisabled, to === lastIndex);
    $nextButton.attr('aria-disabled', to === lastIndex);
    $nextButton.attr('href', hrefBase + '#' + (nextId ? nextId : ''));
  };

  $.extend(true, $.slide.defaults, {
    classes: {
      navDisabled: 'slide-nav-disabled'
    },

    selectors: {
      nextLink: '.slide-next-link',
      previousLink: '.slide-prev-link'
    }
  });

  $document.bind('slide.init', function() {
    var options = $.slide('getOptions');
    var slides = $.slide('getSlides');
    var $current = $.slide('getSlide');
    var $prevButton = $(options.selectors.previousLink);
    var $nextButton = $(options.selectors.nextLink);
    var index;
    
    $prevButton.unbind('click.slidenavigation');
    $prevButton.bind('click.slidenavigation', function(event) {
      $.slide('prev');
      event.preventDefault();
    });

    $nextButton.unbind('click.slidenavigation');
    $nextButton.bind('click.slidenavigation', function(event) {
      $.slide('next');
      event.preventDefault();
    });

    $.each(slides, function(i, $slide) {
      if ($slide === $current) {
        index = i;
        return false;
      }
    });
    updateButtons(null, index, index);
  });

  $document.bind('slide.change', updateButtons);
})(jQuery);

(function($, undefined) {
    var position = {x: 0, y: window.innerHeight/2};
    var counter = 0;
    var defaultSize = 35;
    var angleDistortion = 0;
    var patterns = ".";

    var canvas;
    var context;
    var mouse = {x: 0, y: 0, down: false}

    function init() {
        canvas = document.getElementById( 'canvas' );
        context = canvas.getContext( '2d' );
        canvas.width = window.innerWidth;
        canvas.height = window.innerHeight;

        canvas.addEventListener('mousemove', mouseMove, false);
        canvas.addEventListener('mousedown', mouseDown, false);
        canvas.addEventListener('mouseup',   mouseUp,   false);
        canvas.addEventListener('mouseout',  mouseUp,  false);
        canvas.addEventListener('dblclick', doubleClick, false);

        window.onresize = function(event) {
            canvas.width = window.innerWidth;
            canvas.height = window.innerHeight;
        }
    }

    function mouseMove ( event ){
        mouse.x = event.pageX;
        mouse.y = event.pageY;
        draw();
    }

    function draw() {
        if ( mouse.down ) {
            var d = distance( position, mouse );
            var fontSize = defaultSize;
            var pattern = patterns[counter];
            var stepSize = textWidth( pattern, fontSize );

            if (d > stepSize) {
                var angle = Math.atan2(mouse.y-position.y, mouse.x-position.x);

                context.font = fontSize + "px Georgia";

                context.save();
                context.translate( position.x, position.y);
                context.rotate( angle );
                context.fillStyle="#0097ba";
                context.fillText(pattern,0,0);
                context.restore();

                counter++;
                if (counter > patterns.length-1) {
                    counter = 0;
                }

                position.x = position.x + Math.cos(angle) * stepSize;
                position.y = position.y + Math.sin(angle) * stepSize;

            }
        }
    }

    function distance( pt, pt2 ){
        var xs = 0;
        var ys = 0;

        xs = pt2.x - pt.x;
        xs = xs * xs;

        ys = pt2.y - pt.y;
        ys = ys * ys;

        return Math.sqrt( xs + ys );
    }

    function mouseDown( event ){
        mouse.down = true;
        position.x = event.pageX;
        position.y = event.pageY;

        document.getElementById('info').style.display = 'none';
    }

    function mouseUp( event ){
        mouse.down = false;
    }

    function doubleClick( event ) {
        canvas.width = canvas.width;
    }

    function textWidth( string, size ) {
        context.font = size + "px Georgia";

        if ( context.fillText ) {
            return context.measureText( string ).width;
        } else if ( context.mozDrawText) {
            return context.mozMeasureText( string );
        }

    }

    init();

})(jQuery);