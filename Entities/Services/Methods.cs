using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Services {
	public static class Methods {
		public static string GetTick(int randomLength = 6) {
			long ticks = DateTime.Now.Ticks;
			string random = "";
			for(int i = 0; i < randomLength; i++) {
				random += new Random().Next(0, 10);
			}
			return $"{ticks}_{random}";
		}
	}
}
