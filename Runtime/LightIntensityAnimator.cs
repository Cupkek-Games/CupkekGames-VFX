using UnityEngine;

namespace CupkekGames.VFX
{
    public class LightIntensityAnimator : MonoBehaviour
    {
        private Light fireLight;
        public float minIntensity = 1f;
        public float maxIntensity = 3f;
        public float speed = 1f;

        private float targetIntensity;
        private float currentIntensity;

        void Start()
        {
            targetIntensity = Random.Range(minIntensity, maxIntensity);
            fireLight = GetComponent<Light>();
        }

        void Update()
        {
            currentIntensity = Mathf.Lerp(currentIntensity, targetIntensity, Time.deltaTime * speed);
            fireLight.intensity = currentIntensity;

            if (Mathf.Abs(currentIntensity - targetIntensity) < 0.1f)
            {
                targetIntensity = Random.Range(minIntensity, maxIntensity);
            }
        }
    }
}
