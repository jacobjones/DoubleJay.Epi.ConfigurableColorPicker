namespace DoubleJay.Epi.ConfigurableColorPicker.Models
{
    /// <inheritdoc />
    public class Color : IColor
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> class.
        /// </summary>
        public Color()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> class.
        /// </summary>
        /// <param name="id">The ID.</param>
        public Color(int id)
        {
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Color"/> class.
        /// </summary>
        /// <param name="id">The ID.</param>
        /// <param name="name">The name.</param>
        /// <param name="value">The value.</param>
        public Color(int id, string name, string value)
        {
            Id = id;
            Name = name;
            Value = value;
        }

        /// <inheritdoc />
        public int Id { get; set; }

        /// <inheritdoc />
        public string Name { get; }

        /// <inheritdoc />
        public string Value { get; }
    }
}