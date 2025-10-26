using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace SoundEditorPlugin.Helpers
{
	// Token: 0x0200002D RID: 45
	public abstract class HelperBase<T> where T : HelperBase<T>, new()
	{
		// Token: 0x1700007C RID: 124
		// (get) Token: 0x06000179 RID: 377 RVA: 0x0000A7B7 File Offset: 0x000089B7
		// (set) Token: 0x0600017A RID: 378 RVA: 0x0000A7BF File Offset: 0x000089BF
		public string BasePath { get; protected set; } = "Base";

		// Token: 0x1700007D RID: 125
		// (get) Token: 0x0600017B RID: 379 RVA: 0x0000A7C8 File Offset: 0x000089C8
		// (set) Token: 0x0600017C RID: 380 RVA: 0x0000A7D0 File Offset: 0x000089D0
		public string ResourceName { get; protected set; } = "Resource";

		// Token: 0x1700007E RID: 126
		// (get) Token: 0x0600017D RID: 381 RVA: 0x0000A7D9 File Offset: 0x000089D9
		// (set) Token: 0x0600017E RID: 382 RVA: 0x0000A7E1 File Offset: 0x000089E1
		public string ToolName { get; protected set; } = "Tool";

		// Token: 0x1700007F RID: 127
		// (get) Token: 0x0600017F RID: 383 RVA: 0x0000A7EA File Offset: 0x000089EA
		public string ResourcePath
		{
			get
			{
				return Path.Combine(this.BasePath, this.ToolName);
			}
		}

		// Token: 0x17000080 RID: 128
		// (get) Token: 0x06000180 RID: 384 RVA: 0x0000A7FD File Offset: 0x000089FD
		public static T Instance
		{
			get
			{
				return HelperBase<T>.lazy.Value;
			}
		}

		// Token: 0x06000181 RID: 385 RVA: 0x0000A80C File Offset: 0x00008A0C
		public async Task InitializeAsync()
		{
			await this._semaphore.WaitAsync();
			this.State = HelperBase<T>.InitializedState.Initializing;
			Directory.CreateDirectory(this.BasePath);
			Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(this.ResourceName);
			ZipArchive archive = new ZipArchive(stream, ZipArchiveMode.Read, false);
			foreach (ZipArchiveEntry zipArchiveEntry in archive.Entries)
			{
				Stream entryStream = null;
				FileStream outputStream = null;
				try
				{
					entryStream = zipArchiveEntry.Open();
					string text = Path.Combine(this.BasePath, zipArchiveEntry.Name);
					if (!File.Exists(text))
					{
						outputStream = new FileStream(text, FileMode.Create, FileAccess.Write, FileShare.Read);
						await entryStream.CopyToAsync(outputStream).ConfigureAwait(false);
					}
				}
				finally
				{
					Stream stream2 = entryStream;
					if (stream2 != null)
					{
						stream2.Dispose();
					}
					FileStream fileStream = outputStream;
					if (fileStream != null)
					{
						fileStream.Dispose();
					}
				}
				entryStream = null;
				outputStream = null;
			}
			IEnumerator<ZipArchiveEntry> enumerator = null;
			this.State = HelperBase<T>.InitializedState.Initialized;
			archive.Dispose();
			stream.Dispose();
			this._semaphore.Release();
		}

		// Token: 0x06000182 RID: 386 RVA: 0x0000A850 File Offset: 0x00008A50
		public async Task WaitForSemaphore()
		{
			await this._semaphore.WaitAsync();
			this._semaphore.Release();
		}

		// Token: 0x040000A7 RID: 167
		private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

		// Token: 0x040000A8 RID: 168
		public HelperBase<T>.InitializedState State;

		// Token: 0x040000AC RID: 172
		private static readonly Lazy<T> lazy = new Lazy<T>(() => new T());

		// Token: 0x02000056 RID: 86
		public enum InitializedState
		{
			// Token: 0x04000281 RID: 641
			NotInitialized,
			// Token: 0x04000282 RID: 642
			Initializing,
			// Token: 0x04000283 RID: 643
			Initialized
		}
	}
}
