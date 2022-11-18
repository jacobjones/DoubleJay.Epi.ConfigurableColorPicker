using System;
using System.Collections.Generic;
using DoubleJay.Epi.ConfigurableColorPicker.Models;
using EPiServer.DataAbstraction;
using EPiServer.Framework.Cache;
using EPiServer.ServiceLocation;

namespace DoubleJay.Epi.ConfigurableColorPicker.Manager.Caching
{
    public class ColorPaletteManagerCachingProxy : ColorPaletteManager
    {
        private readonly IObjectInstanceCache _objectInstanceCache;

        private readonly TimeSpan _cachingTimeSpan = TimeSpan.FromHours(12);

        public ColorPaletteManagerCachingProxy(
            IPropertyDefinitionRepository propertyDefinitionRepository, IContentTypeRepository contentTypeRepository,
            ContentTypeModelRepository contentTypeModelRepository, IObjectInstanceCache objectInstanceCache) : base(
            propertyDefinitionRepository, contentTypeRepository, contentTypeModelRepository)
        {
            _objectInstanceCache = objectInstanceCache;
        }

        public override IColor GetColor(int id, int propertyDefinitionId)
        {
            var cacheKey = $"{nameof(ColorPaletteManager)}:{nameof(GetColor)}:{id}:{propertyDefinitionId}";

            if (_objectInstanceCache.TryGet(cacheKey, ReadStrategy.Immediate, out IColor color))
            {
                return color;
            }

            color =  base.GetColor(id, propertyDefinitionId);

            _objectInstanceCache.Insert(cacheKey, color, new CacheEvictionPolicy(_cachingTimeSpan, CacheTimeoutType.Absolute));

            return color;
        }

        public override ICollection<IColorPalette> GetPalettes()
        {
            var cacheKey = $"{nameof(ColorPaletteManager)}:{nameof(GetPalettes)}";

            if (_objectInstanceCache.TryGet(cacheKey, ReadStrategy.Immediate, out ICollection<IColorPalette> palettes))
            {
                return palettes;
            }

            palettes = base.GetPalettes();

            _objectInstanceCache.Insert(cacheKey, palettes, new CacheEvictionPolicy(_cachingTimeSpan, CacheTimeoutType.Absolute));

            return palettes;
        }

    }
}
