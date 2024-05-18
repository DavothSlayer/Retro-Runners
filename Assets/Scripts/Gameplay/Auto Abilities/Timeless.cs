using UnityEngine;

namespace RetroCode
{
    [CreateAssetMenu(fileName = "Timeless", menuName = "New Timeless Ability")]
    public class Timeless : AutoAbility
    {
        public override void ActivateAbility(AutoMobile auto)
        {
            auto.TimelessMethod();
        }
    }
}
