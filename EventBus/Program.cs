using System;

namespace EventBus
{
    public struct EventGameOnFocus : IEvent
    {
        
    }
    
    public struct EventGameOnPause : IEvent
    {
        
    }
    
    class Program
    {
        static void Main(string[] args)
        {

            EventBus.Subscribe<EventGameOnFocus>(ApplicationFocus);
            EventBus.Dispatch<EventGameOnFocus>();
        }

        private static void ApplicationFocus(EventGameOnFocus evt)
        {
            Console.WriteLine("Hello World!");
        }
    }
}