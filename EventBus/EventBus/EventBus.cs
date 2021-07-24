using System;
using System.Collections.Generic;
using System.Linq;

namespace EventBus
{
    //游戏中事件派发器
    //类型安全
    //订阅事件返回Listener 便于统一取消订阅，避免手动去逐个订阅造成意外漏调错调
    //支持事件订阅者按照优先级链式处理事件内容
    public static class EventBus
    {
        private static Dictionary<Type, Delegate> handlerMap;
        private static Dictionary<Type, IEventHandlerScheduler> priorityHandlerMap;

        static EventBus()
        {
            handlerMap = new Dictionary<Type, Delegate>();
            priorityHandlerMap = new Dictionary<Type, IEventHandlerScheduler>();
        }

        public static Listener Subscribe<T>(Action<T> action) where T : IEvent
        {
            Type eventType = typeof(T);
            if (handlerMap.ContainsKey(eventType))
            {
                var handler = handlerMap[eventType] as Action<T>;
                if (!ReferenceEquals(handler, null) && handler.GetInvocationList().Contains(action))
                {
                    return null;
                }
                handler += action;
                handlerMap[eventType] = handler;
            }
            else
            {
                handlerMap.Add(eventType, action);
            }

            var listener = new Listener(eventType, action);
            return listener;
        }

        public static Listener Subscribe<T>(Action<Action, T, IEventHandlerScheduler> action, int priority)
            where T : IEvent
        {
            Type eventType = typeof(T);
            if (priorityHandlerMap.ContainsKey(eventType))
            {
                var scheduler = priorityHandlerMap[eventType] as EventHandlerScheduler<T>;
                if (ReferenceEquals(scheduler, null))
                {
                    return null;
                }
                scheduler.AddingHandler(action, priority);
            }
            else
            {
                var schedule = new EventHandlerScheduler<T>();
                priorityHandlerMap.Add(eventType, schedule);
                schedule.AddingHandler(action,priority);
            }
            var listener = new Listener(eventType, action);
            return listener;
        }

        public static void UnSubscribe(Listener listener)
        {
            if (ReferenceEquals(listener, null))
            {
                return;
            }

            if (handlerMap.ContainsKey(listener.eventType))
            {
                var handle = handlerMap[listener.eventType];
                if (handle.GetInvocationList().Contains(listener.eventHandler))
                {
                    handle = Delegate.Remove(handle, listener.eventHandler);
                    if (handle == null)
                    {
                        handlerMap.Remove(listener.eventType);
                        return;
                    }

                    handlerMap[listener.eventType] = handle;
                }
            }

            if (priorityHandlerMap.ContainsKey(listener.eventType))
            {
                var schedule = priorityHandlerMap[listener.eventType];
                schedule.RemoveHandler(listener.eventHandler);
            }
        }

        public static void Dispatch<T>(T eventData = default) where T : IEvent
        {
            if (handlerMap.ContainsKey(eventData.GetType()))
            {
                var handler = handlerMap[eventData.GetType()] as Action<T>;
                handler?.Invoke(eventData);
            }
        }

        public static void Dispatch<T>(T eventData, Action handleEndAction) where T : IEvent
        {
            if (priorityHandlerMap.ContainsKey(eventData.GetType()))
            {
                var schedule = priorityHandlerMap[eventData.GetType()] as EventHandlerScheduler<T>;
                schedule?.Schedule(eventData, handleEndAction);
            }
            else
            {
                handleEndAction?.Invoke();
            }
        }
        
        public class Listener
        {
            public readonly Type eventType;
            public readonly Delegate eventHandler;

            public Listener(Type inEventType, Delegate inEventHandler)
            {
                eventType = inEventType;
                eventHandler = inEventHandler;
            }
            
        }
    }
}