using UnityEngine;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;


public class AudioManager : Singleton<AudioManager> {
    private List<AudioSource> soundSourceList = new List<AudioSource>();
    private List<AudioSource> musicSourceList = new List<AudioSource>();

    private int currentMusicIndex = -1;

    private bool soundEnable = true;
    private bool musicEnable = true;

	private float soundVol = 1;
	private float musicVol = 1;

	// UserInfo userInfo = UserInfo.Instance;

    void Awake() {
        soundEnable = PlayerPrefs.GetInt(Define.PP_Sound, 1) > 0;
        musicEnable = PlayerPrefs.GetInt(Define.PP_Music, 1) > 0;

        foreach (string file in Define.soundFiles) {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = Resources.Load<AudioClip>(file);
            soundSourceList.Add(source);
        }

        foreach (string file in Define.musicFiles) {
            var source = gameObject.AddComponent<AudioSource>();
            source.clip = Resources.Load<AudioClip>(file);
            source.loop = true;
            musicSourceList.Add(source);
        }
    }

	// Use this for initialization
	void Start () {	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void PlaySound(int index, float musicTmpVol = -1) {
        if (!soundEnable) {
            return;
        }
        if (index >= 0 && index < soundSourceList.Count) {
			var source = soundSourceList[index];
			source.volume = soundVol;
			source.Play();

            if (currentMusicIndex >= 0 && musicTmpVol >= 0) {
                musicSourceList[currentMusicIndex].volume = musicTmpVol;
                StartCoroutine(WaitForAudioSource(source, delegate() {
                    StartCoroutine(ProgressiveAudioSourceVol(musicSourceList[currentMusicIndex], musicVol));
                }));
            }
        }
    }

    // public void PlaySound(string path, float musicTmpVol = -1) {
    //     if (!soundEnable) {
    //         return;
    //     }

    //     var source = new AudioSource();
    //     source.clip = Resources.Load<AudioClip>(path);
    //     source.volume = soundVol;
    //     source.Play();

    //     if (currentMusicIndex >= 0 && musicTmpVol > 0) {
    //         musicSourceList[currentMusicIndex].volume = musicTmpVol;
    //         StartCoroutine(WaitForAudioSource(source, delegate() {
    //             StartCoroutine(ProgressiveAudioSourceVol(musicSourceList[currentMusicIndex], musicVol));
    //         }));
    //     }
    // }

    public void PlayMusic(int index) {
        if (currentMusicIndex == index) {
            return;
        }
        if (!musicEnable) {
            currentMusicIndex = index;
            return;
        }

        if (currentMusicIndex >= 0 && currentMusicIndex < musicSourceList.Count) {
            musicSourceList[currentMusicIndex].Stop();
        }

        if (index >= 0 && index < musicSourceList.Count) {
			var source = musicSourceList[index];
			source.volume = musicVol;
			source.Play();
        }
        
        currentMusicIndex = index;
    }

    public void StopAllMusic() {
        foreach (var source in musicSourceList) {
            source.Stop();
        }
    }

    public float GetMusicVol() {
        return musicVol;
    }

    public float GetSoundVol() {
        return soundVol;
    }

	public void SetMusicVol(float vol) {
		musicVol = vol;

		if (currentMusicIndex >= 0) {
			var source = musicSourceList[currentMusicIndex];
			if (vol > 0) {
				source.volume = musicVol;
				if (!source.isPlaying) {
					source.Play();
				}
			}
			else {
				if (source.isPlaying) {
					source.Stop();
				}
			}
		}
	}

	public void SetSoundVol(float vol) {
		soundVol = vol;
	}

    public void SetMusicEnable(bool enable) {
        musicEnable = enable;

        if (currentMusicIndex >= 0 && musicSourceList.Count > 0) {
            var source = musicSourceList[currentMusicIndex];
            if (enable) {
                if (!source.isPlaying) {
                    source.Play();
                }
            }
            else {
                if (source.isPlaying) {
                    source.Stop();
                }
            }
        }
    }

    public void SetSoundEnable(bool enable) {
        soundEnable = enable;
    }

    private IEnumerator WaitForAudioSource(AudioSource source, UnityAction callback, float delayTime = 0) {
        do {
            yield return null;
        } while (source.isPlaying);

        if (delayTime > 0) {
            yield return new WaitForSeconds(delayTime);
        }

        if (callback != null) {
            callback();
        }
    }

    private IEnumerator ProgressiveAudioSourceVol(AudioSource source, float volume) {
        yield return new WaitForSeconds(0.1f);
        source.volume += 0.05f;
        if (source.volume >= volume) {
            source.volume = source.volume;
        } else {
            StartCoroutine(ProgressiveAudioSourceVol(source, volume));
        }
    }
}
