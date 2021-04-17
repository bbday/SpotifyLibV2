using System.Collections.Generic;

namespace SpotifyLibrary.Validation
{
	internal interface IValidations
	{
		bool Any { get; }

		bool AnyErrors { get; }

		bool AnyWarnings { get; }

		bool AnyInfos { get; }

		IEnumerable<string> Infos { get; }

		IEnumerable<string> Warnings { get; }

		IEnumerable<string> Errors { get; }
	}
}
