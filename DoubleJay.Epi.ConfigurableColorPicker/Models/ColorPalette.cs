using System.Collections.Generic;

namespace DoubleJay.Epi.ConfigurableColorPicker.Models
{
    /// <inheritdoc />
    internal class ColorPalette : IColorPalette
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ColorPalette"/> class.
        /// </summary>
        /// <param name="id">The palette ID.</param>
        /// <param name="name">The palette name.</param>
        /// <param name="hosts">The hostnames.</param>
        /// <param name="fallbackId">The fallback palette ID.</param>
        /// <param name="maxColumns">The max number of columns.</param>
        /// <param name="showClearButton">Indicates whether the clear value button should be show.</param>
        /// <param name="colors">The colors for this color palette.</param>
        public ColorPalette(int? id, string name, ICollection<string> hosts, int? fallbackId, int? maxColumns,
            bool? showClearButton, ICollection<IColor> colors)
        {
            Id = id;
            Name = name;
            Hosts = hosts;
            FallbackId = fallbackId;
            MaxColumns = maxColumns;
            ShowClearButton = showClearButton;
            Colors = colors;
        }

        /// <inheritdoc />
        public int? Id { get; set; }

        /// <inheritdoc />
        public string Name { get; set; }

        /// <inheritdoc />
        public ICollection<string> Hosts { get; set; }

        /// <inheritdoc />
        public int? FallbackId { get; set; }

        /// <inheritdoc />
        public int? MaxColumns { get; set; }

        /// <inheritdoc />
        public bool? ShowClearButton { get; set; }

        /// <inheritdoc />
        public ICollection<IColor> Colors { get; set; }
    }
}