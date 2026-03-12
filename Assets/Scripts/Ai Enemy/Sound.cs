using UnityEngine;

namespace GamePlay
{
    public class Sound
    {
        public enum SoundType { Default = -1, Interesting, Dangerous }

        public readonly SoundType soundType;
        public readonly Vector3 pos;
        public readonly float range;

        public Sound(Vector3 pos, float range, SoundType type = SoundType.Default)
        {
            this.pos = pos;
            this.range = range;
            this.soundType = type;
        }
    }
}