using System;
using System.Collections.Generic;
using System.Text;
using SlackAppBackend.Configuration;
using SlackAppBackend.Utils.Interfaces;
using SystemEvents.Api.Client.CSharp.Contracts;

namespace SlackAppBackend.Utils
{
    public class SlackModalTemplateBuilder : ISlackModalTemplateBuilder
    {
        private readonly IAppConfiguration _configuration;

        public SlackModalTemplateBuilder(IAppConfiguration configuration)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public string GetDialogTemplateWithCategories(List<Category> categories = null)
        {
            var templateSb = new StringBuilder(_newSystemEventModalTemplate);

            var categorySelect = String.Empty;
            if (_configuration.ShowPredefinedCategory && categories != null)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < categories.Count; i++)
                {
                    sb.Append(_categoryOptionTemplate.Replace("<CATEGORY_NAME>", categories[i].Name));
                    
                    if (i < categories.Count - 1)
                    {
                        sb.AppendLine(",");
                    }
                }

                categorySelect = _categorySelectTemplate.Replace("<CATEGORY_OPTIONS>", sb.ToString());
            }

            templateSb.Replace("<CATEGORY_SELECT>", categorySelect);
            // Make category select optional if the custom categories are enabled
            templateSb.Replace("<CATEGORY_SELECT_OPTIONAL>", 
                    _configuration.ShowCustomCategory? "true" : "false");

            if (_configuration.ShowCustomCategory)
            {
                templateSb.Replace("<CUSTOM_CATEGORY_INPUT>", _customCategoryInputTemplate);
                // Make custom category optional if the predefined categories are enabled
                templateSb.Replace("<CUSTOM_CATEGORY_OPTIONAL>", 
                        _configuration.ShowPredefinedCategory? "true" : "false");
            }
            else 
            {
                templateSb.Replace("<CUSTOM_CATEGORY_INPUT>", String.Empty);
            }

            if (_configuration.ShowPredefinedCategory && _configuration.ShowCustomCategory)
            {
                templateSb.Replace("<MULTIPLE_CATEGORY_INPUT_HELP>", _multipleCategoryInputHelp);
            }
            else 
            {
                templateSb.Replace("<MULTIPLE_CATEGORY_INPUT_HELP>", String.Empty);
            }
            
            return templateSb.ToString();
        }


        private const string _categoryOptionTemplate = @"{
            ""text"": {
                ""type"": ""plain_text"",
                ""text"": ""<CATEGORY_NAME>""
            },
            ""value"": ""<CATEGORY_NAME>""
        }";

        private const string _categorySelectTemplate = @"
        {
            ""block_id"": ""event_category_select"",
            ""type"": ""input"",
            ""optional"": <CATEGORY_SELECT_OPTIONAL>,
            ""label"": {
				""type"": ""plain_text"",
				""text"": ""Category"",
				""emoji"": true
			},
            ""element"": {
                ""action_id"": ""category"",
                ""type"": ""static_select"",
                ""placeholder"": {
                    ""type"": ""plain_text"",
                    ""text"": ""Select a category""
                },
                ""options"": [
                    <CATEGORY_OPTIONS>
                ]
            }
        },";

        private const string _multipleCategoryInputHelp = @"
        {
            ""type"": ""context"",
            ""elements"": [
                {
                    ""type"": ""mrkdwn"",
                    ""text"": ""Or enter your own category :point_down:""
                }
            ]
        },";
        
        private const string _customCategoryInputTemplate = @"
        {
            ""block_id"": ""event_custom_category"",
            ""type"": ""input"",
            ""optional"": <CUSTOM_CATEGORY_OPTIONAL>,
            ""element"": {
                ""type"": ""plain_text_input"",
                ""action_id"": ""custom_category"",
                ""placeholder"": {
                    ""type"": ""plain_text"",
                    ""text"": ""Your custom category here...""
                }
            },
            ""label"": {
                ""type"": ""plain_text"",
                ""text"": ""Custom Category"",
                ""emoji"": true
            }
        },";
        
        private const string _newSystemEventModalTemplate = @"{
            ""type"": ""modal"",
            ""title"": {
                ""type"": ""plain_text"",
                ""text"": ""Send system event"",
                ""emoji"": false
            },
            ""submit"": {
                ""type"": ""plain_text"",
                ""text"": ""Submit"",
                ""emoji"": true
            },
            ""close"": {
                ""type"": ""plain_text"",
                ""text"": ""Cancel"",
                ""emoji"": true
            },
            ""blocks"": [
                <CATEGORY_SELECT>
                <MULTIPLE_CATEGORY_INPUT_HELP>
                <CUSTOM_CATEGORY_INPUT>
                {
                    ""type"": ""context"",
                    ""elements"": [
                        {
                            ""type"": ""mrkdwn"",
                            ""text"": ""Ex. Service Deployment, Database Migration, Service Configuration Updated""
                        }
                    ]
                },
                {
                    ""block_id"": ""event_message"",
                    ""type"": ""input"",
                    ""element"": {
                        ""type"": ""plain_text_input"",
                        ""action_id"": ""message"",
                        ""placeholder"": {
                            ""type"": ""plain_text"",
                            ""text"": ""Your event message here...""
                        }
                    },
                    ""label"": {
                        ""type"": ""plain_text"",
                        ""text"": ""Message""
                    }
                },
                {
                    ""type"": ""context"",
                    ""elements"": [
                        {
                            ""type"": ""mrkdwn"",
                            ""text"": ""The sender will be included as part of the message""
                        }
                    ]
                },
                {
                    ""block_id"": ""event_target"",
                    ""type"": ""input"",
                    ""element"": {
                        ""type"": ""plain_text_input"",
                        ""action_id"": ""target"",
                        ""placeholder"": {
                            ""type"": ""plain_text"",
                            ""text"": ""Your target here...""
                        }
                    },
                    ""label"": {
                        ""type"": ""plain_text"",
                        ""text"": ""Target""
                    }
                },
                {
                    ""type"": ""context"",
                    ""elements"": [
                        {
                            ""type"": ""mrkdwn"",
                            ""text"": ""The name of the service or system your event is targeting. Ex. my-service, ha-proxy, traefik, users-db""
                        }
                    ]
                },
                {
                    ""block_id"": ""event_tags"",
                    ""type"": ""input"",
                    ""optional"": ""true"",
                    ""element"": {
                        ""type"": ""plain_text_input"",
                        ""action_id"": ""tags"",
                        ""placeholder"": {
                            ""type"": ""plain_text"",
                            ""text"": ""Your tags here...""
                        }
                    },
                    ""label"": {
                        ""type"": ""plain_text"",
                        ""text"": ""Tags""
                    }
                },
                {
                    ""type"": ""context"",
                    ""elements"": [
                        {
                            ""type"": ""mrkdwn"",
                            ""text"": ""A commma separated list of tags""
                        }
                    ]
                },
                {
                    ""block_id"": ""event_level"",
                    ""type"": ""input"",
                    ""label"": {
                        ""type"": ""plain_text"",
                        ""text"": ""Level"",
                        ""emoji"": true
                    },
                    ""element"": {
                        ""type"": ""radio_buttons"",
                        ""action_id"": ""level"",
                        ""initial_option"": {
                            ""text"": {
                                ""type"": ""plain_text"",
                                ""text"": ""Information""
                            },
                            ""value"": ""information""
                        },
                        ""options"": [
                            {
                                ""text"": {
                                    ""type"": ""plain_text"",
                                    ""text"": ""Information""
                                },
                                ""value"": ""information""
                            },
                            {
                                ""text"": {
                                    ""type"": ""plain_text"",
                                    ""text"": ""Critical""
                                },
                                ""value"": ""critical"",
                                ""description"": {
                                    ""type"": ""plain_text"",
                                    ""text"": ""Use only for events that impact multiple systems in production. Ex. Network Maintenance, Apigateway Deployment,...""
                                }
                            }
                        ]
                    }
                }
            ]
        }";
    }
}