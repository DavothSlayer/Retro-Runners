using UnityEngine;

namespace RetroCode
{
    [CreateAssetMenu(fileName = "Rockstar", menuName = "New Rockstar Ability")]
    public class Rockstar : AutoAbility
    {
        public override void ActivateAbility(AutoMobile auto)
        {
            auto.RockstarMethod();
        }
    }
}
