using System;

namespace edit.gui {
	[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
	public abstract class DisplaySetting : Attribute {
	}
}