using Steamworks;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteamCallBack :BaseManager<SteamCallBack>
{
    public Callback<GameConnectedFriendChatMsg_t> OnGameConnectedFriendChatMsg;
    public Callback<LobbyKicked_t> OnLobbyKicked;
    public Callback<LobbyCreated_t> OnLobbyCreated;
    public Callback<LobbyEnter_t> OnLobbyEnter;
    public Callback<LobbyDataUpdate_t> OnLobbyDataUpdate;
    public Callback<LobbyChatMsg_t> OnLobbyChatMsg;
    public Callback<LobbyInvite_t> OnLobbyInvited;
    public Callback<GameLobbyJoinRequested_t> OnGameLobbyJoinRequested;
    public Callback<GameRichPresenceJoinRequested_t> OnGameRichPresenceJoinRequested;
    public Callback<LobbyMatchList_t> lobbyMatchList;
}
