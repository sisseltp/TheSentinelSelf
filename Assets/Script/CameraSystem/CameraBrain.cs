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
        private bool logWorldEvents;
        
        [SerializeField] 
        private CameraTracker cameraTracker;
        
        private WorldEvent currentEvent;
        
        private List<WorldEvent> worldEvents = new List<WorldEvent>();

        private List<WorldEvents> currentListening = new List<WorldEvents>();
        
        private readonly List<WorldEvents> allWorldEvents = new List<WorldEvents>()
        {
            WorldEvents.SentinelDies,
            WorldEvents.SentinelAteAntigen,
            WorldEvents.SentinelAtePlastic,
            WorldEvents.SentinelBecomesEgg,
            WorldEvents.SentinelGoesToPathogen,
            WorldEvents.TCellIsCorrupted,
            WorldEvents.SentinelGoesToLymphNode,
            WorldEvents.TCellGoesToPathogen,
            WorldEvents.TCellKillsPathogen,
            WorldEvents.TCellReachedPathogenEmitter,
            WorldEvents.InfectedSentinelGoesToTCell
        };
        
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

        private void Awake()
        {
            Instance = this;

            cameraTracker = GetComponent<CameraTracker>();

            currentListening.Clear();
            currentListening.AddRange(allWorldEvents);
        }

        public void RegisterEvent(WorldEvent newWorldEvent)
        {
            if (!cameraTracker.tracking) return;
            
            if (logWorldEvents)
            {
                DebugEvents(newWorldEvent);
            }
            
            if (currentListening.Contains(newWorldEvent.EventType) && newWorldEvent.EventTarget != null)
            {
                Debug.Log("Interesting event came in");
                
                currentListening.Remove(newWorldEvent.EventType);
                
                if (currentListening.Count <= allWorldEvents.Count / 2)
                {
                    currentListening.Clear();
                    currentListening.AddRange(allWorldEvents);
                }
                
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

            if (!worldEvent.EventTarget) return;
            
            currentEvent = worldEvent;
            cameraTracker.OverrideTracked(currentEvent.EventTarget);
        }

        public Transform GetNextInteresting()
        {
            if (worldEvents.Count <= 0) return null;
            
            var worldEvent = worldEvents[0];
            if (worldEvent.EventTarget)
            {
                currentEvent = worldEvent;
                return worldEvent.EventTarget;
            }

            worldEvents.RemoveAt(0);
            GetNextInteresting();
            return null;
        }

        private int GetHeuristicValue(WorldEvents eventType)
        {
           return heuristicValues[eventType];
        }
        
        private void DebugEvents(WorldEvent newWorldEvent)
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
                    Debug.Log($"<color=green>Sentinel Event:</color> {newWorldEvent.EventType}", newWorldEvent.EventTarget.gameObject);
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
        }
    }

    public class WorldEvent
    {
        public WorldEvents EventType;
        public Transform EventTarget;

        public WorldEvent(WorldEvents eventType, Transform eventTarget, EventData eventValue = null)
        {
            EventType = eventType;
            EventTarget = eventTarget;
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
