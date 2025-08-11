using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading;

namespace Atavism
{

    public struct AtavismEventData
    {
        public string eventType;
        public string[] eventArgs;
    }

    public delegate void AtavismEventHandler(AtavismEventData args);
    public class AtavismEventSystem
    {

        // Dictionary of event names and subscribers
        public static Dictionary<string, List<Component>> eventRegistry =
                new Dictionary<string, List<Component>>();
        public static Queue<AtavismEventData> eventQueue = new Queue<AtavismEventData>();

        public static Dictionary<string, List<AtavismEventHandler>> functionEventRegistry =
            new Dictionary<string, List<AtavismEventHandler>>();
        
        public static void RegisterEvent(string eventName, AtavismEventHandler go)
        {
            Monitor.Enter(functionEventRegistry);
            try
            {
        
                if (!functionEventRegistry.ContainsKey(eventName))
                    functionEventRegistry[eventName] = new List<AtavismEventHandler>();
                if (!functionEventRegistry[eventName].Contains(go))
                    functionEventRegistry[eventName].Add(go);
            }
            finally
            {
                Monitor.Exit(functionEventRegistry);
            }
        }
        
        
        public static void UnregisterEvent(string eventName, AtavismEventHandler frame)
        {
            Monitor.Enter(functionEventRegistry);
            try
            {
                if (!functionEventRegistry.ContainsKey(eventName))
                    return;
                if (functionEventRegistry[eventName].Contains(frame))
                    functionEventRegistry[eventName].Remove(frame);
            }
            finally
            {
                Monitor.Exit(functionEventRegistry);
            }
        }
        public static void RegisterEvent(string eventName, Component go)
        {
            Monitor.Enter(eventRegistry);
            try
            {

                if (!eventRegistry.ContainsKey(eventName))
                    eventRegistry[eventName] = new List<Component>();
                if (!eventRegistry[eventName].Contains(go))
                    eventRegistry[eventName].Add(go);
            }
            finally
            {
                Monitor.Exit(eventRegistry);
            }
        }

        public static void UnregisterEvent(string eventName, Component frame)
        {
            Monitor.Enter(eventRegistry);
            try
            {
                if (!eventRegistry.ContainsKey(eventName))
                    return;
                if (eventRegistry[eventName].Contains(frame))
                    eventRegistry[eventName].Remove(frame);
            }
            finally
            {
                Monitor.Exit(eventRegistry);
            }
        }

        public static void DispatchEvent(string eventName, string[] eventArgs)
        {
         //   Debug.LogError("DispatchEvent "+eventName);
            AtavismEventData aed = new AtavismEventData();
            aed.eventType = eventName;
            aed.eventArgs = eventArgs; 
                
           eventQueue.Enqueue(aed);
        }

       public static void Update()
        {
            while (eventQueue.Count > 0)
            {
                var msg = eventQueue.Dequeue();
                DispatchMessage(msg.eventType, msg.eventArgs);
            }
        }
       
        static void DispatchMessage(string eventName, string[] eventArgs){
            if (!eventRegistry.ContainsKey(eventName) && !functionEventRegistry.ContainsKey(eventName))
            {
                AtavismLogger.LogWarning("DispatchEvent eventName: " + eventName + " not in event registry ");
                return;
            }
            Monitor.Enter(eventRegistry);
            try
            {

                if (eventRegistry.ContainsKey(eventName))
                {
                    List<Component> sub = eventRegistry[eventName];
                    // Create an EventData object to pass to the subscribers
                    List<Component> subscribers = new List<Component>(sub);
                    AtavismEventData eData = new AtavismEventData();
                    eData.eventType = eventName;
                    eData.eventArgs = eventArgs;
                    // subscribers.GetEnumerator().
                    foreach (Component go in subscribers)
                    {
                        try
                        {
                            go.SendMessage("OnEvent", eData, SendMessageOptions.DontRequireReceiver);
                        }
                        catch (Exception ex)
                        {
                            // Debug.LogError("Exception in event handler: " + ex + " with arg: " + eventArgs[0]);
                            AtavismLogger.LogWarning("Exception in event handler: " + ex + " with arg: " +
                                                     eventArgs[0]);
                        }
                    }
                }
            }
            finally
            {
                Monitor.Exit(eventRegistry);
            }
             
            Monitor.Enter(functionEventRegistry);
            try
            {
                if (functionEventRegistry.ContainsKey(eventName))
                {
                    List<AtavismEventHandler> sub = functionEventRegistry[eventName];
                    // Create an EventData object to pass to the subscribers
                    List<AtavismEventHandler> subscribers = new List<AtavismEventHandler>(sub);
                    AtavismEventData eData = new AtavismEventData();
                    eData.eventType = eventName;
                    eData.eventArgs = eventArgs;
                    // subscribers.GetEnumerator().
                    foreach (AtavismEventHandler go in subscribers)
                    {
                        try
                        {
                            go(eData);
                        }
                        catch (Exception ex)
                        {
                            // Debug.LogError("Exception in event handler: " + ex + " with arg: " + eventArgs[0]);
                            AtavismLogger.LogWarning("Exception in event handler: " + ex + " with arg: " +
                                                     eventArgs[0]);
                        }
                    }
                }
                else
                {
                    // Debug.LogWarning("No subscribers found for event name: " + eventName);
                }
            }
            finally
            {
                Monitor.Exit(functionEventRegistry);
            }
        }
    }
}