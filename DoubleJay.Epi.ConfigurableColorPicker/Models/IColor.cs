namespace DoubleJay.Epi.ConfigurableColorPicker.Models
{
    /// <summary>
    /// Represents a color palette color.
    /// </summary>
    public interface IColor
    {
        // The ID.
        int Id { get; }

        // The name.
        string Name { get; }

        // The value (Hex code, RGB code etc.)
        string Value { get; }
    }
}