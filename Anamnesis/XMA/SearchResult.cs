﻿// Concept Matrix 3.
// Licensed under the MIT license.

namespace Anamnesis.XMA
{
	using System;
	using System.Text.Json.Serialization;

	[Serializable]
	public class SearchResult
	{
		[JsonPropertyName("id")]
		public string? Id { get; set; }

		[JsonPropertyName("name")]
		public string? Name { get; set; }

		[JsonPropertyName("thumbnail")]
		public string? Thumbnail { get; set; }

		[JsonPropertyName("author")]
		public AuthorInfo? Author { get; set; }

		public class AuthorInfo
		{
			[JsonPropertyName("xma_id")]
			public string? XmaId { get; set; }

			[JsonPropertyName("display_name")]
			public string? DisplayName { get; set; }
		}
	}
}
