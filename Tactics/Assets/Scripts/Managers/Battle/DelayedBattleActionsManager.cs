using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Object = UnityEngine.Object;

namespace Battle
{
    public class DelayedBattleActionsManager : MonoBehaviour
    {
        /// <summary>
        ///  Dict of callbacks and IEnumerator, when stuff needs to be played before action (for example before triggering an event, as in E before C)
        ///  Its a Dict of key C and value Queue of E that need to be invoke before C
        /// When playing an E, before C it instead queues E to C's queue
        /// </summary>
        private static Dictionary<Action,Queue<IEnumerator>> delayedActionQueueDict = new ();
        private static Queue<IEnumerator> currentQueue;
        private static Queue<Action> queuedCallbacks = new Queue<Action>();
        private static Action currentCallback;
        private static MonoBehaviour routineInvoker;
        
        public static bool IsPlayingDelayedAction { get; private set; }

        public static void Init(MonoBehaviour obj)
        {
            routineInvoker = obj;
            
            delayedActionQueueDict.Clear();
            queuedCallbacks.Clear();
            currentQueue = null;
            currentCallback = null;

            IsPlayingDelayedAction = false;
        }
        
        public static void PlayDelayedAction(IEnumerator routine,Action callback)
        {
            //add routine to dict
            GetCoroutineQueue(callback).Enqueue(DelayedActionRoutine());
            
            //add key to queue (if not already in)
            if(!queuedCallbacks.Contains(callback)) queuedCallbacks.Enqueue(callback);
            
            //if no action playing, play next routine
            if(!IsPlayingDelayedAction) PlayNextRoutine();
            
            IEnumerator DelayedActionRoutine()
            {
                IsPlayingDelayedAction = true;
                
                yield return routineInvoker.StartCoroutine(routine);
                
                PlayNextRoutine();
            }
        }

        private static void PlayNextRoutine()
        {
            if (currentCallback == null)
            {
                currentCallback = queuedCallbacks.Dequeue();
                currentQueue = delayedActionQueueDict[currentCallback];
            }
            
            if (currentQueue.Count <= 0) //nothing left to invoke before callback, invoking callback
            {
                IsPlayingDelayedAction = false;
                
                var callback = currentCallback;
                currentCallback = null;
                callback.Invoke();
                
                return;
            }
            
            routineInvoker.StartCoroutine(currentQueue.Dequeue());
        }
        
        private static Queue<IEnumerator> GetCoroutineQueue(Action key)
        {
            if(!delayedActionQueueDict.ContainsKey(key)) delayedActionQueueDict.Add(key,new Queue<IEnumerator>());

            return delayedActionQueueDict[key];
        }
    }

}

