
// TODO: If range does not allow negative numbers!!

; (function ($, window, undefined) {

    // Enums
    var numberType = Object.freeze({ NUMBER: 'number', CURRENCY: 'currency', PERCENT: 'percent' })
    // Constants
    var _background = 'background-color;'
    var _color = 'color';
    var _transparent = 'transparent';

    function numberFormat() {
        // Defaults for .NET invariant culture
        this.type = numberType.NUMBER;
        this.precision = '2';
        this.decimalSeperator = '2E';
        this.positivePattern = 'n';
        this.negativePattern = '-n';
        this.groupSeperator = '2C';
        this.groupSizes = '3';
        this.negativeSign = '2D';
    }

    //Defaults
    var defaults = {
        rounding: undefined
    };

    // Constructor
    function numeric(element, options) {
        // Assign the DOM element
        this.element = $(element).closest('div');
        this.options = $.extend({}, defaults, options);
        this.defaults = defaults;
        // Initialise the select
        this.initialise();
    }

    // Initialise
    numeric.prototype.initialise = function () {
        var self = this;
        // Declare the main UI components
        this.container = $(this.element);
        this.displayText = $(this.element).children('.numeric-text');
        this.input = $(this.element).children('.numeric-input');
        // Get current input colours
        this.colour = this.input.css(_color);
        this.background = this.input.css(_background);
        // Determine format
        this.format = new numberFormat();
        // Set format defaults
        if (this.displayText.hasClass('currency')) {
            this.format.type = numberType.CURRENCY;
            this.format.positivePattern = '$n';
            this.format.negativePattern = '($n)';
        } else if (this.displayText.hasClass('percent')) {
            this.format.type = numberType.PERCENT;
            this.format.positivePattern = 'n %';
            this.format.negativePattern = '-n %';
        }
        // Set format properties
        var data = this.container.data();
        this.format.precision = data['precision'];
        this.format.negativeSign = data['negativesign'];
        this.format.decimalSeperator = data['decimalseperator'];
        this.format.positivePattern = data['positivepattern'];
        this.format.negativePattern = data['negativepattern'];
        if (data['groupseperator']) {
            this.format.groupSeperator = data['groupseperator'];
        }
        if (data['groupsizes']) {
            this.format.groupSizes = data['groupsizes'].toString().split(',');
        }
        this.decimalKeyCode = parseInt(this.format.decimalSeperator, 16);
        this.negativeKeyCode = parseInt(this.format.negativeSign, 16);
        // Make the input transparent
        this.input.css(_color, _transparent).css(_background, _transparent);

        // ************ Events ************

        this.displayText.mousedown(function (e) {
            // Prevent selection of the display text
            e.preventDefault();
            // Ensure the input text is focused
            if (!self.input.is(':focus')) {
                self.input.focus();
            }
        });

        // Sets the colour and background colors
        this.input.focus(function (e) {
            self.displayText.hide();
            self.input.css(_color, self.colour).css(_background, self.background);
        });

        // Allow only numbers, decimal point and minus sign
        this.input.keypress(function (e) {
            var k = e.keyCode;
            if (!((k > 47 && k < 58) || k == self.decimalKeyCode || k == self.negativeKeyCode)) {
                window.event.returnValue = false;
            };
        });

        // Apply rounding
        this.input.change(function () {
            if (self.options.rounding === undefined) {
                return;
            }
            var rounding = 1 / self.options.rounding;
            var number = new Number(self.input.val());
            number = Math.round(number * rounding) / rounding;
            self.input.val(number.toFixed(self.format.precision));
        });

        // Sets the colour and background colors and formats the display text
        this.input.blur(function (e) {
            // Format display text
            self.displayText.text(self.formatValue(self.input.val()));
            if (self.isNegative(self.input.val())) {
                self.displayText.addClass('negative')
            } else {
                self.displayText.removeClass('negative')
            }
            // Hide the input text and show the display text
            self.input.css(_color, _transparent).css(_background, _transparent);
            self.displayText.show();
        });
    }

    // Determines if a number is negative
    numeric.prototype.isNegative = function (number) {
        var regex = new RegExp('\\x' + this.format.negativeSign);
        return regex.test(number);
    }

    // Returns a formatted number
    numeric.prototype.formatValue = function (number) {
        if (!number) {
            return '';
        }
        number = number.toString();
        if (number.length === 0) {
            return '';
        }
        var isNegative = this.isNegative(number);
        if (isNegative) {
            // Remove the negative sign
            var regex = new RegExp('\\x' + this.format.negativeSign);
            number = number.replace(regex, '');
        }
        // Get the components
        var decimalSeperator = String.fromCharCode('0x' + this.format.decimalSeperator);
        // TODO: Test with different cultures
        if (this.format.type === numberType.PERCENT) {
            var percent = (number.replace(decimalSeperator, '.') * 100).toString();
            number = percent.replace('.', decimalSeperator);
        }
        var components = number.split(decimalSeperator);
        var integer = components[0];
        var mantissa = components[1];

        // Format integer part
        var groupSeperator = String.fromCharCode('0x' + this.format.groupSeperator);
        if (integer.length === 0) {
            integer = 0;
        } else if (this.format.groupSizes.length === 1) {
            var size = this.format.groupSizes[0];
            var remaining = integer.length - size;
            while (remaining > 0) {
                integer = integer.substr(0, remaining) + groupSeperator + integer.substr(remaining);
                remaining -= size;
            }
            // Alternative using regex
            //whole = whole.replace(/\B(?=(\d{3})+(?!\d))/g, seperator);
        } else {
            var index = 0;
            var size = this.format.groupSizes[0];
            var remaining = integer.length - size;
            while (remaining > 0) {
                // Insert the separator
                integer = integer.substr(0, remaining) + groupSeperator + integer.substr(remaining);
                if (index < this.format.groupSizes.length - 1) {
                    index++;
                }
                // Get the next group size
                size = this.format.groupSizes[index]
                // If the group size is 0, there is no more grouping
                if (size == 0) {
                    break;
                }
                // Move to the next position
                remaining -= size;
            }
        }
        // Format mantissa
        if (mantissa === undefined) {
            mantissa = '';
        }
        if (mantissa.length > this.format.precision) {
            // Round cents to required precision
            mantissa = Math.round(mantissa / Math.pow(10, (mantissa.length - this.format.precision)));
        }
        // Ensure correct precision
        while (mantissa.toString().length < this.format.precision) {
            mantissa += '0'
        };
        if (this.format.precision === 0) {
            number = integer;
        } else {
            number = integer + decimalSeperator + mantissa;
        }     
        if (isNegative) {
            return this.format.negativePattern.replace('n', number);
        } else {
            return this.format.positivePattern.replace('n', number)
        }
    }

    // Numeric definition
    $.fn.numeric = function (options) {
        return this.each(function () {
            if (!$.data(this, 'numeric')) {
                $.data(this, 'numeric', new numeric(this, options));
            }
        });
    }
   
    // Returns a formatted number
    $.fn.getFormattedValueFor = function (number) {
        var self = this.data('numeric');
        if (!self) {
            console.error('The element is not a numeric control');
            return number;
        }
        return self.formatValue(number);
    }

    // Returns a value indicating if a number is negative
    $.fn.isNumberNegative = function (number) {
        var self = this.data('numeric');
        if (!self) {
            console.error('The element is not a numeric control');
            return undefined;
        }
        return self.isNegative(number);
    }

}(jQuery, window));
