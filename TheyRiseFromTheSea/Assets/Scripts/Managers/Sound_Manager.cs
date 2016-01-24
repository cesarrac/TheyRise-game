using UnityEngine;
using System.Collections;

public class Sound_Manager : MonoBehaviour {

    public static Sound_Manager Instance { get; protected set; }

    public AudioClip build_sound;

    float soundCooldown = 0;

    void OnEnable()
    {
        Instance = this;
    }

    void Update()
    {
        soundCooldown -= Time.deltaTime;
    }

    public void Build()
    {
        if (soundCooldown > 0)
            return;

        AudioSource.PlayClipAtPoint(build_sound, Camera.main.transform.position);
        soundCooldown = -0.1f;
    }

}
