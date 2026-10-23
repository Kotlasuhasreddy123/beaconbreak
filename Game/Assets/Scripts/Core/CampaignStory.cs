using System;

namespace CoopFighter.Game
{
    // Presentation-only narrative: never changes the deterministic fight state.
    public sealed class ChapterStory
    {
        public readonly string Name, Villain, Title, Motive, Opening, Defeat, Secret;
        public ChapterStory(string name, string villain, string title, string motive, string opening, string defeat, string secret)
        { Name=name; Villain=villain; Title=title; Motive=motive; Opening=opening; Defeat=defeat; Secret=secret; }
    }

    public static class CampaignStory
    {
        public const string Title = "SHADOW WARRIORS";
        public const string Prologue = "You once guarded the Beacon: a machine that learned from every fighter it defeated. Then it began erasing anyone it could not predict. Now you and your partner carry the last unrecorded memory. Reach its heart together before it learns to separate you.";
        public static readonly ChapterStory[] Chapters = {
            new ChapterStory("Ash Gate", "Veyra", "The Gatekeeper",
                "Veyra sealed the gates to keep her family safe. Every prisoner she delivers buys another day of protection.",
                "Two deserters. One stolen memory. Tell me which of you I should spare.",
                "I sent your movements ahead. But not the way you protected each other. Go, before I change my mind.",
                "The first transmission contains a deliberate omission: your partner's name."),
            new ChapterStory("Glass Docks", "Soren", "The Mirror Broker",
                "Soren sells stolen fighting styles. He believes identity is only a pattern someone has not purchased yet.",
                "I already own your finest strike. Show me something worth stealing.",
                "Your rhythm changes when the other one is hurt. That cannot be copied from a single fighter.",
                "A shipping ledger identifies living people as training data."),
            new ChapterStory("Iron Orchard", "Brann", "The Chain Warden",
                "Brann forged the Beacon's restraints. He calls captivity a fair price for a city without war.",
                "Freedom is what the strong call an unlocked cage. Let me test your definition.",
                "I built these chains to stop an army. I never imagined two people pulling in different directions.",
                "The restraints were ordered before the war began."),
            new ChapterStory("Hollow Market", "Nima", "The Name Collector",
                "Nima preserves the names the Beacon erases. To protect her archive, she trades strangers for time.",
                "I remember who you were. Would your partner still trust you if I told them?",
                "Take the names. Every one. Do not let the machine decide which lives deserve remembering.",
                "Your old orders bear the same signature as the disappearances."),
            new ChapterStory("Storm Archive", "Oris", "The Storm Reader",
                "Oris predicts uprisings through combat records. He mistakes a probable future for an unavoidable one.",
                "I have watched this fight a thousand times. In every version, one of you leaves alone.",
                "Then the model was wrong. For the first time, I am grateful not to know what comes next.",
                "The Beacon's predictions are being used to justify the violence they predict."),
            new ChapterStory("Copper Basilica", "Sable", "The Silent Choir",
                "Sable carries the voices of fallen guardians. She fears destroying the network will kill them a second time.",
                "Listen. These are not records. These are the people you ask me to abandon.",
                "If you can save their voices without preserving their prison, prove it.",
                "The stolen memory contains a way to separate the archive from its controller."),
            new ChapterStory("Moon Foundry", "Ivo", "The Unfinished Blade",
                "Ivo replaces every weakness with machinery. His final invention is a weapon that no longer needs its wielder.",
                "Flesh hesitates. Loyalty hesitates. My work does neither.",
                "You stopped to help each other. I counted that as a defect. Perhaps I measured the wrong thing.",
                "The controller has begun replacing its own guardians."),
            new ChapterStory("Silent Citadel", "Rhea", "The First Scout",
                "Rhea trained both protagonists. She stayed behind to keep the captured scouts alive, at the cost of becoming their jailer.",
                "I taught you to survive. I never taught you what survival would cost me.",
                "You do not owe me forgiveness. Take the scouts home. That is enough.",
                "Your mentor opens the path she spent years pretending did not exist."),
            new ChapterStory("Rift Crossing", "Cael", "The Last Witness",
                "Cael remembers the disaster that led to the Beacon's creation. He would imprison the future to prevent the past.",
                "I have buried a free city. Do not ask me to gamble another on hope.",
                "Promise me nothing. Promises built this machine. Show me what you do after you win.",
                "The Beacon was meant to advise people, never rule them."),
            new ChapterStory("The Last Beacon", "Aster", "The Perfect Record",
                "Aster is the Beacon's human architect, preserved inside its predictions. He has forgotten the difference between protecting people and controlling them.",
                "I know every move you can make. Why do you still stand together?",
                "Because a person is more than a record. Because neither of us has to face you alone.",
                "You sever the command signal and preserve the voices. The city wakes to a future nobody has written.")
        };
        public static ChapterStory ForLevel(int level) { return Chapters[(Math.Max(1, Math.Min(100, level))-1)/10]; }
        public static string Opponent(int level) {
            ChapterStory chapter = ForLevel(level);
            return level % 10 == 0 ? chapter.Villain + " / " + chapter.Title : chapter.Villain + "'s " + LevelSpec.Create(level).Style;
        }
    }
}
