using CoopFighter.Game;
using UnityEngine;

namespace CoopFighter.UnityGame
{
    public sealed partial class GameBootstrap
    {
        private int chronicleChapter;
        private int skillsPage;
        private string fighterName = "Wanderer";
        private int personalLevel = 1;
        private bool campaignComplete;
        private void LoadPersonalRecord() {
            fighterName = PlayerPrefs.GetString("dusk_name", "Wanderer");
            personalLevel = Mathf.Clamp(PlayerPrefs.GetInt("cf_highest_level", 1), 1, 100);
            campaignComplete = PlayerPrefs.GetInt("dusk_complete", 0) == 1;
        }
        // Called only on host-confirmed victory, on BOTH devices. Idempotent
        // maximum progress prevents retries or duplicate packets granting points twice.
        private void SavePersonalVictory(MatchState final) {
            if (automatedTest || browserBot || final == null || final.Result != MatchResult.Victory) return;
            personalLevel = Mathf.Max(personalLevel, Mathf.Min(100, final.Level + 1));
            PlayerPrefs.SetInt("cf_highest_level", personalLevel);
            if (final.Level == 100) { campaignComplete = true; PlayerPrefs.SetInt("dusk_complete", 1); }
            PlayerPrefs.Save();
        }
        private void ReturnToHub() {
            CloseNetwork();
            LoadPersonalRecord();
            nextLevel = personalLevel;
            skillMask = PlayerPrefs.GetInt("cf_skill_mask", 0);
            screen = ScreenMode.Menu;
        }
        private void DrawChronicle() {
            Panel(new Rect(40, 35, 1200, 650));
            GUI.Label(new Rect(75, 60, 1100, 50), "THE BEACON CHRONICLE", titleStyle);
            GUI.Label(new Rect(75, 112, 1100, 38), "Ten chapters. Two outcasts. A machine that learns from every victory.", bodyStyle);
            for (int i=0;i<CampaignStory.Chapters.Length;i++) {
                if (ActionButton(new Rect(75, 175+i*43, 300, 37), (i+1).ToString("00")+"  "+CampaignStory.Chapters[i].Name, i==chronicleChapter)) chronicleChapter=i;
            }
            ChapterStory chapter=CampaignStory.Chapters[chronicleChapter];
            bool unlocked=personalLevel>=chronicleChapter*10+1;
            GUI.Label(new Rect(420,180,755,45), chapter.Villain.ToUpper()+" / "+chapter.Title, titleStyle);
            GUI.Label(new Rect(420,240,740,100), unlocked?chapter.Motive:"This guardian's history is sealed. Reach mission "+(chronicleChapter*10+1)+" to discover it.", bodyStyle);
            GUI.Label(new Rect(420,360,740,95), unlocked?"“"+chapter.Opening+"”":"Your journey has not reached this chapter yet.", bodyStyle);
            bool resolved=personalLevel>=(chronicleChapter+1)*10+1 || chronicleChapter==9&&campaignComplete;
            GUI.Label(new Rect(420,485,740,100), resolved?chapter.Secret:"Defeat the chapter guardian to uncover the truth behind the Beacon.", smallStyle);
            if(ActionButton(new Rect(930,610,260,48),"Back to camp")) screen=ScreenMode.Menu;
        }
        private void DrawGuide() {
            Panel(new Rect(65,45,1150,625));
            GUI.Label(new Rect(100,75,1040,50), "FIGHT TOGETHER. SURVIVE TOGETHER.",titleStyle);
            GUI.Label(new Rect(100,145,1040,100), CampaignStory.Prologue,bodyStyle);
            string[] headings={"01 / POSITION","02 / READ & RESPOND","03 / BREAK THE PATTERN"};
            string[] details={"A / D to move. Space to jump.\nTake opposite sides of the boss.\nYour strikes can hurt your ally.","J: low strike. K: high strike.\nL: guard while facing the attack.\nPunish the boss after its wind-up.","Vary low, high and aerial attacks.\nBosses learn your repeated moves.\nAt mission 5, press U together\nnear the boss for a Duo Strike."};
            for(int i=0;i<3;i++){
                Panel(new Rect(100+i*350,285,325,230));
                GUI.Label(new Rect(120+i*350,305,290,50),headings[i],smallStyle);
                GUI.Label(new Rect(120+i*350,365,285,140),details[i],bodyStyle);
            }
            GUI.Label(new Rect(100,540,1050,55),"Phone: turn sideways and use the touch controls. Each player uses their own device.\nProgress is saved in each player's browser. Clearing site data removes that save.",smallStyle);
            if(ActionButton(new Rect(875,600,300,46),"Back to camp",true))screen=ScreenMode.Menu;
        }
        private void DrawProfile() {
            Panel(new Rect(180,70,920,580));
            GUI.Label(new Rect(225,105,820,60),"YOUR FIGHTER RECORD",titleStyle);
            GUI.Label(new Rect(225,175,820,40),"Fighter name (saved on this device)",bodyStyle);
            fighterName=GUI.TextField(new Rect(225,225,810,50),fighterName,20);
            GUI.Label(new Rect(225,310,800,100),"Mission reached: "+personalLevel+" / 100\n"+(campaignComplete?"The Beacon is free. Campaign complete.":"Current chapter: "+CampaignStory.ForLevel(personalLevel).Name),bodyStyle);
            GUI.Label(new Rect(225,430,800,95),"This is a local player record, not an online account. Both players retain mission progress after confirmed victories. Expedition upgrades are chosen by the host and apply to both fighters.",bodyStyle);
            if(ActionButton(new Rect(225,565,380,50),"Save name & return",true)){
                fighterName=fighterName.Trim();if(fighterName.Length==0)fighterName="Wanderer";
                PlayerPrefs.SetString("dusk_name",fighterName);PlayerPrefs.Save();screen=ScreenMode.Menu;
            }
        }
        private void DrawUpgrades() {
            Panel(new Rect(45,30,1190,655));
            GUI.Label(new Rect(80,55,1100,50),"EXPEDITION UPGRADES",titleStyle);
            GUI.Label(new Rect(80,110,1100,55),SkillTree.PointsAvailable(personalLevel,skillMask)+" points available / Earn one point per five missions. Host upgrades help both fighters.",bodyStyle);
            int start=skillsPage*6;
            for(int slot=0;slot<6 && start+slot<SkillTree.OrderedNodes.Length;slot++){
                SkillNode node=SkillTree.OrderedNodes[start+slot];int x=80+(slot%2)*565,y=185+(slot/2)*130;
                bool owned=SkillTree.Has(skillMask,node.Bit),can=SkillTree.CanBuy(personalLevel,skillMask,node);
                Panel(new Rect(x,y,535,114));
                GUI.Label(new Rect(x+18,y+12,315,32),node.Name,bodyStyle);
                GUI.Label(new Rect(x+18,y+48,315,52),node.Effect+" / Mission "+node.RequiredLevel,smallStyle);
                bool old=GUI.enabled;GUI.enabled=can && (network==null || playMode!=PlayMode.Client);
                if(ActionButton(new Rect(x+355,y+25,160,60),owned?"Unlocked":can?"Unlock / 1 pt":"Locked",can)){
                    skillMask=SkillTree.Buy(personalLevel,skillMask,node);PlayerPrefs.SetInt("cf_skill_mask",skillMask);PlayerPrefs.Save();
                }GUI.enabled=old;
            }
            if(ActionButton(new Rect(80,610,170,45),"Previous"))skillsPage=Mathf.Max(0,skillsPage-1);
            GUI.Label(new Rect(270,620,210,30),"PAGE "+(skillsPage+1)+" / 4",smallStyle);
            if(ActionButton(new Rect(465,610,170,45),"Next"))skillsPage=Mathf.Min(3,skillsPage+1);
            if(ActionButton(new Rect(920,610,280,45),"Back"))screen=returnFromSkills;
        }
    }
}
