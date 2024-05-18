using UnityEngine;

namespace RetroCode
{
    public class GuardRail : StaticObstacle
    {
        public GameManager gameManager;
        [SerializeField]
        private GameObject mainObject;
        [SerializeField]
        private GameObject deadObject;

        private void OnEnable()
        {
            mainObject.SetActive(true);
            deadObject.SetActive(false);
        }

        public override void Damage(int dmg)
        {
            base.Damage(dmg);

            mainObject.SetActive(false);
            deadObject.SetActive(true);
        } 
    }
}
