using System;
using ConsoleElasticsearchParentChildGrandChild.Model;
using ElasticsearchCRUD;
using ElasticsearchCRUD.ContextAddDeleteUpdate.IndexModel;
using ElasticsearchCRUD.Tracing;
using ElasticsearchCRUD.Utils;

namespace ConsoleElasticsearchParentChildGrandChild
{
	class Program
	{
		private static readonly IElasticsearchMappingResolver ElasticsearchMappingResolver = new ElasticsearchMappingResolver();
		private const bool SaveChildObjectsAsWellAsParent = true;
		private const bool ProcessChildDocumentsAsSeparateChildIndex = true;
		private const bool UserDefinedRouting = true;
		private static readonly ElasticsearchSerializerConfiguration Config = new ElasticsearchSerializerConfiguration(ElasticsearchMappingResolver, SaveChildObjectsAsWellAsParent,
				ProcessChildDocumentsAsSeparateChildIndex, UserDefinedRouting);

		private const string ConnectionString = "http://localhost:9200";

		static void Main(string[] args)
		{
			// Define the mapping for the type so that all use the same index as the parent
			ElasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(LeagueCup), MappingUtils.GetElasticsearchMapping("leagues"));
			ElasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(Team), MappingUtils.GetElasticsearchMapping("leagues"));
			ElasticsearchMappingResolver.AddElasticSearchMappingForEntityType(typeof(Player), MappingUtils.GetElasticsearchMapping("leagues"));

			CreateIndexWithRouting();
			Console.ReadLine();

			var leagueAndRoutingId = CreateNewLeague();
			Console.ReadLine();

			var teamId = AddTeamToCup(leagueAndRoutingId);
			Console.ReadLine();

			AddPlayerToTeam(teamId, leagueAndRoutingId);
			Console.ReadLine();

			var player = GetPlayer(3, leagueAndRoutingId, teamId);
			Console.WriteLine("Found player: " + player.Name);
			Console.ReadLine();
		}

		private static void CreateIndexWithRouting()
		{
			// Use routing for the child parent relationship. This is required if you use grandchild documents.
			// If routing ensures that the grandchild documents are saved to the same shard as the parent document.
			// --------------
			// If you use only parent and child documents, routing is not required. The child documents are saved
			// to the same shard as the parent document using the parent definition.
			// -------------- 
			// The routing definition can be defined using the configuration parameter: UserDefinedRouting in the ElasticsearchSerializerConfiguration
			//var config = new ElasticsearchSerializerConfiguration(ElasticsearchMappingResolver, SaveChildObjectsAsWellAsParent,
			//	ProcessChildDocumentsAsSeparateChildIndex, UserDefinedRouting);
		
			using (var context = new ElasticsearchContext(ConnectionString, Config))
			{
				context.TraceProvider = new ConsoleTraceProvider();
			
				// Create index in Elasticsearch
				// This creates a index leagues and 3 types, leaguecup, team, player
				var ret = context.IndexCreate<LeagueCup>();
			}
		}

		private static long CreateNewLeague()
		{
			var swissCup = new LeagueCup {Description = "Nataional Cup Switzerland", Id = 1, Name = "Swiss Cup"};

			using (var context = new ElasticsearchContext(ConnectionString, Config))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(swissCup, swissCup.Id);
				context.SaveChanges();
			}

			return swissCup.Id;
		}

		/// <summary>
		/// The parentId is the id of the parent object
		/// The routing Id is required for Elasticsearch to force that all child objects are saved to the same shard. This is good for performance.
		/// As this is a first level child, the routingId and the parentId are the same.
		/// </summary>
		private static long AddTeamToCup(long leagueId)
		{
			var youngBoys = new Team {Id=2,Name="Young Boys", Stadium="Wankdorf Bern"};

			using (var context = new ElasticsearchContext(ConnectionString, Config))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(youngBoys, youngBoys.Id, new RoutingDefinition { ParentId = leagueId, RoutingId = leagueId });
				context.SaveChanges();
			}

			return youngBoys.Id;
		}

		private static void AddPlayerToTeam(long teamId, long leagueId)
		{
			var yvonMvogo = new Player { Id = 3, Name = "Yvon Mvogo", Age = 20, Goals = 0, Assists = 0, Position = "Goalkeeper" };

			using (var context = new ElasticsearchContext(ConnectionString, Config))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				context.AddUpdateDocument(yvonMvogo, yvonMvogo.Id, new RoutingDefinition { ParentId = teamId, RoutingId = leagueId });
				context.SaveChanges();
			}
		}

		private static Player GetPlayer(long playerId, long leagueId, long teamId)
		{
			Player player;
			using (var context = new ElasticsearchContext(ConnectionString, Config))
			{
				context.TraceProvider = new ConsoleTraceProvider();
				player = context.GetDocument<Player>(playerId, new RoutingDefinition { ParentId = teamId, RoutingId = leagueId });
			}

			return player;
		}
	}
}
