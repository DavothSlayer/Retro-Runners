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
        private GameObject main;
        [SerializeField]
        private DeadNPC deadModel;

        private float topSpeed;

        public virtual void OnEnable()
        {
            AutoMobile auto = gameManager.playerCar;
            AutoData data = auto.data;
            
            // RESET HEALTH METHOD NEEDED //
            health = 1;

            deadModel.gameObject.SetActive(false);
            rigidBody.isKinematic = false;

            if (transform.position.x > 0f)
                topSpeed = data.autoLevelData[auto.engineLevel].TopSpeed * 0.5f;
            else
                topSpeed = data.autoLevelData[auto.engineLevel].TopSpeed * 0.35f;

            rigidBody.AddForce(transform.forward * topSpeed, ForceMode.VelocityChange);

            main.SetActive(true);
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
            main.SetActive(false);
            rigidBody.isKinematic = true;
            
            deadModel.gameObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
            deadModel.gameObject.SetActive(true);
            deadModel.OnDead(rigidBody.velocity);
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
