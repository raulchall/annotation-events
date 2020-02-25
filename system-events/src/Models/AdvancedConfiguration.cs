using System.Collections.Generic;
using SystemEvents.Enums;

namespace SystemEvents.Models
{
    public interface IAdvanceConfiguration
    {
        List<Category> Categories { get; }
        List<CategorySubscription> Subscriptions { get; }
    }
    
    public class AdvancedConfiguration : IAdvanceConfiguration
    {
        public List<Category> Categories { get; set; }
        public List<CategorySubscription> Subscriptions { get; set; }
    }

    public class Category
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Level? Level { get; set; }
    }

    public class CategorySubscription
    {
        public NotificationChannelType Type { get;set; }
        public string Category { get;set; }
        public string TopicArn { get;set; }
        public string WebhookUrl { get;set; }
    }

    public enum NotificationChannelType
    {
        Slack,
        Sns
    }
}