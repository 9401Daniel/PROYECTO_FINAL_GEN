using System.Collections.Generic;
using UnityEngine;

namespace Objects
{
    /// <summary>
    /// Makes a 3D object fade to a configurable opacity when the camera gets close.
    /// It fades every Renderer found at any depth in the hierarchy (children and
    /// children of children).
    ///
    /// Works with the URP "Universal Render Pipeline/Lit" shader: it switches each
    /// material to transparent mode while fading (via shader keywords) and back to
    /// opaque when restored. Materials are instantiated so shared assets are never
    /// modified.
    /// </summary>
    public class HideWhenClose : MonoBehaviour
    {
        [Header("Trigger")]
        [Tooltip("Distance (world units) at which the fade starts.")]
        [SerializeField] private float fadeDistance = 5f;

        [Header("Fade")]
        [Tooltip("Target opacity when faded (0 = invisible, 1 = fully opaque).")]
        [Range(0f, 1f)]
        [SerializeField] private float targetOpacity = 0.2f;

        [Tooltip("How fast the object fades in/out.")]
        [SerializeField] private float fadeSpeed = 8f;

        [Header("Camera")]
        [Tooltip("Leave empty to use Camera.main (works with Cinemachine when the camera is tagged MainCamera).")]
        [SerializeField] private Camera targetCamera;

        private readonly List<Renderer> renderers = new List<Renderer>();
        private readonly List<Material> instanceMaterials = new List<Material>();
        private readonly List<Color> baseColors = new List<Color>();
        private readonly List<int> baseSurface = new List<int>();

        private float currentOpacity = 1f;

        private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
        private static readonly int ColorId = Shader.PropertyToID("_Color");
        private static readonly int SurfaceId = Shader.PropertyToID("_Surface");
        private static readonly int SrcBlendId = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlendId = Shader.PropertyToID("_DstBlend");
        private static readonly int ZWriteId = Shader.PropertyToID("_ZWrite");

        // URP shader keywords that actually control transparent rendering.
        private static readonly string SurfaceTypeTransparent = "_SURFACE_TYPE_TRANSPARENT";
        private static readonly string BlendKeyword = "_BLEND";
        private static readonly string AlphaTestOn = "_ALPHATEST_ON";

        private void Awake()
        {
            CollectRenderers();
        }

        private void OnValidate()
        {
            targetOpacity = Mathf.Clamp01(targetOpacity);
        }

        private void CollectRenderers()
        {
            List<Renderer> found = new List<Renderer>();
            CollectRecursive(transform, found);

            foreach (Renderer r in found)
            {
                if (r is ParticleSystemRenderer || r.sharedMaterial == null)
                    continue;

                Material baseMat = r.sharedMaterial;
                Material inst = r.material; // instantiates so we don't touch the asset

                renderers.Add(r);
                instanceMaterials.Add(inst);

                if (baseMat.HasProperty(BaseColorId))
                    baseColors.Add(baseMat.GetColor(BaseColorId));
                else if (baseMat.HasProperty(ColorId))
                    baseColors.Add(baseMat.GetColor(ColorId));
                else
                    baseColors.Add(Color.white);

                baseSurface.Add(baseMat.HasProperty(SurfaceId) ? baseMat.GetInt(SurfaceId) : 0);
            }
        }

        private void Update()
        {
            float target = IsCameraClose() ? targetOpacity : 1f;

            if (Mathf.Approximately(currentOpacity, target))
                return;

            currentOpacity = Mathf.MoveTowards(currentOpacity, target, fadeSpeed * Time.deltaTime);
            ApplyOpacity(currentOpacity);
        }

        private bool IsCameraClose()
        {
            Camera cam = targetCamera != null ? targetCamera : Camera.main;
            if (cam == null)
                return false;

            return Vector3.Distance(cam.transform.position, transform.position) <= fadeDistance;
        }

        private void ApplyOpacity(float opacity)
        {
            bool transparent = opacity < 0.999f;

            for (int i = 0; i < renderers.Count; i++)
            {
                Material mat = instanceMaterials[i];
                if (mat == null)
                    continue;

                SetSurfaceMode(mat, transparent, baseSurface[i]);

                Color c = baseColors[i];
                c.a = opacity;

                if (mat.HasProperty(BaseColorId))
                    mat.SetColor(BaseColorId, c);
                else if (mat.HasProperty(ColorId))
                    mat.SetColor(ColorId, c);
            }
        }

        private void SetSurfaceMode(Material mat, bool transparent, int originalSurface)
        {
            if (!mat.HasProperty(SurfaceId))
            {
                // Non-URP fallback: just rely on the color alpha.
                return;
            }

            if (transparent)
            {
                mat.SetFloat(SurfaceId, 1f);
                mat.EnableKeyword(SurfaceTypeTransparent);
                mat.EnableKeyword(BlendKeyword);
                mat.DisableKeyword(AlphaTestOn);

                mat.SetFloat(SrcBlendId, 5f); // SrcAlpha
                mat.SetFloat(DstBlendId, 10f); // OneMinusSrcAlpha
                mat.SetFloat(ZWriteId, 0f);
                mat.SetOverrideTag("RenderType", "Transparent");
                mat.renderQueue = 3000;
            }
            else
            {
                mat.SetFloat(SurfaceId, originalSurface);
                mat.DisableKeyword(SurfaceTypeTransparent);
                mat.DisableKeyword(BlendKeyword);

                // Re-enable alpha test only if the original surface was cutout.
                if (originalSurface == 0)
                    mat.EnableKeyword(AlphaTestOn);

                mat.SetFloat(SrcBlendId, 1f); // One
                mat.SetFloat(DstBlendId, 0f); // Zero
                mat.SetFloat(ZWriteId, 1f);
                mat.SetOverrideTag("RenderType", "Opaque");
                mat.renderQueue = -1;
            }
        }

        private static void CollectRecursive(Transform t, List<Renderer> output)
        {
            Renderer r = t.GetComponent<Renderer>();
            if (r != null)
                output.Add(r);

            for (int i = 0; i < t.childCount; i++)
                CollectRecursive(t.GetChild(i), output);
        }
    }
}
