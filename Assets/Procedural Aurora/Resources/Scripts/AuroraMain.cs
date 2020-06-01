using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProceduralAurora
{
    public enum AudioVisualizationSource { None, AvgAmplitude, Frequencies }

    public class AuroraMain : MonoBehaviour
    {
        [Header("Audio Visualizer")]
        public bool useAudioSourceVisualization;
        public AudioSource audioSource;
        public int audioSamples; //Must be a power of 2
        public FFTWindow audioSamplingWindow;
        public float audioMultiplier;
        [Range(0, 1f)]
        public float audioFrequenciesScale;
        [Range(0, 1f)]
        public float audioBufferSmoothness;
        public AudioVisualizationSource audioVisualizeColorGradient;
        public AudioVisualizationSource audioVisualizeOpacity;
        public AudioVisualizationSource audioVisualizePositionOffset;
        public AudioVisualizationSource audioVisualizeFrequency;
        public AudioVisualizationSource audioVisualizeHeight;
        [Range(0, 1f)]
        public float audioVisualizePositionOffsetMutliplier;
        public float audioHeightMultiplier;
        public Gradient audioColorGradient;

        [Header("Base Settings")]
        public Vector3 scale;
        public int auroraSeed;
        [Range(10, 10000)]
        public int auroraParticlesCount;
        [Range(0, 0.99f)]
        public float auroraAnimationFrequency;
        [Range(0, 360f)]
        public float auroraRotation;
        public float auroraCurvature;
        public Vector3 auroraSizes;
        public float auroraParticleThickness;

        [Header("Volumetric Aurora")]
        public bool auroraVolumetric;
        public bool auroraCircular;
        public Vector2 auroraVolumetricRange;

        [Header("Aurora Lights")]
        public bool auroraLights;
        [Range(1, 100)]
        public int auroraLightsCount;
        public float auroraLightsRange;
        public float auroraLightsIntesity;

        [Header("Colors")]
        public Gradient auroraColorMain;

        [Header("Resources")]
        public Material auroraMaterialMain;

        void Start()
        {
            Initialize();
        }

        //Local variables
        private ParticleSystem pSystem;
        private ParticleSystem.MainModule p_mMain;
        private ParticleSystem.EmissionModule p_mEmission;
        private ParticleSystemRenderer pRenderer;

        private ParticleSystem.Particle[] p_Particles;
        private Light[] l_Lights;
        private float[] aSamples;
        private float[] aBuffer;

        // Main Aurora Initialization
        private void Initialize()
        {
            Random.InitState(auroraSeed);

            GameObject m_Particle = new GameObject("m_Particle");
            m_Particle.transform.SetParent(transform);
            m_Particle.transform.localScale = scale;
            m_Particle.transform.localPosition = Vector3.zero;

            //Create particle system
            pSystem = m_Particle.AddComponent<ParticleSystem>();
            pRenderer = m_Particle.GetComponent<ParticleSystemRenderer>();
            p_mEmission = pSystem.emission;
            p_mMain = pSystem.main;

            p_mEmission.enabled = false;
            p_mMain.startSpeed = 0;
            p_mMain.maxParticles = auroraParticlesCount;

            pRenderer.material = auroraMaterialMain;
            pRenderer.renderMode = ParticleSystemRenderMode.VerticalBillboard;
            pRenderer.maxParticleSize = 100f;

            p_Particles = new ParticleSystem.Particle[auroraParticlesCount];
            pSystem.Emit(auroraParticlesCount);
            pSystem.GetParticles(p_Particles);

            //Create lights
            if (auroraLights)
                InitializeLights();

            //Prepare audio visualizer
            if (useAudioSourceVisualization)
            {
                if (audioSource == null)
                    Debug.LogError("[Procedural Aurora] AudioSource wasn't found!");
                if ((audioSamples & (audioSamples - 1)) != 0 || audioSamples <= 0)
                    Debug.LogError("[Procedural Aurora] Audio Samples value is incorrect! It should be a power of 2");
                if (audioSamples > auroraParticlesCount)
                    Debug.LogError("[Procedural Aurora] Audio Samples value is incorrect! It should be less or equal to Aurora Particles Count");
                aSamples = new float[audioSamples];
                aBuffer = new float[audioSamples];
            }
        }

        //Lights Initialization
        private void InitializeLights()
        {
            l_Lights = new Light[auroraLightsCount];

            Transform m_Lights = new GameObject("m_Lights").transform;
            m_Lights.SetParent(transform);
            m_Lights.transform.localScale = scale;
            m_Lights.transform.localPosition = Vector3.zero;

            for (int i = 0; i < auroraLightsCount; i++)
            {
                Transform obj_Light = new GameObject("AuroraLight " + i).transform;
                obj_Light.SetParent(m_Lights, true);

                Light l_Light = obj_Light.gameObject.AddComponent<Light>();

                l_Lights[i] = l_Light;
            }
        }

        //Base Aurora Update
        private void FixedUpdate()
        {
            float aAmplitude = 0;
            if (useAudioSourceVisualization)
            {
                audioSource.GetSpectrumData(aSamples, 0, audioSamplingWindow);
                for (int s = 0; s < aSamples.Length; s++)
                {
                    aSamples[s] *= audioMultiplier;
                    aBuffer[s] = Mathf.Lerp(aSamples[s], aBuffer[s], audioBufferSmoothness);
                    aAmplitude += aBuffer[s];
                }
                aAmplitude /= aSamples.Length;
            }

            float angleOffset = 0;
            int lightOffset = (auroraParticlesCount - 1) / auroraLightsCount;
            if (auroraVolumetric || auroraCircular)
                Random.InitState(auroraSeed);

            for (int i = 0; i < p_Particles.Length; i++)
            {
                float time = i / (float)(p_Particles.Length - 1);
                int sample = (int)((audioSamples - 1) * time * (1f - audioFrequenciesScale));
                float perlin = 0;
                if (useAudioSourceVisualization && audioVisualizeFrequency != AudioVisualizationSource.None)
                    perlin = Mathf.PerlinNoise(Time.time * auroraAnimationFrequency * aAmplitude, time * auroraCurvature);
                else
                    perlin = Mathf.PerlinNoise(Time.time * auroraAnimationFrequency, time * auroraCurvature);
                float offset = perlin * 2f - 1f;

                if (useAudioSourceVisualization && audioVisualizePositionOffset != AudioVisualizationSource.None)
                    offset *= aAmplitude * audioVisualizePositionOffsetMutliplier;

                Vector3 p_Position;
                if (auroraCircular)
                    p_Position = Quaternion.Euler(0, auroraRotation + angleOffset, 0) * new Vector3(offset * auroraSizes.x, 0, auroraSizes.z) + transform.position;
                else
                    p_Position = Quaternion.Euler(0, auroraRotation + angleOffset, 0) * new Vector3(offset * auroraSizes.x, 0, time * auroraSizes.z) + transform.position;
                Color p_Color = auroraColorMain.Evaluate(time);

                float sizeY = auroraSizes.y;

                if (useAudioSourceVisualization)
                {
                    if (audioVisualizeColorGradient == AudioVisualizationSource.AvgAmplitude)
                        p_Color = audioColorGradient.Evaluate(aAmplitude);
                    if (audioVisualizeColorGradient == AudioVisualizationSource.Frequencies)
                        p_Color = audioColorGradient.Evaluate(aBuffer[sample]);
                    if (audioVisualizeOpacity == AudioVisualizationSource.AvgAmplitude)
                        p_Color.a *= aAmplitude;
                    if (audioVisualizeOpacity == AudioVisualizationSource.Frequencies)
                        p_Color.a *= aBuffer[sample];
                    if (audioVisualizeHeight == AudioVisualizationSource.AvgAmplitude)
                        sizeY = aAmplitude * auroraSizes.y * audioHeightMultiplier;
                    if (audioVisualizeHeight == AudioVisualizationSource.Frequencies)
                        sizeY = Mathf.SmoothStep(0, 1, aBuffer[sample]) * auroraSizes.y * audioHeightMultiplier;
                }

                p_Particles[i].position = p_Position + new Vector3(0, sizeY / 3f, 0);
                p_Particles[i].startSize3D = new Vector3(auroraParticleThickness, sizeY, auroraParticleThickness);
                p_Particles[i].startColor = p_Color;

                if (auroraVolumetric || auroraCircular)
                    angleOffset += Random.Range(auroraVolumetricRange.x, auroraVolumetricRange.y) / p_Particles.Length;

                if (auroraLights && i != 0 && i % lightOffset == 0)
                {
                    int n = i / (lightOffset + 1);
                    l_Lights[n].transform.position = p_Position;
                    l_Lights[n].color = p_Color;
                    l_Lights[n].range = auroraLightsRange;
                    l_Lights[n].intensity = auroraLightsIntesity * Mathf.Clamp01(p_Color.a);
                }
            }
            pSystem.SetParticles(p_Particles, auroraParticlesCount);
        }
    }
}