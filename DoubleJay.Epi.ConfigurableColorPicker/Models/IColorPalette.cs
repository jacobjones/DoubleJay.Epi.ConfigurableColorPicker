using System.Collections.Generic;

namespace DoubleJay.Epi.ConfigurableColorPicker.Models
{
    /// <summary>
    /// Represents a color palette.
    /// </summary>
    public interface IColorPalette
    {
        /// <summary>
        /// The palette ID, or null if none defined.
        /// </summary>
        int? Id { get; set; }

        /// <summary>
        /// The palette name.
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// The hostnames the color palette applies to, or empty if all.
        /// </summary>
        ICollection<string> Hosts { get; set; }

        /// <summary>
        /// The fallback palette ID, or null if none defined.
        /// </summary>
        int? FallbackId { get; set; }

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