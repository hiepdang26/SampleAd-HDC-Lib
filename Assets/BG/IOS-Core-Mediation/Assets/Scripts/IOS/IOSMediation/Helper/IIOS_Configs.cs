namespace BG_Library.NET.Mediation.IOS
{
	public interface IIOS_Configs
	{
		IOS_FAInfo[] GetIOSFAInfo();
		IOS_RWInfo GetIOSRWInfo();
		IOS_BNInfo GetIOSBNInfo();
		IOS_PUInfo[] GetIOSPUInfo();
	}
}
