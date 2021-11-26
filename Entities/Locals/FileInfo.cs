using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Locals {
	public class FileInfo {
		public event EventHandler FileChanged;
		public string FileName { get; private set; }
		public string FilePath { get; private set; }

		public FileInfo(string fileName, string filePath) {
			FileName = fileName;
			FilePath = filePath;
		}
	}
}
