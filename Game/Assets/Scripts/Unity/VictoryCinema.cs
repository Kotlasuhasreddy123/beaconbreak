using UnityEngine;

namespace CoopFighter.UnityGame
{
    public sealed partial class GameBootstrap
    {
        // Presentation begins only after a confirmed result. Only the host can
        // release the transition; early skipping requires both players' votes.
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
    }
}
