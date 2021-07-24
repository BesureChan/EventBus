using System;
using System.Collections.Generic;

namespace EventBus
{
    public interface IEventHandlerScheduler
    {
        void RemoveHandler(Delegate handler);
        void StopHandler();
    }
        
    //Event处理链式调度器，根据EventHandler的优先级，调用handleAction
    public class EventHandlerScheduler<T>: IEventHandlerScheduler where T: IEvent
    {
        private bool isScheduleStopped;
        private List<Handler> eventHandlers;

        public EventHandlerScheduler()
        {
            eventHandlers = new List<Handler>();
        }

        public void AddingHandler(Action<Action, T, IEventHandlerScheduler> handler, int priority)
        {
            eventHandlers.Add(new Handler(handler, priority));
        }

        public void Schedule(T eventData, Action scheduleEndAction)
        {
            isScheduleStopped = false;
            if (eventHandlers.Count>0)
            {
                eventHandlers.Sort((handlerA, handlerB) => { return -handlerA.priority.CompareTo(handlerB.priority);});
                ScheduleHandlerByIndex(0,eventData, scheduleEndAction);
                return;
            }
            scheduleEndAction?.Invoke();
        }

        private void ScheduleHandlerByIndex(int index, T eventData, Action scheduleEndAction)
        {
            if (!isScheduleStopped && eventHandlers.Count>index)
            {
                eventHandlers[index].handlerAction?.Invoke(() =>
                {
                    ScheduleHandlerByIndex(index + 1, eventData, scheduleEndAction);
                }, eventData, this);
            }
        }

        public void RemoveHandler(Delegate handler)
        {
            for (int i = 0; i < eventHandlers.Count; i++)
            {
                if (eventHandlers[i].handlerAction.Equals(handler))
                {
                    eventHandlers.RemoveAt(i);
                    break;
                }
            }
        }
        
        public void InvalidateHandler()
        {
            eventHandlers.Clear();
        }

        public void StopHandler()
        {
            isScheduleStopped = true;
        }

        struct  Handler
        {
            public int priority;
            public Action<Action, T, IEventHandlerScheduler> handlerAction;

            public Handler(Action<Action, T, IEventHandlerScheduler> inHandlerAction, int inPriority)
            {
                priority = inPriority;
                handlerAction = inHandlerAction;
            }
        }
    }
}