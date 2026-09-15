using System;
using UnityEngine;

public class TaserProjectile : MonoBehaviour
{
    [SerializeField] private float speed = 15f;
    [SerializeField] private float hitDistance = 0.1f;

    private Transform target;
    private Action onHit;

    private bool hasHit = false;

    public void Initialize(Transform targetTransform, Action hitCallback)
    {
        target = targetTransform;
        onHit = hitCallback;
        hasHit = false;
    }

    private void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        Vector3 direction = target.position - transform.position;

        if (direction.sqrMagnitude > 0.001f)
        {
            transform.forward = direction.normalized;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target.position,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target.position) <= hitDistance)
        {
            HitTarget();
        }
    }

    private void HitTarget()
    {
        if (hasHit)
            return;

        hasHit = true;

        Debug.Log("Taser Hit!");

        onHit?.Invoke();

        Destroy(gameObject);
    }
}
