using System.ComponentModel;

namespace PinnacleCodingConvention.Options
{
    internal class GeneralOptions : BaseOptionModel<GeneralOptions>
    {
        [DisplayName("Separate Constants and Variables")]
        [Description("Separate class constants and class variables into different regions")]
        [DefaultValue(false)]
        public bool IsSeparateConstAndVar { get; set; }


        [DisplayName("Order By Access Level first")]
        [Description("Order the class variables by Access Level then Name")]
        [DefaultValue(true)]
        public bool OrderByAccessLevelFirst { get; set; }

        [DisplayName("Profile")]
        [Description("Specifies whether to run Code Clean up automatically on save or not.")]
        [DefaultValue(CodeCleanupProfile.Profile1)]
        [TypeConverter(typeof(EnumConverter))]
        public CodeCleanupProfile Profile { get; set; } = CodeCleanupProfile.Profile1;
    }
}
