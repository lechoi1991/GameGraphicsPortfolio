using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class TaserController : MonoBehaviour
{
    [Header("Taser References")]
    [SerializeField] private Transform taserMuzzle;
    [SerializeField] private Transform taserTarget;

    [Header("Hit VFX")]
    [SerializeField] private GameObject hitVfxPrefab;

    [Header("Taser Beam")]
    [SerializeField] private GameObject taserBeam;

    private TaserBeamController beamController;
    private GameObject activeHitVfx;
    private bool isFiring;

    private void Awake()
    {
        if (taserBeam != null)
        {
            beamController = taserBeam.GetComponent<TaserBeamController>();
            taserBeam.SetActive(false);
        }
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            FireTaser();
        }
    }

    private void FireTaser()
    {
        if (isFiring)
            return;

        if (taserMuzzle == null || taserTarget == null || beamController == null)
        {
            Debug.LogWarning(
                "TaserController: Muzzle, Target 또는 Beam reference가 설정되지 않았습니다."
            );
            return;
        }

        isFiring = true;
        RemoveHitVfx();

        beamController.PlayBeam(OnBeamConnected, OnBeamFinished);
    }

    private void OnBeamConnected()
    {
        Debug.Log("Taser beam connected!");
        SpawnHitVfx();
    }

    private void OnBeamFinished()
    {
        RemoveHitVfx();
        isFiring = false;
    }

    private void SpawnHitVfx()
    {
        if (hitVfxPrefab == null || taserTarget == null)
        {
            Debug.LogWarning(
                "TaserController: Hit VFX Prefab 또는 Taser Target이 설정되지 않았습니다."
            );
            return;
        }

        activeHitVfx = Instantiate(
            hitVfxPrefab,
            taserTarget.position,
            taserTarget.rotation,
            taserTarget
        );

        // 기존 VFX Graph의 방사형 불꽃은 끄고, 캐릭터 실루엣을 감싸는
        // 감전 전기 아크로 교체한다.
        VisualEffect vfx = activeHitVfx.GetComponent<VisualEffect>();
        if (vfx != null)
        {
            vfx.Stop();
            vfx.enabled = false;
        }

        Material electricMaterial = null;
        Renderer beamRenderer = taserBeam != null
            ? taserBeam.GetComponent<Renderer>()
            : null;

        if (beamRenderer != null)
        {
            electricMaterial = beamRenderer.sharedMaterial;
        }

        ElectricShockVfxController shockVfx =
            activeHitVfx.AddComponent<ElectricShockVfxController>();
        shockVfx.Initialize(taserTarget, electricMaterial);
    }

    private void RemoveHitVfx()
    {
        if (activeHitVfx == null)
            return;

        Destroy(activeHitVfx);
        activeHitVfx = null;
    }

    private void OnDisable()
    {
        RemoveHitVfx();
        isFiring = false;
    }
}
