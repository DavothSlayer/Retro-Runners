using UnityEngine;

namespace RetroCode
{
    [CreateAssetMenu(fileName = "Harvester", menuName = "New Harvester Ability")]
    public class Harvester : AutoAbility
    {
        public override void ActivateAbility(AutoMobile auto)
        {
            //auto.HarvesterMethod();
        }
    }
}
