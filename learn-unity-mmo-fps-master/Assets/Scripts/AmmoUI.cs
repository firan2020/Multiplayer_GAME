using UnityEngine;
using TMPro;

public class AmmoUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI ammoText;
    [SerializeField] PlayerController player;
    SingleShotGun currentGun;


    void Start()
    {
        if (!player) player = FindObjectOfType<PlayerController>();
        SubscribeToCurrentGun();
    }

    void OnDestroy()
    {
        UnsubscribeFromCurrentGun();
    }

    void Update()
    {
        // Проверяем, не сменилось ли оружие
        SingleShotGun newGun = GetCurrentGun();
        if (newGun != currentGun)
        {
            UnsubscribeFromCurrentGun(); // Отписываемся от старого оружия
            currentGun = newGun;
            SubscribeToCurrentGun();     // Подписываемся на новое
        }
    }

    void SubscribeToCurrentGun()
    {
        if (currentGun != null)
        {
            currentGun.OnAmmoChanged += UpdateAmmoText;
            UpdateAmmoText(currentGun.CurrentAmmo); // Сразу обновляем текст
        }
        else
        {
            ammoText.gameObject.SetActive(false);
        }
    }

    void UnsubscribeFromCurrentGun()
    {
        if (currentGun != null)
            currentGun.OnAmmoChanged -= UpdateAmmoText;
    }

    void UpdateAmmoText(int ammo)
    {
        ammoText.text = ammo.ToString();
        ammoText.gameObject.SetActive(true);
    }

    SingleShotGun GetCurrentGun()
    {
        if (player == null || player.CurrentWeaponIndex < 0 || player.CurrentWeaponIndex >= player.Weapons.Length)
            return null;

        GameObject weaponObj = player.Weapons[player.CurrentWeaponIndex];
        return weaponObj.GetComponent<SingleShotGun>();
    }
}
