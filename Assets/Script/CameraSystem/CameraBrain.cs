using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Script.CameraSystem
{
    public class CameraBrain : MonoBehaviour
    {
        public static CameraBrain Instance;

        [SerializeField] 
        private CameraTracker cameraTracker;
        
        private float currentTimer;
        private WorldEvent currentEvent;
        
        private List<WorldEvent> worldEvents = new List<WorldEvent>();

        private readonly Dictionary<WorldEvents, int> heuristicValues = new Dictionary<WorldEvents, int>()
        {
            {WorldEvents.SentinelDies, 10},
            {WorldEvents.SentinelAteAntigen, 0},
            {WorldEvents.SentinelAtePlastic, 1},
            {WorldEvents.SentinelBecomesEgg, 15},
            {WorldEvents.SentinelGoesToPathogen, 1},
            {WorldEvents.TCellIsCorrupted, 20},
            {WorldEvents.SentinelGoesToLymphNode, 5},
            {WorldEvents.TCellGoesToPathogen, 1},
            {WorldEvents.TCellKillsPathogen, 20},
            {WorldEvents.TCellReachedPathogenEmitter, 30},
            {WorldEvents.InfectedSentinelGoesToTCell, 20}
        };
        
        private readonly Dictionary<WorldEvents, int> eventTimes = new Dictionary<WorldEvents, int>()
        {
            {WorldEvents.SentinelDies, 20},
            {WorldEvents.SentinelAteAntigen, 5},
            {WorldEvents.SentinelAtePlastic, 10},
            {WorldEvents.SentinelBecomesEgg, 20},
            {WorldEvents.SentinelGoesToPathogen, 5},
            {WorldEvents.TCellIsCorrupted, 20},
            {WorldEvents.SentinelGoesToLymphNode, 20},
            {WorldEvents.TCellGoesToPathogen, 5},
            {WorldEvents.TCellKillsPathogen, 5},
            {WorldEvents.TCellReachedPathogenEmitter, 20},
            {WorldEvents.InfectedSentinelGoesToTCell, 30}
        };

        private void Awake()
        {
            Instance = this;
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
                    Debug.Log($"<color=blue>TCell Event:</color> {newWorldEvent.EventType}", newWorldEvent.EventTarget.gameObject);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            // Add to the queue
            worldEvents.Add(newWorldEvent);
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
        }

        private void NextEvent()
        {
            currentEvent = null;
            
            // Debug.Log("Next event if any");
            
            // If no new event was triggered go trough the list and check the highest interest event
        }

        private void Update()
        {
            if (currentEvent == null) return;
            
            currentTimer -= Time.deltaTime;

            if (currentTimer <= 0)
            {
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
