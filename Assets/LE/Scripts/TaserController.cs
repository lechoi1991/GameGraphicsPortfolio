using UnityEngine;
using UnityEngine.InputSystem;

public class TaserController : MonoBehaviour
{
    [Header("Taser References")]
    [SerializeField] private Transform taserMuzzle;
    [SerializeField] private Transform taserTarget;
    [SerializeField] private GameObject taserProjectilePrefab;

    private bool isFiring = false;

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
        // 이미 발사체가 날아가는 중이면 다시 생성하지 않음
        if (isFiring)
            return;

        if (taserMuzzle == null ||
            taserTarget == null ||
            taserProjectilePrefab == null)
        {
            Debug.LogWarning("TaserController: Reference가 설정되지 않았습니다.");
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
            Debug.LogError("TaserProjectile 컴포넌트를 찾을 수 없습니다.");
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

        // STEP 10에서 여기에 전기 VFX를 연결합니다.
    }
}