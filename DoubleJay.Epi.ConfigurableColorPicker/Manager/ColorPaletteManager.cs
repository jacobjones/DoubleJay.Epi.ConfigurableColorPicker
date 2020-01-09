using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DoubleJay.Epi.ConfigurableColorPicker.Infrastructure;
using DoubleJay.Epi.ConfigurableColorPicker.Models;
using EPiServer.DataAbstraction;
using EPiServer.Logging;
using EPiServer.Web;

namespace DoubleJay.Epi.ConfigurableColorPicker.Manager
{
    /// <inheritdoc />
    public class ColorPaletteManager : IColorPaletteManager
    {
        private readonly IPropertyDefinitionRepository _propertyDefinitionRepository;
        private readonly IContentTypeRepository _contentTypeRepository;
        private readonly ContentTypeModelRepository _contentTypeModelRepository;

        private static readonly ILogger Logger = LogManager.GetLogger(typeof(ColorPaletteManager));

        public ColorPaletteManager(IPropertyDefinitionRepository propertyDefinitionRepository, IContentTypeRepository contentTypeRepository, ContentTypeModelRepository contentTypeModelRepository)
        {
            _propertyDefinitionRepository = propertyDefinitionRepository;
            _contentTypeRepository = contentTypeRepository;
            _contentTypeModelRepository = contentTypeModelRepository;
        }

        /// <inheritdoc />
        public IColorPalette GetPalette(string paletteName = null)
        {
            var palettes = GetPalettes();

            var siteUrl = SiteDefinition.Current.SiteUrl;

            if (siteUrl == null)
            {
                Logger.Error("The site hostname could not be resolved, returning the default palette.");

                return palettes.SingleOrDefault(x =>
                    !x.Hosts.Any() && string.Equals(x.Name, paletteName, StringComparison.Ordinal));
            }

            var palette = palettes.SingleOrDefault(x =>
                x.Hosts.Contains(siteUrl.Host, StringComparer.OrdinalIgnoreCase) &&
                string.Equals(x.Name, paletteName, StringComparison.Ordinal));

            if (palette != null)
            {
                return palette;
            }

            palette = palettes.SingleOrDefault(x =>
                !x.Hosts.Any() && string.Equals(x.Name, paletteName, StringComparison.Ordinal));

            if (palette != null)
            {
                return palette;
            }

            Logger.Error($"No palette matches host {siteUrl.Host}{(paletteName != null ? $" and/or palette name '{paletteName}'." : null)}.");

            return null;
        }

        /// <inheritdoc />
        public virtual ICollection<IColorPalette> GetPalettes()
        {
            var palettes = LoadColorPalettes();

            foreach (var palette in palettes)
            {
                ApplyPaletteFallback(palette, palette, palettes);
            }

            return palettes;
        }

        /// <inheritdoc />
        public virtual IColor GetColor(int id, int propertyDefinitionId)
        {
            string paletteName = null;

            // If any palettes actually have a name defined then we need to resolve the
            // attribute, so we can get the relevant name.
            if (GetPalettes().Any(x => !string.IsNullOrEmpty(x.Name)))
            {
                var colorPickerAttribute = GetAttribute(propertyDefinitionId);

                if (colorPickerAttribute == null)
                {
                    Logger.Error($"Failed to resolve color picker attribute for property definition ID {propertyDefinitionId}.");
                    return null;
                }

                paletteName = colorPickerAttribute.PaletteName;
            }

            var palette = GetPalette(paletteName);

            if (palette == null)
            {
                Logger.Error(
                    $"Could not resolve a color palette when attempting to {nameof(GetColor)} with ID {id}{(!string.IsNullOrEmpty(paletteName) ? $" and palette name {paletteName}" : null)}.");
                return null;
            }

            var color = palette.Colors.SingleOrDefault(x => x.Id == id);

            if (color == null)
            {
                Logger.Warning($"No color with ID {id} found in {palette.Name ?? "(default)"} color palette.");
            }

            return color;
        }

        /// <summary>
        /// Resolves the relevant ColorPickerAttribute from a property definition ID.
        /// </summary>
        /// <param name="propertyDefinitionId">The property definition ID.</param>
        /// <returns>Color picker attribute, or null if none returned.</returns>
        internal IColorPickerAttribute GetAttribute(int propertyDefinitionId)
        {
            var propertyDefinition = _propertyDefinitionRepository.Load(propertyDefinitionId);

            if (propertyDefinition == null)
            {
                Logger.Error($"Failed to load property definition with ID {propertyDefinitionId}.");
                return null;
            }

            var contentType = _contentTypeRepository.Load(propertyDefinition.ContentTypeID);

            if (contentType?.ModelType == null)
            {
                Logger.Error($"Failed to load content type with ID {propertyDefinition.ContentTypeID}.");
                return null;
            }

            var contentTypeModel = _contentTypeModelRepository.GetContentTypeModel(contentType.ModelType);

            if (contentTypeModel == null)
            {
                Logger.Error($"Failed to get content type model that corresponds to model type {contentType.ModelType.Name}.");
                return null;
            }

            var propertyDefinitionModel =
                contentTypeModel.PropertyDefinitionModels?.SingleOrDefault(x => string.Equals(x.Name, propertyDefinition.Name, StringComparison.Ordinal));

            if (propertyDefinitionModel == null)
            {
                Logger.Error($"Content type model '{contentTypeModel.Name}' contains no property definition model with name '{propertyDefinition.Name}'.");
                return null;
            }

            var attribute = propertyDefinitionModel.Attributes?.GetAllAttributes<Attribute>()
                .OfType<IColorPickerAttribute>().SingleOrDefault();

            if (attribute == null)
            {
                Logger.Error($"Property definition model '{propertyDefinitionModel.Name}' contains no attribute of type {nameof(ColorPickerAttribute)}.");
                return null;
            }

            return attribute;
        }

        /// <summary>
        /// Loads color palettes from the config.
        /// </summary>
        /// <returns>All defined color palettes.</returns>
        internal ICollection<IColorPalette> LoadColorPalettes()
        {
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ColorPalette.config");

            XDocument xDoc;

            try
            {
                xDoc = XDocument.Load(path);
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to load configuration at: {path}", e);
                return new List<IColorPalette>();
            }

            var paletteElements = xDoc.Element("colorPalettes")?.Elements("colorPalette");

            if (paletteElements == null)
            {
                Logger.Warning("No color palettes defined in the config.");
                return new List<IColorPalette>();
            }

            ICollection<IColorPalette> palettes = new List<IColorPalette>();

            foreach (var paletteElement in paletteElements)
            {
                var palette = TransformColorPalette(paletteElement);

                if (!ValidateColorPalette(palette, palettes))
                {
                    continue;
                }

                palettes.Add(palette);
            }

            return palettes;
        }

        /// <summary>
        /// Transforms a palette XML element into a color palette.
        /// </summary>
        /// <param name="paletteElement">The palette XML element.</param>
        /// <returns>Color palette.</returns>
        internal IColorPalette TransformColorPalette(XElement paletteElement)
        {
            var name = paletteElement.Element("name")?.Value;

            // We don't accept an empty string for the name, set to null.
            if (string.IsNullOrEmpty(name))
            {
                name = null;
            }

            var idValue = paletteElement.Attribute("id")?.Value;

            if (!TryGetNullableInt(idValue, out var id))
            {
                Logger.Warning($"Color palette {name ?? "(default)"} has as invalid ID set ({idValue}). ID ignored.");
            }

            if (id.HasValue && id.Value < 1)
            {
                Logger.Warning($"Color palette {name ?? "(default)"} has as invalid ID set ({idValue}). ID ignored.");
                id = null;
            }

            var fallbackIdValue = paletteElement.Attribute("fallback")?.Value;

            if (!TryGetNullableInt(fallbackIdValue, out var fallbackId))
            {
                Logger.Warning($"Color palette {name ?? "(default)"} has as invalid fallback ID set ({fallbackIdValue}). Fallback ID ignored.");
            }

            if (fallbackId.HasValue && fallbackId.Value < 1)
            {
                Logger.Warning($"Color palette {name ?? "(default)"} has as invalid fallback ID set ({fallbackIdValue}). Fallback ID ignored.");
                fallbackId = null;
            }

            var hosts =
                paletteElement.Element("hosts")?.Elements("host").Select(x => x.Value).Where(x => !string.IsNullOrEmpty(x)).ToList() ?? new List<string>();

            var colors =
                paletteElement.Element("colors")?.Elements("color").Select(x => TransformColor(x, name)).Where(x => x != null).ToList() ?? new List<IColor>();


            return new ColorPalette(id, name, hosts, fallbackId,
                (int?)paletteElement.Element("maxColumns"), (bool?)paletteElement.Element("showClearButton"), colors);
        }

        /// <summary>
        /// Validate a color palette against the already loaded palettes.
        /// </summary>
        /// <param name="palette">The palette.</param>
        /// <param name="palettes">The current palettes.</param>
        /// <returns><c>true</c> if valid, otherwise <c>false</c>.</returns>
        internal bool ValidateColorPalette(IColorPalette palette, ICollection<IColorPalette> palettes)
        {
            // If a palette has no colors and 
            if (!palette.Colors.Any() && !palette.FallbackId.HasValue)
            {
                // Not necessarily an error, maybe they are all fallbacks.
                Logger.Error($"No colors or fallback defined for {palette.Name ?? "(default)"} palette, skipped.");
                return false;
            }

            if (palette.Id.HasValue && palettes.Any(x => x.Id == palette.Id))
            {
                Logger.Error($"Multiple color palettes have the ID {palette.Id}. {palette.Name ?? "(default)"} was skipped.");
                return false;
            }

            var ids = palette.Colors.Select(x => x.Id).ToList();

            // Check IDs are all unique.
            if (ids.Distinct().Count() != ids.Count)
            {
                Logger.Error($"{palette.Name ?? "(default)"} color palette contains multiple colors with the same ID, skipped.");
                return false;
            }

            // See if any palettes exist with the same name & any of the same hosts
            var matchingPalettes = palettes.Where(x =>
                x.Hosts.Intersect(palette.Hosts, StringComparer.OrdinalIgnoreCase).Any() &&
                string.Equals(palette.Name, x.Name, StringComparison.Ordinal)).ToList();

            if (matchingPalettes.Any())
            {
                foreach (var matchingPalette in matchingPalettes)
                {
                    var matchingHosts = matchingPalette.Hosts.Intersect(palette.Hosts, StringComparer.OrdinalIgnoreCase)
                        .Distinct(StringComparer.OrdinalIgnoreCase);

                    Logger.Warning(
                        $"Multiple color palettes defined for palette '{matchingPalette.Name}' and host(s): {string.Join(", ", matchingHosts)}, the first will be used.");
                }

                return false;
            }

            return true;
        }

        /// <summary>
        /// Updates any necessary values on the destination color palettes based on the fallback chain.
        /// </summary>
        /// <param name="destinationPalette">The destination palette.</param>
        /// <param name="currentPalette">The current palette in the fallback chain.</param>
        /// <param name="palettes">All color palettes.</param>
        internal void ApplyPaletteFallback(IColorPalette destinationPalette, IColorPalette currentPalette,
            ICollection<IColorPalette> palettes)
        {
            // No fallback defined for this palette
            if (currentPalette.FallbackId == null)
            {
                return;
            }

            var fallback = palettes.SingleOrDefault(x => x.Id == currentPalette.FallbackId);

            if (fallback == null)
            {
                Logger.Warning($"No color palette defined for host that matches {currentPalette.FallbackId}.");
                return;
            }

            // Sets the max columns value based of the fallback.
            if (!destinationPalette.MaxColumns.HasValue && currentPalette.MaxColumns.HasValue)
            {
                destinationPalette.MaxColumns = currentPalette.MaxColumns;
            }

            // Sets the show clear button boolean based of the fallback.
            if (!destinationPalette.ShowClearButton.HasValue && currentPalette.ShowClearButton.HasValue)
            {
                destinationPalette.ShowClearButton = currentPalette.ShowClearButton;
            }

            var colorIds = destinationPalette.Colors.Select(x => x.Id).ToList();

            // Get the fallback colors...
            var colors = fallback.Colors.Where(x => !colorIds.Contains(x.Id)).ToList();

            // ...add those already resolved...
            colors.AddRange(destinationPalette.Colors);

            // ...and update the palette.
            destinationPalette.Colors = colors.OrderBy(x => x.Id).ToList();

            ApplyPaletteFallback(destinationPalette, fallback, palettes);
        }

        /// <summary>
        /// Transforms an XML element into a color for a color palette.
        /// </summary>
        /// <param name="colorElement">The color XML element.</param>
        /// <param name="name">The palette name.</param>
        /// <returns>The color.</returns>
        internal IColor TransformColor(XElement colorElement, string name)
        {
            var value = colorElement.Element("value")?.Value;

            var idValue = colorElement.Attribute("id")?.Value;

            if (!TryGetNullableInt(idValue, out var id))
            {
                Logger.Warning($"Color with value '{value}' in palette {name ?? "(default)"} does not have a valid ID. Skipped.");
                return null;
            }

            // ID is required for colors
            if (!id.HasValue)
            {
                Logger.Warning($"Color with value '{value}' in palette {name ?? "(default)"} does not have an ID. Skipped.");
                return null;
            }

            if (id < 1)
            {
                Logger.Warning($"Color with ID '{value}' in palette {name ?? "(default)"} does not have a valid ID (ID must be greater than 0). Skipped.");
                return null;
            }

            if (string.IsNullOrEmpty(value))
            {
                Logger.Warning($"Color with ID '{value}' in palette {name ?? "(default)"} does not have a value. Skipped.");
                return null;
            }

            return new Color(id.Value, colorElement.Element("name")?.Value, value);
        }

        /// <summary>
        /// Try and parse a nullable integer from a string value.
        /// </summary>
        /// <param name="value">The string value.</param>
        /// <param name="intValue">The int value.</param>
        /// <returns><c>true</c> if the string was a valid nullable int, otherwise <c>false</c>.</returns>
        private static bool TryGetNullableInt(string value, out int? intValue)
        {
            intValue = null;

            if (string.IsNullOrEmpty(value))
            {
                // It was never set
                return true;
            }

            if (!int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var intParsed))
            {
                return false;
            }

            intValue = intParsed;
            return true;
        }
    }
}