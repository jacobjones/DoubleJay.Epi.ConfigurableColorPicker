using System.Collections.Generic;

namespace DoubleJay.Epi.ConfigurableColorPicker.Models
{
    /// <inheritdoc />
    internal class ColorPalette : IColorPalette
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPalette"/> class.
        /// </summary>
        /// <param name="host">The host.</param>
        /// <param name="fallback">The fallback.</param>
        /// <param name="maxColumns">The max number of columns.</param>
        /// <param name="showClearButton">Indicates whether the clear value button should be show.</param>
        /// <param name="colors">The colors for this color palette.</param>
        public ColorPalette(string host, string fallback, int? maxColumns, bool? showClearButton, ICollection<IColor> colors)
        {
            Host = host;
            Fallback = fallback;
            MaxColumns = maxColumns;
            ShowClearButton = showClearButton;
            Colors = colors;
        }

        /// <inheritdoc />
        public string Host { get; set; }

        /// <inheritdoc />
        public string Fallback { get; set; }

        /// <inheritdoc />
        public int? MaxColumns { get; set; }

        /// <inheritdoc />
        public bool? ShowClearButton { get; set; }

        /// <inheritdoc />
        public ICollection<IColor> Colors { get; set; }
    }
}