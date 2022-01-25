using System;
using Lio.Core.Contracts;
using Lio.Core.Logs;
using Lio.Core.Options;
using Lio.Core.Runner;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using NodaTime;

namespace Lio.Core.Extensions;

public static class LioServiceCollectionExtensions
{
   public static IServiceCollection AddLioCore(this IServiceCollection services, Action<LioOptionsBuilder>? configure = default)
   {
      var optionsBuilder = new LioOptionsBuilder(services);
      configure?.Invoke(optionsBuilder);

      services.AddScoped<IScriptRunner, ScriptRunner>();
      services
         .TryAddSingleton<IClock>(SystemClock.Instance);
      services.AddMediatR(mediatr => mediatr.AsScoped(), typeof(ICommand), typeof(ScriptExecutionLogWriter));
      return services;
   }
   
   
   public static IServiceCollection AddMutator<T>(this IServiceCollection services) where T : class, ISpecificMutator =>
      services
         .AddSingleton<T>()
         .AddSingleton<ISpecificMutator>(sp => sp.GetRequiredService<T>());
}