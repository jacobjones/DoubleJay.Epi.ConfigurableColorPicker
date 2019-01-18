define([
    "dijit/_Widget",
    "dijit/_TemplatedMixin",
    "dijit/_WidgetsInTemplateMixin",
    "dijit/_PaletteMixin",
    "dojo/_base/declare",
    "dojo/dom-class",
    "dojo/dom-construct",
    "dojo/string"
], function (_Widget, _TemplatedMixin, _WidgetsInTemplateMixin, _PaletteMixin, declare, domClass, domConstruct, string) {

    var ColorPalette = declare("configurablecolorpicker/ColorPalette", [_Widget, _TemplatedMixin, _WidgetsInTemplateMixin, _PaletteMixin], {

        templateString:
            "<div class='dijitInline dijitColorPalette' role='grid'>" +
                "<table dojoAttachPoint='paletteTableNode' class='dijitPaletteTable' cellSpacing='0' cellPadding='0' role='presentation'>" +
                    "<tbody data-dojo-attach-point='gridNode'></tbody>" +
                "</table>" +
                "<button data-dojo-props='iconClass:\"dijitEditorIcon dijitEditorIconDelete\", showLabel: false' " +
                    "data-dojo-type='dijit/form/Button' " +
                    "data-dojo-attach-event='onClick: clearSelection' " +
                    "id='${id}_clearButton'>Clear</button>" +
            "</div>",

        widgetsInTemplate: true,

        clearSelection: function () {
            this.set("value", null);
        },

        _dyeFactory: function (value, row, col, title) {
            return new this._dyeClass(value, row, col, title);
        },

        buildRendering: function () {
            this.inherited(arguments);

            if (!this.showClearButton) {
                dijit.byId(this.id + "_clearButton").destroy();
            }

            // Extract all the colors
            var values = this.colors.map(a => a.value);

            var paletteValues = [];

            // Separate into rows, based on the user specified 'maxColumns' value
            while (values.length) {
                paletteValues.push(values.splice(0, this.maxColumns));
            }

            var tooltips = {};

            // Extract the tooltips, using the color name or Hex code
            // if no name is supplied.
            for (var i = 0; i < this.colors.length; i++) {
                tooltips[this.colors[i].value] = this.colors[i].name || this.colors[i].value;
            }

            this._dyeClass = declare(ColorPalette._Color, {
                colors: this.colors
            });

            this._preparePalette(paletteValues, tooltips);
        },
        _setValueAttr: function (value, priorityChange) {
            // clear old selected cell
            if (this._selectedCell >= 0) {
                domClass.remove(this._cells[this._selectedCell].node, this.cellClass + "Selected");
            }
            this._selectedCell = -1;

            // search for cell matching specified value
            if (value) {
                for (var i = 0; i < this._cells.length; i++) {
                    if (value.id === this._cells[i].dye.getValue().id) {
                        this._selectedCell = i;
                        domClass.add(this._cells[i].node, this.cellClass + "Selected");
                        break;
                    }
                }
            }

            // record new value, or null if no matching cell
            this._set("value", this._selectedCell >= 0 ? value : null);

            if (priorityChange || priorityChange === undefined) {
                this.onChange(value);
            }
        }
    });

    ColorPalette._Color = declare(null, {
        template:
            "<span class='dijitInline dijitPaletteImg' style='line-height:normal;padding-bottom:1px;'>" +
                "<img src='${blankGif}' alt='${alt}' title='${title}' class='dijitColorPaletteSwatch' style='width:20px;height:20px;background-color: ${color}'/>" +
            "</span>",

        constructor: function (alias, row, col, title) {
            // Find the color based on it's Hex
            var color = this.colors.find(o => { return o.value === alias; });

            this._color = alias;
            this._title = title;
            this._value = color;
        },

        getValue: function () {
            return this._value;
        },

        fillCell: function (cell, blankGif) {
            var html = string.substitute(this.template, {
                color: this._color,
                blankGif: blankGif,
                alt: this._title,
                title: this._title
            });

            domConstruct.place(html, cell);
        }
    });

    return ColorPalette;
});