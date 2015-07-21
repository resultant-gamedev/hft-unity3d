/*
 * Copyright 2014, Gregg Tavares.
 * All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without
 * modification, are permitted provided that the following conditions are
 * met:
 *
 *     * Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 *     * Redistributions in binary form must reproduce the above
 * copyright notice, this list of conditions and the following disclaimer
 * in the documentation and/or other materials provided with the
 * distribution.
 *     * Neither the name of Gregg Tavares. nor the names of its
 * contributors may be used to endorse or promote products derived from
 * this software without specific prior written permission.
 *
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
 * "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
 * LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
 * A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT
 * OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE,
 * DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY
 * THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
 * (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
 * OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using HappyFunTimes;

namespace HappyFunTimes {

public class SpawnInfo {
    public NetPlayer netPlayer;
    public string name;
    public Dictionary<string, object> data;
};

[AddComponentMenu("HappyFunTimes/PlayerSpawner")]
public class PlayerSpawner : MonoBehaviour
{
    public GameObject prefabToSpawnForPlayer;
    public string gameId = "";
    public bool showMessages = false;
    public bool allowMultipleGames;

    [Header("0 = unlimited")]
    public int maxPlayers = 0;

    public GameServer server
    {
        get
        {
            return m_server;
        }
    }

    /// <summary>
    /// Call this to rotate an active player out and start the next waiting player.
    /// </summary>
    /// <param name="netPlayer">The NetPlayer of the player to return</param>
    public void ReturnPlayer(NetPlayer netPlayer) {
        if (m_playerManager != null) {
            m_playerManager.ReturnPlayer(netPlayer);
        }
    }

    /// <summary>
    /// Returns all the current players to the waiting list
    /// and gets new ones if any are waiting
    /// </summary>
    public void FlushCurrentPlayers() {
        if (m_playerManager != null) {
            m_playerManager.FlushCurrentPlayers();
        }
    }

    void StartConnection() {
        GameServer.Options options = new GameServer.Options();
        options.gameId = gameId;
        options.allowMultipleGames = allowMultipleGames;
        options.showMessages = showMessages;

        m_server = new GameServer(options, gameObject);

        m_server.OnConnect += Connected;
        m_server.OnDisconnect += Disconnected;

        m_server.Init();

        if (maxPlayers > 0) {
            int timeoutForDisconnectedPlayerToReconnect = 0;
            m_playerManager = new PlayerManager(m_server, gameObject, maxPlayers, timeoutForDisconnectedPlayerToReconnect, GetPrefab);
        } else {
            m_server.OnPlayerConnect += StartNewPlayer;
        }
    }

    void StartPlayer(NetPlayer netPlayer, Dictionary<string, object> data)
    {
        GameObject gameObject = (GameObject)Instantiate(prefabToSpawnForPlayer);
        SpawnInfo spawnInfo = new SpawnInfo();
        spawnInfo.netPlayer = netPlayer;
        spawnInfo.name = netPlayer.Name;
        spawnInfo.data = data;
        gameObject.SendMessage("InitializeNetPlayer", spawnInfo);
    }

    GameObject GetPrefab(int ndx) {
        return (GameObject)Instantiate(prefabToSpawnForPlayer);
    }

    void StartNewPlayer(object sender, PlayerConnectMessageArgs e)
    {
        StartPlayer(e.netPlayer, e.data);
    }

    public void StartLocalPlayer(NetPlayer netPlayer, string name = "", Dictionary<string, object> data = null)
    {
        if (m_playerManager != null) {
            m_playerManager.StartLocalPlayer(netPlayer, name, data);
        } else {
            StartPlayer(netPlayer, data);
        }
    }

    void Start ()
    {
        StartConnection();
    }

    void Update() {
        if (m_playerManager != null) {
            m_playerManager.Update();
        }
    }

    void Connected(object sender, EventArgs e)
    {
    }

    void Disconnected(object sender, EventArgs e)
    {
        Debug.Log("Quitting");
        Application.Quit();
    }

    void Cleanup()
    {
        if (m_server != null) {
            m_server.Close();
        }
    }

    void OnDestroy()
    {
        Cleanup();
    }

    void OnApplicationExit()
    {
        Cleanup();
    }

    public GameServer GameServer {
        get {
            return m_server;
        }
        private set {
        }
    }

    private GameServer m_server;
    private PlayerManager m_playerManager;
};

}   // namespace HappyFunTimes
