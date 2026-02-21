using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

[RequireComponent(typeof(PhotonView))]
public class SingleShotGun : Gun
{
    [SerializeField] new Camera camera;
    PhotonView photonView;

    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip reloadSound;
    [SerializeField] AudioClip emptyClipSound;
    [SerializeField] AudioClip impactSound;

    [SerializeField, Min(1)] int maxAmmo = 30;
    [SerializeField, Min(0.1f)] float reloadTime = 2f;

    public int CurrentAmmo { get; private set; }
    bool isReloading;
    public event System.Action<int> OnAmmoChanged;

    [SerializeField, Range(0.1f, 10f)] float fireRate = 5f;
    float nextShootTime;

    void Awake()
    {
        photonView = GetComponent<PhotonView>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        CurrentAmmo = maxAmmo;
        if (itemGameObject == null)
            itemGameObject = gameObject;
    }

    public void Activate()
    {
        if (itemGameObject != null)
        {
            itemGameObject.SetActive(true);
            Debug.Log("Оружие активировано: " + itemGameObject.name);
        }
    }

    public void Deactivate()
    {
        if (itemGameObject != null)
        {
            itemGameObject.SetActive(false);
            Debug.Log("Оружие деактивировано: " + itemGameObject.name);
        }
    }

    public override void Use()
    {
        if (isReloading) return;

        if (CurrentAmmo > 0)
        {
            if (Time.time >= nextShootTime)
            {
                Shoot();
                nextShootTime = Time.time + 1f / fireRate;
            }
        }
        else
        {
            if (audioSource && emptyClipSound)
                audioSource.PlayOneShot(emptyClipSound);
        }
    }

    private void Shoot()
    {
        CurrentAmmo--;
        OnAmmoChanged?.Invoke(CurrentAmmo);

        Ray ray = camera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
        ray.origin = camera.transform.position;

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var target = hit.collider.gameObject.GetComponent<IDamagable>();
            if (target != null)
            {
                // Передаём урон и атакующего (владельца оружия)
                target.TakeDamage(((GunInfo)itemInfo).damage, photonView.Owner);
            }

            photonView.RPC("RpcShoot", RpcTarget.All, hit.point, hit.normal);
        }
        else
        {
            photonView.RPC("RpcShoot", RpcTarget.All, Vector3.zero, Vector3.up);
        }
    }

    [PunRPC]
    private void RpcShoot(Vector3 hitPosition, Vector3 hitNormal)
    {
        if (audioSource && shootSound)
            audioSource.PlayOneShot(shootSound);

        if (hitPosition != Vector3.zero)
        {
            var colliders = Physics.OverlapSphere(hitPosition, 0.3f);
            if (colliders.Length > 0)
            {
                var bulletImpactObject = Instantiate(
                    bulletImpactPrefab,
                    hitPosition + hitNormal * 0.0001f,
                    Quaternion.LookRotation(hitNormal, Vector3.up) * bulletImpactPrefab.transform.rotation
                );
                bulletImpactObject.transform.SetParent(colliders[0].transform);
                Destroy(bulletImpactObject, 10f);

                if (audioSource && impactSound)
                    audioSource.PlayOneShot(impactSound);
            }
        }
    }

    public void Reload()
    {
        if (!isReloading && CurrentAmmo < maxAmmo)
        {
            isReloading = true;
            photonView.RPC("RpcReload", RpcTarget.All);
        }
    }

    [PunRPC]
    private void RpcReload()
    {
        if (audioSource && reloadSound)
            audioSource.PlayOneShot(reloadSound);

        StartCoroutine(ReloadCoroutine());
    }

    private IEnumerator ReloadCoroutine()
    {
        yield return new WaitForSeconds(reloadTime);
        CurrentAmmo = maxAmmo;
        OnAmmoChanged?.Invoke(CurrentAmmo);
        isReloading = false;
    }
}