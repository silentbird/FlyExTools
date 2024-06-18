using System;


namespace pure.utils.task {
	[Flags]
	public enum InvalidateType {
		Nothing = 0,
		Style = 1,
		Size = 2,
		Data = 4,
		State = 8,
		Select = 16, // 0x00000010
		TextStyle = 32, // 0x00000020
		Scroll = 64, // 0x00000040
		ScrollPreset = 128, // 0x00000080
		Setting = 256, // 0x00000100
		Prefab = 512, // 0x00000200
		Position = 1024, // 0x00000400
		Rotate = 2048, // 0x00000800
		Scale = 4096, // 0x00001000
		Visible = 8192, // 0x00002000
		Alpha = 16384, // 0x00004000
		Color = 32768, // 0x00008000
		Mesh = 65536, // 0x00010000
		Material = 131072, // 0x00020000
		Render = 262144, // 0x00040000
		All = -1, // 0xFFFFFFFF
	}
}