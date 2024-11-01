using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Script.CameraSystem
{
    // TODO: @Neander: Less switching, react to urgent world events else just let it follow the current target around and switch it up from time to time
    // Have a list of the last N events and after a timer is done check valid events that are different and switch to it
    
    public class CameraBrain : MonoBehaviour
    {
        public static CameraBrain Instance;

        [SerializeField] 
        private CameraTracker cameraTracker;

        [SerializeField]
        private float switchTime = 30f; // In seconds
            
        private float currentTimer;
        private WorldEvent currentEvent;
        
        private List<WorldEvent> worldEvents = new List<WorldEvent>();

        private readonly Dictionary<WorldEvents, int> heuristicValues = new Dictionary<WorldEvents, int>()
        {
            {WorldEvents.SentinelDies, 10},
            {WorldEvents.SentinelAteAntigen, 0},
            {WorldEvents.SentinelAtePlastic, 60},
            {WorldEvents.SentinelBecomesEgg, 70},
            {WorldEvents.SentinelGoesToPathogen, 1},
            {WorldEvents.TCellIsCorrupted, 20},
            {WorldEvents.SentinelGoesToLymphNode, 5},
            {WorldEvents.TCellGoesToPathogen, 8},
            {WorldEvents.TCellKillsPathogen, 20},
            {WorldEvents.TCellReachedPathogenEmitter, 40},
            {WorldEvents.InfectedSentinelGoesToTCell, 50}
        };
        
        private readonly Dictionary<WorldEvents, int> eventTimes = new Dictionary<WorldEvents, int>()
        {
            {WorldEvents.SentinelDies, 20},
            {WorldEvents.SentinelAteAntigen, 15},
            {WorldEvents.SentinelAtePlastic, 30},
            {WorldEvents.SentinelBecomesEgg, 30},
            {WorldEvents.SentinelGoesToPathogen, 15},
            {WorldEvents.TCellIsCorrupted, 30},
            {WorldEvents.SentinelGoesToLymphNode, 30},
            {WorldEvents.TCellGoesToPathogen, 15},
            {WorldEvents.TCellKillsPathogen, 15},
            {WorldEvents.TCellReachedPathogenEmitter, 20},
            {WorldEvents.InfectedSentinelGoesToTCell, 30}
        };

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            currentTimer = switchTime;
        }

        public void RegisterEvent(WorldEvent newWorldEvent)
        {
            switch (newWorldEvent.EventType)
            {
                case WorldEvents.SentinelGoesToPathogen:
                case WorldEvents.SentinelGoesToLymphNode:
                case WorldEvents.SentinelAtePlastic:
                case WorldEvents.SentinelAteAntigen:
                case WorldEvents.SentinelDies:
                case WorldEvents.SentinelBecomesEgg:
                case WorldEvents.InfectedSentinelGoesToTCell:
                    // Debug.Log($"<color=green>Sentinel Event:</color> {newWorldEvent.EventType}", newWorldEvent.EventTarget.gameObject);
                    break;
                case WorldEvents.TCellGoesToPathogen:
                case WorldEvents.TCellKillsPathogen:
                case WorldEvents.TCellReachedPathogenEmitter:
                case WorldEvents.TCellIsCorrupted:
                    // Debug.Log($"<color=blue>TCell Event:</color> {newWorldEvent.EventType}", newWorldEvent.EventTarget.gameObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Add to the queue
            if (currentEvent != null && newWorldEvent.EventType != currentEvent.EventType)
            {
                if (worldEvents.Count >= 5)
                {
                    worldEvents.RemoveAt(0);
                    worldEvents.Add(newWorldEvent);
                }
            }
            // Evaluate event
            EvaluateEvent(newWorldEvent);
        }

        private void EvaluateEvent(WorldEvent eventToEvaluate)
        {
            // Should we have interest in this event?
            // If we have interest remove from the list and setup the camera follow
            if (currentEvent == null || GetHeuristicValue(eventToEvaluate.EventType) > GetHeuristicValue(currentEvent.EventType))
            {
                SetNextEvent(eventToEvaluate);
            }
            else
            {
                // Else sort the list of events and continue with the current one
                worldEvents = worldEvents.OrderBy(x => GetHeuristicValue(x.EventType)).ToList();
            }
        }

        private void SetNextEvent(WorldEvent worldEvent)
        {
            worldEvents.Remove(worldEvent);
            
            currentEvent = worldEvent;
            currentTimer = GetEvenTime(currentEvent.EventType);

            // cameraTracker.SetTracked(currentEvent.EventTarget);
        }

        private void NextEvent()
        {
            if (worldEvents.Count <= 0) return;
            if (worldEvents.Count(x => x.EventTarget != null) <= 0) return;
            
            currentEvent = null;
            // If no new event was triggered go trough the list and check the highest interest event

            var currentValue = -100;
            WorldEvent chosenEvent = null;
            
            foreach (var worldEvent in worldEvents)
            {
                if (GetHeuristicValue(worldEvent.EventType) > currentValue && worldEvent.EventTarget != null)
                {
                    chosenEvent = worldEvent;
                }
            }
            
            SetNextEvent(chosenEvent);
        }

        private void Update()
        {
            currentTimer -= Time.deltaTime;
            
            if (currentTimer <= 0)
            {
                currentTimer = switchTime;
                // Check new event
                NextEvent();
            }
        }

        private int GetHeuristicValue(WorldEvents eventType)
        {
           return heuristicValues[eventType];
        }

        private int GetEvenTime(WorldEvents eventType)
        {
            return eventTimes[eventType];
        }
    }

    public class WorldEvent
    {
        public WorldEvents EventType;
        public Transform EventTarget;
        public EventData EventValue;

        public WorldEvent(WorldEvents eventType, Transform eventTarget, EventData eventValue = null)
        {
            EventType = eventType;
            EventTarget = eventTarget;
            EventValue = eventValue;
        }
    }

    public class EventData
    {
        public float Current;
        public float Threshold;

        public EventData(float current, float threshold)
        {
            Current = current;
            Threshold = threshold;
        }
    }

    public enum WorldEvents
    {
        SentinelGoesToPathogen,
        SentinelGoesToLymphNode,
        SentinelAtePlastic,
        SentinelAteAntigen,
        SentinelDies,
        SentinelBecomesEgg,
        InfectedSentinelGoesToTCell,
        TCellGoesToPathogen,
        TCellKillsPathogen,
        TCellReachedPathogenEmitter,
        TCellIsCorrupted,
    }
}
