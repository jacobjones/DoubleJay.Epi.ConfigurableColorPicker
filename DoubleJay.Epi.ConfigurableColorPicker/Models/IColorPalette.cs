using System.Collections.Generic;

namespace DoubleJay.Epi.ConfigurableColorPicker.Models
{
    /// <summary>
    /// Represents a color palette.
    /// </summary>
    public interface IColorPalette
    {
        /// <summary>
        /// The host the color palette applies to, or null if default.
        /// </summary>
        string Host { get; set; }

        /// <summary>
        /// The fallback host, or null if none defined.
        /// </summary>
        string Fallback { get; set; }

        /// <summary>
        /// The max number of columns to show in the color palette.
        /// </summary>
        int? MaxColumns { get; set; }

        /// <summary>
        /// Indicates whether the clear value button should be show for the color palette.
        /// </summary>
        bool? ShowClearButton { get; set; }

        /// <summary>
        /// The colors.
        /// </summary>
        ICollection<IColor> Colors { get; set; }
    }
}