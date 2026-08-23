using UnityEditor;
using UnityEngine;

public static class TempSoundBombVFXSetup
{
[MenuItem("Tools/SoundBomb/Setup VFX (Temp)")]
    
public static void Setup()
    {
        GameObject root = GameObject.Find("SoundBombVFX");
        if (root == null)
        {
            Debug.LogError("No se encontro 'SoundBombVFX' en la escena.");
            return;
        }

        Transform shockwaveT = root.transform.Find("Shockwave");
        Transform debrisT = root.transform.Find("Debris");
        Transform flashT = root.transform.Find("Flash");

        ParticleSystem[] allPs = root.GetComponentsInChildren<ParticleSystem>(true);
        foreach (var ps in allPs)
        {
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }

        Material matRing = AssetDatabase.LoadAssetAtPath<Material>("Assets/Project/Materials/VFX/Mat_Ring_Shockwave.mat");
        Material matDebris = AssetDatabase.LoadAssetAtPath<Material>("Assets/Project/Materials/VFX/Mat_Debris.mat");
        Material matFlash = AssetDatabase.LoadAssetAtPath<Material>("Assets/Project/Materials/VFX/Mat_Flash_Core.mat");

        // ================= SHOCKWAVE =================
        ParticleSystem psRing = shockwaveT.GetComponent<ParticleSystem>();
        ParticleSystemRenderer rendRing = shockwaveT.GetComponent<ParticleSystemRenderer>();

        var mainRing = psRing.main;
        mainRing.duration = 0.4f;
        mainRing.loop = false;
        mainRing.startLifetime = 0.4f;
        mainRing.startSpeed = 0f;
        mainRing.startSize = 0.2f;
        mainRing.startColor = Color.white;
        mainRing.simulationSpace = ParticleSystemSimulationSpace.World;
        mainRing.playOnAwake = true;
        mainRing.startRotation3D = false;

        var emissionRing = psRing.emission;
        emissionRing.rateOverTime = 0f;
        emissionRing.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

        var shapeRing = psRing.shape;
        shapeRing.enabled = false;

        var sizeOverLifeRing = psRing.sizeOverLifetime;
        sizeOverLifeRing.enabled = true;
        AnimationCurve ringSizeCurve = new AnimationCurve(
            new Keyframe(0f, 0.2f),
            new Keyframe(0.3f, 1.6f),
            new Keyframe(1f, 2.0f)
        );
        sizeOverLifeRing.size = new ParticleSystem.MinMaxCurve(1f, ringSizeCurve);

        var colorOverLifeRing = psRing.colorOverLifetime;
        colorOverLifeRing.enabled = true;
        Gradient ringGrad = new Gradient();
        ringGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.5f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifeRing.color = ringGrad;

        rendRing.renderMode = ParticleSystemRenderMode.Billboard;
        rendRing.sharedMaterial = matRing;
        rendRing.sortingFudge = -10f;

        // ================= DEBRIS =================
        ParticleSystem psDebris = debrisT.GetComponent<ParticleSystem>();
        ParticleSystemRenderer rendDebris = debrisT.GetComponent<ParticleSystemRenderer>();

        var mainDebris = psDebris.main;
        mainDebris.duration = 1f;
        mainDebris.loop = false;
        mainDebris.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
        mainDebris.startSpeed = new ParticleSystem.MinMaxCurve(1.5f, 3.5f);
        mainDebris.startSize = new ParticleSystem.MinMaxCurve(0.12f, 0.2f);
        mainDebris.gravityModifier = 1.5f;
        mainDebris.simulationSpace = ParticleSystemSimulationSpace.World;
        mainDebris.playOnAwake = true;
        mainDebris.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
        mainDebris.maxParticles = 40;

        var emissionDebris = psDebris.emission;
        emissionDebris.rateOverTime = 0f;
        emissionDebris.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, new ParticleSystem.MinMaxCurve(14, 20)) });

        var shapeDebris = psDebris.shape;
        shapeDebris.enabled = true;
        shapeDebris.shapeType = ParticleSystemShapeType.Sphere;
        shapeDebris.radius = 0.15f;

        var texSheetDebris = psDebris.textureSheetAnimation;
        texSheetDebris.enabled = true;
        texSheetDebris.numTilesX = 2;
        texSheetDebris.numTilesY = 2;
        texSheetDebris.animation = ParticleSystemAnimationType.WholeSheet;
        texSheetDebris.frameOverTime = new ParticleSystem.MinMaxCurve(0f);
        texSheetDebris.startFrame = new ParticleSystem.MinMaxCurve(0, 3.99f);

        var colorOverLifeDebris = psDebris.colorOverLifetime;
        colorOverLifeDebris.enabled = true;
        Gradient debrisGrad = new Gradient();
        debrisGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.7f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifeDebris.color = debrisGrad;

        rendDebris.renderMode = ParticleSystemRenderMode.Billboard;
        rendDebris.sharedMaterial = matDebris;

        // ================= FLASH =================
        ParticleSystem psFlash = flashT.GetComponent<ParticleSystem>();
        ParticleSystemRenderer rendFlash = flashT.GetComponent<ParticleSystemRenderer>();

        var mainFlash = psFlash.main;
        mainFlash.duration = 0.15f;
        mainFlash.loop = false;
        mainFlash.startLifetime = 0.15f;
        mainFlash.startSpeed = 0f;
        mainFlash.startSize = 1.2f;
        mainFlash.startColor = Color.white;
        mainFlash.simulationSpace = ParticleSystemSimulationSpace.World;
        mainFlash.playOnAwake = true;

        var emissionFlash = psFlash.emission;
        emissionFlash.rateOverTime = 0f;
        emissionFlash.SetBursts(new ParticleSystem.Burst[] { new ParticleSystem.Burst(0f, 1) });

        var shapeFlash = psFlash.shape;
        shapeFlash.enabled = false;

        var sizeOverLifeFlash = psFlash.sizeOverLifetime;
        sizeOverLifeFlash.enabled = true;
        AnimationCurve flashSizeCurve = new AnimationCurve(
            new Keyframe(0f, 1f),
            new Keyframe(1f, 0.6f)
        );
        sizeOverLifeFlash.size = new ParticleSystem.MinMaxCurve(1f, flashSizeCurve);

        var colorOverLifeFlash = psFlash.colorOverLifetime;
        colorOverLifeFlash.enabled = true;
        Gradient flashGrad = new Gradient();
        flashGrad.SetKeys(
            new GradientColorKey[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new GradientAlphaKey[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(1f, 0.4f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        colorOverLifeFlash.color = flashGrad;

        rendFlash.renderMode = ParticleSystemRenderMode.Billboard;
        rendFlash.sharedMaterial = matFlash;
        rendFlash.sortingFudge = -20f;

        // ================= DESTROY SCRIPT =================
        var existing = root.GetComponent<DestroyParticleSystemOnFinish>();
        if (existing == null)
        {
            root.AddComponent<DestroyParticleSystemOnFinish>();
        }

        EditorUtility.SetDirty(root);
        UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(root.scene);

        Debug.Log("SoundBombVFX configurado correctamente: Shockwave, Debris, Flash + DestroyParticleSystemOnFinish.");
    }
}
