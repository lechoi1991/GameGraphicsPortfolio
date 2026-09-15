using UnityEngine;

public class TaserBeamController : MonoBehaviour
{
    [SerializeField] private Transform muzzle;
    [SerializeField] private Transform target;

    [SerializeField] private float beamRadius = 0.04f;

    private void LateUpdate()
    {
        if (muzzle == null || target == null)
            return;

        Vector3 start = muzzle.position;
        Vector3 end = target.position;

        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
            return;

        // 경찰과 도둑의 정확한 중간 위치
        transform.position = (start + end) * 0.5f;

        // Cylinder의 Y축을 경찰 → 도둑 방향으로 회전
        transform.up = direction.normalized;

        // 기본 Cylinder 높이는 2
        // 따라서 Y Scale = 실제 거리 / 2
        transform.localScale = new Vector3(
            beamRadius,
            distance * 0.5f,
            beamRadius
        );
    }
}