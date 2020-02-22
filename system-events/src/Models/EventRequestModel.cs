using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using SystemEvents.Enums;

namespace SystemEvents.Models
{
    /// <summary>
    /// A system event model
    /// </summary>
    public class EventRequestModel
    {
        /// <summary>
        /// The category of the system event.
        /// Ex. Maintenance, Service Deployment, Database Deployment
        /// </summary>
        public string Category { get; set; }

        /// <summary>
        /// The system event target key.
        /// The TargetKey is considered a filter field
        /// and can be use to reduce the scope of the results
        /// in your filter. Given that category groups 
        /// events by the action and not whether they belong 
        /// to the same system or not. 
        /// Ex. Use your service or application name on TargetKey 
        /// so you can reduce noise when visualizing the events.
        /// </summary>
        public string TargetKey { get; set; }

        /// <summary>
        /// The system event <see cref="Level"/>
        /// Most events are considered of level Information,
        /// use the Level field to differentiate between 
        /// events that affect independent systems within
        /// your stack and events that could potentially affect
        /// multiple or all systems in your stack. 
        /// Ex. A user might not care about a service deployment
        /// on a different system but would probably be interested on
        /// a Networking Maintenance Event that could potentialy be 
        /// the cause of an outage. Suggestion is to not abuse 
        /// of the Level, save Critical Events for the things that 
        /// all the actors in your stack should be aware of.
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public Level Level { get; set; }
        
        /// <summary>
        /// The system event message.
        /// Do not include the Sender information as part of the message
        /// instead use the Sender parameter. Sender will be included automatically
        /// at the end of the message.
        /// </summary>
        public string Message { get; set; }
        
        /// <summary>
        /// The system event sender. The username or application name
        /// sending the system event.
        /// </summary>
        public string Sender { get; set; }

        /// <summary>
        /// A collection of system events tags.
        /// </summary>
        public ICollection<string> Tags { get; set; }
    }
}