﻿using System.ComponentModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using WinHue3.Philips_Hue.BridgeObject.BridgeObjects;
using WinHue3.Philips_Hue.Communication;
using WinHue3.Philips_Hue.HueObjects.Common;
using WinHue3.Utils;
using Xceed.Wpf.Toolkit.PropertyGrid.Attributes;
using XYEditor = WinHue3.Functions.PropertyGrid.XYEditor;

namespace WinHue3.Philips_Hue.HueObjects.LightObject
{
    /// <summary>
    /// Class for light State.
    /// </summary>
    [JsonObject]
    public class State : ValidatableBindableBase, IBaseProperties
    {
        private bool? _reachable;
        private bool? _on;
        private byte? _bri;
        private ushort? _hue;
        private byte? _sat;
        private decimal[] _xy;
        private string _effect;
        private string _colormode;
        private ushort? _transitiontime;
        private string _alert;
        private short? _briInc;
        private short? _satInc;
        private int? _hueInc;
        private short? _ctInc;
        private decimal[] _xyInc;
        private ushort? _ct;

        /// <summary>
        /// Reachable is the light is reachable ( true is the light is available to control, false if the bridge cannot control the light )
        /// </summary>
        [Description("Reachable is the light is reachable ( true is the light is available to control, false if the bridge cannot control the light )"),Category("Properties"), DontSerialize, ReadOnly(true)]
        public bool? reachable
        {
            get => _reachable;
            set => SetProperty(ref _reachable ,value);
        }


        /// <summary>
        /// On state of the group.
        /// </summary>
        [Description("On state."), Category("Properties"), DefaultValue(null)]
        public bool? @on
        {
            get => _on;
            set => SetProperty(ref _on,value);
        }

        /// <summary>
        /// Brightness of the group.
        /// </summary>
        [Description("Brightness(0-255)"), Category("Properties"), DefaultValue(null)]
        public byte? bri
        {
            get => _bri;
            set => SetProperty(ref _bri,value);
        }

        /// <summary>
        /// Hue/Color of the group.
        /// </summary>
        [Description("Color (0-65535)"), Category("Properties")]
        public ushort? hue
        {
            get => _hue;
            set => SetProperty(ref _hue,value);
        }

        /// <summary>
        /// Saturation of the group.
        /// </summary>
        [Description("Saturation (0-255)"), Category("Properties"), DefaultValue(null)]
        public byte? sat
        {
            get => _sat;
            set => SetProperty(ref _sat,value);
        }

        /// <summary>
        /// Float color of the group.
        /// </summary>
        [Description("Color coordinates in CIE color space. ( float value between 0.000 and 1.000)"), Category("Properties"), Editor(typeof(XYEditor),typeof(XYEditor)), DefaultValue(null)]
        public decimal[] xy
        {
            get => _xy;
            set => SetProperty(ref _xy ,value);
        }

        /// <summary>
        /// effect of the group.
        /// </summary>
        [ItemsSource(typeof(EffectItemsSource)),Description("Dynamic effect. ( none or colorloop )"), Category("Properties"), DefaultValue(null)]
        public string effect
        {
            get => _effect;
            set => SetProperty(ref _effect,value);
        }

        /// <summary>
        /// Color mode of the group.
        /// </summary>
        [ItemsSource(typeof(ColormodeItemSource)), Description("Color mode. ( hs for hue saturation, xy for XY , ct for color temperature )"),Category("Properties"), DefaultValue(null)]
        public string colormode
        {
            get => _colormode;
            set => SetProperty(ref _colormode,value);
        }

        /// <summary>
        /// Transition time of the group.
        /// </summary>
        [Description("Transition time ( Given in multiple of 100ms , Default to 4 )"),Category("Properties"), DefaultValue(null)]
        public ushort? transitiontime
        {
            get => _transitiontime;
            set => SetProperty(ref _transitiontime,value);
        }

        /// <summary>
        /// Alert of the group.
        /// </summary>
        [ItemsSource(typeof(AlertItemsSource)),Description("Alert Effect ( none , select or lselect )"), Category("Properties"), DefaultValue(null)]
        public string alert
        {
            get => _alert;
            set => SetProperty(ref _alert,value);
        }

        /// <summary>
        /// Brightness increment.
        /// </summary>
        [Description("Brightness increment."),Category("Incrementors"), DefaultValue(null)]
        public short? bri_inc
        {
            get => _briInc;
            set => SetProperty(ref _briInc,value);
        }

        /// <summary>
        /// Saturation increment.
        /// </summary>
        [Description("Saturation increment."),Category("Incrementors"), DefaultValue(null)]
        public short? sat_inc
        {
            get => _satInc;
            set => SetProperty(ref _satInc,value);
        }

        /// <summary>
        /// Hue increment.
        /// </summary>
        [Description("Saturation increment."),Category("Incrementors"), DefaultValue(null)]
        public int? hue_inc
        {
            get => _hueInc;
            set => SetProperty(ref _hueInc,value);
        }

        /// <summary>
        /// Color temperature increment.
        /// </summary>
        [Description("Color temperature increment."),Category("Incrementors"), DefaultValue(null)]
        public short? ct_inc
        {
            get => _ctInc;
            set => SetProperty(ref _ctInc,value);
        }

        /// <summary>
        /// XY increment.
        /// </summary>
        [Description("XY increment."),Category("Incrementors"), Editor(typeof(XYEditor), typeof(XYEditor)), DefaultValue(null)]
        [XYEditor.MaxMin(0.5f, 0f)]
        public decimal[] xy_inc
        {
            get => _xyInc;
            set => SetProperty(ref _xyInc,value);
        }

        /// <summary>
        /// ColorTemperature
        /// </summary>
        [Description("Color temperature"),Category("Properties"), DefaultValue(null)]
        public ushort? ct
        {
            get => _ct;
            set => SetProperty(ref _ct,value);
        }

        /// <summary>
        /// Convert state to string.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Serializer.SerializeJsonObject(this);
        }
    }
}
