using Unity.FilmInternalUtilities;

namespace Unity.MaterialSwitch {

internal class MaterialSwitchTrackMixerEvent : AnalyticsEvent {

    internal MaterialSwitchTrackMixerEvent(int clips, int materials) : base(new EventData { numClips = clips, numMaterials = materials}) { }
    
    private class EventData : AnalyticsEventData {
        public int numClips;
        public int numMaterials;
    }

//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "materialswitch_materialswitchtrack_mixer";
    internal override int    maxItems  => 1;

    
}

} //end namespace