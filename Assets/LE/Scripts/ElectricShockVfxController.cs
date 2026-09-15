using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ElectricShockVfxController : MonoBehaviour
{
    private const int ContourCount = 3;
    private const int BranchCount = 7;
    private const int ContourPointCount = 28;
    private const int BranchPointCount = 5;

    [Header("Silhouette")]
    [SerializeField, Min(0f)] private float surfacePadding = 0.035f;
    [SerializeField, Min(0f)] private float contourLayerSpacing = 0.022f;
    [SerializeField, Min(0f)] private float contourJitter = 0.035f;

    [Header("Electric Branches")]
    [SerializeField] private float branchLength = 0.12f;
    [SerializeField] private float branchJitter = 0.03f;
    [SerializeField] private float redrawInterval = 0.045f;

    private readonly List<LineRenderer> contours = new List<LineRenderer>();
    private readonly List<LineRenderer> branches = new List<LineRenderer>();

    private Transform followTarget;
    private Renderer followRenderer;
    private Camera viewCamera;
    private Material lineMaterial;
    private bool ownsMaterial;
    private float redrawTimer;
    private int redrawIndex;

    public void Initialize(Transform target, Material electricMaterial)
    {
        followTarget = target;
        followRenderer = target != null ? target.GetComponentInParent<Renderer>() : null;
        viewCamera = Camera.main;
        lineMaterial = electricMaterial;

        if (lineMaterial == null)
        {
            Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
            if (shader == null)
            {
                shader = Shader.Find("Sprites/Default");
            }

            lineMaterial = new Material(shader)
            {
                name = "Runtime Electric Shock Material"
            };
            lineMaterial.SetColor("_BaseColor", new Color(4f, 2.4f, 0.05f, 1f));
            lineMaterial.SetColor("_Color", new Color(1f, 0.78f, 0.05f, 1f));
            ownsMaterial = true;
        }

        CreateLines();
        RedrawElectricity();
    }

    private void CreateLines()
    {
        if (contours.Count > 0)
            return;

        for (int i = 0; i < ContourCount; i++)
        {
            LineRenderer line = CreateLine($"Electric Contour {i + 1}", ContourPointCount, true);
            float width = 0.045f - i * 0.01f;
            line.startWidth = width;
            line.endWidth = width;
            contours.Add(line);
        }

        for (int i = 0; i < BranchCount; i++)
        {
            LineRenderer line = CreateLine($"Electric Branch {i + 1}", BranchPointCount, false);
            line.startWidth = 0.035f;
            line.endWidth = 0.012f;
            branches.Add(line);
        }
    }

    private LineRenderer CreateLine(string objectName, int pointCount, bool loop)
    {
        GameObject lineObject = new GameObject(objectName);
        lineObject.transform.SetParent(transform, false);

        LineRenderer line = lineObject.AddComponent<LineRenderer>();
        line.sharedMaterial = lineMaterial;
        line.positionCount = pointCount;
        line.loop = loop;
        line.useWorldSpace = true;
        line.alignment = LineAlignment.View;
        line.textureMode = LineTextureMode.Stretch;
        line.numCornerVertices = 2;
        line.numCapVertices = 2;
        line.shadowCastingMode = ShadowCastingMode.Off;
        line.receiveShadows = false;
        line.lightProbeUsage = LightProbeUsage.Off;
        line.reflectionProbeUsage = ReflectionProbeUsage.Off;

        Gradient gradient = new Gradient();
        gradient.SetKeys(
            new[]
            {
                new GradientColorKey(new Color(1f, 0.62f, 0.02f), 0f),
                new GradientColorKey(new Color(1f, 0.98f, 0.25f), 0.5f),
                new GradientColorKey(new Color(1f, 0.52f, 0.01f), 1f)
            },
            new[]
            {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.8f),
                new GradientAlphaKey(0.7f, 1f)
            }
        );
        line.colorGradient = gradient;

        return line;
    }

    private void LateUpdate()
    {
        if (followTarget == null)
        {
            Destroy(gameObject);
            return;
        }

        if (viewCamera == null)
        {
            viewCamera = Camera.main;
        }

        redrawTimer -= Time.deltaTime;
        if (redrawTimer <= 0f)
        {
            redrawTimer = redrawInterval;
            redrawIndex++;
            RedrawElectricity();
        }
    }

    private void RedrawElectricity()
    {
        Vector3 right = viewCamera != null ? viewCamera.transform.right : Vector3.right;
        Vector3 up = viewCamera != null ? viewCamera.transform.up : Vector3.up;
        Vector3 center = followTarget.position;
        float surfaceHalfWidth = 0.25f;
        float surfaceHalfHeight = 0.5f;

        if (followRenderer != null)
        {
            Bounds bounds = followRenderer.bounds;
            Vector3 extents = bounds.extents;
            center = bounds.center;

            // 카메라 화면의 가로/세로 축으로 Renderer의 실제 표면 크기를 투영한다.
            surfaceHalfWidth =
                Mathf.Abs(right.x) * extents.x +
                Mathf.Abs(right.y) * extents.y +
                Mathf.Abs(right.z) * extents.z;
            surfaceHalfHeight =
                Mathf.Abs(up.x) * extents.x +
                Mathf.Abs(up.y) * extents.y +
                Mathf.Abs(up.z) * extents.z;
        }

        for (int contourIndex = 0; contourIndex < contours.Count; contourIndex++)
        {
            LineRenderer line = contours[contourIndex];
            float contourWidth = surfaceHalfWidth + surfacePadding + contourIndex * contourLayerSpacing;
            float contourHeight = surfaceHalfHeight + surfacePadding + contourIndex * contourLayerSpacing;
            float phase = contourIndex * 1.7f;

            for (int pointIndex = 0; pointIndex < ContourPointCount; pointIndex++)
            {
                float angle = pointIndex / (float)ContourPointCount * Mathf.PI * 2f;
                float alternatingSpike = pointIndex % 2 == 0 ? 1f : -0.45f;
                float noise = SampleNoise(pointIndex, contourIndex + phase) - 0.5f;
                float jaggedOffset = contourJitter * (alternatingSpike + noise);

                Vector3 offset =
                    right * (Mathf.Sin(angle) * (contourWidth + jaggedOffset)) +
                    up * (Mathf.Cos(angle) * (contourHeight + jaggedOffset));

                line.SetPosition(pointIndex, center + offset);
            }

            float pulse = 1f + Mathf.Sin((Time.time + contourIndex) * 28f) * 0.18f;
            float width = (0.045f - contourIndex * 0.01f) * pulse;
            line.startWidth = width;
            line.endWidth = width;
            line.enabled = (redrawIndex + contourIndex) % 7 != 0;
        }

        for (int branchIndex = 0; branchIndex < branches.Count; branchIndex++)
        {
            LineRenderer line = branches[branchIndex];
            float angle = branchIndex / (float)BranchCount * Mathf.PI * 2f +
                          Mathf.Sin(redrawIndex * 0.73f + branchIndex) * 0.16f;
            Vector3 outward = (right * Mathf.Sin(angle) + up * Mathf.Cos(angle)).normalized;
            Vector3 tangent = Vector3.Cross(outward, viewCamera != null ? viewCamera.transform.forward : Vector3.forward);
            float outerOffset = surfacePadding + (ContourCount - 1) * contourLayerSpacing;

            Vector3 start = center +
                            right * (Mathf.Sin(angle) * (surfaceHalfWidth + outerOffset)) +
                            up * (Mathf.Cos(angle) * (surfaceHalfHeight + outerOffset));

            for (int pointIndex = 0; pointIndex < BranchPointCount; pointIndex++)
            {
                float progress = pointIndex / (float)(BranchPointCount - 1);
                float zigzag = pointIndex == 0
                    ? 0f
                    : (pointIndex % 2 == 0 ? 1f : -1f) * branchJitter * (1f - progress * 0.35f);
                float noise = (SampleNoise(pointIndex + branchIndex * 3, branchIndex + 9f) - 0.5f) * branchJitter;

                line.SetPosition(
                    pointIndex,
                    start + outward * (branchLength * progress) + tangent * (zigzag + noise)
                );
            }

            line.enabled = (redrawIndex + branchIndex * 2) % 5 != 0;
        }
    }

    private float SampleNoise(float point, float layer)
    {
        return Mathf.PerlinNoise(point * 0.47f + layer * 2.13f, redrawIndex * 0.37f + layer);
    }

    private void OnDestroy()
    {
        if (ownsMaterial && lineMaterial != null)
        {
            Destroy(lineMaterial);
        }
    }
}
