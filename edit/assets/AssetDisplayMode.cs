using System.ComponentModel;

namespace edit.assets {
	internal enum AssetDisplayMode {
		[Description("被使用")]
		Reference,

		[Description("使用")]
		Dependency,
	}
}