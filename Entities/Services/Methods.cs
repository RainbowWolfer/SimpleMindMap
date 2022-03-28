using System;

namespace MindMap.Entities.Services {
	public static class Methods {
		public static string GetTick(int randomLength = 6) {
			long ticks = DateTime.Now.Ticks;
			string random = "";
			for(int i = 0; i < randomLength; i++) {
				random += new Random().Next();
			}
			return $"{ticks}_{random}";
		}
	}
}
