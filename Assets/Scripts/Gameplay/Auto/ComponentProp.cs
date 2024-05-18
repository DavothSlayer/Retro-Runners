using UnityEngine;
using V3CTOR;

namespace RetroCode
{
    public class ComponentProp : MonoBehaviour
    {
        public ItemData itemData;
        public ParticleSystem unlockFX;

        public void PlayUnlockFX()
        {
            unlockFX.PlaySystem();
        }
    }
}