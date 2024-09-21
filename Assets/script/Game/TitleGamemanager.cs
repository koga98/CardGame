using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class TitleGamemanager : MonoBehaviour
{
    string filePath;
    public static SoundDataBase soundDataBase;
    public Slider bgmSlider;
    public Slider SeSlider;
    public Slider voiceSlider;
    // Start is called before the first frame update

     void Awake()
    {
        soundDataBase = new SoundDataBase{
            soundSetting = new List<float>()
        };
    }
    void Start()
    {
        Screen.SetResolution(1280, 720, false);
        filePath = Application.persistentDataPath + "/" + "SoundData.json";
        LoadButton();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void LoadButton()
    {
        if (File.Exists(filePath))
        {
            StreamReader streamReader = new StreamReader(filePath);
            string data = streamReader.ReadToEnd();
            streamReader.Close();
            soundDataBase = JsonUtility.FromJson<SoundDataBase>(data);

            if (soundDataBase != null && soundDataBase.soundSetting.Count > 0)
            {
                bgmSlider.value = soundDataBase.soundSetting[0];
                SeSlider.value = soundDataBase.soundSetting[1];
                voiceSlider.value = soundDataBase.soundSetting[2];
                AudioManager.Instance.audioSource.volume = bgmSlider.value;
                AudioManager.Instance.SEaudioSource.volume = SeSlider.value;
                AudioManager.Instance.voiceSource.volume = voiceSlider.value;
            }
            else
                Debug.LogError("Failed to load deck data.");
        }
    }
}
