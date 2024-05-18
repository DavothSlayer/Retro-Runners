using UnityEngine;
using V3CTOR;

namespace RetroCode
{
    public class NPC : MonoBehaviour, Damageable, NearMissable
    {
        [Header("Core")]
        public Rigidbody rigidBody;
        [SerializeField]
        private int health;
        [SerializeField]
        private int damageToPlayer;
        public SpawnManager spawnManager;
        public GameManager gameManager;

        [Header("Variants")]
        [SerializeField]
        private GameObject[] variants;
        [SerializeField]
        private GameObject deadModel;

        private float topSpeed;

        public virtual void OnEnable()
        {
            AutoMobile auto = gameManager.playerCar;
            AutoData data = auto.data;
            
            // RESET HEALTH METHOD NEEDED //
            health = 1;

            deadModel.SetActive(false);

            rigidBody.constraints = RigidbodyConstraints.FreezePositionY | RigidbodyConstraints.FreezePositionX | RigidbodyConstraints.FreezeRotation;
            rigidBody.velocity = Vector3.zero;

            if (transform.position.x > 0f)
                topSpeed = data.autoLevelData[auto.engineLevel].TopSpeed * 0.5f;
            else
                topSpeed = data.autoLevelData[auto.engineLevel].TopSpeed * 0.35f;

            rigidBody.AddForce(transform.forward * topSpeed, ForceMode.VelocityChange);

            AssignRandomColor();
        }

        public virtual void Update()
        {
            NPCMath();
        }

        public virtual void FixedUpdate()
        {
            NPCMovement();
        }

        public virtual void NPCMath()
        {
            rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, topSpeed);
        }

        public virtual void NPCMovement()
        {
            if(GameManager.gameState == GameState.GameOver) { return; }

            rigidBody.AddForce(transform.forward * topSpeed * rigidBody.mass * rigidBody.drag, ForceMode.Force);
        }

        private void AssignRandomColor()
        {
            foreach (GameObject vrnt in variants)
                vrnt.SetActive(false);

            variants[Random.Range(0, variants.Length)].SetActive(true);
        }

        public void Damage(int dmg)
        {
            if (health <= 0) return;

            health -= dmg;

            if(health <= 0) HandleDeath();
        }

        public virtual void HandleDeath()
        {
            foreach (GameObject var in variants) var.SetActive(false);
            deadModel.SetActive(true);

            rigidBody.constraints = RigidbodyConstraints.None;
            Vector3 explosionTorqueVector =
                transform.right * Random.Range(-5f, 5f) +
                transform.up * Random.Range(-5f, 5f) +
                transform.forward * Random.Range(-10f, 10f);

            rigidBody.AddForce(Vector3.up, ForceMode.VelocityChange);
            rigidBody.AddTorque(explosionTorqueVector, ForceMode.VelocityChange);
        }

        public int Health()
        {
            return health;
        }

        public int DamageToPlayer()
        {
            return damageToPlayer;
        }
    }
}
