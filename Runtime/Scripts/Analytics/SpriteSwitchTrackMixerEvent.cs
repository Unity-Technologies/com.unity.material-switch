using Unity.FilmInternalUtilities;

namespace Unity.MaterialSwitch {

internal class SpriteSwitchTrackMixerEvent : AnalyticsEvent {

    internal SpriteSwitchTrackMixerEvent(int clips) : base(new EventData { numClips = clips, }) { }
    
    private class EventData : AnalyticsEventData {
        public int numClips;
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "materialswitch_spriteswitchtrack_mixer";
    internal override int    maxItems  => 1;

    
}

} //end namespace