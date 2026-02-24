using System;
using UnityEngine;

namespace DesertArena.Player
{
    /// <summary>
    /// Tracks player stats (HP, speed, damage, attack speed) and handles
    /// damage, healing, death, and temporary invulnerability.
    /// </summary>
    public class PlayerStats : MonoBehaviour
    {
        #region Events

        /// <summary>Fired when health changes. Passes (currentHP, maxHP).</summary>
        public event Action<float, float> OnHealthChanged;

        /// <summary>Fired when the player dies.</summary>
        public event Action OnDeath;

        #endregion

        #region Serialized Fields

        [Header("Base Stats")]
        [SerializeField] private float _baseMaxHP = 100f;
        [SerializeField] private float _baseMoveSpeed = 5f;
        [SerializeField] private float _baseAttackDamage = 10f;
        [SerializeField] private float _baseAttackSpeed = 1f;

        [Header("Invulnerability")]
        [SerializeField]
        [Tooltip("Duration of invulnerability after taking damage (seconds).")]
        private float _invulnerabilityDuration = 0.5f;

        #endregion

        #region Private Fields

        private float _currentHP;
        private float _invulnerabilityTimer;
        private bool _isDead;

        #endregion

        #region Properties

        /// <summary>Maximum hit points (can be buffed by upgrades).</summary>
        public float MaxHP { get; set; }

        /// <summary>Current hit points.</summary>
        public float CurrentHP => _currentHP;

        /// <summary>Movement speed (can be buffed by upgrades).</summary>
        public float MoveSpeed { get; set; }

        /// <summary>Attack damage (can be buffed by upgrades).</summary>
        public float AttackDamage { get; set; }

        /// <summary>Attacks per second (can be buffed by upgrades).</summary>
        public float AttackSpeed { get; set; }

        /// <summary>True while the player cannot take damage.</summary>
        public bool IsInvulnerable { get; set; }

        /// <summary>True if the player is dead.</summary>
        public bool IsDead => _isDead;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            ResetStats();
        }

        private void Update()
        {
            if (_invulnerabilityTimer > 0f)
            {
                _invulnerabilityTimer -= Time.deltaTime;
                if (_invulnerabilityTimer <= 0f)
                {
                    IsInvulnerable = false;
                }
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Applies damage to the player. Respects invulnerability.
        /// </summary>
        /// <param name="damage">Amount of damage to deal.</param>
        public void TakeDamage(float damage)
        {
            if (_isDead || IsInvulnerable) return;
            if (damage <= 0f) return;

            _currentHP = Mathf.Max(_currentHP - damage, 0f);
            OnHealthChanged?.Invoke(_currentHP, MaxHP);

            if (_currentHP <= 0f)
            {
                Die();
                return;
            }

            // Brief invulnerability after taking damage
            IsInvulnerable = true;
            _invulnerabilityTimer = _invulnerabilityDuration;
        }

        /// <summary>
        /// Heals the player by the given amount, clamped to MaxHP.
        /// </summary>
        /// <param name="amount">Amount to heal.</param>
        public void Heal(float amount)
        {
            if (_isDead) return;
            if (amount <= 0f) return;

            _currentHP = Mathf.Min(_currentHP + amount, MaxHP);
            OnHealthChanged?.Invoke(_currentHP, MaxHP);
        }

        /// <summary>
        /// Kills the player immediately.
        /// </summary>
        public void Die()
        {
            if (_isDead) return;

            _isDead = true;
            _currentHP = 0f;
            OnHealthChanged?.Invoke(_currentHP, MaxHP);
            OnDeath?.Invoke();
        }

        /// <summary>
        /// Resets all stats to their base values and restores full HP.
        /// </summary>
        public void ResetStats()
        {
            MaxHP = _baseMaxHP;
            MoveSpeed = _baseMoveSpeed;
            AttackDamage = _baseAttackDamage;
            AttackSpeed = _baseAttackSpeed;

            _currentHP = MaxHP;
            _isDead = false;
            IsInvulnerable = false;
            _invulnerabilityTimer = 0f;

            OnHealthChanged?.Invoke(_currentHP, MaxHP);
        }

        /// <summary>
        /// Revives the player at full health.
        /// </summary>
        public void Revive()
        {
            _isDead = false;
            _currentHP = MaxHP;
            IsInvulnerable = true;
            _invulnerabilityTimer = _invulnerabilityDuration;
            OnHealthChanged?.Invoke(_currentHP, MaxHP);
        }

        #endregion
    }
}
