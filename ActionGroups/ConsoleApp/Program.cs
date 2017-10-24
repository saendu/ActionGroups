using System;
using System.Threading;
using System.Threading.Tasks;

namespace ActionGroups
{
    class Program
    {
        static void Main(string[] args)
        {

            var groupName = "MyGroup";
            var subscriptionName = "MyName";
            Func<Task> func = () => Task.Run(() => {
                Thread.Sleep(3000);
                Console.WriteLine("Func1");
            });
            Func<Task> func2 = () => Task.Run(() =>
            {
                Thread.Sleep(3000);
                Console.WriteLine("Func2");
            });

            // Create Action Group
            ActionGroups.Create(groupName);
            // Subscribe to Action Group
            ActionGroups.Subscribe(func, groupName, subscriptionName);
            ActionGroups.Subscribe(func2, groupName, subscriptionName);
            ActionGroups.Subscribe(func2, groupName, "NewSubscription");
            // TriggerAsync Action Group
            var t = ActionGroups.TriggerAsync(groupName);
            //Task.WaitAll(t);
            // Unsubscribe from Action Group
            ActionGroups.Unsubscribe(groupName, subscriptionName);
            Thread.Sleep(5000);
        }
    }
}
