using SkrilStudio;
using UnityEngine;
using V3CTOR;

namespace RetroCode
{
    public class AutoProp : MonoBehaviour
    {
        [Header("References")]
        public AutoData data;
        public GameObject lockedModel;
        public GameObject unlockedModel;
        public RealisticEngineSound engineSFX;

        [Header("FX")]
        public AnimationCurve revCurve;
        public ParticleSystem unlockFX;

        public void OnEnable()
        {
            //engineSFX.
        }

        public void PlayUnlockFX()
        {
            unlockFX.PlaySystem();
        }
    }
}
