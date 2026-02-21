using Photon.Pun;
using Photon.Realtime;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(PhotonView))]
public class PlayerController : MonoBehaviourPunCallbacks, IDamagable
{
    [SerializeField] private CanvasGroup vignetteCanvasGroup;
    [SerializeField] private Image vignetteImage;

    [SerializeField] private float vignetteIntensityOnDamage = 0.4f;
    [SerializeField] private float vignetteFadeInDuration = 0.2f;
    [SerializeField] private float vignetteHoldDuration = 0.3f;
    [SerializeField] private float vignetteFadeOutDuration = 0.5f;

    public float currentHealth => _currentHealth;
    [SerializeField] private float _currentHealth;
    const float maxHealth = 100f;
    private bool isFootstepPlaying;
    public UnityAction<float> onTakeDamage;

    [SerializeField] GameObject cameraHolder;
    [SerializeField] float mouseSensitivity = 1f;
    [SerializeField] float sprintSpeed = 1f;
    [SerializeField] float walkSpeed = 1f;
    [SerializeField] float jumpForce = 1f;
    [SerializeField] float smoothTime = 1f;
    [SerializeField] private GameObject[] _weapons;
    public GameObject[] Weapons => _weapons;
    [SerializeField] Canvas canvas;
    [SerializeField] Image healthGaugeImage;
    [SerializeField] private int _currentWeaponIndex = -1;
    public int CurrentWeaponIndex => _currentWeaponIndex;
    private int _previousWeaponIndex = -1;

    float verticalLookRotation;
    bool grounded;
    Vector3 smoothMoveVelocity;
    Vector3 moveAmount;
    new Rigidbody rigidbody;
    new PhotonView photonView;
    PlayerManager playerManager;

    [SerializeField] AudioSource footstepAudioSource;
    [SerializeField] AudioClip footstepSound;
    [SerializeField] float minSpeedForFootstep = 0.1f;
    [SerializeField] float footstepInterval = 0.5f;
    float nextFootstepTime;

    private Player lastAttacker;
    private int currentKills = 0;
    private int currentDeaths = 0;

    public void SetGroundedState(bool grounded) => this.grounded = grounded;

    public void TakeDamage(float damage, Player attacker)
    {
        photonView.RPC("RpcTakeDamage", RpcTarget.All, damage, attacker.ActorNumber);
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, ExitGames.Client.Photon.Hashtable changedProps)
    {
        if (!photonView.IsMine && targetPlayer == photonView.Owner && changedProps.ContainsKey("WeaponIndex"))
            EquipItem((int)changedProps["WeaponIndex"]);
    }

    void Awake()
    {
        rigidbody = GetComponent<Rigidbody>();
        photonView = GetComponent<PhotonView>();
        playerManager = PhotonView.Find((int)photonView.InstantiationData[0]).GetComponent<PlayerManager>();

        if (!footstepAudioSource) footstepAudioSource = gameObject.AddComponent<AudioSource>();
        _currentHealth = maxHealth;

        onTakeDamage += (health) =>
        {
            if (healthGaugeImage)
            {
                var scale = healthGaugeImage.transform.localScale;
                scale.x = health / maxHealth;
                healthGaugeImage.transform.localScale = scale;
            }
        };

        if (vignetteImage != null)
        {
            vignetteCanvasGroup = vignetteImage.GetComponent<CanvasGroup>();
            if (vignetteCanvasGroup == null)
                vignetteCanvasGroup = vignetteImage.gameObject.AddComponent<CanvasGroup>();
            vignetteCanvasGroup.alpha = 0f;
        }
        else Debug.LogError("vignetteImage не назначен в инспекторе!");
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            Player owner = photonView.Owner;

            if (owner.CustomProperties.TryGetValue("Kills", out object killsObj))
                currentKills = (int)killsObj;
            else currentKills = 0;

            if (owner.CustomProperties.TryGetValue("Deaths", out object deathsObj))
                currentDeaths = (int)deathsObj;
            else currentDeaths = 0;

            ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable();
            props["Kills"] = currentKills;
            props["Deaths"] = currentDeaths;
            owner.SetCustomProperties(props);

            Debug.Log($"PlayerController.Start: восстановлены Kills={currentKills}, Deaths={currentDeaths} для {owner.NickName}");

            if (_weapons == null || _weapons.Length == 0)
            {
                Debug.LogError("PlayerController: Массив _weapons не заполнен!");
                return;
            }

            EquipItem(0);
        }
        else
        {
            Destroy(GetComponentInChildren<Camera>().gameObject);
            Destroy(rigidbody);
            if (canvas) Destroy(canvas.gameObject);
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            Look();
            Move();
            Jump();
            ChangeItem();

            if (Input.GetMouseButton(0) && _currentWeaponIndex >= 0 && _currentWeaponIndex < _weapons.Length)
            {
                var weaponScript = _weapons[_currentWeaponIndex].GetComponent<SingleShotGun>();
                weaponScript?.Use();
            }

            if (Input.GetKeyDown(KeyCode.R) && _currentWeaponIndex >= 0 && _currentWeaponIndex < _weapons.Length)
            {
                var weaponScript = _weapons[_currentWeaponIndex].GetComponent<SingleShotGun>();
                weaponScript?.Reload();
            }

            if (moveAmount.magnitude > minSpeedForFootstep && Time.time >= nextFootstepTime)
            {
                PlayFootstep();
                nextFootstepTime = Time.time + footstepInterval;
            }

            if (!isFootstepPlaying && grounded && moveAmount.magnitude > minSpeedForFootstep)
            {
                PlayFootstep();
                nextFootstepTime = Time.time + footstepInterval;
            }

            if (transform.position.y < -10f) playerManager.Die();
        }
    }

    private void PlayFootstep()
    {
        if (footstepAudioSource && footstepSound && grounded)
        {
            footstepAudioSource.PlayOneShot(footstepSound);
            isFootstepPlaying = true;
        }
    }

    void FixedUpdate()
    {
        if (photonView.IsMine)
            rigidbody.MovePosition(rigidbody.position + transform.TransformDirection(moveAmount) * Time.fixedDeltaTime);
    }

    [PunRPC]
    void RpcTakeDamage(float damage, int attackerActorNumber)
    {
        lastAttacker = PhotonNetwork.CurrentRoom.GetPlayer(attackerActorNumber);
        _currentHealth -= damage;
        _currentHealth = Mathf.Max(0, _currentHealth);

        onTakeDamage.Invoke(_currentHealth);
        ShowVignette();

        if (_currentHealth <= 0)
        {
            IncrementDeaths();

            if (lastAttacker != null && lastAttacker != PhotonNetwork.LocalPlayer)
                photonView.RPC("RPC_AddKill", lastAttacker);

            playerManager.Die();
        }
    }

    [PunRPC]
    void RPC_AddKill() => IncrementKills();

    public void IncrementKills()
    {
        currentKills++;
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable { { "Kills", currentKills } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        Debug.Log($"Kills увеличены: {currentKills}");
    }

    private void IncrementDeaths()
    {
        currentDeaths++;
        ExitGames.Client.Photon.Hashtable props = new ExitGames.Client.Photon.Hashtable { { "Deaths", currentDeaths } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
        Debug.Log($"Смертей теперь: {currentDeaths}");
    }

    private void Move()
    {
        Vector3 moveDirection = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical")).normalized;
        float speed = Input.GetKey(KeyCode.LeftShift) ? sprintSpeed : walkSpeed;
        moveAmount = Vector3.SmoothDamp(moveAmount, moveDirection * speed, ref smoothMoveVelocity, smoothTime);
    }

    private void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && grounded)
        {
            rigidbody.AddForce(transform.up * jumpForce);
            isFootstepPlaying = false;
            footstepAudioSource?.Stop();
        }
    }

    private void Look()
    {
        transform.Rotate(Vector3.up * Input.GetAxisRaw("Mouse X") * mouseSensitivity);
        verticalLookRotation += Input.GetAxisRaw("Mouse Y") * mouseSensitivity;
        verticalLookRotation = Mathf.Clamp(verticalLookRotation, -90f, 90f);
        cameraHolder.transform.localEulerAngles = Vector3.left * verticalLookRotation;
    }

    private void ChangeItem()
    {
        for (int i = 0; i < _weapons.Length && i < 9; i++)
            if (Input.GetKeyDown((i + 1).ToString())) { EquipItem(i); break; }

        float scroll = Input.GetAxisRaw("Mouse ScrollWheel");
        if (scroll > 0 && _currentWeaponIndex < _weapons.Length - 1) EquipItem(_currentWeaponIndex + 1);
        else if (scroll < 0 && _currentWeaponIndex > 0) EquipItem(_currentWeaponIndex - 1);
    }

    private void EquipItem(int index)
    {
        if (index < 0 || index >= _weapons.Length || index == _currentWeaponIndex) return;

        if (_previousWeaponIndex >= 0 && _previousWeaponIndex < _weapons.Length)
            _weapons[_previousWeaponIndex].GetComponent<SingleShotGun>()?.Deactivate();

        _currentWeaponIndex = index;
        _weapons[_currentWeaponIndex].GetComponent<SingleShotGun>()?.Activate();
        _previousWeaponIndex = _currentWeaponIndex;

        if (photonView.IsMine)
        {
            ExitGames.Client.Photon.Hashtable hashtable = new ExitGames.Client.Photon.Hashtable { { "WeaponIndex", _currentWeaponIndex } };
            PhotonNetwork.LocalPlayer.SetCustomProperties(hashtable);
        }
    }

    public static void AddKillToLocalPlayer()
    {
        int kills = PhotonNetwork.LocalPlayer.CustomProperties.TryGetValue("Kills", out object k) ? (int)k : 0;
        kills++;
        PhotonNetwork.LocalPlayer.SetCustomProperties(new ExitGames.Client.Photon.Hashtable { { "Kills", kills } });
        Debug.Log($"AddKillToLocalPlayer: Kills теперь {kills}");
    }

    public void ShowVignette() => StartCoroutine(FadeVignetteViaCanvasGroup());

    private IEnumerator FadeVignetteViaCanvasGroup()
    {
        for (float t = 0; t < vignetteFadeInDuration; t += Time.deltaTime)
        {
            vignetteCanvasGroup.alpha = t / vignetteFadeInDuration;
            yield return null;
        }
        yield return new WaitForSeconds(vignetteHoldDuration);
        for (float t = 0; t < vignetteFadeOutDuration; t += Time.deltaTime)
        {
            vignetteCanvasGroup.alpha = 1f - (t / vignetteFadeOutDuration);
            yield return null;
        }
        vignetteCanvasGroup.alpha = 0f;
    }
}

/// <summary>
/// Интерфейс для объектов, которые можно повредить.
/// </summary>
public interface IDamagable
{
    void TakeDamage(float damage, Player attacker);
    float currentHealth { get; }
}