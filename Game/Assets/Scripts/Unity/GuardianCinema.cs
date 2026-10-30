using UnityEngine;
using CoopFighter.Game;

namespace CoopFighter.UnityGame
{
    public sealed partial class GameBootstrap
    {
        // Authored tableaux share an actor rig, but each scene stages a different
        // action and prop. No random shot selection or mission-number substitution.
        private void DrawGuardianScene(int chapter, int shot, float p) {
            float t=Mathf.SmoothStep(0,1,p), floor=515;
            Color red=new Color(.95f,.32f,.27f), paper=new Color(.93f,.83f,.61f);
            float left=380, right=660;
            switch(chapter) {
                case 0: // Keeper interposes; Veyra lowers her blade and opens the gate.
                    MotionActor(470,floor,1,1,scoutColor,0,3,0);
                    MotionActor(Mathf.Lerp(280,570,t),floor,1,-1,gold,p*6,shot==0?1:0,0);
                    MotionActor(780,floor,1.2f,-1,red,0,shot==0&&p<.5f?4:0,0);
                    Box(new Rect(950,220,90,295-(shot==1?t*260:0)),ink);
                    break;
                case 1: // Mirror splits, cage opens, child runs to two fighters.
                    for(int i=0;i<6;i++) Line(new Vector2(780+i*12+(shot==0?t*90:90),230+t*70),new Vector2(750+i*12,420+t*60),5,paper);
                    for(int i=0;i<5;i++)Box(new Rect(790+i*35,350-(shot==1?t*190:0),5,160),gold);
                    MotionActor(shot==1?Mathf.Lerp(850,560,t):850,floor,.6f,-1,paper,p*14,1,0);
                    MotionActor(left,floor,1,1,scoutColor,0,5,0);MotionActor(right-100,floor,1,1,gold,0,5,0);
                    break;
                case 2: // Opposed pulls break the chain and expose a sealed order.
                    left=450-t*160;right=780+t*130;
                    MotionActor(left,floor,1,-1,scoutColor,0,5,0);MotionActor(right,floor,1,1,gold,0,5,0);
                    Line(new Vector2(left+30,430),new Vector2(610-t*80,430+t*80),7,gold);
                    Line(new Vector2(640+t*80,430+t*80),new Vector2(right-30,430),7,gold);
                    if(shot==1)Box(new Rect(570,300,130,150),paper);
                    break;
                case 3: // The two incriminating seals; empty hand instead of attack.
                    Box(new Rect(430,360,420,105),paper);
                    Disc(new Vector2(520,410),23,scoutColor);Disc(new Vector2(750,410),23,gold);
                    MotionActor(300+t*65,floor,1,1,scoutColor,0,5,0);
                    MotionActor(950-t*65,floor,1,-1,gold,0,shot==0&&p<.5f?4:5,0);
                    break;
                case 4: // False future splits; the Keeper returns through lightning.
                    MotionActor(430,floor,1,1,scoutColor,0,3,0);
                    MotionActor(Mathf.Lerp(950,540,t),floor,1,-1,gold,p*13,1,0);
                    for(int i=0;i<5;i++)Line(new Vector2(650+i*65,130),new Vector2(620+i*65,450-t*220),3,new Color(.6f,.8f,1,1-t));
                    Disc(new Vector2(850,270),80*(1-t),new Color(.5f,.7f,1,.3f));
                    break;
                case 5: // Archive voices flow into the memory carried by the Keeper.
                    MotionActor(850,floor,1.2f,-1,red,0,5,0);
                    MotionActor(360,floor,1,1,scoutColor,0,0,0);MotionActor(560,floor,1,1,gold,0,5,0);
                    for(int i=0;i<12;i++){float u=(p+i/12f)%1;Disc(Vector2.Lerp(new Vector2(880,210+i*14),new Vector2(590,390),u),5,paper);}
                    Disc(new Vector2(590,390),16+t*12,scoutColor);
                    break;
                case 6: // Automated blade intercepted from two sides; core removed.
                    MotionActor(850,floor,1.1f,-1,red,0,3,0);
                    MotionActor(420+t*130,floor,1,1,scoutColor,p*8,4,0);
                    MotionActor(920-t*180,floor,1,-1,gold,p*8,4,0);
                    Line(new Vector2(650,210+t*140),new Vector2(650,390+t*40),12,paper);
                    if(shot==1){Disc(new Vector2(650,300-t*100),23,scoutColor);Line(new Vector2(650,330),new Vector2(750,470),3,red);}
                    break;
                case 7: // Rhea opens the hidden door; scouts depart without salute.
                    Box(new Rect(750,210,150,305),ink);Box(new Rect(750+t*145,210,12,305),gold);
                    MotionActor(950,floor,1.2f,-1,red,0,5,0);
                    for(int i=0;i<4;i++)MotionActor(Mathf.Lerp(820-i*50,220-i*50,t),floor,.8f,-1,paper,p*12+i,1,0);
                    MotionActor(450,floor,1,1,scoutColor,0,0,0);
                    break;
                case 8: // Three people brace a crossing while refugees pass.
                    Line(new Vector2(240,390),new Vector2(1060,390-t*35),20,paper);
                    MotionActor(380,floor,1,1,scoutColor,0,5,0);MotionActor(630,floor,1,-1,gold,0,5,0);MotionActor(870,floor,1.1f,-1,red,0,5,0);
                    for(int i=0;i<3;i++)MotionActor(Mathf.Lerp(290,1050,t)-i*100,380,.65f,1,paper,p*14+i,1,0);
                    break;
                case 9: // One breaks command, the other carries the living archive.
                    for(int i=0;i<12;i++){float a=i*Mathf.PI/6;Vector2 v=new Vector2(Mathf.Cos(a),Mathf.Sin(a));Line(new Vector2(640,300)+v*(100+t*150),new Vector2(640,300)+v*(125+t*180),6,new Color(1,.7f,.3f,1-t));}
                    MotionActor(480,floor,1,1,scoutColor,p*5,4,0);
                    MotionActor(740+t*220,floor,1,1,gold,p*8,1,0);
                    Disc(new Vector2(770+t*220,410),20,scoutColor);
                    break;
            }
        }
    }
}
