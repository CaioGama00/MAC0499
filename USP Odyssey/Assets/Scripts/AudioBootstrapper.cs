using UnityEngine;

/// <summary>
/// Ensures UI/music audio sources stay 2D and are restarted after a scene load.
/// Prevents the build-only issue where these sounds become inaudible because
/// they are far from the listener or their initial PlayOnAwake call is missed.
/// </summary>
public static class AudioBootstrapper
{
    // Names taken from the scene hierarchy for the UI/music sources.
    private static readonly string[] AudioObjectNames = { "Menu Music", "Main Music", "Menu Sound" };

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void EnsureUiAudioIsAudible()
    {
        foreach (string objectName in AudioObjectNames)
        {
            Force2DAndPlay(objectName);
        }
    }

    private static void Force2DAndPlay(string objectName)
    {
        GameObject go = GameObject.Find(objectName);
        if (go == null)
        {
            return;
        }

        foreach (AudioSource source in go.GetComponentsInChildren<AudioSource>(includeInactive: true))
        {
            if (source == null) continue;

            // Push UI/music audio to 2D so distance to the listener does not mute it.
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.rolloffMode = AudioRolloffMode.Linear;

            // If PlayOnAwake was skipped during scene load, kick the clip on again.
            if (source.playOnAwake && source.clip != null && !source.isPlaying)
            {
                source.Play();
            }
        }
    }
}
