using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using FrostySdk.IO;

namespace SoundEditorPlugin.Helpers
{
	// Token: 0x0200002F RID: 47
	public class VgmStreamHelper : HelperBase<VgmStreamHelper>
	{
		// Token: 0x06000189 RID: 393 RVA: 0x0000AA0C File Offset: 0x00008C0C
		public VgmStreamHelper()
		{
			base.BasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
			base.ResourceName = "SoundEditorPlugin.Resources.vgmstream.zip";
			base.ToolName = "vgmstream-cli.exe";
		}

		// Token: 0x0600018A RID: 394 RVA: 0x0000AA58 File Offset: 0x00008C58
		public async Task<short[]> Decode(byte[] soundBuffer)
		{
			if (this.State == HelperBase<VgmStreamHelper>.InitializedState.Initializing)
			{
				await base.WaitForSemaphore();
			}
			else if (this.State == HelperBase<VgmStreamHelper>.InitializedState.NotInitialized)
			{
				await base.InitializeAsync();
			}
			string text = base.BasePath + Guid.NewGuid().ToString() + ".sps";
			string text2 = base.BasePath + Guid.NewGuid().ToString() + ".wav";
			File.WriteAllBytes(text, soundBuffer);
			try
			{
				Process process = new Process();
				process.StartInfo = new ProcessStartInfo(base.ResourcePath + ".")
				{
					Arguments = "-o " + text2 + " " + text,
					UseShellExecute = false,
					CreateNoWindow = true
				};
				process.EnableRaisingEvents = true;
				process.Start();
				process.WaitForExit();
				if (process.ExitCode != 0 && !File.Exists(text2))
				{
					throw new FileFormatException(string.Format("Failed to decode the file. Error: {0}", process.ExitCode));
				}
			}
			finally
			{
				try
				{
					if (File.Exists(text))
					{
						File.Delete(text);
					}
				}
				catch (Exception)
				{
				}
			}
			short[] array2;
			using (NativeReader nativeReader = new NativeReader(new FileStream(text2, FileMode.Open, FileAccess.Read)))
			{
				nativeReader.Position = 22L;
				nativeReader.ReadUShort(0);
				nativeReader.ReadUInt(0);
				nativeReader.Position = 34L;
				ushort num = nativeReader.ReadUShort(0);
				nativeReader.Position = 40L;
				int num2 = (int)(nativeReader.ReadUInt(0) / (uint)(num / 8));
				short[] array = new short[num2];
				for (int i = 0; i < num2; i++)
				{
					array[i] = nativeReader.ReadShort(0);
				}
				array2 = array;
			}
			File.Delete(text2);
			return array2;
		}
	}
}
