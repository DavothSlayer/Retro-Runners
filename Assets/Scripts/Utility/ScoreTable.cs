using UnityEngine;

namespace RetroCode
{
    [CreateAssetMenu(fileName = "Score Table", menuName = "New Score Table")]
    public class ScoreTable : ScriptableObject
    {
        [Header("Game Score & Multipliers")]
        public float scorePerSecond;
        public float scorePerNearMiss;
        public float thrillSeekerMultiplier;
        [Range(0, 1f)]
        public float rewardRatio;

        [Header("Material Library")]
        public Material specterMaterial;
        public Material universalMaterial;
        public Material godrayMaterial;
    }
}
