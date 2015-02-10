using System;
using System.Collections.Generic;
using System.Globalization;
using ConsoleElasticsearchParentChildGrandChild.Model;
using ElasticsearchCRUD;
using ElasticsearchCRUD.ContextSearch.SearchModel;
using ElasticsearchCRUD.Model.SearchModel;
using ElasticsearchCRUD.Model.SearchModel.Filters;
using ElasticsearchCRUD.Model.SearchModel.Queries;
using ElasticsearchCRUD.Tracing;
using ElasticsearchCRUD.Utils;

namespace ConsoleElasticsearchParentChildGrandChild
{
	public class GlobalSearch
	{
		private readonly string _connectionString;

		public GlobalSearch(string connectionString)
		{
			_connectionString = connectionString;
			_elasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(object), new GlobalElasticsearchMapping());
		}

		private readonly IElasticsearchMappingResolver _elasticsearchMappingResolver = new ElasticsearchMappingResolver();
	
		public void RunGlobalSearch()
		{
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


		/// <summary>
		/// Get all the teams and the player documents
		/// </summary>
		/// <param name="leagueId">Requires the route for the explicit league</param>
		public void GetAllForRouteFilterForPlayersAndTeams(long leagueId)
		{
			var search = new Search
			{
				Filter = new Filter(
					new IndicesFilter(
						new List<string> { "leagues" },
						new OrFilter(
							new List<IFilter>
							{
								new TypeFilter("team"),
								new TypeFilter("player")
							}
						)
					)
					{
						NoMatchFilter = new TypeFilter("leaguecup")
					}
				)
			};

			using (var context = new ElasticsearchContext(_connectionString, _elasticsearchMappingResolver))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				var result = context.Search<object>(search,
					new SearchUrlParameters
					{
						Routing = leagueId.ToString(CultureInfo.InvariantCulture)
					});

				Console.WriteLine("Found {0}, Expected 2", result.PayloadResult.Hits.Total);
			}
		}
	}
}