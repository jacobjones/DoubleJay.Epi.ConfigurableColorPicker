# Configurable Color Picker for Optimizely CMS

![Configurable Color Picker](https://raw.githubusercontent.com/jacobjones/DoubleJay.Epi.ConfigurableColorPicker/master/images/configurable-color-picker.png)

## Description
This package provides an easy-to-use color picker for Optimizely CMS allows full configurability of the color palette.

## Features
* Available colors are fully configurable
* Colors can be swapped-out as the ID is persisted (not the color code)
* Support for different color palettes per site in a multi-site solution
* Allows for individual properties to use a specified palette

## Getting started
### Installation
* The NuGet package can be installed from the [Optimizely NuGet feed](https://nuget.optimizely.com/feed/)
* See the installation details here: https://nuget.optimizely.com/package/?id=DoubleJay.Epi.ConfigurableColorPicker

### Configuration & Usage
In CMS 12, to register you should call the `AddConfigurableColorPicker` method in your startup class `ConfigureServices` method::

```cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddConfigurableColorPicker();
}
```

**Version 2** added the ability to name palettes and use these throughout the website, as such, configuration and usage changed significantly. Breaking changes are listed in the [documentation](https://github.com/jacobjones/DoubleJay.Epi.ConfigurableColorPicker/wiki).

Please consult with the relevant documentation for further details:

* [Version 2+](https://github.com/jacobjones/DoubleJay.Epi.ConfigurableColorPicker/wiki/Usage-%28Version-2%29)
* [Version 1](https://github.com/jacobjones/DoubleJay.Epi.ConfigurableColorPicker/wiki/Usage-%28Version-1%29)

### Further Information

Creating this was the subject of two blog posts you can checkout here:

* https://jakejon.es/blog/making-a-configurable-color-picker-for-episerver-part-1
* https://jakejon.es/blog/making-a-configurable-color-picker-for-episerver-part-2