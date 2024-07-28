using UnityEngine;
using V3CTOR;

namespace RetroCode
{
    public class NPC : MonoBehaviour, Damageable, NearMissable
    {
        [Header("Core")]
        public Rigidbody rigidBody;
        [SerializeField]
        private Rigidbody[] deadRigidbodies;
        [SerializeField]
        private Rigidbody deadReference;
        [SerializeField]
        private int health;
        [SerializeField]
        private int damageToPlayer;
        public SpawnManager spawnManager;
        public GameManager gameManager;

        [Header("Variants")]
        [SerializeField]
        private GameObject liveModel;
        [SerializeField]
        private GameObject deadModel;

        private float topSpeed;

        public virtual void OnEnable()
        {
            AutoMobile auto = gameManager.playerCar;
            AutoData data = auto.data;
            
            // RESET HEALTH METHOD NEEDED //
            health = 1;

            rigidBody.isKinematic = false;

            if (transform.position.x > 0f)
                topSpeed = data.autoLevelData[auto.engineLevel].TopSpeed * 0.5f;
            else
                topSpeed = data.autoLevelData[auto.engineLevel].TopSpeed * 0.35f;

            rigidBody.AddForce(transform.forward * topSpeed, ForceMode.VelocityChange);

            liveModel.SetActive(true);
        }

        public virtual void OnDisable()
        {
            rigidBody.isKinematic = true;

            foreach (Rigidbody rb in deadRigidbodies)
            {
                rb.gameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
                rb.isKinematic = true;
            }

            deadModel.SetActive(false);
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
            if(transform.position.x > 0f)
            {
                if (deadReference.transform.position.z + 200f <= gameManager.playerTransform.position.z)
                    EXMET.RemoveSpawnable(gameObject, spawnManager.activeNPCsRL, spawnManager.NPCPool);
            }
            
            if(transform.position.x < 0f)
            {
                if (deadReference.transform.position.z + 200f <= gameManager.playerTransform.position.z)
                    EXMET.RemoveSpawnable(gameObject, spawnManager.activeNPCsLL, spawnManager.NPCPool);
            }

            if (rigidBody.isKinematic) return;

            rigidBody.velocity = Vector3.ClampMagnitude(rigidBody.velocity, topSpeed);
        }

        public virtual void NPCMovement()
        {
            if(GameManager.gameState == GameState.GameOver || rigidBody.isKinematic) { return; }

            rigidBody.AddForce(transform.forward * topSpeed * rigidBody.mass * rigidBody.drag, ForceMode.Force);
        }

        public void Damage(int dmg)
        {
            if (health <= 0) return;

            health -= dmg;

            if(health <= 0) HandleDeath();
        }

        public virtual void HandleDeath()
        {
            Vector3 explosionTorqueVector =
                transform.right * Random.Range(-15f, 15f) +
                transform.up * Random.Range(-5f, 5f) +
                transform.forward * Random.Range(-25f, 25f);

            foreach (Rigidbody rb in deadRigidbodies)
            {
                rb.gameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
                rb.isKinematic = false;
                rb.velocity = rigidBody.velocity;

                rb.AddForce(Vector3.up, ForceMode.VelocityChange);
                rb.AddTorque(explosionTorqueVector, ForceMode.VelocityChange);
            }

            liveModel.SetActive(false);
            rigidBody.isKinematic = true;

            deadModel.SetActive(true);
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
