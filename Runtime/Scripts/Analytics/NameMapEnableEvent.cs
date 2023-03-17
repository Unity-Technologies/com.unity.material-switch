using Unity.FilmInternalUtilities;

namespace Unity.MaterialSwitch {

internal class NameMapEnableEvent : AnalyticsEvent {

    internal NameMapEnableEvent(int mappedProperties) : base(
        new EventData {
            numMappedProperties   = mappedProperties,
        }) 
    { }
    
    private class EventData : AnalyticsEventData {
        public double numMappedProperties;
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "materialswitch_namemap_enable";
    internal override int    maxItems  => 1;
    
}

} //end namespace