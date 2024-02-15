using UnityEngine;

public class ChargeVfx : MonoBehaviour
{
    // Config parameters
    [SerializeField] private int baseParticlesNumber = 70;

    [SerializeField] private Gradient[] intensityColors;
    
    // Cached variables
    private ParticleSystem _particleSystem;
    private ParticleSystem.EmissionModule _emissionModule;
    private ParticleSystem.MainModule _mainModule;
    
    void Start()
    {
        _particleSystem = GetComponent<ParticleSystem>();
        _emissionModule = _particleSystem.emission;
        _mainModule = _particleSystem.main;
        var startColor = _mainModule.startColor;
        startColor.mode = ParticleSystemGradientMode.RandomColor;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetIntensity(int intensity)
    {
        _emissionModule.rateOverTimeMultiplier = intensity * baseParticlesNumber;

        if (intensityColors.Length == 0 || intensity == 0)
        {
            return;
        }

        var gradient = intensityColors[Mathf.Min(intensityColors.Length, intensity - 1)];
        _mainModule.startColor = gradient;
    }

}
