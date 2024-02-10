using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class SteamBaseInfo
{
    public CSteamID steamId;
    public Texture2D downloadedAvatar;
    public string NickName;
    public EPersonaState personaState;
}
public class SteamBase : MonoBehaviour
{
    public AppId_t AppId = (AppId_t)1109040;
    public static SteamBase Instance { get; set; }
    [Header("��ѯ���ѵ�����")]
    public EFriendFlags eFriendFlags = EFriendFlags.k_EFriendFlagAll;
    public SteamBaseInfo Ower//�Լ�����Ϣ
    {
        get
        {
            return AllMySteamUsersInfosList[0];
        }
    }
    [Header("����[��������]����[�����Լ�]")]
    public List<SteamBaseInfo> AllMySteamUsersInfosList = new List<SteamBaseInfo>();

    public List<SteamBaseInfo> GetOnlySteamFridendUsersList//�����������������[��������]����
    {
        get
        {
            List<SteamBaseInfo> tempList = new List<SteamBaseInfo>();
            foreach (var item in AllMySteamUsersInfosList)
            {
                tempList.Add(item);
            }
            tempList.RemoveAt(0);
            if (tempList.Count == 0)
            {
                return AllMySteamUsersInfosList;
            }
            else
            {
                return tempList;
            }
        }
    }

    private Coroutine DownLoadSteamAvtorCoroutine;
    [HideInInspector]
    public Texture2D DownLoadSteamAvtorTex_Temp;
    public SteamBaseInfo GetSteamBaseInfoBySteamId(CSteamID cSteamID)
    {
        foreach (var item in AllMySteamUsersInfosList)
        {
            if (item.steamId == cSteamID)
            {
                return item;
            }
        }
        return default(SteamBaseInfo);
    }
    void Awake()
    {
        Instance = this;

    }
    void Start()
    {
        SteamFridends.Instance.UpdateFridendListEvent += UpdateSteamFridendInfo;
        //�����Լ�
        CSteamID csid = SteamApps.GetAppOwner();
        SteamBaseInfo ower = new SteamBaseInfo
        {
            steamId = csid,
            downloadedAvatar = null,
            NickName = SteamFriends.GetFriendPersonaName(csid),
            personaState = SteamFriends.GetFriendPersonaState(csid)
        };
        AllMySteamUsersInfosList.Add(ower);
        //��������
        for (int i = 0; i < SteamFriends.GetFriendCount(eFriendFlags); i++)
        {
            CSteamID steamid = SteamFriends.GetFriendByIndex(i, eFriendFlags);
            if (SteamApps.BIsSubscribedApp(AppId))//����ӵ�б���Ʒ�ĺ���
            {
                AllMySteamUsersInfosList.Add(new SteamBaseInfo
                {
                    steamId = steamid,
                    downloadedAvatar = null,
                    NickName = SteamFriends.GetFriendPersonaName(steamid),
                    personaState = SteamFriends.GetFriendPersonaState(steamid)
                });
            }
            print("[" + SteamFriends.GetFriendPersonaName(steamid) + "]�Ƿ�ӵ�д˲�Ʒ?=" + SteamApps.BIsSubscribedApp(AppId));
        }
        //�첽����ͷ��
        DownLoadSteamAvtorCoroutine = StartCoroutine(_FetchAcatar());
        Debug.Log("Ϊʲô��������");
        StartCoroutine(SendScoialUIFridendInfo());
    }

    public void UpdateSteamFridendInfo()
    {
        for (int i = 0; i < AllMySteamUsersInfosList.Count; i++)
        {
            AllMySteamUsersInfosList[i].personaState = SteamFriends.GetFriendPersonaState(AllMySteamUsersInfosList[i].steamId);
        }
    }
    public void GetDownLoadAvator(CSteamID cSteamID, Action funccallBack)
    {
        StartCoroutine(DownLoadAvatorBySteamId(cSteamID, funccallBack));
    }
    IEnumerator DownLoadAvatorBySteamId(CSteamID cSteamID, Action funccallBack)
    {
        Coroutine DownLoadSteamAvtorCoroutine_Temp = StartCoroutine(IE_DownLoadAvatorBySteamId(cSteamID));
        yield return DownLoadSteamAvtorCoroutine_Temp;
        funccallBack();
    }
    /// <summary>
    /// ����ָ��ͷ��
    /// </summary>
    /// <returns></returns>
    IEnumerator IE_DownLoadAvatorBySteamId(CSteamID cSteamID)
    {
        uint width = 100;
        uint height = 100;
        //��ȡ�е�ͼƬ�������ȡ���Ǵ�ͼƬ�Ļ������ܻ�ȡ����
        int AvatarInt = SteamFriends.GetMediumFriendAvatar(cSteamID);
        // Debug.Log("AvatarInt" + AvatarInt);
        while (AvatarInt == -1)
        {
            yield return null;
        }
        if (AvatarInt > 0)
        {
            SteamUtils.GetImageSize(AvatarInt, out width, out height);

            if (width > 0 && height > 0)
            {
                byte[] avatarStream = new byte[4 * (int)width * (int)height];
                SteamUtils.GetImageRGBA(AvatarInt, avatarStream, 4 * (int)width * (int)height);
                DownLoadSteamAvtorTex_Temp = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                DownLoadSteamAvtorTex_Temp.LoadRawTextureData(avatarStream);
                DownLoadSteamAvtorTex_Temp.Apply();

                DownLoadSteamAvtorTex_Temp = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                DownLoadSteamAvtorTex_Temp.LoadRawTextureData(avatarStream);
                DownLoadSteamAvtorTex_Temp.Apply();
            }
        }
    }

    /// <summary>
    /// ����ͷ�����
    /// </summary>
    /// <returns></returns>
    IEnumerator SendScoialUIFridendInfo()
    {
        yield return DownLoadSteamAvtorCoroutine;
        print("�������!");
        SteamFridends.Instance.UpdateFridendListEvent.Invoke();
    }
    /// <summary>
    /// ����ͷ��
    /// </summary>
    /// <returns></returns>
    IEnumerator _FetchAcatar()
    {
        uint width = 100;
        uint height = 100;
        for (int i = 0; i < AllMySteamUsersInfosList.Count; i++)
        {
            //��ȡ�е�ͼƬ�������ȡ���Ǵ�ͼƬ�Ļ������ܻ�ȡ����
            int AvatarInt = SteamFriends.GetMediumFriendAvatar(AllMySteamUsersInfosList[i].steamId);
            // Debug.Log("AvatarInt" + AvatarInt);
            while (AvatarInt == -1)
            {
                yield return null;
            }
            if (AvatarInt > 0)
            {
                SteamUtils.GetImageSize(AvatarInt, out width, out height);

                if (width > 0 && height > 0)
                {
                    byte[] avatarStream = new byte[4 * (int)width * (int)height];
                    SteamUtils.GetImageRGBA(AvatarInt, avatarStream, 4 * (int)width * (int)height);

                    AllMySteamUsersInfosList[i].downloadedAvatar = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                    AllMySteamUsersInfosList[i].downloadedAvatar.LoadRawTextureData(avatarStream);
                    AllMySteamUsersInfosList[i].downloadedAvatar.Apply();

                    AllMySteamUsersInfosList[i].downloadedAvatar = new Texture2D((int)width, (int)height, TextureFormat.RGBA32, false);
                    AllMySteamUsersInfosList[i].downloadedAvatar.LoadRawTextureData(avatarStream);
                    AllMySteamUsersInfosList[i].downloadedAvatar.Apply();
                }
            }
        }
        print("����������....");
    }
}


