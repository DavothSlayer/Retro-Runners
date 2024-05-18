using UnityEngine;
using V3CTOR;

namespace RetroCode
{
    public class COP : MonoBehaviour
    {
        [Header("Core")]
        public Rigidbody rigidBody;
        [SerializeField]
        private Collider mainCollider;
        public SpawnManager spawnManager;
        public GameManager gameManager;

        [Header("Data")]
        public int health;
        [SerializeField]
        private Vector3 raycastPos;
        [SerializeField]
        private float forwardRayDistance;
        [SerializeField]
        private float sideRayDistance;
        [SerializeField]
        private float laneChangeForce;
        [SerializeField]
        private LayerMask playerMask;

        [Header("References")]
        [SerializeField]
        private GameObject mainModel;
        [SerializeField]
        private GameObject deadModel;
        [SerializeField]
        private GameObject sirenVar1;
        [SerializeField]
        private GameObject sirenVar2;

        [HideInInspector]
        public Transform playerT;
        [HideInInspector]
        public AutoMobile playerAuto;
        
        private float targetSpeed;
        private float cruiseSpeed;
        private float chaseSpeed;

        public void OnEnable()
        {
            deadModel.SetActive(false);

            playerAuto = gameManager.playerCar;
            playerT = gameManager.playerTransform;
            health = 1;

            mainCollider.enabled = true;
            rigidBody.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
            rigidBody.velocity = Vector3.zero;
            rigidBody.AddForce(transform.forward * cruiseSpeed, ForceMode.VelocityChange);

            mainModel.SetActive(true);
        }

        public void Update()
        {        
            COPMath();
            WaitForDespawn();
            SirenMethod();
        }

        public void FixedUpdate()
        {
            COPMovement();
            ChasePlayer();
        }

        #region COP
        private float xDifference;
        private float interceptDistance;
        private void COPMath()
        {
            if (playerT == null) { return; }

            AutoMobile auto = gameManager.playerCar;
            AutoData data = auto.data;

            xDifference = playerT.position.x - transform.position.x;
            chaseSpeed = data.autoLevelData[auto.engineLevel].TopSpeed * 1.25f;
            cruiseSpeed = data.autoLevelData[auto.engineLevel].TopSpeed * 0.5f;
            interceptDistance = 4f;

            if (GameManager.gameState == GameState.GameOver)
            {
                targetSpeed = -rigidBody.velocity.z * 2f;
                return;
            }

            if (playerT.position.z - interceptDistance > transform.position.z)
            {
                targetSpeed = Mathf.Lerp(targetSpeed, chaseSpeed, Time.deltaTime * 35f);
            }
            
            if(playerT.position.z - interceptDistance <= transform.position.z)
            {
                targetSpeed = Mathf.Lerp(targetSpeed, cruiseSpeed, Time.deltaTime * 6f);                
            }
        }

        private void COPMovement()
        {
            if(GameManager.gameState == GameState.GameOver || health <= 0) { return; }

            rigidBody.AddForce(transform.forward * targetSpeed * rigidBody.drag, ForceMode.Acceleration);
        }

        private void ChasePlayer()
        {
            if (GameManager.gameState == GameState.GameOver || health <= 0) { return; }

            if(playerT == null) { return; }

            // CHECK IF THERE IS A COMING ENTITY //
            Vector3 rayPos = transform.position + raycastPos;
            for(int i = 0; i < 3; i++)
            {
                Debug.DrawRay(rayPos, Quaternion.Euler(0f, -2f + i * 2f, 0f) * transform.forward * forwardRayDistance, Color.blue);
                if (Physics.Raycast(rayPos, Quaternion.Euler(0f, -2f + i * 2f, 0f) * transform.forward, forwardRayDistance, playerMask))
                {
                    // RIGHT & LEFT SIDE CHECK //
                    if (!Physics.Raycast(rayPos, transform.right, sideRayDistance, playerMask))
                    {
                        // RIGHT CLEAR, GO RIGHT //
                        rigidBody.AddForce(transform.right * laneChangeForce * rigidBody.mass * rigidBody.drag, ForceMode.Force);
                    }
                    else if (!Physics.Raycast(rayPos, -transform.right, sideRayDistance, playerMask))
                    {
                        // LEFT CLEAR, GO LEFT //
                        rigidBody.AddForce(-transform.right * laneChangeForce * rigidBody.mass * rigidBody.drag, ForceMode.Force);
                    }
                }
                else
                {
                    if (playerT.position.z > transform.position.z)
                        rigidBody.AddForce(transform.right * 2f * xDifference * rigidBody.mass * rigidBody.drag, ForceMode.Force);
                }
            }
        }

        public void OnCollisionEnter(Collision col)
        {
            Damageable dmgble = col.collider.GetComponentInParent<Damageable>();
            if (dmgble == null) { col.collider.GetComponent<Damageable>(); }

            if (Vector3.Dot(col.GetContact(0).otherCollider.transform.forward, transform.forward) == 1f)
            {
                if (Vector3.Dot(col.GetContact(0).normal, transform.forward) != -1f) return;

                if(dmgble != null){ dmgble.Damage(dmgble.Health()); }
            }
            else if (Vector3.Dot(col.GetContact(0).otherCollider.transform.forward, transform.forward) == -1f)
            {
                if (Vector3.Dot(col.GetContact(0).normal, transform.forward) != -1f) return;

                if (dmgble != null) { dmgble.Damage(dmgble.Health()); }
                health--;
                HandleDeath();
            }

            rigidBody.AddForce(col.relativeVelocity * rigidBody.mass * rigidBody.drag, ForceMode.Impulse);
        }

        public void HandleDeath()
        {
            for (int i = 0; i < spawnManager.activeCOPs.Count; i++)
            {
                if (spawnManager.activeCOPs[i] == gameObject) { spawnManager.activeCOPs.Remove(gameObject); }
            }

            mainCollider.enabled = false;
            mainModel.SetActive(false);
            deadModel.SetActive(true);

            if (transform.position.z < playerAuto.transform.position.z)
                gameManager.DestroyedCOP();

            rigidBody.constraints = RigidbodyConstraints.None;
            Vector3 explosionTorqueVector =
                transform.right * Random.Range(-5f, 5f) +
                transform.up * Random.Range(-5f, 5f) +
                transform.forward * Random.Range(-10f, 10f);

            rigidBody.AddForce(Vector3.up, ForceMode.VelocityChange);
            rigidBody.AddTorque(explosionTorqueVector, ForceMode.VelocityChange);
        }

        private void WaitForDespawn()
        {
            if (gameManager.playerTransform.position.z >= transform.position.z + 125f)
            {
                deadModel.SetActive(false);
                mainCollider.enabled = true;
                transform.parent = spawnManager.COPPoolParent;
                spawnManager.COPPool.Add(gameObject);
                gameObject.SetActive(false);
            }
        }

        private float timer;
        private float sirenFrequency = 0.3f;
        private void SirenMethod()
        {
            timer += Time.deltaTime;

            if(timer >= sirenFrequency)
            {
                sirenVar1.SetActive(!sirenVar1.activeInHierarchy);
                sirenVar2.SetActive(!sirenVar2.activeInHierarchy);

                timer = 0f;
            }
        }
        #endregion
    }
}
