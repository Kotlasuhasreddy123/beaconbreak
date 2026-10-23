using System;
using System.Reflection;
using CoopFighter.Game;
using UnityEngine;

namespace CoopFighter.UnityGame.Editor
{
    public static class ProgressionValidation
    {
        public static void Run()
        {
            // Editor PlayerPrefs are separate from browser saves. Preserve even
            // these test keys so running validation never resets a developer save.
            bool hadLevel=PlayerPrefs.HasKey("cf_highest_level"),hadEnd=PlayerPrefs.HasKey("dusk_complete");
            int oldLevel=PlayerPrefs.GetInt("cf_highest_level"),oldEnd=PlayerPrefs.GetInt("dusk_complete");
            GameObject root=null;
            try {
                PlayerPrefs.SetInt("cf_highest_level",1);PlayerPrefs.DeleteKey("dusk_complete");
                root=new GameObject("Progression validation only");
                GameBootstrap game=root.AddComponent<GameBootstrap>();
                BindingFlags flags=BindingFlags.Instance|BindingFlags.NonPublic;
                MethodInfo save=typeof(GameBootstrap).GetMethod("SavePersonalVictory",flags);
                MatchState result=Simulation.Start(5,null,123);
                save.Invoke(game,new object[]{result});
                Require(PlayerPrefs.GetInt("cf_highest_level")==1,"Unconfirmed fight granted progress");
                result.Result=MatchResult.Victory;
                save.Invoke(game,new object[]{result});save.Invoke(game,new object[]{result});
                Require(PlayerPrefs.GetInt("cf_highest_level")==6,"Victory or duplicate handling failed");
                MatchState lower=Simulation.Start(1,null,123);lower.Result=MatchResult.Victory;
                save.Invoke(game,new object[]{lower});
                Require(PlayerPrefs.GetInt("cf_highest_level")==6,"Replay downgraded progress");
                typeof(GameBootstrap).GetField("browserBot",flags).SetValue(game,true);
                MatchState last=Simulation.Start(100,null,123);last.Result=MatchResult.Victory;
                save.Invoke(game,new object[]{last});
                Require(PlayerPrefs.GetInt("cf_highest_level")==6,"Automation changed player save");
                typeof(GameBootstrap).GetField("browserBot",flags).SetValue(game,false);
                save.Invoke(game,new object[]{last});
                Require(PlayerPrefs.GetInt("cf_highest_level")==100 && PlayerPrefs.GetInt("dusk_complete")==1,"Ending did not persist");
                Debug.Log("DUSKBOUND_PROGRESSION_PASS: no mid-fight reward, victory, duplicate, replay, bot isolation, ending");
            } finally {
                if(root!=null)UnityEngine.Object.DestroyImmediate(root);
                if(hadLevel)PlayerPrefs.SetInt("cf_highest_level",oldLevel);else PlayerPrefs.DeleteKey("cf_highest_level");
                if(hadEnd)PlayerPrefs.SetInt("dusk_complete",oldEnd);else PlayerPrefs.DeleteKey("dusk_complete");
                PlayerPrefs.Save();
            }
        }
        private static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
    }
}
