using System;
using System.Threading.Tasks;
using TrisubModel;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {

            var groupName = "MyGroup";
            var subscriptionName = "MyName";
            Func<Task> func = () => Task.Run(() => { return 5; });
            Func<Task> func2 = () => Task.Run(() => { return 10; });

            // Create Action Group
            ActionGroups.Create(groupName);
            // Subscribe to Action Group
            ActionGroups.Subscribe(func, groupName, subscriptionName);
            ActionGroups.Subscribe(func2, groupName, subscriptionName);
            ActionGroups.Subscribe(func2, groupName, "NewSubscription");
            // Trigger Action Group
            var t = ActionGroups.Trigger(groupName);
            Task.WaitAll(t);
            // Unsubscribe from Action Group
            ActionGroups.Unsubscribe(groupName, subscriptionName);
        }
    }
}
