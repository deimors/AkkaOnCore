using System;
using System.Collections.Generic;
using System.Linq;
using Functional;

namespace AkkaOnCore.Domain
{
	public static class ResultExtensions
	{
		public static Result<TSuccess, TFailure> FailIf<TSuccess, TFailure>(this Result<TSuccess, TFailure> source, bool condition, Func<TFailure> failureFactory)
			=> source.Bind(success => Result.Create(!condition, success, failureFactory()));

		public static Result<IEnumerable<TOut>, TFailure> BuildSequence<TIn, TOut, TFailure>(this Result<TIn, TFailure> source, params TOut[] events)
			=> source.Select(_ => events.AsEnumerable());
	}
}