﻿using System;
using System.Diagnostics;
using System.Runtime.Remoting.Messaging;
using System.Threading;
using System.Web;
using System.Web.Configuration;
using System.Web.Hosting;
using OpenRasta.Concordia;
using OpenRasta.Diagnostics;
using OpenRasta.Pipeline;
using OpenRasta.Web;

namespace OpenRasta.Hosting.AspNet
{
  public class OpenRastaModuleAsync : IHttpModule
  {
    public void Dispose()
    {
    }

    public static AspNetPipeline Pipeline => _pipeline.Value;
    public static ILogger Log = new TraceSourceLogger(new TraceSource("openrasta"));

    static readonly Lazy<AspNetPipeline> _pipeline = new Lazy<AspNetPipeline>(() =>
      HttpRuntime.UsingIntegratedPipeline
        ? (AspNetPipeline) new IntegratedPipeline()
        : new ClassicPipeline(), LazyThreadSafetyMode.PublicationOnly);

    PipelineStageAsync _postResolveStep;
    AspNetHost _host;

    public void Init(HttpApplication context)
    {
      var startupProperties = new StartupProperties
      {
        OpenRasta =
        {
          Pipeline =
          {
            ContributorTrailers =
            {
              [call => call.Target is KnownStages.IUriMatching] =
              () => new YieldBeforeNextMiddleware(nameof(KnownStages.IUriMatching))
            }
          }
        }
      };
      _host = new AspNetHost(startupProperties);
      _hostManager = HostManager.RegisterHost(_host);
      _host.RaiseStart();

      _postResolveStep = new PipelineStageAsync(nameof(KnownStages.IUriMatching), _host, Pipeline);

      context.AddOnPostResolveRequestCacheAsync(_postResolveStep.Begin, _postResolveStep.End);
      
      context.EndRequest += HandleHttpApplicationEndRequestEvent;
    }

    const string COMM_CONTEXT_KEY = "__OR_COMM_CONTEXT";

    void HandleHttpApplicationEndRequestEvent(object sender, EventArgs e)
    {
      var httpContext = ((HttpApplication) sender).Context;
      CallContext.LogicalSetData("__OR_CONTEXT", httpContext);
      var commContext = (ICommunicationContext) httpContext.Items[COMM_CONTEXT_KEY];
      if (commContext.PipelineData.ContainsKey("openrasta.hosting.aspnet.handled"))
        _host.RaiseIncomingRequestProcessed(commContext);
    }

    HostManager _hostManager;
  }
}