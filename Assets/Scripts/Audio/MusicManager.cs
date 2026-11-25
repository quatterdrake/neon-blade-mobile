using System.Collections;
using UnityEngine;

// Two looping AudioSources (both Output = Tracks). A new track fades in on the
// free source while the old one fades out. Lives on the AudioManager object,
// so it survives scene loads and the music never restarts on a level reload.
public class MusicManager : MonoBehaviour
{
    // music sits under the effects: 0.45 = about -7 dB on the source, the sliders work on top of that
    public float trackVolume = 0.45f;

    AudioSource a, b, current;

    public void Init(AudioSource first, AudioSource second)
    {
        a = first; b = second; current = a;
    }

    public void Play(AudioClip clip, float fade = 1f)
    {
        if (clip == null) return;
        if (current.clip == clip && current.isPlaying) return;   // same track keeps playing

        AudioSource next = current == a ? b : a;
        next.clip = clip;
        next.volume = 0f;
        next.Play();

        StopAllCoroutines();
        StartCoroutine(Crossfade(current, next, fade));
        current = next;
    }

    IEnumerator Crossfade(AudioSource from, AudioSource to, float time)
    {
        float fromStart = from.volume;
        for (float t = 0f; t < 1f; t += Time.unscaledDeltaTime / time)   // unscaled: works in pause
        {
            // equal-power curve: no loudness dip in the middle of the fade
            from.volume = fromStart * Mathf.Cos(t * Mathf.PI * 0.5f);
            to.volume = trackVolume * Mathf.Sin(t * Mathf.PI * 0.5f);
            yield return null;
        }
        from.Stop();
        from.volume = 0f;
        to.volume = trackVolume;
    }
}
