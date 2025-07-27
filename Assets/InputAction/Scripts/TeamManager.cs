using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance;
    private int currentTeamIndex=0;
    public int TeamCount = 4;
    public Color[] playerColors;

    private void Awake()
    {
        Instance = this;
        for(int i = 0; i < TeamCount; i++) 
        {
            CreateTeam(i, playerColors[i]);
        }
    }

    public List<Team> teams = new List<Team>();

    //private TeamManager() { }

    public void CreateTeam(int id,Color color)
    {
        teams.Add(new Team(id,color));
    }

    public Team GetTeamByName(int id)
    {
        return teams.FirstOrDefault(t => t.TeamID == id);
    }
    public Team CurrentTeam()
    {
       return teams[currentTeamIndex];
    }
    public Team NextTeam()
    {
        currentTeamIndex++;
        if (currentTeamIndex >= teams.Count)
            currentTeamIndex = 0;
        return teams[currentTeamIndex];
    }
    public List<Team> GetAllTeams() => teams;
}
