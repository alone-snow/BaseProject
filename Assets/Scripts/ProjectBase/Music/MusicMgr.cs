using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using static Unity.VisualScripting.Member;

public class MusicMgr : BaseManager<MusicMgr>
{
    //唯一的背景音乐组件
    private AudioSource bkMusic = null;
    //音乐大小
    public float bkValue = 1;

    //音效依附对象
    private GameObject soundObj = null;
    //音效列表
    private List<AudioSource> soundList = new List<AudioSource>();
    private Dictionary<string, Stack<AudioSource>> soundDic = new Dictionary<string, Stack<AudioSource>>();
    //音效大小
    public float soundValue = 1;

    public Action<float> onChangebkValue;
    public Action<float> onChangesoundValue;

    public MusicMgr()
    {
        MonoMgr.Instance.AddUpdateListener(Update);
        if (soundObj == null)
        {
            soundObj = new GameObject();
            GameObject.DontDestroyOnLoad(soundObj);
            soundObj.name = "Sound";
        }
    }

    public void Init(float bkValue,float soundValue)
    {
        this.bkValue = bkValue;
        this.soundValue = soundValue;
    }

    private void Update()
    {
        for( int i = soundList.Count - 1; i >=0; --i )
        {
            if( soundList[i] == null)
            {
                soundList.RemoveAt(i);
            }
            else
            {
                if (!soundList[i].isPlaying)
                {
                    PushSound(soundList[i]);
                    soundList.RemoveAt(i);
                }
            }

        }
    }

    /// <summary>
    /// 播放背景音乐
    /// </summary>
    /// <param name="name"></param>
    public void PlayBkMusic(string name)
    {
        if(bkMusic == null)
        {
            GameObject obj = new GameObject();
            GameObject.DontDestroyOnLoad(obj);
            obj.name = "BkMusic";
            bkMusic = obj.AddComponent<AudioSource>();
        }
        //异步加载背景音乐 加载完成后 播放
        ResMgr.Instance.LoadAsync<AudioClip>("Music/BK/" + name, (clip) =>
        {
            bkMusic.clip = clip;
            bkMusic.loop = true;
            bkMusic.volume = bkValue;
            bkMusic.Play();
        });

    }

    /// <summary>
    /// 暂停背景音乐
    /// </summary>
    public void PauseBKMusic()
    {
        if (bkMusic == null)
            return;
        bkMusic.Pause();
    }

    /// <summary>
    /// 停止背景音乐
    /// </summary>
    public void StopBKMusic()
    {
        if (bkMusic == null)
            return;
        bkMusic.Stop();
    }

    /// <summary>
    /// 改变背景音乐 音量大小
    /// </summary>
    /// <param name="v"></param>
    public void ChangeBKValue(float v)
    {
        bkValue = v;
        onChangebkValue?.Invoke(bkValue);
        if (bkMusic == null)
            return;
        bkMusic.volume = bkValue;
    }

    /// <summary>
    /// 播放音效
    /// </summary>
    public void PlaySound(string name, bool isLoop = false, UnityAction<AudioSource> callBack = null)
    {
        //当音效资源异步加载结束后 再添加一个音效
        AudioSource source = GetSound(name);
        source.loop = isLoop;
        source.volume = soundValue;
        source.Play();
        soundList.Add(source);
        if (callBack != null)
            callBack(source);
    }

    /// <summary>
    /// 改变音效声音大小
    /// </summary>
    /// <param name="value"></param>
    public void ChangeSoundValue( float value )
    {
        soundValue = value;
        onChangesoundValue(soundValue);
        for (int i = 0; i < soundList.Count; ++i)
            soundList[i].volume = value;
    }

    /// <summary>
    /// 停止音效
    /// </summary>
    public void StopSound(AudioSource source)
    {
        if( soundList.Contains(source) )
        {
            soundList.Remove(source);
            PushSound(source);
        }
    }

    private AudioSource GetSound(string name)
    {
        if(soundDic.TryGetValue(name, out var stack))
        {
            if(stack.Count!=0) return stack.Pop();
        }
        AudioClip clip = ResMgr.Instance.Load<AudioClip>("Music/Sound/" + name);
        AudioSource source = soundObj.AddComponent<AudioSource>();
        source.clip = clip;
        return source;
    }

    private void PushSound(AudioSource audio)
    {
        audio.Stop();
        if(!soundDic.TryGetValue(audio.name,out var stack))
        {
            stack = new Stack<AudioSource>();
            soundDic[audio.name] = stack;
        }
        stack.Push(audio);
    }
}
