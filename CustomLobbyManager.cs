using Mirror;
using Steamworks;
using TMPro;
using UnityEngine;

public class CustomLobbyManager : MonoBehaviour
{
    protected Callback<LobbyCreated_t> LobbyCreate;
    protected Callback<GameLobbyJoinRequested_t>LobbyJoin;
    protected Callback<LobbyEnter_t> LobbyEnter;

    public ulong CurrentLobbyID;
    private const string HostAddressKey = "HostAddressKey";
    private CustomNetworkManager manager;

    public GameObject HostBtn;
    public TMP_Text LobbyName;

    private void Start()
    {
        if(!SteamManager.Initialized){return;}

        manager = GetComponent<CustomNetworkManager>();
        LobbyCreate = Callback<LobbyCreated_t>.Create(LobbyCreated);
        LobbyJoin = Callback<GameLobbyJoinRequested_t>.Create(LobbyJoinRequest);
        LobbyEnter = Callback<LobbyEnter_t>.Create(LobbyEnterd);
    }

    public void Host()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly,manager.maxConnections);
    }

    private void LobbyCreated(LobbyCreated_t callback)
    {
        if(callback.m_eResult != EResult.k_EResultOK){return;}

        Debug.Log("LobbyCreate");

        manager.StartHost();

        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),HostAddressKey,SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),"name",SteamFriends.GetPersonaName() + "'sLobby");
    }

    private void LobbyJoinRequest(GameLobbyJoinRequested_t callback)
    {
        Debug.Log("Request to Join Lobby");
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    private void LobbyEnterd(LobbyEnter_t callback)
    {
        HostBtn.SetActive(false);
        CurrentLobbyID = callback.m_ulSteamIDLobby;
        LobbyName.gameObject.SetActive(true);
        LobbyName.text = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),"name");

        if(NetworkServer.active){return;}

        manager.networkAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby), HostAddressKey);
        manager.StartClient();
    }
}
