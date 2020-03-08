using System.Collections.Generic;
using SystemEvents.Api.Client.CSharp.Contracts;

namespace SlackAppBackend.Utils.Interfaces
{
    public interface ISlackModalTemplateBuilder
    {
         string GetDialogTemplateWithCategories(List<Category> categories = null);
    }
}