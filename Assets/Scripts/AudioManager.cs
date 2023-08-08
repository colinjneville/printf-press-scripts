using Functional.Option;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using UnityEngine;

using L = LocalizationString;
using LC = LocalizationConstant;
using LE = ILocalizationExpression;
using LF = LocalizationFormat;
using LI = LocalizationInt;
using LD = LocalizationDefault;

public sealed class AudioManager : MonoBehaviour {
    private sealed class AudioSourceExtra {
        public AudioSource AudioSource { get; }
        public float DefaultVolume { get; set; }
        public System.Collections.IEnumerator CurrentCoroutine { get; private set; }
        public AudioSourceExtra(AudioSource audioSource) {
            AudioSource = audioSource;
            DefaultVolume = 1f;
        }

        public void ResetVolume() => AudioSource.volume = DefaultVolume;

        public void StartCoroutine(MonoBehaviour mb, System.Collections.IEnumerable enumerable) {
            StopCoroutine(mb);
            var enumerator = CoroutineWrapper(enumerable).GetEnumerator();
            CurrentCoroutine = enumerator;
            mb.StartCoroutine(enumerator);
        }

        public void StopCoroutine(MonoBehaviour mb) {
            if (CurrentCoroutine is object) {
                mb.StopCoroutine(CurrentCoroutine);
                CurrentCoroutine = null;
            }
        }

        private System.Collections.IEnumerable CoroutineWrapper(System.Collections.IEnumerable enumerable) {
            foreach (var step in enumerable) {
                yield return step;
            }
            CurrentCoroutine = null;
        }
    }

    private AudioSourceExtra oneShotSource;
    private AudioSourceExtra windIntroSource;
    private AudioSourceExtra windLoopSource;
    private List<AudioSourceExtra> sources;
    private int windCount;

    private void Awake() {
        sources = new List<AudioSourceExtra>();
    }

    private void Start() {
        oneShotSource = NewSource();
        windIntroSource = NewSource();
        windIntroSource.AudioSource.clip = Overseer.GlobalAssets.WindIntroClip;
        windIntroSource.DefaultVolume = 0.2f;
        windIntroSource.ResetVolume();
        windLoopSource = NewSource();
        windLoopSource.AudioSource.clip = Overseer.GlobalAssets.WindLoopClip;
        windLoopSource.AudioSource.loop = true;
        windLoopSource.DefaultVolume = 0.2f;
        windIntroSource.ResetVolume();
    }

    private AudioSourceExtra GetSource() {
        if (sources.Count > 0) {
            var source = sources[sources.Count - 1];
            sources.RemoveAt(sources.Count - 1);
            source.DefaultVolume = 1f;
            return source;
        } else {
            return NewSource();
        }
    }

    private AudioSourceExtra NewSource() {
        var source = gameObject.AddComponent<AudioSource>();
        return new AudioSourceExtra(source);
    }

    public void PlayOneShot(AudioClip clip) {
        oneShotSource.AudioSource.PlayOneShot(clip);
    }

    public void StartWind() {
        if (windCount++ == 0) {
            var time = AudioSettings.dspTime;

            windIntroSource.StopCoroutine(this);
            windLoopSource.StopCoroutine(this);
            windIntroSource.ResetVolume();
            windLoopSource.ResetVolume();
            windIntroSource.AudioSource.Play();
            windLoopSource.AudioSource.PlayScheduled(time + Overseer.GlobalAssets.WindIntroClip.length);
        }
    }

    public void StopWind() {
        if (--windCount == 0) {
            if (windIntroSource.AudioSource.isPlaying) {
                windIntroSource.StartCoroutine(this, FadeOut(windIntroSource, 0.25f));
            }
            if (windLoopSource.AudioSource.isPlaying) {
                windLoopSource.StartCoroutine(this, FadeOut(windLoopSource, 0.25f));
            }
        }
    }

    private System.Collections.IEnumerable FadeOut(AudioSourceExtra source, float duration) {
        var prevTime = AudioSettings.dspTime;
        while (source.AudioSource.volume > 0) {
            yield return null;
            var time = AudioSettings.dspTime;
            var timeDiff = time - prevTime;
            prevTime = time;
            source.AudioSource.volume -= (float)(source.DefaultVolume * timeDiff / duration);
        }
        source.AudioSource.Stop();
        source.ResetVolume();
    }
}
