# Configurable Color Picker for Episerver

## Description
This package provides a easy-to-use color picker for Episerver—which allows full configurability of the color pallete.

## Features
* Available colors are fully configurable
* Colors can be swapped-out as the ID is persisted (not the color code)
* Support for different color palettes per site in a multi-site solution

## Getting started
### Installation
* The NuGet package can be build using the ``Build-Package.ps1`` PowerShell script
* The package can then be installed via the Package Manger Console

### Configuration

Once installed, it's necesary to create a cofiguration file in the web root. This should be called: ``ColorPalette.config``.

A detailed example of the configuration file is shown here:

 ```xml
<?xml version="1.0" encoding="utf-8"?>
<colorPalettes>
  <colorPalette>
    <maxColumns>3</maxColumns>
    <showClearButton>true</showClearButton>
    <colors>
      <color id="1">
        <value>#fff</value>
        <name>White</name>
      </color>
      <color id="2">
        <value>#ff0000</value>
        <name>Red</name>
      </color>
      <color id="3">
        <value>#0000ff</value>
        <name>Blue</name>
      </color>
    </colors>
    </colorPalette>
    <colorPalette host="alloy.local">
      <maxColumns>3</maxColumns>
      <colors>
        <color id="1">
          <value>#ff00ff</value>
          <name>Magenta</name>
        </color>
        <color id="2">
          <value>#ffa500</value>
          <!--No name-->
        </color>
      </colors>
    </colorPalette>
    <colorPalette host="alloymicro.local" fallback="alloy.local">
      <maxColumns>2</maxColumns>
      <showClearButton>false</showClearButton>
      <colors>
        <color id="1">
          <value>#ffff00</value>
          <name>Yellow</name>
        </color>
      </colors>
    </colorPalette>
</colorPalettes>
```

Things to note:
* At least 1 ``colorPalette`` definition is required
* ``maxColumns`` indicates the maximum width of the palette (in the UI)
* ``showClearButton`` allows the editor clear the color selection
* Each colorPalette can have a ``host`` (optional) and specify a ``fallback`` (optional)
* If a color palette has neither ``host`` nor ``fallback`` it is considered the default, all other configurations fallback to this. This is not required.
* ``fallback`` allows a ``colorPalette`` to use another as it's base definition (e.g. alloymicro.local fallbacks to alloy.local, which fallbacks to the default). 
* Each ``color`` must specify an ``id`` and ``value``, ``name`` is optional
* The ``color`` has a minimum value of 1
* A ``color`` can be overridden by specifying another with the same id (e.g. in the example above the alloy.local site takes the 'Blue' color from the default, but overrides the others)

### Usage

Using the color palette is as simple as:

```cs
[UIHint(ColorPickerUIHint.ColorPicker)]
[BackingType(typeof(PropertyPaletteColor))]
public virtual IColor Color { get; set; }
```

This gives you an ``IColor`` which contains then ID, name and value.

Setting a default value is also straight-forward:

```cs
public override void SetDefaultValues(ContentType contentType)
{
    base.SetDefaultValues(contentType);
    Color = new Color(3);
}
```