using System;
using System.Collections.Generic;
using DoubleJay.Epi.ConfigurableColorPicker.Manager;
using DoubleJay.Epi.ConfigurableColorPicker.Models;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;

namespace DoubleJay.Epi.ConfigurableColorPicker.Infrastructure
{
    [EditorDescriptorRegistration(TargetType = typeof(Color), UIHint = ColorPickerUIHint.ColorPicker)]
    public class ColorPickerEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            var colorPaletteManager = ServiceLocator.Current.GetInstance<IColorPaletteManager>();
            var palette = colorPaletteManager.GetPalette();

            ClientEditingClass = "configurablecolorpicker/ColorPalette";
            EditorConfiguration["colors"] = palette?.Colors;
            EditorConfiguration["maxColumns"] = palette?.MaxColumns ?? 4;
            EditorConfiguration["showClearButton"] = palette?.ShowClearButton ?? true;

            base.ModifyMetadata(metadata, attributes);
        }
    }
}