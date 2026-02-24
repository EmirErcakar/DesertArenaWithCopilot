using System;
using UnityEngine;
using UnityEngine.AI;
using DesertArena.Core;

namespace DesertArena.Enemies
{
    /// <summary>
    /// Detailed enemy sub-types used for spawn configuration and behavior.
    /// Maps to <see cref="Core.EnemyType"/> for event bus compatibility.
    /// </summary>
    public enum EnemySubType
    {
        MeleeKnife,
        MeleeSword,
        Ranged,
        Boss
    }

    /// <summary>
    /// Abstract base class for all enemies. Provides health, movement via
    /// NavMeshAgent, damage handling, and death rewards.
    /// </summary>
    [RequireComponent(typeof(NavMeshAgent))]
    public abstract class EnemyBase : MonoBehaviour
    {
        #region Events

        /// <summary>Fired when this enemy dies. Passes this instance.</summary>
        public event Action<EnemyBase> OnDeath;

        #endregion

        #region Serialized Fields

        [Header("Stats")]
        [SerializeField] [Tooltip("Maximum hit points.")]
        protected float maxHP = 50f;

        [SerializeField] [Tooltip("Movement speed.")]
        protected float moveSpeed = 3.5f;

        [SerializeField] [Tooltip("Damage dealt per attack.")]
        protected float attackDamage = 10f;

        [SerializeField] [Tooltip("Distance at which the enemy can attack.")]
        protected float attackRange = 1.5f;

        [Header("Type")]
        [SerializeField] [Tooltip("Sub-type of this enemy.")]
        protected EnemySubType enemySubType = EnemySubType.MeleeKnife;

        [Header("Rewards")]
        [SerializeField] [Tooltip("Coins dropped on death.")]
        protected int coinReward = 5;

        [SerializeField] [Tooltip("XP awarded on death.")]
        protected int xpReward = 10;

        #endregion

        #region Protected Fields

        protected NavMeshAgent agent;
        protected Transform playerTransform;
        protected float currentHP;

        #endregion

        #region Properties

        /// <summary>True while the enemy has health remaining.</summary>
        public bool IsAlive => currentHP > 0f;

        /// <summary>The detailed sub-type of this enemy.</summary>
        public EnemySubType SubType => enemySubType;

        /// <summary>Current hit points.</summary>
        public float CurrentHP => currentHP;

        /// <summary>Maximum hit points.</summary>
        public float MaxHP => maxHP;

        #endregion

        #region Unity Lifecycle

        protected virtual void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            agent.speed = moveSpeed;
            currentHP = maxHP;
        }

        protected virtual void Start()
        {
            // Find the player by tag
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                playerTransform = playerObj.transform;
            }
        }

        protected virtual void Update()
        {
            if (!IsAlive || playerTransform == null) return;

            MoveTowardPlayer();
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies damage to this enemy. Calls <see cref="Die"/> when HP reaches zero.
        /// </summary>
        /// <param name="damage">Amount of damage to deal.</param>
        public virtual void TakeDamage(float damage)
        {
            if (!IsAlive) return;
            if (damage <= 0f) return;

            currentHP = Mathf.Max(currentHP - damage, 0f);

            if (currentHP <= 0f)
            {
                Die();
            }
        }

        /// <summary>
        /// Handles death: fires events, grants rewards, and destroys the GameObject.
        /// </summary>
        public virtual void Die()
        {
            if (currentHP > 0f) currentHP = 0f;

            // Fire instance event
            OnDeath?.Invoke(this);

            // Fire global event bus
            EventBus.RaiseEnemyKilled(MapToEnemyType());

            // Grant rewards
            EventBus.RaiseCoinCollected(coinReward);
            EventBus.RaiseXPGained(xpReward);

            // Stop movement
            if (agent != null && agent.isOnNavMesh)
            {
                agent.isStopped = true;
            }

            Destroy(gameObject);
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Moves the NavMeshAgent toward the player's position.
        /// </summary>
        protected virtual void MoveTowardPlayer()
        {
            if (agent == null || !agent.isOnNavMesh) return;

            agent.SetDestination(playerTransform.position);
        }

        /// <summary>
        /// Maps <see cref="EnemySubType"/> to <see cref="Core.EnemyType"/> for event bus.
        /// </summary>
        protected EnemyType MapToEnemyType()
        {
            switch (enemySubType)
            {
                case EnemySubType.MeleeKnife:
                case EnemySubType.MeleeSword:
                    return EnemyType.Melee;
                case EnemySubType.Ranged:
                    return EnemyType.Ranged;
                case EnemySubType.Boss:
                    return EnemyType.Boss;
                default:
                    return EnemyType.Melee;
            }
        }

        /// <summary>
        /// Returns the squared distance to the player.
        /// </summary>
        protected float DistanceToPlayerSqr()
        {
            if (playerTransform == null) return float.MaxValue;
            return Vector3.SqrMagnitude(playerTransform.position - transform.position);
        }

        /// <summary>
        /// Invokes the <see cref="OnDeath"/> event. Use in subclasses that
        /// override <see cref="Die"/> and need to fire the event manually.
        /// </summary>
        protected void RaiseOnDeath()
        {
            OnDeath?.Invoke(this);
        }

        #endregion
    }
}
