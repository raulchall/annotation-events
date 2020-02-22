using System.Runtime.Serialization;

namespace SystemEvents.Enums
{
    public enum Level
    {
        [EnumMember( Value = "critical" )]
        Critical,

        [EnumMember( Value = "information" )]
        Information
    }
}