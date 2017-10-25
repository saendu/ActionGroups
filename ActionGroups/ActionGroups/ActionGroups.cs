using System;
using System.CodeDom;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Threading.Tasks;

namespace ActionGroups
{
    public static class ActionGroups
    {
        private static readonly ConcurrentDictionary<string, ActionGroup> ActionGroupDictionary = new ConcurrentDictionary<string, ActionGroup>();
        private static readonly object ThisLock = new object();

        public static void Create(string groupName)
        {
            ActionGroupDictionary.TryAdd(groupName, new ActionGroup(groupName));
        }

        public static void Subscribe(Func<Task> func, string groupName, string subscriptionName = "")
        {
            subscriptionName = string.IsNullOrEmpty(subscriptionName) ? Guid.NewGuid().ToString() : subscriptionName;
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
                ActionGroupDictionary.TryAdd(groupName, actionGroup);
            }
        }

        public static void Unsubscribe(string groupName, string subscriptionName)
        {
            var isExistingActionGroup = ActionGroupDictionary.TryGetValue(groupName, out var actionGroup); 
            if (isExistingActionGroup)
            {
                // remove subscription
                actionGroup.Remove(subscriptionName);
            }
            
        }

        public static void UnsubscribeAll(string groupName)
        {
            var isExistingActionGroup = ActionGroupDictionary.TryGetValue(groupName, out var actionGroup);
            if (isExistingActionGroup)
            {
                // remove all subscription
                var allSubscriptions = actionGroup.Keys.ToList();
                allSubscriptions.ForEach(s => actionGroup.Remove(s));
            }

        }

        public static async Task TriggerAsync(string groupName)
        {
            List<Task> triggeredTasks = new List<Task>();
            bool isExistingGroup;
            ActionGroup subscriptions;
            lock (ThisLock)
            {
                isExistingGroup = ActionGroupDictionary.TryGetValue(groupName, out subscriptions);
            }
            
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
