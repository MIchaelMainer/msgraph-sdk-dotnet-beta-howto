// This file is provided in case you need to add custom models that are not described by the 

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

// Your beta models must be in this namepspace.
namespace Microsoft.Graph
{
    // Use the JsonObjectAttribute for all models you want serialized. Check whether it inherits from other models.
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public partial class CustomClass
    {
        // Use this as a template for adding a property that returns a collection.
        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "collectionOfObjects", Required = Newtonsoft.Json.Required.Default)]
        //public IEnumerable<MyObjectType> CollectionOfObjects { get; set; }

        //[JsonProperty(NullValueHandling = NullValueHandling.Ignore, PropertyName = "myProperty", Required = Newtonsoft.Json.Required.Default)]
        //public string MyProperty { get; set; }

    }
}
