using System;

namespace CoopFighter.Game
{
    // Immutable presentation recipes: a unique mission identity and ordered
    // sequence, composed from authored actions. Never used to resolve a hit.
    public enum CinemaAction { Pursuit, Vault, Slide, WallRun, Duel, Rescue, ChainBreak, Ambush, Collapse, Signal }
    public sealed class MissionCinema
    {
        public readonly int Level, Chapter, Beat;
        public readonly string Title, Objective, Consequence;
        public readonly CinemaAction First, Second;
        private MissionCinema(int level) {
            Level=level; Chapter=(level-1)/10; Beat=(level-1)%10;
            Title=Places[Chapter]+" / "+Beats[Beat];
            Objective=Objectives[Beat];
            Consequence=Consequences[Chapter]+" "+Outcomes[Beat];
            First=(CinemaAction)Beat;
            Second=(CinemaAction)((Beat+Chapter+1)%10);
            // Avoid repeating an action in the last chapter while retaining a
            // distinct ordered sequence within each environment.
            if(Second==First)Second=(CinemaAction)((Beat+5)%10);
        }
        private static readonly MissionCinema[] Cache=new MissionCinema[101];
        public static MissionCinema ForLevel(int level) {
            if(level<1||level>100)throw new ArgumentOutOfRangeException("level");
            return Cache[level]??(Cache[level]=new MissionCinema(level));
        }
        public string Key { get { return "mission-"+Level.ToString("000")+"/"+Chapter+"/"+First+"/"+Second; } }
        public static string EnemyName(int level) {
            var m=ForLevel(level);
            return m.Beat==9?CampaignStory.ForLevel(level).Villain:Places[m.Chapter]+" "+Ranks[m.Beat];
        }
        private static readonly string[] Places={"Ash","Glass","Iron","Hollow","Storm","Copper","Moon","Silent","Rift","Beacon"};
        private static readonly string[] Ranks={"Runner","Vaultkeeper","Blade","Climber","Duelist","Captor","Chainkeeper","Watcher","Executioner"};
        private static readonly string[] Beats={"The stolen route","Above the barricade","Beneath the blades","The forbidden ascent","A broken oath","No one left behind","The lock gives way","Eyes in the dark","The falling bridge","The final transmission"};
        private static readonly string[] Objectives={
            "Intercept the courier before the record reaches the next gate.",
            "Cross the barricade together. The high road is no longer safe.",
            "Get beneath the searchlights and recover the missing memory.",
            "Climb past the watchtower. One fighter cannot make this crossing alone.",
            "Disarm the oathkeeper; the stolen seal will open the next route.",
            "Bring your partner home. A victory is worthless if it costs the other fighter.",
            "Break the restraint before the Beacon replaces its fallen guardian.",
            "Expose the watcher hiding behind the next combat record.",
            "Escape the collapsing route before the archive seals it forever.",
            "Face the chapter guardian and decide what survives the transmission."
        };
        private static readonly string[] Consequences={
            "Veyra leaves a gate unguarded.","Soren's mirrored ledger reveals a smuggling route.",
            "Brann's restraints lose their command signal.","Nima restores another erased name.",
            "Oris sees a future his model did not predict.","Sable hears a voice outside the choir.",
            "Ivo's machines hesitate for the first time.","Rhea sends the captured scouts a warning.",
            "Cael opens a path across the broken city.","Aster's perfect record begins to fracture."
        };
        private static readonly string[] Outcomes={
            "You take the courier's route, but the record outruns you.",
            "The barricade falls behind you. Ahead, a new guardian is waiting.",
            "The blades miss. Your escape becomes the next enemy's lesson.",
            "Two shadows reach the roof. Below, the alarm changes its rhythm.",
            "The oathkeeper yields the seal, but sends your timing ahead.",
            "A hand reaches back. The machine records a choice it cannot explain.",
            "The chain breaks. Its last pulse carries your combat habits onward.",
            "The watcher is exposed. Now you know who has been studying you.",
            "The bridge disappears. There is no route back, only forward together.",
            "The chapter closes. The next guardian inherits your combat record."
        };
    }
}
