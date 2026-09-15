using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.VFX;

public class TaserController : MonoBehaviour
{
    [Header("Taser References")]
    [SerializeField] private Transform taserMuzzle;
    [SerializeField] private Transform taserTarget;
    [SerializeField] private GameObject taserProjectilePrefab;

    [Header("Hit VFX")]
    [SerializeField] private GameObject hitVfxPrefab;

    private bool isFiring = false;

    private GameObject activeHitVfx;

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

        if (taserMuzzle == null ||
            taserTarget == null ||
            taserProjectilePrefab == null)
        {
            Debug.LogWarning(
                "TaserController: Reference가 설정되지 않았습니다."
            );

            return;
        }

        isFiring = true;

        GameObject projectile = Instantiate(
            taserProjectilePrefab,
            taserMuzzle.position,
            taserMuzzle.rotation
        );

        TaserProjectile taserProjectile =
            projectile.GetComponent<TaserProjectile>();

        if (taserProjectile == null)
        {
            Debug.LogError(
                "TaserProjectile 컴포넌트를 찾을 수 없습니다."
            );

            Destroy(projectile);
            isFiring = false;

            return;
        }

        taserProjectile.Initialize(
            taserTarget,
            OnTaserHit
        );
    }

    private void OnTaserHit()
    {
        Debug.Log("Taser 공격 성공!");

        isFiring = false;

        SpawnHitVFX();
    }

    private void SpawnHitVFX()
    {
        if (hitVfxPrefab == null)
        {
            Debug.LogWarning(
                "TaserController: Hit VFX Prefab이 설정되지 않았습니다."
            );

            return;
        }

        if (taserTarget == null)
        {
            Debug.LogWarning(
                "TaserController: Taser Target이 없습니다."
            );

            return;
        }

        // 기존 VFX가 남아 있다면 제거
        if (activeHitVfx != null)
        {
            Destroy(activeHitVfx);
        }

        // 적중 지점에 VFX 생성
        activeHitVfx = Instantiate(
            hitVfxPrefab,
            taserTarget.position,
            Quaternion.identity,
            taserTarget
        );

        // 코드에서 VFX 실행
        VisualEffect vfx =
            activeHitVfx.GetComponent<VisualEffect>();

        if (vfx != null)
        {
            vfx.Play();
        }
    }
}