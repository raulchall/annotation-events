namespace SlackAppBackend.Models.Slack
{
    public class InteractivePayload
    {
        public string payload { get; set; }
    }

    public class Payload
    {
        public View view { get; set; }
        public string type { get; set; }
        public Team team { get; set; }
        public User user { get; set; }
        public string token { get; set; }
    }

    public class Team 
    {
        public string id { get; set; }
        public string domain { get; set; }
    }

    public class User 
    {
        public string id { get; set; }
        public string username { get; set; }
        public string name { get; set; }
        public string team_id { get; set; }
    }

    public class View
    {
        public State state { get; set; }
    }

    public class State
    {
        public Values values { get; set; }
    }

    public class Values
    {
        public CategorySelect event_category_select { get; set; }
        public CustomCategory event_custom_category { get; set; }
        public EventMessage event_message { get; set; }
        public Target event_target { get; set; }
        public Tags event_tags { get; set; }
        public Level event_level { get; set; }
    }

    public class CategorySelect
    {
        public Action category { get; set; }
    }

    public class CustomCategory
    {
        public Action custom_category { get; set; }
    }

    public class EventMessage
    {
        public Action message { get; set; }
    }

    public class Target
    {
        public Action target { get; set; }
    }

    public class Tags
    {
        public Action tags { get; set; }
    }

    public class Level
    {
        public Action level { get; set; }
    }

    public class Action
    {
        public string type { get; set; }
        public string value { get; set; }
        public SelectedOption selected_option { get; set; }
    }

    public class SelectedOption
    {
        public string value { get; set; }
    }
}