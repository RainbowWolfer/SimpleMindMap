using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Locals {
	public class ImagesAssets {
		private List<ImageAsset> assets = new();

		public ImagesAssets() { }

		public string AddImage(string base64) {
			string key = CalculateID(base64);
			assets.Add(new ImageAsset(key, base64));
			return key;
		}

		public int RemoveImageByBase64(string base64) {
			if(string.IsNullOrWhiteSpace(base64)) {
				return 0;
			}
			return assets.RemoveAll(a => a.Base64 == base64);
		}

		public int RemoveImageByKey(string key) {
			if(string.IsNullOrWhiteSpace(key)) {
				return 0;
			}
			return assets.RemoveAll(a => a.Key == key);
		}

		public int RemoveByDateTime(DateTime time) {
			return assets.RemoveAll(a => a.AddTime.CompareTo(time) > 0);
		}

		public void SetAssets(List<ImageAsset> assets) {
			this.assets = assets;
		}

		public List<ImageAsset> GetAssets() {
			return assets.ToList();
		}

		public string? FindBase64(string key) {
			return assets.Find(a => a.Key == key)?.Base64;
		}

		private string CalculateID(string? base64 = null) {
			return $"{DateTime.Now.Ticks}_{new Random().Next()}_{base64?.Length ?? 0}";
		}
	}


	public class ImageAsset {
		public string Key { get; set; }
		public string Base64 { get; set; }
		public DateTime AddTime { get; private set; }

		public ImageAsset(string key, string base64) {
			Key = key;
			Base64 = base64;
			AddTime = DateTime.Now;
		}
	}
}
