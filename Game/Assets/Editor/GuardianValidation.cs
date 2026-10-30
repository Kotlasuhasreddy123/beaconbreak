using System;
using System.Reflection;
using System.IO;
using CoopFighter.Game;
using UnityEngine;

namespace CoopFighter.UnityGame.Editor
{
    public static class GuardianValidation
    {
        static void Require(bool ok,string message) { if(!ok)throw new Exception(message); }
        static MatchState Arena(int level) {
            var s=Simulation.Start(level,null,191);s.Fighters[0].X=9000;s.Fighters[1].X=12000;s.Fighters[2].X=10000;
            s.Fighters[0].Facing=1;s.Fighters[2].Facing=-1;return s;
        }
        static int Hit(MatchState s,AttackType type,int from,int to) {
            var f=s.Fighters[from];f.Attack=type;f.AttackTicks=7;f.HitTargets=0;
            int before=s.Fighters[to].Health;
            typeof(Simulation).GetMethod("ResolveAttack",BindingFlags.Static|BindingFlags.NonPublic).Invoke(null,new object[]{s,from,to});
            return before-s.Fighters[to].Health;
        }
        public static void Run() {
            Require(StoryCampaign.Normalize(1)==10 && StoryCampaign.Normalize(11)==20 && StoryCampaign.Next(90)==100,"Save migration");
            for(int level=10;level<100;level+=10)Require(CounterProfile.FromBattle(level,new BattleTelemetry()).TargetLevel==level+10,"Campaign transfer");
            var s=Arena(10);s.Fighters[2].Guarding=true;int front=Hit(s,AttackType.High,0,2);
            s=Arena(10);s.Fighters[2].Guarding=true;s.Fighters[0].X=11000;s.Fighters[0].Facing=-1;
            Require(Hit(s,AttackType.High,0,2)>front,"Veyra rear opening");
            s=Arena(20);int varied=Hit(s,AttackType.Low,0,2);s=Arena(20);s.Telemetry.Attempts[0]=10;s.Telemetry.Low[0]=9;
            Require(Hit(s,AttackType.Low,0,2)<varied,"Soren copied defense");
            s=Arena(20);varied=Hit(s,AttackType.High,0,2);s=Arena(20);s.Telemetry.Attempts[0]=10;
            Require(Hit(s,AttackType.High,0,2)<varied,"Soren also counters repeated high strikes");
            s=Arena(30);int x=s.Fighters[0].X;Hit(s,AttackType.BossStrike,2,0);Require(s.Fighters[0].X>x,"Brann pull toward ally");
            s=Arena(40);s.Telemetry.BossDamage[1]=100;Simulation.Step(s,Buttons.None,Buttons.None);Require(s.Fighters[2].Facing==1,"Nima target trade");
            s=Arena(50);s.Fighters[2].ComboDepth=2;Require(StoryCampaign.Attack(s,AttackType.None)==AttackType.BossSweep,"Oris sweep");s.Fighters[2].ComboDepth=3;Require(StoryCampaign.Attack(s,AttackType.None)==AttackType.BossAntiAir,"Oris sky cut");
            s=Arena(60);int flank=Hit(s,AttackType.High,0,2);s=Arena(60);s.Fighters[1].X=8500;Require(Hit(s,AttackType.High,0,2)<flank,"Sable surround defense");
            s=Arena(70);s.Fighters[2].Attack=AttackType.BossCharge;s.Fighters[2].AttackTicks=15;s.Fighters[2].Facing=1;Simulation.Step(s,Buttons.None,Buttons.None);Require(s.Fighters[2].X==10250,"Ivo committed charge");
            s=Arena(80);s.Fighters[2].ComboDepth=3;Require(StoryCampaign.Attack(s,AttackType.None)==AttackType.BossAntiAir,"Rhea third strike");
            s=Arena(90);s.Fighters[0].X=14000;Require(Hit(s,AttackType.BossWave,2,0)>0,"Cael rear shockwave");s=Arena(90);s.Fighters[0].Y=800;Require(Hit(s,AttackType.BossWave,2,0)==0,"Cael jump counter");
            s=Arena(100);Require(StoryCampaign.Attack(s,AttackType.None)==AttackType.BossStrike,"Aster phase one");s.Fighters[2].Health=s.Fighters[2].MaxHealth/2;Require(StoryCampaign.Attack(s,AttackType.None)==AttackType.BossCharge,"Aster phase two");s.Fighters[2].Health=1;Require(StoryCampaign.Attack(s,AttackType.None)==AttackType.BossWave,"Aster final phase");
            Directory.CreateDirectory("TestResults");File.WriteAllText("TestResults/guardian-rules.txt",SessionProtocol.Version+"\nPASS: save migration, nine transfers, ten guardian mechanics and counterplay assertions.\n");
            Debug.Log("GUARDIAN_RULES_PASS");
        }
        public static void Build() { Run();MissionCinemaValidation.Run();BuildGame.BuildVerifiedWeb(); }
    }
}
