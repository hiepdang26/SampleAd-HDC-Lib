using BG_Library.NET.Tracking;

namespace BG_Library.NET.AdCore.MainAndroid
{
    internal sealed class FallbackCandidate<TGroup>
        where TGroup : class, IGroup
    {
        public FallbackCandidate(string label, string mediation, TGroup group)
        {
            Label = label;
            Mediation = mediation;
            Group = group;
        }

        public string Label { get; }
        public string Mediation { get; }
        public TGroup Group { get; }
        public bool WasInitialized { get; set; }
    }
}
