using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Services {
	public static class WindowsService {
		public static string LastSaveFilePath { get; private set; } = "";

		public static SaveResult CreateSaveFileDialog(string filter) {
			SaveFileDialog dialog = new() {
				Filter = filter,
				InitialDirectory =
					string.IsNullOrWhiteSpace(LastSaveFilePath) ?
					Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) :
					LastSaveFilePath,
			};
			string path = "";
			bool success = dialog.ShowDialog() == true;
			if(success) {
				path = dialog.FileName;
				LastSaveFilePath = dialog.FileName;
			}
			return new SaveResult(dialog, success, path);
		}
	}

	public class SaveResult {
		public SaveFileDialog Dialog { get; set; }
		public bool Success { get; set; }
		public string Path { get; set; }

		public SaveResult(SaveFileDialog dialog, bool success, string path) {
			Dialog = dialog;
			Success = success;
			Path = path;
		}
	}
}
