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
        public ParticleSystem unlockFX;

        public void PlayUnlockFX()
        {
            unlockFX.PlaySystem();
        }
    }
}
