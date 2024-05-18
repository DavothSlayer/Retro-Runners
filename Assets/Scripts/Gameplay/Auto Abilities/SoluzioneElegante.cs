using UnityEngine;

namespace RetroCode
{
    [CreateAssetMenu(fileName = "Soluzione Elegante", menuName = "New Soluzione Elegante Ability")]
    public class SoluzioneElegante : AutoAbility
    {
        public override void ActivateAbility(AutoMobile auto)
        {
            auto.SoluzioneEleganteMethod();
        }
    }
}
