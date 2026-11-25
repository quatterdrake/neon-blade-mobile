using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// One object for the whole game: creates itself before the first scene,
// survives scene loads (DontDestroyOnLoad) and owns every AudioSource.
// Gameplay code only calls AudioManager.Play("jump") etc.
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioMixer Mixer { get; private set; }

    AudioSource[] sfxPool; // Output = SFX group; several sources so a pitch change never bends a sound still ringing
    int nextSfx;
    AudioSource ui;       // Output = UI group (child of SFX)
    MusicManager music;   // two sources, Output = Tracks group
    AudioClip menuMusic, levelMusic;

    // "hit" -> { hit_1, hit_2, hit_3 }
    readonly Dictionary<string, AudioClip[]> clips = new Dictionary<string, AudioClip[]>();
    readonly Dictionary<string, int> lastIndex = new Dictionary<string, int>();
    Scene handledScene;   // default = no scene yet

    // volume of each event inside the SFX group, and whether its pitch is randomised.
    // Short repeated sounds vary, jingles and the enemy wind-up stay exact.
    struct Ev { public float vol; public bool vary; public bool ui; public Ev(float v, bool r, bool u = false) { vol = v; vary = r; ui = u; } }
    static readonly Dictionary<string, Ev> events = new Dictionary<string, Ev>
    {
        { "jump",   new Ev(0.45f, true) },
        { "land",   new Ev(0.40f, true) },
        { "dash",   new Ev(0.60f, true) },
        { "hit",    new Ev(1.00f, true) },
        { "pickup", new Ev(0.75f, true) },
        { "windup", new Ev(1.00f, false) },
        { "alert",  new Ev(0.55f, false) },
        { "death",  new Ev(1.00f, false) },
        { "win",    new Ev(0.80f, false) },
        { "lose",   new Ev(0.80f, false) },
        { "click",  new Ev(0.60f, true, true) },
    };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Boot()
    {
        if (Instance != null) return;
        var go = new GameObject("AudioManager");
        DontDestroyOnLoad(go);
        go.AddComponent<AudioManager>();
    }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        Mixer = Resources.Load<AudioMixer>("GameMix");
        if (Mixer == null)
            Debug.LogWarning("AudioManager: Assets/Resources/GameMix.mixer not found. Sounds play without the mixer.");
        else
            Mixer.updateMode = AudioMixerUpdateMode.UnscaledTime;   // snapshots keep moving while Time.timeScale = 0

        sfxPool = new AudioSource[6];
        for (int k = 0; k < sfxPool.Length; k++) sfxPool[k] = MakeSource("SFX", false);
        ui = MakeSource("UI", false);
        music = gameObject.AddComponent<MusicManager>();
        music.Init(MakeSource("Tracks", true), MakeSource("Tracks", true));

        LoadClips();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void Start()
    {
        // mixer.SetFloat does not work in Awake, so saved volumes are applied here
        VolumeSettings.ApplyAll();
        OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
    }

    void OnDestroy()
    {
        if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // ------------------------------------------------------------ public API

    public static void Play(string id)
    {
        if (Instance != null) Instance.PlayNow(id);
    }

    public static void PlayDelayed(string id, float delay)
    {
        if (Instance != null) Instance.StartCoroutine(Instance.DelayRoutine(id, delay));
    }

    public static void Click() => Play("click");

    // Paused snapshot: Tracks group quieter and muffled (lowpass)
    public void SetPaused(bool paused, float time = 0.3f)
    {
        if (Mixer == null) return;
        AudioMixerSnapshot snap = Mixer.FindSnapshot(paused ? "Paused" : "Normal");
        if (snap != null) snap.TransitionTo(time);
    }

    // ------------------------------------------------------------ internals

    void PlayNow(string id)
    {
        if (!clips.TryGetValue(id, out AudioClip[] list) || list.Length == 0)
        {
            Debug.LogWarning("AudioManager: no clip for '" + id + "'");
            return;
        }
        events.TryGetValue(id, out Ev ev);
        if (ev.vol == 0f) ev = new Ev(1f, true);

        // pick a different clip than last time, so 2–3 versions really alternate
        int i = Random.Range(0, list.Length);
        if (list.Length > 1 && lastIndex.TryGetValue(id, out int last) && i == last)
            i = (i + 1) % list.Length;
        lastIndex[id] = i;

        AudioSource src = ui;
        if (!ev.ui) { src = sfxPool[nextSfx]; nextSfx = (nextSfx + 1) % sfxPool.Length; }
        // PlayOneShot uses the source pitch: set it right before the shot (on the next pooled source)
        src.pitch = ev.vary ? Random.Range(0.92f, 1.08f) : 1f;
        src.PlayOneShot(list[i], ev.vol);
    }

    IEnumerator DelayRoutine(string id, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);   // real time: works during a scene reload or pause
        PlayNow(id);
    }

    AudioSource MakeSource(string groupName, bool loop)
    {
        var s = gameObject.AddComponent<AudioSource>();
        s.playOnAwake = false;
        s.loop = loop;
        s.spatialBlend = 0f;            // 2D game: same volume everywhere
        s.outputAudioMixerGroup = FindGroup(groupName);
        return s;
    }

    AudioMixerGroup FindGroup(string groupName)
    {
        if (Mixer == null) return null;
        foreach (string path in new[] { groupName, "Master" })
            foreach (var g in Mixer.FindMatchingGroups(path))
                if (g.name == groupName) return g;
        Debug.LogWarning("AudioManager: mixer group '" + groupName + "' not found");
        return null;
    }

    void LoadClips()
    {
        // every file in Resources/Audio/SFX named <event>_<n>.wav
        var groups = new Dictionary<string, List<AudioClip>>();
        foreach (var c in Resources.LoadAll<AudioClip>("Audio/SFX"))
        {
            int cut = c.name.LastIndexOf('_');
            string id = cut > 0 ? c.name.Substring(0, cut) : c.name;
            if (!groups.ContainsKey(id)) groups[id] = new List<AudioClip>();
            groups[id].Add(c);
        }
        foreach (var kv in groups) clips[kv.Key] = kv.Value.ToArray();

        menuMusic = Resources.Load<AudioClip>("Audio/Music/music_menu");
        levelMusic = Resources.Load<AudioClip>("Audio/Music/music_level");
        Debug.Log("AudioManager: loaded " + clips.Count + " sound events");
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene == handledScene) return;   // Start and sceneLoaded can both report the first scene
        handledScene = scene;

        PauseMenu.ResetState();
        SetPaused(false, 0f);
        music.Play(scene.name == "MainMenu" ? menuMusic : levelMusic, 1f);
        HookButtonClicks();
    }

    // every menu button clicks; gameplay buttons (Jump, Attack, move) already have their own sounds
    void HookButtonClicks()
    {
        foreach (var b in FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            if (!IsGameplayButton(b)) b.onClick.AddListener(Click);
    }

    static bool IsGameplayButton(Button b)
    {
        if (b.GetComponent<HoldButton>() != null) return true;
        for (int i = 0; i < b.onClick.GetPersistentEventCount(); i++)
            if (b.onClick.GetPersistentTarget(i) is PlayerController) return true;
        return false;
    }

    // phone: the app can be killed from the background, so save settings when it leaves
    void OnApplicationPause(bool paused) { if (paused) SaveManager.Save(); }
    void OnApplicationQuit() { SaveManager.Save(); }
}
