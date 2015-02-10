using System;
using ElasticsearchCRUD;

namespace ConsoleElasticsearchParentChildGrandChild
{
	public class GlobalLeaguesElasticsearchMapping : ElasticsearchMapping
	{
		public override string GetIndexForType(Type type)
		{
			return "leagues";
		}

		public override string GetDocumentType(Type type)
		{
			return "";
		}
	}
}