using UnityEngine;
using GamePlay;

public class PlayerNoiseEmitter : MonoBehaviour
{
    public float defaultNoiseRadius = 15f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
            EmitNoise(defaultNoiseRadius, Sound.SoundType.Interesting);
    }

    public void EmitNoise(float radius, Sound.SoundType type = Sound.SoundType.Interesting)
    {
        Sound sound = new Sound(transform.position, radius, type);
        Sounds.MakeSound(sound);
    }

    public void EmitNoise() => EmitNoise(defaultNoiseRadius);
}