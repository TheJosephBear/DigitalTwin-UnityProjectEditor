using UnityEngine;
using Cinemachine;

[RequireComponent(typeof(FirstPersonMovement))]
public class MovementScreenShake: MonoBehaviour {

    [Header("Cinemachine Target")]
    public CinemachineVirtualCamera targetVirtualCamera;

    [Header("Noise Profiles")]
    public float idleAmplitude = 0.5f;
    public float idleFrequency = 0.5f;

    public float walkAmplitude = 1.5f;
    public float walkFrequency = 1.0f;

    public float sprintAmplitude = 3.0f;
    public float sprintFrequency = 2.0f;

    private FirstPersonMovement fpm;
    private CinemachineBasicMultiChannelPerlin perlinNoise;

    void Start() {
        fpm = GetComponent<FirstPersonMovement>();

        if (targetVirtualCamera != null) {
            perlinNoise = targetVirtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (perlinNoise == null) {
                Debug.LogWarning("Selected VCam doesn't have a Basic Multi Channel Perlin noise component assigned.");
            }
        } else {
            Debug.LogError("Target Virtual Camera is not assigned on MovementScreenShake.");
        }
    }

    void Update() {
        ApplyScreenShake();
    }

    void ApplyScreenShake() {
        if (perlinNoise == null) return;

        float playerSpeed = fpm.GetCurrentSpeed();
        bool isSprinting = fpm.IsSprinting();

        if (playerSpeed <= 0.5f) {
            // Idle state
            SetNoiseProperties(idleAmplitude, idleFrequency);
        } else if (isSprinting) {
            // Sprinting state
            SetNoiseProperties(sprintAmplitude, sprintFrequency);
        } else {
            // Walking state
            SetNoiseProperties(walkAmplitude, walkFrequency);
        }
    }

    void SetNoiseProperties(float amplitude, float frequency) {
        perlinNoise.m_AmplitudeGain = Mathf.Lerp(perlinNoise.m_AmplitudeGain, amplitude, Time.deltaTime * 5f);
        perlinNoise.m_FrequencyGain = Mathf.Lerp(perlinNoise.m_FrequencyGain, frequency, Time.deltaTime * 5f);
    }
}
