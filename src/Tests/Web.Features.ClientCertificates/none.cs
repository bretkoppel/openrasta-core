using System;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using OpenRasta.Configuration;
using OpenRasta.Hosting.AspNetCore;
using OpenRasta.IO;
using OpenRasta.Web;
using Xunit;

namespace Tests.Web.Features.ClientCertificates
{
  public class none
  {
    static Random randomPort = new Random();

    [Fact]
    public async Task doItAll()
    {
      if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) == false)
        return;
      
      var port = randomPort.Next(2048, 4096);
      var names = GetType().Assembly.GetManifestResourceNames();
      var serverCert = new X509Certificate2(GetType().Assembly
          .GetManifestResourceStream("Tests.Web.Features.ClientCertificates.keys.certificates.server.pfx").ReadToEnd(),
        "openrasta");
      var clientCert = new X509Certificate2(GetType().Assembly
          .GetManifestResourceStream("Tests.Web.Features.ClientCertificates.keys.certificates.client.pfx").ReadToEnd(),
        "openrasta");


      using (var server = new WebHostBuilder()
        .UseKestrel(options =>
          options.Listen(IPAddress.Any, port, listenOptions => { listenOptions.UseHttps(serverCert); }))
        .Configure(app => app.UseOpenRasta(new CertUsingApi()))
        .Build())
      {
        server.RunAsync();

        var client = new HttpClient(new HttpClientHandler()
        {
          ClientCertificates = {clientCert},
          ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
        var response = await client.GetAsync($"https://localhost:{port}/verify");

        response.EnsureSuccessStatusCode();
      }
    }
  }

  public class CertUsingApi : IConfigurationSource
  {
    public void Configure()
    {
      ResourceSpace.Has.ResourcesNamed("verify")
        .AtUri("/verify")
        .HandledBy<Handler>();
    }

    class Handler
    {
      public OperationResult Get => new OperationResult.OK();
    }
  }
}