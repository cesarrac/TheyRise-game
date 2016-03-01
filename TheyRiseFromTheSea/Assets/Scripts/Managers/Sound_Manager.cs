using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Sound_Manager : MonoBehaviour {

    public static Sound_Manager Instance { get; protected set; }

    float soundCooldown = 0;

    public AudioClip[] sounds;

    Dictionary<string, AudioClip> soundMap = new Dictionary<string, AudioClip>();

    void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            DestroyImmediate(gameObject);
        }
    }

    void Start()
    {
        if (soundMap.Count == 0)
            InitSoundMap();
    }

    void InitSoundMap()
    {
        foreach (AudioClip sound in sounds)
        {
            soundMap.Add(sound.name, sound);
        }

       // Debug.Log("SOUNDMAN: Sound map initialized with " + soundMap.Count + " sounds!");
    }

    void Update()
    {
        soundCooldown -= Time.deltaTime;
    }


    public void PlaySound(string id)
    {
        if (soundMap.ContainsKey(id))
        {
            if (soundCooldown > 0)
                return;

            AudioSource.PlayClipAtPoint(soundMap[id], Camera.main.transform.position);
            soundCooldown = -0.2f;
        }
   
    }


}
