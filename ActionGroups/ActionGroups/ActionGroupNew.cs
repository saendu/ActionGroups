using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Caching;

namespace ActionGroups
{
    public enum ActionGroupMember
    {
        Subscriber, // subscriber
        Publisher // publisher
    }
    public static class ActionGroups
    {
        //public delegate TResult Func<in T, out TResult>(T arg);
        //TODO: fix already existing Lists
        //TODO: lock this shit 
        
        private static List<ActionGroup> _agList = new List<ActionGroup>();

        public static void Main()
        {
            Subscribe(() => Task.Run(() =>
            {
                Console.Write("executed");
                return true;
            }), "groupName", "myName");

            var t = Trigger("groupName"); 
        }

        public static void Subscribe(Func<Task> func, string groupName, string subscriptionName)
        {
            var ag = new ActionGroup(groupName);
            var taskList = new ActionList();
            taskList.Add(func);
            ag.Add(subscriptionName, taskList);
            _agList.Add(ag);
        }

        public static void Unsubscribe(string groupName, string subscriptionName)
        {
            var ags = _agList.Where(ag => ag.ContainsKey(groupName));
            foreach (ActionGroup ag in ags)
            {
                ag.Remove(subscriptionName);
            }
        }

        public static async Task Trigger(string groupName)
        {
            var matchedGroups = _agList.Where(g => g.ContainsKey(groupName));
            List<Task> runningTasks = new List<Task>();

            foreach (var matchedGroup in matchedGroups)
            {
                foreach (var subscription in matchedGroup)
                {
                    ActionList actionList = subscription.Value;
                    actionList.ForEach(a => runningTasks.Add(a.Invoke())); //trigger action
                }
            }

            await Task.Run(() => Task.WaitAll(runningTasks.ToArray()));
        }

    }

    public class ActionList : List<Func<Task>>
    {
        private List<Func<Task>> _list; 
        public ActionList()
        {
            _list = new List<Func<Task>>();
        }
    }

    public class ActionGroup : Dictionary<string, ActionList> //Dictionary<Subscription, List<Functions>>
    {
        private ActionList _actionList;
        private Dictionary<string, ActionList> _dic; 
        public ActionGroup(string groupName) : base()
        {
            _actionList = new ActionList();
            _dic = new Dictionary<string, ActionList>();
            _dic.Add(groupName, _actionList);
        }
    }
}
