using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.CompilerServices;

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

        public async Task<(byte[] spsData, byte[] seekTableData)> ImportSound(string importFileName, string codec, bool isSeekable)
        {
            if (this.State == HelperBase<ToolHelper>.InitializedState.NotInitialized)
            {
                await base.InitializeAsync();
            }

            // Use a unique name for temporary output files
            string tempFileName = base.BasePath + Guid.NewGuid().ToString();

            byte[] spsData;
            byte[] seekTableData = null;

            // Ensure the base directory exists before running the external tool
            Directory.CreateDirectory(base.BasePath);

            try
            {
                Process process = new Process();
                process.StartInfo = new ProcessStartInfo(base.ResourcePath) // ResourcePath is the executable path
                {
                    Arguments = string.Concat(
                        "-sndplayer -fileformatversion1 -",
                        codec,
                        " ",
                        isSeekable ? "-seekable" : "",
                        " \"",
                        importFileName,
                        "\" -=\"",
                        tempFileName,
                        "\""
                    ),
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                process.EnableRaisingEvents = true;
                process.Start();
                process.WaitForExit();

                string spsFilePath = tempFileName + ".sps";
                string seekFilePath = tempFileName + ".sek";

                if (process.ExitCode != 0 || !File.Exists(spsFilePath))
                {
                    throw new FileFormatException(string.Format("Failed to import the file. Error: {0} (Exit Code: {1})",
                        File.Exists(spsFilePath) ? "File exists but exit code is non-zero" : "Output file not found",
                        process.ExitCode));
                }

                // Read output files
                spsData = File.ReadAllBytes(spsFilePath);
                if (isSeekable && File.Exists(seekFilePath))
                {
                    seekTableData = File.ReadAllBytes(seekFilePath);
                }
            }
            finally
            {
                // Cleanup temporary files
                string spsFilePath = tempFileName + ".sps";
                string seekFilePath = tempFileName + ".sek";
                string sphFilePath = tempFileName + ".sph"; // Additional header file that might be generated

                try
                {
                    if (File.Exists(spsFilePath)) File.Delete(spsFilePath);
                    if (File.Exists(seekFilePath)) File.Delete(seekFilePath);
                    if (File.Exists(sphFilePath)) File.Delete(sphFilePath);
                }
                catch (Exception ex)
                {
                    // Log cleanup failure but don't re-throw
                    Debug.WriteLine($"Warning: Failed to delete temporary file during cleanup. Path: {tempFileName}, Error: {ex.Message}");
                }
            }
            // Return uses the ValueTuple constructor
            return (spsData, seekTableData);
        }

        // Calculates duration from the encoded buffer metadata
        public float GetDurationInSecondsFromBuffer(byte[] buffer)
        {
            // Requires buffer length to read header information
            if (buffer == null || buffer.Length < 12)
            {
                throw new ArgumentException("buffer cannot be null or must be at least 12 bytes long to read sound metadata.");
            }

            // Read Sample Rate (at offset 4, Big Endian)
            // The method uses reverseAtIndex which handles the byte reversal for Big Endian.
            int sampleRate = BitConverter.ToInt32(this.reverseAtIndex(buffer, 4), 0);

            // Read Sample Count (at offset 8, Big Endian)
            int sampleCount = BitConverter.ToInt32(this.reverseAtIndex(buffer, 8), 0);

            // The relevant data is in the lower 21 bits for sample rate and 29 bits for sample count.
            // The decompiled logic masks this:
            int rate = sampleRate & 0x0003FFFF; // (262143)
            int count = sampleCount & 0x1FFFFFFF; // (536870911)

            if (rate == 0)
            {
                return 0f;
            }
            return (float)count / (float)rate;
        }

        // Reverses bytes at a specified index to read Big Endian data using BitConverter
        private byte[] reverseAtIndex(byte[] buffer, int index)
        {
            // Read 4 bytes at index
            byte[] array = new ArraySegment<byte>(buffer, index, 4).ToArray<byte>();
            // Reverse them (BitConverter is Little Endian by default)
            Array.Reverse(array);
            return array.ToArray<byte>();
        }
    }
}
