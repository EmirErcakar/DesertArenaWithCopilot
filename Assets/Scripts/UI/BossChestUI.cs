using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DesertArena.Economy;

namespace DesertArena.UI
{
    /// <summary>
    /// Displays the boss chest opening screen. Shows an animated chest,
    /// reveals the reward, and provides "Collect" and "Open x2 (Watch Ad)" buttons.
    /// </summary>
    public class BossChestUI : MonoBehaviour
    {
        #region Serialized Fields

        [Header("References")]
        [SerializeField] [Tooltip("Root panel for the chest UI.")]
        private GameObject _panel;

        [SerializeField] [Tooltip("The BossChestSystem in the scene.")]
        private BossChestSystem _chestSystem;

        [Header("Chest Visual")]
        [SerializeField] [Tooltip("Animator controlling the chest open animation.")]
        private Animator _chestAnimator;

        [SerializeField] [Tooltip("Particle system placeholder for sparkle effect.")]
        private ParticleSystem _sparkleEffect;

        [Header("Reward Display")]
        [SerializeField] [Tooltip("Icon representing the reward type.")]
        private Image _rewardIcon;

        [SerializeField] [Tooltip("Text showing the reward type and amount.")]
        private TextMeshProUGUI _rewardText;

        [Header("Buttons")]
        [SerializeField] [Tooltip("Collect the reward at normal value.")]
        private Button _collectButton;

        [SerializeField] [Tooltip("Watch an ad to double the reward.")]
        private Button _doubleButton;

        #endregion

        #region Private Fields

        private bool _rewardRevealed;

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void OnEnable()
        {
            if (_chestSystem != null)
            {
                _chestSystem.OnChestAvailable += ShowChestScreen;
            }

            if (_collectButton != null) _collectButton.onClick.AddListener(OnCollectClicked);
            if (_doubleButton != null) _doubleButton.onClick.AddListener(OnDoubleClicked);
        }

        private void OnDisable()
        {
            if (_chestSystem != null)
            {
                _chestSystem.OnChestAvailable -= ShowChestScreen;
            }

            if (_collectButton != null) _collectButton.onClick.RemoveListener(OnCollectClicked);
            if (_doubleButton != null) _doubleButton.onClick.RemoveListener(OnDoubleClicked);
        }

        #endregion

        #region Private Methods

        private void ShowChestScreen()
        {
            _rewardRevealed = false;

            if (_panel != null) _panel.SetActive(true);

            // Hide reward display until the chest is opened
            SetRewardVisible(false);

            // Buttons: both active, collect is default
            if (_collectButton != null) _collectButton.interactable = true;
            if (_doubleButton != null) _doubleButton.interactable = true;
        }

        private void OnCollectClicked()
        {
            if (_chestSystem == null) return;

            DisableButtons();

            _chestSystem.OpenChest(reward =>
            {
                RevealReward(reward);
            });
        }

        private void OnDoubleClicked()
        {
            if (_chestSystem == null) return;

            DisableButtons();

            _chestSystem.OpenChestDoubled(reward =>
            {
                RevealReward(reward);
            });
        }

        private void RevealReward(ChestReward reward)
        {
            _rewardRevealed = true;

            // Trigger chest open animation
            if (_chestAnimator != null)
            {
                _chestAnimator.SetTrigger("Open");
            }

            // Play sparkle effect
            if (_sparkleEffect != null)
            {
                _sparkleEffect.Play();
            }

            // Update reward text
            if (_rewardText != null)
            {
                string typeName = reward.RewardType switch
                {
                    ChestRewardType.Coins        => "Coins",
                    ChestRewardType.SkinFragments => "Skin Fragments",
                    ChestRewardType.MetaTokens    => "Meta Tokens",
                    _                             => "Reward"
                };

                _rewardText.text = $"{typeName} x{reward.Amount}";
            }

            SetRewardVisible(true);

            // After a brief moment, allow closing
            Invoke(nameof(EnableClose), 1.5f);
        }

        private void EnableClose()
        {
            if (_collectButton != null)
            {
                _collectButton.interactable = true;
                // Repurpose collect button as "OK / Close"
                var label = _collectButton.GetComponentInChildren<TextMeshProUGUI>();
                if (label != null) label.text = "OK";
                _collectButton.onClick.RemoveAllListeners();
                _collectButton.onClick.AddListener(ClosePanel);
            }
        }

        private void ClosePanel()
        {
            if (_panel != null) _panel.SetActive(false);
        }

        private void SetRewardVisible(bool visible)
        {
            if (_rewardIcon != null) _rewardIcon.gameObject.SetActive(visible);
            if (_rewardText != null) _rewardText.gameObject.SetActive(visible);
        }

        private void DisableButtons()
        {
            if (_collectButton != null) _collectButton.interactable = false;
            if (_doubleButton != null) _doubleButton.interactable = false;
        }

        #endregion
    }
}
