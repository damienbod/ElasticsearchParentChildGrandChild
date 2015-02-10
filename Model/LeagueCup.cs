using System.Collections.Generic;

namespace ConsoleElasticsearchParentChildGrandChild.Model
{
	public class LeagueCup
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public List<Team> Teams { get; set; }
	}
}