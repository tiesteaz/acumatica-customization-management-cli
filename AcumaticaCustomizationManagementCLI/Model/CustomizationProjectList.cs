using System.Runtime.Serialization;

namespace Acumatica.RESTClient.CustomizationApi.Model
{
    /// <summary>
    /// CustomizationImport
    /// </summary>
    [DataContract]
    public partial class CustomizationProjectList
    {
        public CustomizationProjectList() { }

        [DataMember(Name = "projects", EmitDefaultValue = false)]
        public List<AcumaticaCustomizationProject> Projects { get; set; }

    }

    public partial class AcumaticaCustomizationProject
    {
        public AcumaticaCustomizationProject() { }

        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }
    }
}
