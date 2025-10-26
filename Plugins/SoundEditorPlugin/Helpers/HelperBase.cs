using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics; // Added for Debug.WriteLine

namespace SoundEditorPlugin.Helpers
{
    public abstract class HelperBase<T> where T : HelperBase<T>, new()
    {
        // Public properties defining paths and names
        public string BasePath { get; protected set; } = "Base";

        public string ResourceName { get; protected set; } = "Resource";

        public string ToolName { get; protected set; } = "Tool";

        public string ResourcePath
        {
            get
            {
                return Path.Combine(this.BasePath, this.ToolName);
            }
        }

        public static T Instance
        {
            get
            {
                return HelperBase<T>.lazy.Value;
            }
        }

        /// <summary>
        /// Initializes the helper by extracting the embedded resource zip file to the BasePath directory.
        /// Uses a SemaphoreSlim to ensure thread-safe, single initialization.
        /// </summary>
        public async Task InitializeAsync()
        {
            // Wait for the semaphore to ensure only one thread initializes at a time
            await this._semaphore.WaitAsync();

            Stream stream = null;
            ZipArchive archive = null;

            try
            {
                this.State = InitializedState.Initializing;

                // Ensure the base directory exists
                Directory.CreateDirectory(this.BasePath);

                // Load the embedded resource stream
                stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.ResourceName);
                if (stream == null)
                {
                    throw new FileNotFoundException($"Embedded resource stream for '{this.ResourceName}' was not found.");
                }

                archive = new ZipArchive(stream, ZipArchiveMode.Read, false);

                // Extract all files from the zip archive
                foreach (ZipArchiveEntry zipArchiveEntry in archive.Entries)
                {
                    Stream entryStream = null;
                    FileStream outputStream = null;

                    try
                    {
                        string destinationPath = Path.Combine(this.BasePath, zipArchiveEntry.Name);

                        // Only extract if the file does not already exist
                        if (!File.Exists(destinationPath))
                        {
                            entryStream = zipArchiveEntry.Open();
                            outputStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.Read);

                            // Copy the stream asynchronously
                            await entryStream.CopyToAsync(outputStream).ConfigureAwait(false);
                        }
                    }
                    catch (Exception ex)
                    {
                        // Log extraction failures but attempt to continue with other files
                        Debug.WriteLine($"Error extracting file '{zipArchiveEntry.Name}' to '{this.BasePath}'. Error: {ex.Message}");
                    }
                    finally
                    {
                        // Clean disposal for inner streams
                        entryStream?.Dispose();
                        outputStream?.Dispose();
                    }
                }

                this.State = InitializedState.Initialized;
            }
            catch (Exception ex)
            {
                // Log and re-throw critical initialization failure
                this.State = InitializedState.NotInitialized;
                Debug.WriteLine($"CRITICAL Initialization Failure for HelperBase. Resource: {this.ResourceName}. Error: {ex.Message}");
                throw;
            }
            finally
            {
                // Dispose of main resources if they were successfully created
                archive?.Dispose();
                stream?.Dispose();

                // Release the semaphore in the final block
                this._semaphore.Release();
            }
        }

        public async Task WaitForSemaphore()
        {
            await this._semaphore.WaitAsync();
            this._semaphore.Release();
        }

        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

        public InitializedState State;

        private static readonly Lazy<T> lazy = new Lazy<T>(() => new T());

        public enum InitializedState
        {
            NotInitialized,
            Initializing,
            Initialized
        }
    }
}
