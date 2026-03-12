using UnityEngine;
using GamePlay;
using System.Collections;

public class BulletFire : MonoBehaviour
{
    [Header("Bullet")]
    public GameObject bulletPrefab;
    public Transform firePoint;
    public float cooldown = 0.15f;

    [Header("Audio")]
    public AudioSource gunshotAudio;
    public float shortGunshotDuration = 0.5f; // short tap duration

    [Header("Noise")]
    public PlayerNoiseEmitter noiseEmitter;
    public float gunNoiseRadius = 150f;

    private float _nextFireTime;
    private Coroutine _shortShotCoroutine;

    void Update()
    {
        // On mouse button press (long press)
        if (Input.GetMouseButton(0))
        {
            if (!_IsAudioPlayingLongPress())
            {
                // start looping full clip
                if (gunshotAudio != null)
                {
                    gunshotAudio.loop = true;
                    gunshotAudio.Play();
                }
            }

            // Fire bullets on cooldown
            if (Time.time >= _nextFireTime)
                Fire();
        }
        // On mouse button release
        if (Input.GetMouseButtonUp(0))
        {
            StopLongPressAudio();
        }

        // On quick tap (mouse click down)
        if (Input.GetMouseButtonDown(0))
        {
            // Fire immediately (bullet)
            if (Time.time >= _nextFireTime)
                FireShortAudio();
        }
    }

    void Fire()
    {
        _nextFireTime = Time.time + cooldown;
        Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);

        // Emit noise for AI
        noiseEmitter?.EmitNoise(gunNoiseRadius, Sound.SoundType.Dangerous);
    }

    void FireShortAudio()
    {
        // Stop long press audio if any
        StopLongPressAudio();

        if (gunshotAudio != null)
        {
            // Play short 0.5s sound
            if (_shortShotCoroutine != null)
                StopCoroutine(_shortShotCoroutine);

            _shortShotCoroutine = StartCoroutine(PlayGunshotShort());
        }
    }

    private IEnumerator PlayGunshotShort()
    {
        gunshotAudio.loop = false;
        gunshotAudio.Play();
        yield return new WaitForSeconds(shortGunshotDuration);
        gunshotAudio.Stop();
    }

    private void StopLongPressAudio()
    {
        if (gunshotAudio != null)
        {
            gunshotAudio.loop = false;
            gunshotAudio.Stop();
        }
    }

    private bool _IsAudioPlayingLongPress()
    {
        return gunshotAudio != null && gunshotAudio.isPlaying && gunshotAudio.loop;
    }
}
