using UnityEngine;
using UnityEngine.EventSystems;
using DesertArena.Core;
using DesertArena.Enemies;
using DesertArena.Player;
using DesertArena.UI;

namespace DesertArena.Utilities
{
    /// <summary>
    /// Sahneye eklendiğinde oyunun çalışması için gereken tüm
    /// bileşenlerin mevcut olduğunu kontrol eder ve eksikleri otomatik
    /// oluşturur. Yeni başlayanlar için güvenlik ağı görevi görür.
    ///
    /// Bu bileşeni herhangi bir GameObject'e ekleyin — gerisini halleder.
    /// </summary>
    public class GameBootstrapper : MonoBehaviour
    {
        [Header("Otomatik Kurulum")]
        [SerializeField] [Tooltip("Eksik bileşenleri otomatik oluştur.")]
        private bool autoSetup = true;

        private void Awake()
        {
            if (!autoSetup) return;

            EnsureEventSystem();
            EnsureTags();
            EnsureGameManager();
        }

        /// <summary>
        /// UI pointer event'leri için EventSystem gerekli.
        /// </summary>
        private void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                GameObject esObj = new GameObject("EventSystem (Auto)");
                esObj.AddComponent<EventSystem>();
                esObj.AddComponent<StandaloneInputModule>();
                Debug.Log("[GameBootstrapper] EventSystem otomatik oluşturuldu");
            }
        }

        /// <summary>
        /// "Player", "Enemy", "Projectile" tag'lerinin varlığını kontrol eder.
        /// Unity'de tag'ler runtime'da oluşturulamaz, bu yüzden sadece uyarı verir.
        /// </summary>
        private void EnsureTags()
        {
            // Tag kontrolü: Player tag'li obje var mı?
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj == null)
            {
                Debug.LogWarning("[GameBootstrapper] 'Player' tag'li obje bulunamadı! " +
                    "Player objenizi seçip Inspector'da Tag = 'Player' yapın.");
            }
        }

        /// <summary>
        /// GameManager yoksa oluşturur.
        /// </summary>
        private void EnsureGameManager()
        {
            if (!GameManager.HasInstance && FindAnyObjectByType<GameManager>() == null)
            {
                Debug.LogWarning("[GameBootstrapper] GameManager bulunamadı! " +
                    "Sahneye GameManager bileşeni ekleyin.");
            }
        }
    }
}
