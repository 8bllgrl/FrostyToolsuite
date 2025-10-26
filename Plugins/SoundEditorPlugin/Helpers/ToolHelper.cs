using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SoundEditorPlugin.Helpers
{
    public class ToolHelper : HelperBase<ToolHelper>
    {
        public ToolHelper()
        {
            base.BasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            base.ResourceName = "SoundEditorPlugin.Resources.Tool.lib.zip";
            base.ToolName = "Tool";
        }

        // FIX: Removed the explicit [return: TupleElementNames] attribute
        // and used the C# tuple syntax in the return type to define element names.
        public async Task<(byte[] spsData, byte[] seekTableData)> ImportSound(string importFileName, string codec, bool isSeekable)
        {
            if (this.State == HelperBase<ToolHelper>.InitializedState.NotInitialized)
            {
                await base.InitializeAsync();
            }
            string text = base.BasePath + Guid.NewGuid().ToString();
            byte[] array; // Corresponds to spsData
            byte[] array2; // Corresponds to seekTableData
            try
            {
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo(base.ResourcePath + ".")
                {
                    Arguments = string.Concat(new string[]
                    {
                        "-sndplayer -fileformatversion1 -",
                        codec,
                        " ",
                        isSeekable ? "-seekable" : "",
                        " \"",
                        importFileName,
                        "\" -=\"",
                        text,
                        "\""
                    }),
                    UseShellExecute = false,
                    CreateNoWindow = true
                };
                process.EnableRaisingEvents = true;
                process.Start();
                process.WaitForExit();
                if (process.ExitCode != 0 && !File.Exists(text + ".sps"))
                {
                    throw new FileFormatException(string.Format("Failed to import the file. Error: {0}", process.ExitCode));
                }
            }
            finally
            {
                string text2 = text + ".sps";
                string text3 = text + ".sek";
                string text4 = text + ".sph";
                array = File.ReadAllBytes(text2);
                array2 = (isSeekable ? File.ReadAllBytes(text3) : null);
                try
                {
                    if (File.Exists(text2))
                    {
                        File.Delete(text2);
                    }
                    if (File.Exists(text3))
                    {
                        File.Delete(text3);
                    }
                    if (File.Exists(text4))
                    {
                        File.Delete(text4);
                    }
                }
                catch (Exception ex)
                {
                    // Log the exception during cleanup, but continue execution as failure here is non-critical.
                    Debug.WriteLine($"Warning: Failed to delete temporary file during cleanup. Path: {text}, Error: {ex.Message}");
                }
            }
            // Return uses the ValueTuple constructor, which works fine with the named tuple return type.
            return new ValueTuple<byte[], byte[]>(array, array2);
        }

        public float GetDurationInSecondsFromBuffer(byte[] buffer)
        {
            if (buffer == null || buffer.Length < 12)
            {
                throw new ArgumentException("buffer cannot be null or must be at least 2 bytes long.");
            }
            int num = BitConverter.ToInt32(this.reverseAtIndex(buffer, 4), 0);
            int num2 = BitConverter.ToInt32(this.reverseAtIndex(buffer, 8), 0);
            int num3 = num & 262143;
            int num4 = num2 & 536870911;
            if (num3 == 0)
            {
                return 0f;
            }
            return (float)num4 / (float)num3;
        }

        private byte[] reverseAtIndex(byte[] buffer, int index)
        {
            byte[] array = new ArraySegment<byte>(buffer, index, 4).ToArray<byte>();
            Array.Reverse(array);
            return array.ToArray<byte>();
        }
    }
}
