using System;
using DoubleJay.Epi.ConfigurableColorPicker.Manager;
using DoubleJay.Epi.ConfigurableColorPicker.Models;
using EPiServer.Core;
using EPiServer.PlugIn;
using EPiServer.ServiceLocation;

namespace DoubleJay.Epi.ConfigurableColorPicker
{
    [PropertyDefinitionTypePlugIn]
    public class PropertyPaletteColor : PropertyNumber
    {
        public override object Value
        {
            get => !Number.HasValue ? null : Locate.Advanced.GetInstance<IColorPaletteManager>().GetColor(Number.Value, PropertyDefinitionID);
            set => base.Value = (value as IColor)?.Id ?? value;
        }

        public override Type PropertyValueType => typeof(Color);

        public override object SaveData(PropertyDataCollection properties) { return Number; }
    }
}
