using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ConsoleElasticsearchParentChildGrandChild.Model
{
	public class LeagueCup
	{
		public long Id { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public List<Team> Teams { get; set; }
	}

	public class Team
	{
		[Key]
		public long Id { get; set; }
		public string Name { get; set; }
		public string Stadium { get; set; }
		public List<Player> Players { get; set; }
	}

	public class Player
	{
		[Key]
		public long Id { get; set; }
		public string Name { get; set; }
		public int Goals { get; set; }
		public int Assists { get; set; }
		public string Position { get; set; }
		public int Age { get; set; }
	}
}
