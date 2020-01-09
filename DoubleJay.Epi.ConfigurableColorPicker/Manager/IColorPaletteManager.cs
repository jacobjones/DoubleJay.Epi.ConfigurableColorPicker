using System.Collections.Generic;
using DoubleJay.Epi.ConfigurableColorPicker.Models;

namespace DoubleJay.Epi.ConfigurableColorPicker.Manager
{
    /// <summary>
    /// Provides functionality to retrieve defined color palettes.
    /// </summary>
    public interface IColorPaletteManager
    {
        /// <summary>
        /// Gets the color palette for the current site, returns null if no appropriate color palette is defined.
        /// </summary>
        /// <param name="paletteName">The palette name.</param>
        /// <returns>The relevant color palette or null.</returns>
        IColorPalette GetPalette(string paletteName = null);

        /// <summary>
        /// Gets all defined color palettes.
        /// </summary>
        /// <returns>All color palettes.</returns>
        ICollection<IColorPalette> GetPalettes();

        /// <summary>
        /// Gets a color for the current site based on a ID.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="propertyDefinitionId">The property definition ID.</param>
        /// <returns>The relevant color.</returns>
        IColor GetColor(int id, int propertyDefinitionId);
    }
}