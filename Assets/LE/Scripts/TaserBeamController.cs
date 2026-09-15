using System;
using System.Collections;
using UnityEngine;

public class TaserBeamController : MonoBehaviour
{
    [SerializeField] private Transform muzzle;
    [SerializeField] private Transform target;
    [SerializeField] private float beamRadius = 0.04f;

    [Header("Beam Timing")]
    [SerializeField, Min(0.01f)] private float extendDuration = 0.3f;
    [SerializeField, Min(0f)] private float connectedDuration = 1.1f;

    [Header("Electric Motion")]
    [SerializeField, Range(0f, 0.5f)] private float radiusPulse = 0.16f;
    [SerializeField, Min(0f)] private float pulseSpeed = 32f;

    private Coroutine beamRoutine;
    private float reach;

    public void PlayBeam(Action onConnected, Action onFinished)
    {
        if (muzzle == null || target == null)
        {
            Debug.LogWarning("TaserBeamController: Muzzle 또는 Target이 설정되지 않았습니다.");
            onFinished?.Invoke();
            return;
        }

        gameObject.SetActive(true);

        if (beamRoutine != null)
        {
            StopCoroutine(beamRoutine);
        }

        beamRoutine = StartCoroutine(BeamSequence(onConnected, onFinished));
    }

    private IEnumerator BeamSequence(Action onConnected, Action onFinished)
    {
        reach = 0f;
        float elapsed = 0f;

        while (elapsed < extendDuration)
        {
            elapsed += Time.deltaTime;
            reach = Mathf.Clamp01(elapsed / extendDuration);
            yield return null;
        }

        reach = 1f;
        onConnected?.Invoke();

        if (connectedDuration > 0f)
        {
            yield return new WaitForSeconds(connectedDuration);
        }

        beamRoutine = null;
        gameObject.SetActive(false);
        onFinished?.Invoke();
    }

    private void LateUpdate()
    {
        if (muzzle == null || target == null)
            return;

        Vector3 start = muzzle.position;
        Vector3 end = Vector3.Lerp(start, target.position, reach);
        Vector3 direction = end - start;
        float distance = direction.magnitude;

        if (distance <= 0.001f)
        {
            transform.localScale = Vector3.zero;
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.time * pulseSpeed) * radiusPulse;
        float currentRadius = beamRadius * pulse;

        transform.position = (start + end) * 0.5f;
        transform.up = direction.normalized;
        transform.localScale = new Vector3(
            currentRadius,
            distance * 0.5f,
            currentRadius
        );
    }

    private void OnDisable()
    {
        beamRoutine = null;
        reach = 0f;
    }
}
