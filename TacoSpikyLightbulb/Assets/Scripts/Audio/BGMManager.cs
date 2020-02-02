using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMManager : MonoBehaviour
{
    public AudioClip[] AudioClips;
    private int curClipIdx = -1;

    private AudioSource audioSource;

    private List<string> uniqueStrings;

    // Start is called before the first frame update
    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        SetBGMIndex(0);

        uniqueStrings = new List<string>();
    }

    public bool SetBGMIndex(int idx)
    {
        var i = Mathf.Clamp(idx, 0, AudioClips.Length - 1);
        if (curClipIdx == i) return false;

        curClipIdx = i;
        audioSource.clip = AudioClips[curClipIdx];
        audioSource.Play();

        return true;
    }

    public void IncrementBGM(string uniqueString)
    {
        if (uniqueStrings.Contains(uniqueString)) return;
        uniqueStrings.Add(uniqueString);

        SetBGMIndex(curClipIdx + 1);
    }

    // Update is called once per frame
    void Update()
    {
        /*if (Input.GetKeyDown(KeyCode.E))
        {
            IncrementBGM();
        }*/
    }
}
