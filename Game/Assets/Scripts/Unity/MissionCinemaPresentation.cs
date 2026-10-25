using CoopFighter.Game;
using UnityEngine;

namespace CoopFighter.UnityGame
{
    public sealed partial class GameBootstrap
    {
        private readonly Color scoutColor=new Color(.2f,.84f,.94f);
        private static readonly Color[] chapterSky={
            new Color(.24f,.12f,.13f),new Color(.08f,.23f,.31f),new Color(.22f,.23f,.15f),
            new Color(.21f,.12f,.3f),new Color(.1f,.16f,.29f),new Color(.3f,.18f,.12f),
            new Color(.11f,.25f,.28f),new Color(.15f,.18f,.25f),new Color(.22f,.1f,.28f),new Color(.25f,.18f,.08f)
        };
        private void DrawVictoryCinema() {
            float elapsed=Mathf.Clamp(Time.unscaledTime-cinemaStarted,0,CinemaDuration);
            int shot=elapsed<4?0:elapsed<9?1:2;
            float p=shot==0?elapsed/4:shot==1?(elapsed-4)/5:(elapsed-9)/5;
            int level=match.Level,next=Mathf.Min(100,level+1);
            var recipe=MissionCinema.ForLevel(level);
            DrawChapterScenery(recipe.Chapter,elapsed,515);
            string heading,caption;
            if(level==100) {
                heading=shot==0?"MISSION 100 / THE PERFECT RECORD BREAKS":shot==1?"THE VOICES ARE FREE":"CAMPAIGN COMPLETE / A FUTURE FOR TWO";
                if(shot<2)DrawCinemaAction(shot==0?CinemaAction.ChainBreak:CinemaAction.Collapse,p,recipe);
                else { MotionActor(490,515,1.2f,1,scoutColor,0,0,0);MotionActor(720,515,1.2f,-1,gold,0,0,0); }
                caption=shot==0?"Aster's command breaks. The archive no longer owns the people it remembers.":shot==1?"You carry the voices out together while the controller falls behind you.":CampaignStory.ForLevel(100).Secret;
            } else if(shot<2) {
                DrawCinemaAction(shot==0?recipe.First:recipe.Second,p,recipe);
                heading="MISSION "+level.ToString("000")+" COMPLETE / "+(shot==0?recipe.First:recipe.Second).ToString().ToUpper();
                caption=shot==0?recipe.Objective:recipe.Consequence;
            } else {
                var incoming=MissionCinema.ForLevel(next);var stats=LevelSpec.Create(next);
                // The reveal names the actual next opponent, not its chapter boss.
                DrawChapterScenery(incoming.Chapter,elapsed,515);
                MotionActor(Mathf.Lerp(1080,960,Mathf.SmoothStep(0,1,p)),515,1.65f,-1,new Color(.98f,.35f,.25f),p*3,0,(int)stats.Style);
                Box(new Rect(45,125,720,355),new Color(.015f,.025f,.035f,.92f));
                GUI.Label(new Rect(75,145,670,60),CampaignStory.Opponent(next).ToUpper(),titleStyle);
                GUI.Label(new Rect(75,215,670,75),incoming.Title+"\n"+CampaignStory.ForLevel(next).Name,bodyStyle);
                GUI.Label(new Rect(75,310,660,40),"BOSS HEALTH  "+stats.BossHealth+"     DAMAGE  "+stats.BossDamage,bodyStyle);
                GUI.Label(new Rect(75,360,660,45),"STYLE  "+stats.Style.ToString().ToUpper()+"     SPEED  "+stats.BossSpeed,smallStyle);
                GUI.Label(new Rect(75,410,650,45),"READ THE WIND-UP. CHANGE YOUR RHYTHM TOGETHER.",smallStyle);
                heading="NEXT / MISSION "+next.ToString("000")+" OF 100";
                caption=next%10==0?CampaignStory.ForLevel(next).Opening:(nextProfile==null?incoming.Objective:nextProfile.StoryLine);
            }
            Box(new Rect(0,0,1280,95),ink);Box(new Rect(0,550,1280,170),ink);
            GUI.Label(new Rect(45,25,1200,60),heading,titleStyle);
            GUI.Label(new Rect(65,567,1150,85),caption,bodyStyle);
            Box(new Rect(65,675,875,3),new Color(.2f,.25f,.27f));
            Box(new Rect(65,675,875*elapsed/CinemaDuration,3),gold);
            GUI.Label(new Rect(65,686,880,30),"MISSION "+level.ToString("000")+" / "+recipe.Title.ToUpper()+" / SHOT "+(shot+1)+" OF 3",smallStyle);
            if(cinemaVote)GUI.Label(new Rect(970,665,280,45),"Waiting for partner's vote",smallStyle);
            else if(ActionButton(new Rect(970,665,260,42),"Vote to skip")){cinemaVote=true;Send(new NetworkEnvelope{kind="cinema_vote"});}
        }
        // Ten chapter silhouettes use different architecture, not just tinting.
        // They also appear in combat; none of these props has a hidden collider.
        private void DrawChapterScenery(int chapter,float time,float ground) {
            Box(new Rect(0,0,1280,720),chapterSky[chapter]);
            Disc(new Vector2(1010,185),chapter==9?105:65,new Color(.86f,.75f,.57f,.7f));
            for(int i=0;i<9;i++) {
                float x=i*175-90-(time*8)%175,h=100+(i*43+chapter*31)%140;
                Color far=new Color(.035f,.055f,.07f,.75f);
                if(chapter==0||chapter==7) { // Fortified gate / narrow citadel towers.
                    Box(new Rect(x,ground-h,105,h),far);
                    for(int j=0;j<4;j++)Box(new Rect(x+j*29,ground-h-18,18,25),far);
                    if(chapter==7)Line(new Vector2(x+52,ground-h-85),new Vector2(x+52,ground-h),8,far);
                } else if(chapter==1) { // Dock cranes and reflected water.
                    Line(new Vector2(x,ground),new Vector2(x,ground-h-50),10,far);
                    Line(new Vector2(x,ground-h-50),new Vector2(x+125,ground-h-20),8,far);
                    Line(new Vector2(x+110,ground-h-25),new Vector2(x+110,ground-45),2,gold);
                    for(int j=0;j<4;j++)Line(new Vector2(x,ground+10+j*9),new Vector2(x+80,ground+10+j*9),2,new Color(.2f,.6f,.7f,.3f));
                } else if(chapter==2) { // Bent metal orchard.
                    Line(new Vector2(x,ground),new Vector2(x+15,ground-h),14,far);
                    Line(new Vector2(x+10,ground-h+60),new Vector2(x-65,ground-h-15),9,far);
                    Line(new Vector2(x+10,ground-h+90),new Vector2(x+85,ground-h+15),9,far);
                    Disc(new Vector2(x+85,ground-h+15),9,gold);
                } else if(chapter==3) { // Market stalls and hanging lanterns.
                    Box(new Rect(x,ground-95,130,95),far);
                    Line(new Vector2(x-10,ground-95),new Vector2(x+65,ground-135),12,far);
                    Line(new Vector2(x+65,ground-135),new Vector2(x+140,ground-95),12,far);
                    Line(new Vector2(x,ground-170),new Vector2(x+175,ground-155),2,gold);
                    Disc(new Vector2(x+90,ground-150),10,new Color(.9f,.4f,.3f));
                } else if(chapter==4) { // Archive columns, electric storm.
                    Box(new Rect(x,ground-h-30,35,h+30),far);
                    Box(new Rect(x-10,ground-h-40,55,14),far);
                    for(int j=0;j<8;j++)Line(new Vector2(x+j*18,(time*140+j*59)%500),new Vector2(x+j*18-12,(time*140+j*59)%500+28),1,new Color(.6f,.75f,.9f,.3f));
                } else if(chapter==5) { // Basilica arches.
                    Box(new Rect(x,ground-h,24,h),far);Box(new Rect(x+125,ground-h,24,h),far);
                    for(int j=0;j<10;j++){float a=Mathf.PI+j*Mathf.PI/10,b=a+Mathf.PI/10;Line(new Vector2(x+74+62*Mathf.Cos(a),ground-h+62*Mathf.Sin(a)),new Vector2(x+74+62*Mathf.Cos(b),ground-h+62*Mathf.Sin(b)),15,far);}
                } else if(chapter==6) { // Foundry gears.
                    Disc(new Vector2(x+50,ground-90),65,far);
                    for(int j=0;j<8;j++){float a=j*Mathf.PI/4+time*.2f;Line(new Vector2(x+50,ground-90),new Vector2(x+50+80*Mathf.Cos(a),ground-90+80*Mathf.Sin(a)),13,far);}
                    Box(new Rect(x+90,ground-h,35,h),far);
                } else if(chapter==8) { // Floating broken crossing.
                    Box(new Rect(x,ground-h/3,110,22),far);
                    Line(new Vector2(x+20,ground-h/3+22),new Vector2(x+50,ground+80),25,far);
                    Disc(new Vector2(x+50,ground-h-20),4,scoutColor);
                } else { // Beacon's concentric record rings.
                    for(int j=0;j<14;j++){float a=j*Mathf.PI/7+time*.1f;Line(new Vector2(640+220*Mathf.Cos(a),300+150*Mathf.Sin(a)),new Vector2(640+240*Mathf.Cos(a),300+165*Mathf.Sin(a)),4,new Color(.8f,.6f,.2f,.4f));}
                    Box(new Rect(x,ground-h,24,h),far);
                }
            }
            Box(new Rect(0,ground,1280,205),new Color(.025f,.035f,.045f));
            Line(new Vector2(0,ground),new Vector2(1280,ground),3,new Color(.55f,.55f,.5f));
        }
        private void DrawCinemaAction(CinemaAction action,float p,MissionCinema recipe) {
            float run=p*18, x=Mathf.Lerp(180,1000,p), y=515;
            Color enemy=new Color(.97f,.3f,.2f);
            switch(action) {
                case CinemaAction.Pursuit:
                    for(int i=0;i<9;i++)Line(new Vector2(0,200+i*31),new Vector2(140+p*80,200+i*31),2,new Color(1,1,1,.08f));
                    MotionActor(x+120,y,.9f,1,enemy,run,1,recipe.Chapter%4);
                    MotionActor(x-120,y,1,1,scoutColor,run+1,1,0);MotionActor(x-280,y,1,1,gold,run+2,1,0);break;
                case CinemaAction.Vault:
                    Box(new Rect(560,400,120,115),ink);
                    MotionActor(x,y-Mathf.Sin(p*Mathf.PI)*170,1,1,scoutColor,run,2,0);
                    MotionActor(x-170,y-Mathf.Sin(Mathf.Clamp01(p-.12f)*Mathf.PI)*150,1,1,gold,run,2,0);break;
                case CinemaAction.Slide:
                    for(int i=0;i<3;i++){float bx=470+i*170;Line(new Vector2(bx,130),new Vector2(bx-80+Mathf.Sin(p*4+i)*50,390),9,gold);}
                    MotionActor(x,y,1,1,scoutColor,run,3,0);MotionActor(x-170,y,1,1,gold,run,3,0);break;
                case CinemaAction.WallRun:
                    Box(new Rect(800,165,110,350),ink);Line(new Vector2(760,170),new Vector2(1120,170),12,ink);
                    MotionActor(Mathf.Lerp(250,780,p),515-p*260,1,1,scoutColor,run,1,0);
                    MotionActor(Mathf.Lerp(100,660,p),515-p*190,1,1,gold,run+2,2,0);break;
                case CinemaAction.Duel:
                    MotionActor(480+Mathf.Sin(p*Mathf.PI)*60,y,1.2f,1,scoutColor,p*9,4,0);
                    MotionActor(720,y,1.2f,-1,enemy,p*9+2,4,recipe.Chapter%4);
                    MotionActor(280,y,1,1,gold,p*5,0,0);
                    for(int i=0;i<8;i++){float a=i*.8f;Line(new Vector2(620,360),new Vector2(620+Mathf.Cos(a)*p*70,360+Mathf.Sin(a)*p*70),2,gold);}break;
                case CinemaAction.Rescue:
                    Box(new Rect(680,470,280,80),chapterSky[recipe.Chapter]);
                    MotionActor(580,515,1,1,scoutColor,p*3,5,0);
                    MotionActor(Mathf.Lerp(800,690,p),Mathf.Lerp(620,490,p),1,-1,gold,p*3,5,0);
                    Line(new Vector2(650,410),new Vector2(Mathf.Lerp(800,700,p),Mathf.Lerp(510,420,p)),5,gold);break;
                case CinemaAction.ChainBreak:
                    MotionActor(430-p*70,y,1,1,scoutColor,p*4,4,0);MotionActor(840+p*70,y,1,-1,gold,p*4,4,0);
                    for(int i=0;i<20;i++){float cx=470+i*18+(i<10?-1:1)*p*p*110;Disc(new Vector2(cx,370+Mathf.Sin(i*.5f)*8+p*p*130),5,i%2==0?gold:scoutColor);}break;
                case CinemaAction.Ambush:
                    MotionActor(520,y,1,1,scoutColor,p*4,0,0);MotionActor(700,y,1,-1,gold,p*4,4,0);
                    MotionActor(Mathf.Lerp(1150,850,p),y-Mathf.Sin(p*Mathf.PI)*230,1,-1,enemy,p*8,2,recipe.Chapter%4);
                    Box(new Rect(1080,180,90,335),ink);break;
                case CinemaAction.Collapse:
                    for(int i=0;i<7;i++)Box(new Rect(220+i*105,515+Mathf.Max(0,p-i*.08f)*180,90,30),new Color(.2f,.23f,.25f));
                    MotionActor(x,y-Mathf.Sin(p*Mathf.PI)*150,1,1,scoutColor,run,2,0);MotionActor(x-160,y-Mathf.Sin(p*Mathf.PI)*120,1,1,gold,run+2,2,0);break;
                default:
                    MotionActor(280,y,1,1,scoutColor,p*3,0,0);MotionActor(430,y,1,1,gold,p*3,0,0);
                    Box(new Rect(850,270,65,245),ink);
                    for(int i=0;i<20;i++)Disc(new Vector2(Mathf.Lerp(400,885,p)-i*12,290+Mathf.Sin(p*5+i*.2f)*45),Mathf.Max(2,9-i*.35f),gold);
                    break;
            }
        }
        // Joint-based poses: locomotion, tucked jump, low slide, attack and reach.
        private void MotionActor(float x,float ground,float size,int facing,Color accent,float cycle,int pose,int weapon) {
            Matrix4x4 before=GUI.matrix;
            GUI.matrix=before*Matrix4x4.TRS(new Vector3(x,ground,0),Quaternion.identity,new Vector3(size*facing,size,1));
            float stride=pose==1?Mathf.Sin(cycle)*32:8;
            Vector2 hip=new Vector2(0,-85),chest=new Vector2(8,-151),head=new Vector2(12,-185);
            Vector2 kneeA=new Vector2(-20-stride,-42),footA=new Vector2(-38-stride,0),kneeB=new Vector2(25+stride,-40),footB=new Vector2(45+stride,0);
            Vector2 hand=new Vector2(55,-116),backHand=new Vector2(-24,-105);
            if(pose==1){chest.x+=18;head.x+=18;hand.x+=stride;backHand.x-=stride;}
            if(pose==2){kneeA=new Vector2(-35,-100);footA=new Vector2(-65,-65);kneeB=new Vector2(40,-115);footB=new Vector2(76,-85);hand=new Vector2(55,-170);}
            if(pose==3){hip=new Vector2(-15,-35);chest=new Vector2(-45,-70);head=new Vector2(-48,-105);kneeA=new Vector2(25,-25);footA=new Vector2(95,-5);kneeB=new Vector2(-55,-15);footB=new Vector2(-90,0);hand=new Vector2(0,-40);}
            if(pose==4){float strike=Mathf.Sin(cycle)*.5f+.5f;chest.x+=strike*20;head.x+=strike*20;hand=new Vector2(55+strike*65,-125-strike*20);}
            if(pose==5){chest.x+=20;head.x+=20;hand=new Vector2(100,-90);}
            Limb(hip,kneeA,footA,20,ink);Limb(hip,kneeB,footB,22,ink);Line(hip,chest,43,ink);Line(chest,head,18,ink);Disc(head,19,ink);
            Limb(chest,new Vector2(chest.x-27,chest.y+27),backHand,17,ink);Limb(chest,new Vector2((chest.x+hand.x)*.5f,chest.y+20),hand,18,ink);
            Line(hip+new Vector2(-19,0),hip+new Vector2(20,0),7,accent);Line(hip,hip+new Vector2(-50-Mathf.Sin(cycle)*12,20),5,accent);
            Line(head+new Vector2(-14,-2),head+new Vector2(17,-2),4,accent);
            if(pose==0||pose==4){Vector2 tip=hand+new Vector2(weapon==3?15:35,weapon==1?-115:-75);Line(hand,tip,weapon==2?12:6,gold);}
            GUI.matrix=before;
        }
    }
}
