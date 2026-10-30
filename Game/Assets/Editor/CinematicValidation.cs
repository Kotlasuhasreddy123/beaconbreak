using System;
using System.Reflection;
using CoopFighter.Game;
using UnityEngine;
namespace CoopFighter.UnityGame.Editor
{
    public static class CinematicValidation
    {
        public static void Run() {
            var root=new GameObject("Cinematic gate validation");
            try {
                var game=root.AddComponent<GameBootstrap>();
                const BindingFlags f=BindingFlags.Instance|BindingFlags.NonPublic;
                Action<string,object> set=(name,value)=>typeof(GameBootstrap).GetField(name,f).SetValue(game,value);
                Func<string,object> get=name=>typeof(GameBootstrap).GetField(name,f).GetValue(game);
                Action<string> call=name=>typeof(GameBootstrap).GetMethod(name,f).Invoke(game,null);
                Action<string,string> mode=(name,value)=>{var field=typeof(GameBootstrap).GetField(name,f);field.SetValue(game,Enum.Parse(field.FieldType,value));};
                var match=Simulation.Start(100,null,123);match.Result=MatchResult.Victory;uint hash=match.Hash();set("match",match);
                mode("screen","Report");mode("playMode","Host");set("encounter",1);call("BeginCinema");
                set("cinemaVote",true);call("UpdateCinema");Require(!(bool)get("cinemaComplete"),"One vote skipped");
                set("remoteCinemaVote",true);call("UpdateCinema");Require((bool)get("cinemaComplete"),"Two votes did not skip");
                set("encounter",2);call("BeginCinema");mode("playMode","Client");set("cinemaStarted",Time.unscaledTime-40);call("UpdateCinema");
                Require(!(bool)get("cinemaComplete"),"Guest released itself without host");
                mode("playMode","Host");mode("screen","Credits");set("encounter",3);call("BeginCinema");
                Require((bool)typeof(GameBootstrap).GetProperty("CinemaActive",f).GetValue(game),"Final boss has no ending scene");
                set("cinemaStarted",Time.unscaledTime-40);call("UpdateCinema");
                Require((bool)get("cinemaComplete")&&get("screen").ToString()=="Credits","Ending did not return to credits");
                call("BeginCinema");Require((bool)get("cinemaComplete"),"Duplicate result replayed cinema");
                Require(match.Hash()==hash,"Cinematic changed combat state");
                Debug.Log("SHADOW_CINEMA_GATE_PASS: single vote, two votes, guest authority, ending, duplicate, state isolation");
            }finally{UnityEngine.Object.DestroyImmediate(root);}
        }
        private static void Require(bool condition,string message){if(!condition)throw new Exception(message);}
    }
}
