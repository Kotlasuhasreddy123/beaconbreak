using CoopFighter.Game;
using UnityEngine;

namespace CoopFighter.UnityGame
{
    public sealed partial class GameBootstrap
    {
        // Cinematics are presentation after a confirmed result, never simulation.
        // The host alone releases the transition; a skip requires both votes.
        private const float CinemaDuration=14f;
        private float cinemaStarted;
        private bool cinemaComplete=true, cinemaVote, remoteCinemaVote;
        private int cinemaEncounter=-1;
        private bool CinemaActive { get { return !cinemaComplete && (screen==ScreenMode.Report || screen==ScreenMode.Credits); } }
        private void BeginCinema() {
            if(cinemaEncounter==encounter)return;
            cinemaEncounter=encounter;cinemaStarted=Time.unscaledTime;
            cinemaComplete=false;cinemaVote=remoteCinemaVote=false;
            localReady=peerReady=false;
        }
        private void UpdateCinema() {
            if(!CinemaActive || playMode==PlayMode.Client)return;
            if(Time.unscaledTime-cinemaStarted>=CinemaDuration || cinemaVote&&remoteCinemaVote){
                cinemaComplete=true;
                Send(new NetworkEnvelope{kind="cinema_end"});
            }
        }
        private void DrawVictoryCinema() {
            float elapsed=Mathf.Min(CinemaDuration,Time.unscaledTime-cinemaStarted);
            int scene=elapsed<4?0:elapsed<9?1:2;
            float phase=scene==0?elapsed/4f:scene==1?(elapsed-4)/5f:(elapsed-9)/5f;
            int level=match.Level,next=Mathf.Min(100,level+1);
            bool ending=level==100;
            ChapterStory current=CampaignStory.ForLevel(level),incoming=CampaignStory.ForLevel(next);
            Color cyan=new Color(0.23f,0.83f,0.9f),red=new Color(0.95f,0.34f,0.2f);
            // Per-encounter palettes, skyline spacing and weapon silhouettes vary
            // the authored three-shot sequence without storing 100 video files.
            Color atmosphere=Color.Lerp(new Color(0.07f,0.16f,0.2f),new Color(0.24f,0.12f,0.09f),(level%10)/9f);
            Box(new Rect(0,0,1280,720),atmosphere);
            Disc(new Vector2(950-elapsed*7,220),115+phase*5,new Color(0.73f,0.62f,0.44f));
            for(int layer=0;layer<3;layer++)for(int i=0;i<12;i++){
                float x=i*145-70-(elapsed*(layer+1)*7)%145;
                float h=90+(i*37+level*13+layer*29)%150;
                Box(new Rect(x,490-h+layer*24,110,h+100),new Color(0.025f+layer*.008f,0.045f+layer*.008f,0.055f+layer*.008f));
                Line(new Vector2(x-12,490-h+layer*24),new Vector2(x+55,465-h+layer*24),9,ink);
                Line(new Vector2(x+55,465-h+layer*24),new Vector2(x+122,490-h+layer*24),9,ink);
            }
            Box(new Rect(0,515,1280,205),ink);
            for(int i=0;i<22;i++){
                float x=(i*71+elapsed*(10+level%7))%1280;
                float y=500-((i*37+elapsed*23)%350);
                Disc(new Vector2(x,y),i%3+1,new Color(gold.r,gold.g,gold.b,.35f));
            }
            string heading,caption;
            if(scene==0){
                heading=ending?"THE LAST GUARDIAN FALLS":"VICTORY / MISSION "+level.ToString("00");
                CinemaActor(330,515,1.2f,1,cyan,0,0);
                CinemaActor(475,515,1.15f,1,gold,0,0);
                CinemaActor(810,515,1.4f,-1,red,Mathf.SmoothStep(0,1,phase),level%4);
                caption=level%10==0?current.Villain+": “"+current.Defeat+"”":"The guardian falls. For one breath, the city is silent. Then its last memory escapes.";
            } else if(scene==1){
                heading=ending?"BREAK THE COMMAND. KEEP THE VOICES.":"YOUR VICTORY HAS BEEN WITNESSED";
                for(int i=0;i<7;i++){
                    float x=110+i*170;Box(new Rect(x,350-i%2*45,18,170+i%2*45),ink);
                    Disc(new Vector2(x+9,340-i%2*45),12,gold);
                }
                float signal=Mathf.Lerp(-70,1330,phase);
                for(int i=0;i<35;i++)Disc(new Vector2(signal-i*9,275+Mathf.Sin(phase*7+i*.12f)*50),Mathf.Max(1,9-i*.2f),new Color(gold.r,gold.g,gold.b,1-i/36f));
                Line(new Vector2(90,385),new Vector2(1180,385),2,new Color(.8f,.65f,.4f,.3f));
                caption=ending?"You sever the signal that gave the Beacon control. The voices of its prisoners remain alive.":"The record crosses "+current.Name+". “"+(nextProfile==null?"They fight as one.":nextProfile.StoryLine)+"”";
            } else {
                heading=ending?"TOGETHER, YOU CHANGED THE ENDING":"NEXT / "+CampaignStory.Opponent(next).ToUpper();
                if(ending){
                    CinemaActor(535,515,1.35f,1,cyan,0,0);CinemaActor(725,515,1.35f,-1,gold,0,0);
                    caption=incoming.Secret;
                }else{
                    CinemaActor(Mathf.Lerp(900,810,Mathf.SmoothStep(0,1,phase)),515,1.7f,-1,red,0,(int)LevelSpec.Create(next).Style);
                    GUI.Label(new Rect(100,265,520,65),incoming.Villain.ToUpper(),heroStyle);
                    GUI.Label(new Rect(100,340,490,80),incoming.Title+"\n"+incoming.Name,bodyStyle);
                    caption="“"+incoming.Opening+"”";
                }
            }
            // Letterbox and captions are outside the panning scenery.
            Box(new Rect(0,0,1280,95),ink);Box(new Rect(0,550,1280,170),ink);
            GUI.Label(new Rect(55,30,1160,55),heading,titleStyle);
            GUI.Label(new Rect(75,572,1130,85),caption,bodyStyle);
            Box(new Rect(75,678,860,3),new Color(.2f,.25f,.27f));
            Box(new Rect(75,678,860*elapsed/CinemaDuration,3),gold);
            GUI.Label(new Rect(75,690,830,25),"SHADOW WARRIORS / "+(scene+1)+" OF 3 / BOTH PLAYERS WATCH TOGETHER",smallStyle);
            if(cinemaVote)GUI.Label(new Rect(955,665,300,50),"Skip requested / waiting",smallStyle);
            else if(ActionButton(new Rect(970,665,260,42),"Vote to skip")){
                cinemaVote=true;Send(new NetworkEnvelope{kind="cinema_vote"});
            }
        }
        private void CinemaActor(float x,float ground,float size,int facing,Color accent,float fall,int weapon){
            Matrix4x4 before=GUI.matrix;
            GUI.matrix=before*Matrix4x4.TRS(new Vector3(x,ground,0),Quaternion.Euler(0,0,fall*78),new Vector3(facing*size,size,1));
            // Original cut-paper silhouette; pose animation is local presentation.
            Vector2 hip=new Vector2(0,-85),shoulder=new Vector2(6,-155),head=new Vector2(9,-190);
            Limb(hip,new Vector2(-28,-42),new Vector2(-40,0),23,ink);
            Limb(hip,new Vector2(30,-48),new Vector2(48,0),25,ink);
            Line(hip,shoulder,48,ink);Disc(head,20,ink);Line(shoulder,head,18,ink);
            Limb(shoulder,new Vector2(-32,-120),new Vector2(-14,-90),18,ink);
            Vector2 hand=new Vector2(57,-111-fall*25);
            Limb(shoulder,new Vector2(39,-132),hand,19,ink);
            Line(new Vector2(-20,-87),new Vector2(25,-87),7,accent);
            Line(new Vector2(1,-87),new Vector2(-46,-64),6,accent);
            Line(head+new Vector2(-16,-2),head+new Vector2(18,-2),4,accent);
            if(weapon==1)Line(hand,new Vector2(89,-225),5,gold);
            else if(weapon==2){Disc(hand,16,accent);Line(new Vector2(-32,-116),new Vector2(-12,-99),8,accent);}
            else if(weapon==3){Line(new Vector2(42,-240),new Vector2(78,-5),7,gold);}
            else Line(hand,new Vector2(83,-183),8,gold);
            GUI.matrix=before;
        }
    }
}
