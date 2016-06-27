﻿using System.Runtime.Serialization;

namespace HueLib_base
{
    /// <summary>
    /// Hue Tap Sensor State.
    /// </summary>
    [DataContract]
    public class HueTapSensorState : SensorState
    {
        /// <summary>
        /// Button event number.
        /// </summary>
        [DataMember]
        public int? buttonevent { get; set; }

    }
}
