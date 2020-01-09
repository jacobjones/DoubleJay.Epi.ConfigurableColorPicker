using System;
using System.Collections.Generic;
using System.Linq;
using DoubleJay.Epi.ConfigurableColorPicker.Manager;
using DoubleJay.Epi.ConfigurableColorPicker.Models;
using EPiServer.ServiceLocation;
using EPiServer.Shell.ObjectEditing;
using EPiServer.Shell.ObjectEditing.EditorDescriptors;

namespace DoubleJay.Epi.ConfigurableColorPicker.Infrastructure
{
    [EditorDescriptorRegistration(TargetType = typeof(Color))]
    public class ColorPickerEditorDescriptor : EditorDescriptor
    {
        public override void ModifyMetadata(ExtendedMetadata metadata, IEnumerable<Attribute> attributes)
        {
            base.ModifyMetadata(metadata, attributes);

            var colorPickerAttribute =
                attributes?.OfType<IColorPickerAttribute>().FirstOrDefault();

            var colorPaletteManager = ServiceLocator.Current.GetInstance<IColorPaletteManager>();
            var palette = colorPaletteManager.GetPalette(colorPickerAttribute?.PaletteName);

            // Hide the property if no palette matches.
            if (palette == null)
            {
                metadata.ShowForEdit = false;
                return;
            }

            metadata.ClientEditingClass = "configurablecolorpicker/ColorPalette";
            metadata.EditorConfiguration["colors"] = palette?.Colors;
            metadata.EditorConfiguration["maxColumns"] = palette?.MaxColumns ?? 4;
            metadata.EditorConfiguration["showClearButton"] = palette?.ShowClearButton ?? true;
        }
    }
}