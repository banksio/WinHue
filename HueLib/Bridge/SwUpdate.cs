﻿using System.Collections.Generic;
using System.Runtime.Serialization;

namespace HueLib
{
    /// <summary>
    /// Class for the software update.
    /// </summary>
    [DataContract]
    public class SwUpdate
    {
        /// <summary>
        /// State of the update  
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public int? updatestate { get; set; }
        /// <summary>
        /// url of the update
        /// </summary>
        [DataMember(IsRequired = false)]
        public string url { get; set; }
        /// <summary>
        /// Message of the update
        /// </summary>
        [DataMember(IsRequired = false)]
        public string text { get; set; }
        /// <summary>
        /// // Notify for the update
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool notify { get; set; }

        /// <summary>
        /// Check for update.
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public bool checkforupdate { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        public Devicetypes devicetypes { get; set; }
    }

    /// <summary>
    /// devicetype
    /// </summary>
    [DataContract]
    public class Devicetypes
    {
        /// <summary>
        /// Type of the update.
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        bool? bridge { get; set; }
        /// <summary>
        /// Lights to update.
        /// </summary>
        [DataMember(EmitDefaultValue = false, IsRequired = false)]
        List<string> lights { get; set; }
    }
}
