using UnityEngine;

namespace RetroCode
{
    [CreateAssetMenu(fileName = "Specter", menuName = "New Specter Ability")]
    public class Specter : AutoAbility
    {
        public override void ActivateAbility(AutoMobile auto)
        {
            auto.SpecterMethod();
        }
    }
}
