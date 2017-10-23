using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace TrisubModel
{
    public static class ActionGroups
    {
        private static readonly Dictionary<string, ActionGroup> ActionGroupDictionary = new Dictionary<string, ActionGroup>();

        public static void Create(string groupName)
        {
            ActionGroupDictionary.Add(groupName, new ActionGroup(groupName));
        }

        public static void Subscribe(Func<Task> func, string groupName, string subscriptionName = "")
        {
            var actionGroupExisting = ActionGroupDictionary.TryGetValue(groupName, out var actionGroup);
            if (actionGroupExisting)
            {
                var actionListExisting = actionGroup.TryGetValue(subscriptionName, out var actionList);
                if (actionListExisting)
                {
                    // add new func to subscription
                    actionList.Add(func);
                }
                else
                {
                    // create new subscription
                    actionGroup.Add(subscriptionName, new ActionList(func));
                } 
            }
            else
            {
                // create new ActionGroup
                actionGroup = new ActionGroup(groupName);
                actionGroup.Add(subscriptionName, new ActionList(func));
                ActionGroupDictionary.Add(groupName, actionGroup);
            }
        }

        public static void Unsubscribe(string groupName, string subscriptionName)
        {
            var existingActionGroup = ActionGroupDictionary.TryGetValue(groupName, out var ag); 
            if (existingActionGroup)
            {
                // remove subscription
                ag.Remove(subscriptionName);
            }
            
        }

        public static async Task Trigger(string groupName)
        {
            List<Task> triggeredTasks = new List<Task>();
            var isExistingGroup = ActionGroupDictionary.TryGetValue(groupName, out var subscriptions);
            if (isExistingGroup)
            {
                subscriptions.ToList().ForEach(subscription =>
                {
                    ActionList actionList = subscription.Value;
                    actionList.ForEach(a => triggeredTasks.Add(a.Invoke())); //trigger action
                });

            }
            
            await Task.Run(() => Task.WaitAll(triggeredTasks.ToArray()));
        }

    }

    class ActionList : List<Func<Task>>
    {
        public ActionList(Func<Task> func)
        {
            Add(func);
        }
    }

    class ActionGroup : Dictionary<string, ActionList>
    {
        public string GroupName { get; } 
        public ActionGroup(string groupName)
        {
            GroupName = groupName;
        }
    }
}
