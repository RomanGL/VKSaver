using System;

namespace VKSaver.Core.Services.Interfaces
{
    public interface IActivityTypesResolver
    {
        Type GetActivityType(string viewName);
        Type GetViewModelType(string activityName);
    }
}