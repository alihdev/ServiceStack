// Code generated by Microsoft (R) AutoRest Code Generator 1.0.1.0
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.

namespace AutorestClient.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// HelloTypes
    /// </summary>
    /// <remarks>
    /// HelloTypes
    /// </remarks>
    public partial class HelloTypes
    {
        /// <summary>
        /// Initializes a new instance of the HelloTypes class.
        /// </summary>
        public HelloTypes()
        {
          CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the HelloTypes class.
        /// </summary>
        public HelloTypes(string stringProperty = default(string), bool boolProperty = default(bool), int intProperty = default(int))
        {
            StringProperty = stringProperty;
            BoolProperty = boolProperty;
            IntProperty = intProperty;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "String")]
        public string StringProperty { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Bool")]
        public bool BoolProperty { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "Int")]
        public int IntProperty { get; set; }

    }
}
