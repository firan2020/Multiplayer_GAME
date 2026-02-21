using System.Collections;
using UnityEngine;
using Photon.Pun; // нужно для PhotonNetwork.LocalPlayer

public class DamageTestTrigger : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private float damageAmount = 10f;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            ApplyDamage();
        }
    }

    private void ApplyDamage()
    {
        if (playerController == null)
        {
            Debug.LogError("PlayerController не назначен!");
            return;
        }

        // Наносим урон цели, передавая атакующего (локального игрока)
        playerController.TakeDamage(damageAmount, PhotonNetwork.LocalPlayer);

        // Запускаем корутину на этом объекте (он не уничтожается после смерти)
        StartCoroutine(DelayedAddKill());
    }

    private IEnumerator DelayedAddKill()
    {
        yield return new WaitForSeconds(0.3f); // Даём время на обработку смерти
        PlayerController.AddKillToLocalPlayer();
    }
}