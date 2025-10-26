using System;
using System.IO;
using FrostySdk.Interfaces;
using FrostySdk.IO;

namespace SoundEditorPlugin.Resources
{
	// Token: 0x0200001F RID: 31
	internal class NativeReader2 : NativeReader
	{
		// Token: 0x1700002F RID: 47
		// (get) Token: 0x060000B6 RID: 182 RVA: 0x00007474 File Offset: 0x00005674
		// (set) Token: 0x060000B7 RID: 183 RVA: 0x0000747C File Offset: 0x0000567C
		public virtual bool KeepUnderlyingStreamOpen { get; set; }

		// Token: 0x060000B8 RID: 184 RVA: 0x00007485 File Offset: 0x00005685
		public NativeReader2(Stream inStream)
			: base(inStream)
		{
		}

		// Token: 0x060000B9 RID: 185 RVA: 0x0000748E File Offset: 0x0000568E
		public NativeReader2(Stream inStream, IDeobfuscator inDeobfuscator)
			: base(inStream)
		{
		}

		// Token: 0x060000BA RID: 186 RVA: 0x00007498 File Offset: 0x00005698
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Stream stream = this.stream;
				this.stream = null;
				if (!this.KeepUnderlyingStreamOpen && stream != null)
				{
					stream.Close();
				}
			}
			this.stream = null;
			this.buffer = null;
		}
	}
}
