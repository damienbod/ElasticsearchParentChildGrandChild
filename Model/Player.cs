using System.ComponentModel.DataAnnotations;

namespace ConsoleElasticsearchParentChildGrandChild.Model
{
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