using UnityEngine;

namespace RetroCode
{
    [CreateAssetMenu(fileName = "Forza Grande", menuName = "New Forza Grande Ability")]
    public class ForzaGrande : AutoAbility
    {
        public override void ActivateAbility(AutoMobile auto)
        {
            auto.ForzaGrandeMethod();
        }
    }
}
