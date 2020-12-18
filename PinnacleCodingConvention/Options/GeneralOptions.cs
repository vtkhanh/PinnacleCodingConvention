using System.ComponentModel;

namespace PinnacleCodingConvention.Options
{
    internal class GeneralOptions : BaseOptionModel<GeneralOptions>
    {
        [DisplayName("Code Cleanup Profile")]
        [Description("Specify Code Cleanup Profile to be run automatically.")]
        [DefaultValue(CodeCleanupProfile.Profile1)]
        [TypeConverter(typeof(EnumConverter))]
        public CodeCleanupProfile Profile { get; set; } = CodeCleanupProfile.Profile1;
    }
}
