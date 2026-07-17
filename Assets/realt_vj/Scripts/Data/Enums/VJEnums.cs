namespace RealtVJ.Data
{
    // Hand Trigger
    public enum WhichHand { Left, Right, LeftOrRight, LeftAndRight }
    public enum HandPosition { Anywhere, LeftScreen, RightScreen, MiddleScreen, NotOnScreen }
    public enum HandGesture { Any, ThumbsUp, ThumbsDown, One, Two, Three, Four, Five, Pinching, FingerGun, Fist, Peace, RockAndRoll, HandsTogether, HandsApart }

    // Music Trigger
    public enum MusicTriggerType { EveryNBeats, EnergyAbove, FrequencyBandEnergy, BPMChange, SilenceDetection, BeatDrop }

    // Time Trigger
    public enum TimeTriggerType { AfterDuration, EveryInterval, RepeatN, DelayedStart }

    // Trigger
    public enum TriggerType { Hand, Music, Time }

    // Result
    public enum ResultExecutionMode { ATST, InOrder }

    // System
    public enum CooldownMode { Full, ForDuration, EveryInterval }
    public enum EdgeType { TriggerToResult, TriggerToTrigger, ResultToResult, ResultToDefault }
}
