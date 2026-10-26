using System;
using System.Collections.Generic;
using UnityEngine;

public class GameState
{
    private int[] playerScore;
    public int[] playerOrder;
    public int curMap;

    public int numHumanPlayers = 1;
    public bool enableBots = true;
    public int p1Character = 0;
    public int p2Character = 1;
    public bool matchStarted = false;

    public int TotalPlayers => (numHumanPlayers == 2 && !enableBots) ? 2 : playerScore.Length;

    private List<int> finishedPlayers;

    private const int maxScore = 3;

    public event Action<int[]> OnScoreChanged;

    public GameState(int numPlayers)
    {
        playerScore = new int[numPlayers];
        finishedPlayers = new List<int>();
        curMap = -1;

        playerOrder = new int[numPlayers];
        for (int i = 0; i < numPlayers; i++)
            playerOrder[i] = i;
    }

    public void ResetPlayerOrder()
    {
        playerOrder = new int[TotalPlayers];
        for (int i = 0; i < playerOrder.Length; i++) playerOrder[i] = i;
    }

    public void ResetMatch()
    {
        for (int i = 0; i < playerScore.Length; i++)
            playerScore[i] = 0;
        int count = TotalPlayers;
        playerOrder = new int[count];
        for (int i = 0; i < count; i++)
            playerOrder[i] = i;
        finishedPlayers.Clear();
        curMap = -1;
        matchStarted = false;
        OnScoreChanged?.Invoke(GetActiveScores());
    }

    public void PlayerFinished(int playerID)
    {
        if (playerID >= 0 && playerID < TotalPlayers && !finishedPlayers.Contains(playerID))
            finishedPlayers.Add(playerID);
    }

    public void RoundOver(int[] playerOrder)
    {
        foreach(int player in finishedPlayers)
        {
            if (player >= 0 && player < playerScore.Length)
            {
                if(finishedPlayers.Count == 1 || playerScore[player] < maxScore - 1)
                    playerScore[player]++;
            }
        }
        OnScoreChanged?.Invoke(GetActiveScores());

        finishedPlayers.Clear();
        this.playerOrder = playerOrder;
    }

    public int GetWinner()
    {
        int count = TotalPlayers;
        for(int i = 0; i < count; i++)
            if (playerScore[i] >= maxScore)
                return i;
        
        return -1;
    }

    public int[] GetScores()
    {
        return playerScore;
    }

    public int[] GetActiveScores()
    {
        int count = TotalPlayers;
        int[] active = new int[count];
        for (int i = 0; i < count; i++)
            active[i] = playerScore[i];
        return active;
    }
}
