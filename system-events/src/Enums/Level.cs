using System.Runtime.Serialization;

namespace SystemEvents.Enums
{
    public enum Level
    {
        [EnumMember( Value = "information" )]
        Information = 0,

        [EnumMember( Value = "critical" )]
        Critical = 1
    }
}