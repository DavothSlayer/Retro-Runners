using UnityEngine;

namespace RetroCode
{
    [CreateAssetMenu(fileName = "Mechanic", menuName = "New Mechanic Ability")]
    public class Mechanic : AutoAbility
    {
        public override void ActivateAbility(AutoMobile auto)
        {
            auto.MechanicMethod();
        }
    }
}