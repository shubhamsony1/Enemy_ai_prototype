using UnityEngine;
using GamePlay;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float speed = 6f;
    public float gravity = -9.8f;

    [Header("Footstep Audio")]
    public AudioSource footstepAudio;

    [Header("Noise")]
    public PlayerNoiseEmitter noiseEmitter;
    public float footstepNoiseRadius = 20f;

    private CharacterController _controller;
    private Vector3 _velocity;
    private float _footstepTimer;

    void Start() => _controller = GetComponent<CharacterController>();

    void Update()
    {
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        Vector3 move = transform.right * x + transform.forward * z;

        _controller.Move(move * speed * Time.deltaTime);
        _velocity.y += gravity * Time.deltaTime;
        _controller.Move(_velocity * Time.deltaTime);

        HandleFootsteps(move);
    }

    void HandleFootsteps(Vector3 move)
    {
        if (move.magnitude > 0.1f)
        {
            if (footstepAudio != null && !footstepAudio.isPlaying)
                footstepAudio.Play();

            _footstepTimer -= Time.deltaTime;
            if (_footstepTimer <= 0f)
            {
                noiseEmitter?.EmitNoise(footstepNoiseRadius, Sound.SoundType.Interesting);
                _footstepTimer = 0.4f;
            }
        }
        else
        {
            if (footstepAudio != null && footstepAudio.isPlaying)
                footstepAudio.Stop();
        }
    }
}