using Unity.FilmInternalUtilities;

namespace Unity.MaterialSwitch {

internal class NameMapEnableEvent : AnalyticsEvent {

    internal NameMapEnableEvent(double duration, bool frameMarkers, int images, int width, int height) : base(
        new EventData {
            clipDuration = duration, 
            showFrameMarkers = frameMarkers, 
            numImages = images, 
            imageResolutionWidth = width, 
            imageResolutionHeight = height
        }) 
    { }
    
    private class EventData : AnalyticsEventData {
        public double clipDuration;
        public bool   showFrameMarkers;
        public int    numImages;
        public int    imageResolutionWidth;
        public int    imageResolutionHeight;
    }
    
//--------------------------------------------------------------------------------------------------------------------------------------------------------------
    
    internal override string eventName => "materialswitch_namemap_enable";
    internal override int    maxItems  => 1;
    
}

} //end namespace