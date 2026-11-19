using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;

[DisallowMultipleComponent]
public class CharacterAnimDriver : MonoBehaviour
{
    class Overlay
    {
        public string name;
        public int index = -1;
        public float length = 1f;
        public bool loop;
        public bool holdLast;
        public float fadeIn = 0.06f, fadeOut = 0.15f, speed = 1f;
        public bool active, held;
        public float time, blend;
        public int scrubFrame = -10;
        public float scrubT;
    }

    PlayableGraph graph;
    AnimationMixerPlayable mixer;
    AnimationClipPlayable[] players;
    float[] weights;

    int idle = -1, walk = -1, run = -1;
    float idleLen = 1f, walkLen = 1f, runLen = 1f;
    float idleTime, locoPhase;
    float move, moveTarget, locoSpeed = 1f;

    readonly List<Overlay> overlays = new List<Overlay>();
    readonly List<Material> bodyMats = new List<Material>();
    Color baseColor = Color.white;
    float flash;
    float destroyAt = -1f;
    bool ready;

    public bool Ready => ready;

    public bool Setup(string resourcePath, string[] overlayNames, string[] loopNames, string[] holdNames)
    {
        AnimationClip[] all = Resources.LoadAll<AnimationClip>(resourcePath);
        List<AnimationClip> clips = new List<AnimationClip>();
        foreach (AnimationClip c in all)
            if (!c.name.StartsWith("__preview__")) clips.Add(c);

        if (clips.Count == 0)
        {
            Debug.LogWarning("[Animaciones] El FBX Resources/" + resourcePath + " cargó, pero NO tiene clips. " +
                             "Seleccioná el .fbx > pestaña Animation y fijate que 'Import Animation' esté marcado.");
            return false;
        }

        string names = "";
        foreach (AnimationClip c in clips) names += c.name + ", ";
        Debug.Log("[Animaciones] " + resourcePath + " clips: " + names);

        Animator animator = GetComponentInChildren<Animator>();
        if (animator == null) animator = gameObject.AddComponent<Animator>();
        animator.runtimeAnimatorController = null;
        animator.applyRootMotion = false;
        animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;

        graph = PlayableGraph.Create("CharAnim_" + name);
        graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
        AnimationPlayableOutput output = AnimationPlayableOutput.Create(graph, "out", animator);

        mixer = AnimationMixerPlayable.Create(graph, clips.Count);
        output.SetSourcePlayable(mixer);
        players = new AnimationClipPlayable[clips.Count];
        weights = new float[clips.Count];

        for (int i = 0; i < clips.Count; i++)
        {
            players[i] = AnimationClipPlayable.Create(graph, clips[i]);
            players[i].SetApplyFootIK(false);
            players[i].SetSpeed(0.0);
            graph.Connect(players[i], 0, mixer, i);
            mixer.SetInputWeight(i, 0f);

            string n = ShortName(clips[i].name);
            if (n == "Idle") { idle = i; idleLen = clips[i].length; }
            else if (n == "Walk") { walk = i; walkLen = clips[i].length; }
            else if (n == "Run") { run = i; runLen = clips[i].length; }
        }

        foreach (string on in overlayNames)
        {
            for (int i = 0; i < clips.Count; i++)
            {
                if (ShortName(clips[i].name) != on) continue;
                Overlay o = new Overlay();
                o.name = on;
                o.index = i;
                o.length = Mathf.Max(0.05f, clips[i].length);
                o.loop = System.Array.IndexOf(loopNames, on) >= 0;
                o.holdLast = System.Array.IndexOf(holdNames, on) >= 0;
                overlays.Add(o);
                break;
            }
        }

        foreach (SkinnedMeshRenderer smr in GetComponentsInChildren<SkinnedMeshRenderer>(true))
            smr.updateWhenOffscreen = true;

        graph.Play();
        ready = true;
        return true;
    }

    static string ShortName(string clipName)
    {
        int i = clipName.LastIndexOf('|');
        return i >= 0 ? clipName.Substring(i + 1) : clipName;
    }

    public void ApplyMaterials(Color body, Color accent, bool castShadows = true)
    {
        baseColor = body;
        foreach (Renderer r in GetComponentsInChildren<Renderer>(true))
        {
            Material[] mats = r.materials;
            for (int i = 0; i < mats.Length; i++)
            {
                string n = mats[i].name.ToLowerInvariant();
                if (n.Contains("body")) { SetColor(mats[i], body); bodyMats.Add(mats[i]); }
                else if (n.Contains("dark")) SetColor(mats[i], new Color(0.10f, 0.10f, 0.13f));
                else if (n.Contains("skin")) SetColor(mats[i], new Color(0.78f, 0.74f, 0.70f));
                else if (n.Contains("accent")) SetColor(mats[i], accent);
                else if (n.Contains("eye")) SetColor(mats[i], Color.white);

                if (mats[i].HasProperty("_Smoothness")) mats[i].SetFloat("_Smoothness", 0.15f);
            }
            r.materials = mats;
            if (!castShadows) r.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        }
    }

    static void SetColor(Material m, Color c)
    {
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", c);
        if (m.HasProperty("_Color")) m.SetColor("_Color", c);
    }

    public void FixImportScale(float expectedLargestDimension)
    {
        Bounds b = default;
        bool has = false;
        foreach (SkinnedMeshRenderer r in GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            if (!has) { b = r.bounds; has = true; }
            else b.Encapsulate(r.bounds);
        }
        if (!has) return;

        float size = Mathf.Max(b.size.x, Mathf.Max(b.size.y, b.size.z));
        if (size < 0.0001f) return;

        float parentScale = transform.parent != null ? transform.parent.lossyScale.y : 1f;
        float ratio = expectedLargestDimension * parentScale / size;

        if (ratio > 3f || ratio < 0.33f)
        {
            transform.localScale *= ratio;
            Debug.LogWarning($"{name}: el modelo medía {size:F3} m (esperado {expectedLargestDimension * parentScale:F2}); lo escalé x{ratio:F2}.");
        }
    }

    public void SetMove(float value, float playbackSpeed = 1f)
    {
        moveTarget = Mathf.Clamp(value, 0f, 2f);
        locoSpeed = Mathf.Max(0.1f, playbackSpeed);
    }

    public void Play(string clipName, float speed = 1f)
    {
        Overlay o = Find(clipName);
        if (o == null) return;
        o.time = 0f;
        o.active = true;
        o.speed = Mathf.Max(0.05f, speed);
    }

    public void SetHeld(string clipName, bool held, float speed = 1f)
    {
        Overlay o = Find(clipName);
        if (o == null) return;
        o.held = held;
        o.speed = Mathf.Max(0.05f, speed);
    }

    public void Scrub(string clipName, float t01)
    {
        Overlay o = Find(clipName);
        if (o == null) return;
        o.scrubFrame = Time.frameCount;
        o.scrubT = Mathf.Clamp01(t01);
    }

    public float ClipLength(string clipName)
    {
        Overlay o = Find(clipName);
        return o != null ? o.length : 1f;
    }

    public void SetBodyColor(Color c) { baseColor = c; }
    public void Flash() { flash = 1f; }
    public void DestroyAfter(float seconds) { destroyAt = Time.time + seconds; }

    Overlay Find(string n)
    {
        for (int i = 0; i < overlays.Count; i++)
            if (overlays[i].name == n) return overlays[i];
        return null;
    }

    void Update()
    {
        if (!ready) return;

        if (destroyAt > 0f && Time.time >= destroyAt)
        {
            Destroy(gameObject);
            return;
        }

        float dt = Time.deltaTime;
        if (dt <= 0f) return;

        for (int i = 0; i < weights.Length; i++) weights[i] = 0f;

        float rem = 1f;
        for (int k = 0; k < overlays.Count; k++)
        {
            Overlay o = overlays[k];
            float target;

            bool scrubbing = o.scrubFrame >= Time.frameCount - 1;
            if (scrubbing)
            {
                o.active = true;
                o.time = o.scrubT * o.length;
                target = o.scrubT > 0.93f ? 0f : 1f;
            }
            else if (o.loop)
            {
                if (o.held && !o.active) { o.active = true; o.time = 0f; }
                if (o.active)
                {
                    o.time += dt * o.speed;
                    if (o.time >= o.length)
                    {
                        if (o.held) o.time -= o.length;
                        else { o.active = false; o.time = 0f; }
                    }
                }
                target = o.active ? 1f : 0f;
                if (o.active && !o.held && o.time > o.length - o.fadeOut * o.speed) target = 0f;
            }
            else
            {
                if (o.active)
                {
                    o.time += dt * o.speed;
                    if (o.time >= o.length)
                    {
                        if (o.holdLast) o.time = o.length;
                        else { o.active = false; o.time = 0f; }
                    }
                }
                target = o.active ? 1f : 0f;
                if (o.active && !o.holdLast && o.time > o.length - o.fadeOut * o.speed) target = 0f;
            }

            float fade = target > o.blend ? o.fadeIn : o.fadeOut;
            o.blend = Mathf.MoveTowards(o.blend, target, dt / Mathf.Max(0.01f, fade));

            float w = o.blend * rem;
            weights[o.index] += w;
            rem -= w;
            players[o.index].SetTime(Mathf.Min(o.time, o.length));
        }

        move = Mathf.MoveTowards(move, moveTarget, 5f * dt);

        float wi = Mathf.Max(0f, 1f - move);
        float wr = Mathf.Max(0f, move - 1f);
        float ww = Mathf.Max(0f, 1f - Mathf.Abs(move - 1f));
        if (walk < 0) { wi += ww; ww = 0f; }
        if (run < 0) { ww += wr; wr = 0f; }

        float cycle = Mathf.Lerp(walkLen, runLen, Mathf.Clamp01(move - 1f));
        locoPhase = Mathf.Repeat(locoPhase + dt * locoSpeed / Mathf.Max(0.1f, cycle), 1f);
        idleTime = Mathf.Repeat(idleTime + dt, Mathf.Max(0.1f, idleLen));

        if (idle >= 0) { weights[idle] += rem * wi; players[idle].SetTime(idleTime); }
        if (walk >= 0) { weights[walk] += rem * ww; players[walk].SetTime(locoPhase * walkLen); }
        if (run >= 0)  { weights[run]  += rem * wr; players[run].SetTime(locoPhase * runLen); }

        for (int i = 0; i < weights.Length; i++) mixer.SetInputWeight(i, weights[i]);
    }

    void LateUpdate()
    {
        if (bodyMats.Count == 0) return;
        flash = Mathf.MoveTowards(flash, 0f, Time.unscaledDeltaTime / 0.15f);
        Color c = Color.Lerp(baseColor, Color.white, flash);
        for (int i = 0; i < bodyMats.Count; i++)
            if (bodyMats[i] != null) SetColor(bodyMats[i], c);
    }

    void OnDestroy()
    {
        if (graph.IsValid()) graph.Destroy();
    }
}
