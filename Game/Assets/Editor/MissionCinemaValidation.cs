using System;
using System.Collections.Generic;
using System.IO;
using CoopFighter.Game;
using UnityEngine;

namespace CoopFighter.UnityGame.Editor
{
    public static class MissionCinemaValidation
    {
        public static void Run() {
            var keys=new HashSet<string>();var titles=new HashSet<string>();var enemies=new HashSet<string>();
            var health=new HashSet<int>();var actions=new HashSet<CinemaAction>();
            var rows=new List<string>{"level,title,first_action,second_action,next_level,boss_health,boss_damage,boss_speed"};
            for(int level=1;level<=100;level++) {
                var m=MissionCinema.ForLevel(level);var s=LevelSpec.Create(level);
                Require(keys.Add(m.Key),"Duplicate sequence identity at "+level);
                Require(titles.Add(m.Title),"Duplicate mission title at "+level);
                Require(enemies.Add(MissionCinema.EnemyName(level)),"Duplicate opponent at "+level);
                Require(health.Add(s.BossHealth),"Duplicate boss health at "+level);
                Require(m.First!=m.Second,"Repeated action within mission "+level);
                Require(m.Chapter==(level-1)/10&&m.Level==level,"Incorrect mission numbering");
                Require(MissionCinema.ForLevel(level)==m,"Unstable content resolution");
                actions.Add(m.First);actions.Add(m.Second);
                rows.Add(level+",\""+m.Title+"\","+m.First+","+m.Second+","+(level==100?"ENDING":(level+1).ToString())+","+s.BossHealth+","+s.BossDamage+","+s.BossSpeed);
            }
            Require(actions.Count==10,"Missing action family");
            Directory.CreateDirectory("TestResults");File.WriteAllLines("TestResults/mission-cinematics.csv",rows);
            CinematicValidation.Run();ProgressionValidation.Run();
            Debug.Log("MISSION_CINEMA_PASS: 100 unique titles, opponent names, boss health values and recipes; 10 action families; ending and progression gates");
        }
        public static void BuildVerifiedWeb(){Run();BuildGame.BuildVerifiedWeb();}
        private static void Require(bool value,string message){if(!value)throw new Exception(message);}
    }
}
