using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using DoubleJay.Epi.ConfigurableColorPicker.Models;
using EPiServer.Framework.Cache;
using EPiServer.Logging;
using EPiServer.ServiceLocation;
using EPiServer.Web;

namespace DoubleJay.Epi.ConfigurableColorPicker.Manager
{
    /// <inheritdoc />
    [ServiceConfiguration(typeof(IColorPaletteManager), Lifecycle = ServiceInstanceScope.Singleton)]
    public class ColorPaletteManager : IColorPaletteManager
    {
        private readonly IObjectInstanceCache _objectInstanceCache;

        private static readonly ILogger Logger = LogManager.GetLogger(typeof(ColorPaletteManager));

        public ColorPaletteManager(IObjectInstanceCache objectInstanceCache)
        {
            _objectInstanceCache = objectInstanceCache;
        }

        /// <inheritdoc />
        public IColorPalette GetPalette()
        {
            var palettes = GetPalettes();

            var siteUrl = SiteDefinition.Current.SiteUrl;

            if (siteUrl == null)
            {
                return null;
            }

            var palette = palettes.SingleOrDefault(x => string.Equals(x.Host, siteUrl.Host, StringComparison.OrdinalIgnoreCase));

            if (palette != null)
            {
                return palette;
            }

            palette = palettes.SingleOrDefault(x => string.IsNullOrEmpty(x.Host));

            if (palette != null)
            {
                return palette;
            }

            Logger.Error($"No palette matches host {siteUrl.Host}.");

            return null;
        }

        /// <inheritdoc />
        public ICollection<IColorPalette> GetPalettes()
        {
            var cacheKey = $"{nameof(ColorPaletteManager)}:{nameof(GetPalettes)}";

            if (_objectInstanceCache.TryGet(cacheKey, ReadStrategy.Immediate, out ICollection<IColorPalette> palettes))
            {
                return palettes;
            }

            palettes = LoadColorPalettes();

            foreach (var palette in palettes)
            {
                ApplyPaletteFallback(palette, palette, palettes);
            }

            _objectInstanceCache.Insert(cacheKey, palettes, new CacheEvictionPolicy(TimeSpan.FromMinutes(5), CacheTimeoutType.Absolute));

            return palettes;
        }

        public IColor GetColor(int id)
        {
            var palette = GetPalette();

            if (palette == null)
            {
                Logger.Error($"Could not resolve a color palette when attempting to {nameof(GetColor)} with ID {id}.");
                return null;
            }

            var color = palette.Colors.SingleOrDefault(x => x.Id == id);

            if (color == null)
            {
                Logger.Error($"No color with ID {id} found in {palette.Host ?? "default"} color palette.");
            }

            return color;
        }

        /// <summary>
        /// Loads color palettes from the config.
        /// </summary>
        /// <returns>All defined color palettes.</returns>
        internal ICollection<IColorPalette> LoadColorPalettes()
        {
            string path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ColorPalette.config");

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


            var palettes = xDoc.Element("colorPalettes")?.Elements("colorPalette");

            if (palettes == null)
            {
                Logger.Warning("No color palettes defined in the config.");
                return new List<IColorPalette>();
            }

            ICollection<IColorPalette> colorPalettes = new List<IColorPalette>();

            foreach (var palette in palettes)
            {
                var host = palette.Attribute("host")?.Value;

                // We don't accept an empty string for the host
                if (string.IsNullOrEmpty("host"))
                {
                    host = null;
                }

                var colors =
                    palette.Element("colors")?.Elements("color").Select(x => TransformColor(x, host)).Where(x => x != null).ToList() ?? new List<IColor>();

                if (!colors.Any())
                {
                    // Not necessarily an error, maybe they are all fallbacks.
                    Logger.Debug($"No colors defined for {host ?? "default"} palette.");
                }

                var ids = colors.Select(x => x.Id).ToList();

                // Check IDs are all unique.
                if (ids.Distinct().Count() != ids.Count)
                {
                    Logger.Error($"{host ?? "default"} color palette contains multiple colors with the same ID.");
                    continue;
                }

                var existing = colorPalettes.SingleOrDefault(x => string.Equals(x.Host, host, StringComparison.OrdinalIgnoreCase));

                if (existing != null)
                {
                    Logger.Warning($"Multiple color palettes defined for {host ?? "default"}, the first will be used.");
                    continue;
                }

                string fallback = palette.Attribute("fallback")?.Value;

                if (host == null && !string.IsNullOrEmpty(fallback))
                {
                    Logger.Warning($"Fallback cannot have a value for the default color palette config, {fallback} value will be ignored.");
                    fallback = null;
                }

                var colorPalette = new ColorPalette(host, fallback,
                    (int?)palette.Element("maxColumns"), (bool?)palette.Element("showClearButton"), colors);

                colorPalettes.Add(colorPalette);
            }

            return colorPalettes;
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
            if (string.IsNullOrEmpty(currentPalette.Host))
            {
                // This is the default palette, don't fallback from here
                return;
            }

            var fallback = palettes.SingleOrDefault(x => string.Equals(x.Host, currentPalette.Fallback, StringComparison.OrdinalIgnoreCase));

            if (fallback == null)
            {
                if (!string.IsNullOrEmpty(currentPalette.Fallback))
                {
                    Logger.Error($"No color palette defined for host that matches {currentPalette.Fallback}.");
                }

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
        /// <param name="xElement">The XML element.</param>
        /// <param name="host">The host name.</param>
        /// <returns>The color.</returns>
        internal IColor TransformColor(XElement xElement, string host)
        {
            var value = xElement.Element("value")?.Value;

            if (!int.TryParse(xElement.Attribute("id")?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var id))
            {
                Logger.Error($"Color with value '{value}' in palette {host ?? "default"} does not have a valid ID. Skipped.");
                return null;
            }

            if (id < 1)
            {
                Logger.Error($"Color with ID '{value}' in palette {host ?? "default"} does not have a valid ID (ID must be greater than 0). Skipped.");
                return null;
            }

            if (string.IsNullOrEmpty(value))
            {
                Logger.Error($"Color with ID '{value}' in palette {host ?? "default"} does not have a value. Skipped.");
                return null;
            }

            return new Color(id, xElement.Element("name")?.Value, value);
        }
    }
}