using System;

namespace Netisu.Settings
{
	public static class WorkshopSettingsList
	{
		public static readonly System.Collections.Generic.Dictionary<string, string> _RENDER_SETTINGS_CONTENT = new() {
			{ "Use smaller render distance", "Specify to either use smaller render distance or not. Enable this option if you are facing lag or lag spikes."},
			{ "Use higher anisotropic filtering level", "This is enabled by default. Turning it off will help boost performance."},
			{ "Use best quality of shadows", "Enabling this will give high quality shadows. This will significantly affect performance." },
			{ "Use ultra quality for SSAO", "This will significantly affect performance.This will significantly affect performance.This will significantly affect performance.This will significantly affect performance.This will significantly affect performance.This will significantly affect performance.This will significantly affect performance.This will significantly affect performance.This will significantly affect performance.This will significantly affect performance." },
		};

		public static readonly System.Collections.Generic.Dictionary<string, string> _PLAYTEST_SETTINGS_CONTENT = new() {
			{ "Enable test client to stream output", "This is enabled by default. This allows the client local-script and local game-server clua scripts output to be dumped into a log file which the workshop reads." },
			{ "Stop dumping output if log file becomes bigger then 100MB", "This will prevent the output file to be written if it exceeds 100MB to prevent taking up big-space." },
		};

		public static readonly System.Collections.Generic.Dictionary<string, string> _BETA_SETTINGS_CONTENT = new() {
			{ "Allow 3rd party plugins", "THIS FEATURE IS IN BETA AND ONLY USE FOR TESTING AND NOT FOR PRODUCTION!" },
		};

		public static readonly System.Collections.Generic.Dictionary<string, string> _INTERNAL_SETTINGS_CONTENT = new() {
			{ "Throw internal workshop errors", "This feature helps with core workshop developers debug any runtime errors inside release builds. Enable this only if you are trying to report a bug to the developers to help dig deeper into the issue." }
		};

		public static readonly bool[] _RENDER_SETTINGS_DEFAULT = [
			false,
			true,
			false,
			false
		];

		public static readonly bool[] _PLAYTEST_SETTINGS_DEFAULT = [
			true, 
			true
		];

		public static readonly bool[] _BETA_SETTINGS_DEFAULT = [
			true
		];

		public static readonly bool[] _INTERNAL_SETTINGS_DEFAULT = [
			false
		];
	}
}
