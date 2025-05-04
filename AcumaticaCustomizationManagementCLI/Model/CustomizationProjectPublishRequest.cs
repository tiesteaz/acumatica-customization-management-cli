using System.Runtime.Serialization;

namespace Acumatica.RESTClient.CustomizationApi.Model
{
    /// <summary>
    /// CustomizationImport
    /// </summary>
    [DataContract]
    public partial class CustomizationProjectUnublishRequest
    {
        public CustomizationProjectUnublishRequest() { }

        [DataMember(Name = "tenantMode", EmitDefaultValue = false)]
        public TenantMode Mode { get; set; }

    }
}
