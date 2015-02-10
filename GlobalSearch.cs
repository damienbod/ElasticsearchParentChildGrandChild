using System;
using ConsoleElasticsearchParentChildGrandChild.Model;
using ElasticsearchCRUD;
using ElasticsearchCRUD.Model.SearchModel;
using ElasticsearchCRUD.Model.SearchModel.Queries;
using ElasticsearchCRUD.Utils;

namespace ConsoleElasticsearchParentChildGrandChild
{
	public class GlobalSearch
	{
		private readonly string _connectionString;

		public GlobalSearch(string connectionString)
		{
			_connectionString = connectionString;
		}

		private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver = new ElasticsearchMappingResolver();
	
		public void RunGlobalSearch()
		{
			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());

			using (var context = new ElasticsearchContext(_connectionString, _elasticsearchMappingResolver))
			{
				long countValue = context.Count<object>();
				Console.WriteLine("Global Count for all indices: {0}, Expected at least 3", countValue);
				var result = context.Search<object>(new Search()
				{
					Query = new Query(new MatchAllQuery())
				});

				
				int count = result.PayloadResult.Hits.Total;

				foreach (var hit in result.PayloadResult.Hits.HitsResult)
				{
					string type = hit.TypeInIndex;
					if (type == "player")
					{
						var player = hit.GetSourceFromJToken<Player>();
						Console.WriteLine("Found a player: {0}, {1}", player.Name, player.Id);
					}
					else if (type == "team")
					{
						var team = hit.GetSourceFromJToken<Team>();
						Console.WriteLine("Found a team: {0}, {1}", team.Name, team.Id);
					}
				}
				
			}
		}
	}
}