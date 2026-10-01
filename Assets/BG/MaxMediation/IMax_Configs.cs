namespace BG_Library.NET.Mediation.Max
{
    public interface IMax_Configs
    {
        Max_FAInfo GetMaxFAInfo();
        Max_RWInfo GetMaxRWInfo();
        Max_AOInfo GetMaxAOInfo();

        Max_BNInfo[] GetMaxBNInfos();
        Max_MrecInfo GetMaxMrecId();
    }
}
