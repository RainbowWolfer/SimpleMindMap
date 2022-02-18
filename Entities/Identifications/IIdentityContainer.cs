using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MindMap.Entities.Identifications {
	public interface IIdentityContainer {
		Identity Identity { get; set; }
	}
}
