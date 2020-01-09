using EPiServer.DataAnnotations;

namespace DoubleJay.Epi.ConfigurableColorPicker.Infrastructure
{
    public class ColorPickerAttribute : BackingTypeAttribute, IColorPickerAttribute
    {
        public string PaletteName { get; }

        public ColorPickerAttribute() : base(typeof(PropertyPaletteColor))
        {
        }

        public ColorPickerAttribute(string paletteName) : base(typeof(PropertyPaletteColor))
        {
            PaletteName = paletteName;
        }
    }
}