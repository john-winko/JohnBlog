using System.ComponentModel;

namespace JohnBlog.Enums
{
    public enum ModerationType
    {
        [Description("Political propoganda")]
        Political,

        [Description("Offensive languague")]
        Language,

        [Description("Drug references")]
        Drugs,

        [Description("Threatening speech")]
        Threatening,

        [Description("Sexual content")]
        Sexual,

        [Description("Hate speech")]
        HateSpeech,

        [Description("Targeted shaming")]
        Shaming
    }
}
