using System;
using System.Collections;
using System.Collections.Generic;
using Ultraleap.PhysicalHands;
using UnityEngine;
using UnityEngine.Audio;

public class MusicPlayerController : MonoBehaviour
{
    public SingleHandedMenu _singleHandedMenu;
    public PhysicalHandsButton playPause, rewind, fastForward;

    public GameObject playIcon, pauseIcon;
    public AudioSource audioSource;

    public bool isPlaying = true;
    public Transform record;
    public float rotationSpeed = -50f;
    private float _rotationSpeed;

    public float fastforwardSpeed = 1.5f;
    public float rewindSpeed = 0.75f;

    private AudioMixerGroup pitchBendGroup;
    [SerializeField]
    private GameObject _handMenuIcon;

    // Start is called before the first frame update
    void Start()
    {
        UpdatePlayState(isPlaying);
        pitchBendGroup = Resources.Load<AudioMixerGroup>("Pitch Bend Mixer");
        audioSource.outputAudioMixerGroup = pitchBendGroup;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPlaying)
        {
            record.Rotate(0, 0, _rotationSpeed * Time.deltaTime);
        }
    }

    private void OnEnable()
    {
        playPause.OnButtonPressed.AddListener(PlayPausePressed);
        rewind.OnButtonPressed.AddListener(RewindPressed);
        fastForward.OnButtonPressed.AddListener(FastForwardPressed);

        fastForward.OnButtonUnPressed.AddListener(ModifierUnpressed);
        rewind.OnButtonUnPressed.AddListener(ModifierUnpressed);

        _singleHandedMenu.OnAttachEnd.AddListener(OnAttachEnd);
        _singleHandedMenu.OnDetachEnd.AddListener(OnDetachEnd);
    }



    private void OnDisable()
    {
        playPause.OnButtonPressed.RemoveListener(PlayPausePressed);
        rewind.OnButtonPressed.RemoveListener(RewindPressed);
        fastForward.OnButtonPressed.RemoveListener(FastForwardPressed);

        fastForward.OnButtonUnPressed.RemoveListener(ModifierUnpressed);
        rewind.OnButtonUnPressed.RemoveListener(ModifierUnpressed);

        _singleHandedMenu.OnAttachEnd.RemoveListener(OnAttachEnd);
        _singleHandedMenu.OnDetachEnd.RemoveListener(OnDetachEnd);
    }

    private void OnAttachEnd(bool attached)
    {
        if (attached)
        {
            _handMenuIcon.gameObject.SetActive(true);
        }
    }

    private void OnDetachEnd(bool detached)
    {
        if (detached)
        {
            _handMenuIcon.gameObject.SetActive(false);
        }
    }

    private void FastForwardPressed()
    {
        UpdateSpeed(fastforwardSpeed);
    }

    private void RewindPressed()
    {
        UpdateSpeed(rewindSpeed);
    }

    private void ModifierUnpressed()
    {
        UpdateSpeed(1);
    }

    private void UpdateSpeed(float newSpeed)
    {
        audioSource.pitch = newSpeed;
        if (pitchBendGroup != null)
        {
            pitchBendGroup.audioMixer.SetFloat("pitchBend", 1f / newSpeed);
        }
        _rotationSpeed = rotationSpeed * newSpeed;
    }

    private void PlayPausePressed()
    {
        UpdatePlayState(!isPlaying);
    }

    private void UpdatePlayState(bool playing)
    {
        isPlaying = playing;

        if (isPlaying)
        {
            playIcon.SetActive(false);
            pauseIcon.SetActive(true);
            audioSource.Play();
            _rotationSpeed = rotationSpeed;
        }
        else
        {
            playIcon.SetActive(true);
            pauseIcon.SetActive(false);
            audioSource.Pause();
            _rotationSpeed = 0;
        }
    }
}
