using System.Collections.Generic;
using UnityEngine;

namespace GamePlay
{
    public static class Sounds
    {
        private static readonly List<IHear> _listeners = new List<IHear>();

        public static void Register(IHear listener)
        {
            if (!_listeners.Contains(listener))
                _listeners.Add(listener);
        }

        public static void Unregister(IHear listener)
        {
            _listeners.Remove(listener);
        }

        public static void MakeSound(Sound sound)
        {
            
            IHear[] snapshot = _listeners.ToArray();
            foreach (IHear listener in snapshot)
                listener.RespondToSound(sound);

            //Debug.DrawLine(sound.pos, sound.pos + Vector3.up * 3f, Color.magenta, 1.5f);
            //Debug.Log($"[Sounds] MakeSound {sound.soundType} pos:{sound.pos} range:{sound.range} | listeners:{snapshot.Length}");
        }
    }
}