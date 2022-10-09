/// <summary>
/// ストレージ空き容量の取得
/// </summary>
extern "C" long _GetAvailableStorage()
{
	NSArray *paths = NSSearchPathForDirectoriesInDomains( NSLibraryDirectory, NSUserDomainMask, YES ) ;
	NSDictionary *dictionary = [ [ NSFileManager defaultManager ] attributesOfFileSystemForPath:[ paths lastObject ] error:nil ] ;
	if( dictionary )
	{
		return [ [ dictionary objectForKey: NSFileSystemFreeSize ] longValue ] ;
	}
	return 0 ;
}
