namespace Tengio
{
    using UnityEngine;
    using UnityEngine.Events;
    using System.Collections.Generic;

    public class EventManager : MonoBehaviour
    {

        public enum Action
        {
            EnteredCollider,
            ExitedCollider,
            DisableControls,
            EnableControls,
            IsDragging,
            NotDragging,
            ClickedCharacter,
        };

        private Dictionary<Action, UnityEvent> eventDictionary;

        private static EventManager eventManager;
        public static EventManager instance
        {
            get
            {
                if (!eventManager)
                {
                    eventManager = FindObjectOfType(typeof(EventManager)) as EventManager;
                    if (!eventManager)
                    {
                        Debug.Log("Error: Check event manager");
                    }
                    else
                    {
                        eventManager.Init();
                    }
                }
                return eventManager;
            }
        }


        void Init()
        {
            if (eventDictionary == null)
            {
                eventDictionary = new Dictionary<Action, UnityEvent>();
            }
        }

        public static void StartListening(Action eventName, UnityAction listener)
        {
            UnityEvent thisEvent = null;
            if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.AddListener(listener);
            }
            else
            {
                thisEvent = new UnityEvent();
                thisEvent.AddListener(listener);
                instance.eventDictionary.Add(eventName, thisEvent);
            }
        }

        public static void StopListening(Action eventName, UnityAction listener)
        {
            if (eventManager == null) return;
            UnityEvent thisEvent = null;

            if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.RemoveListener(listener);
            }
        }

        public static void TriggerEvent(Action eventName)
        {
            UnityEvent thisEvent = null;

            if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
            {
                thisEvent.Invoke();
            }
        }
    }
}