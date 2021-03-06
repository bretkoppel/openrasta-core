using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using OpenRasta.Concordia;
using OpenRasta.Configuration;
using OpenRasta.Configuration.MetaModel.Handlers;
using OpenRasta.Hosting.InMemory;
using OpenRasta.Plugins.Hydra;
using OpenRasta.Web;
using Shouldly;
using Tests.Plugins.Hydra.Implementation;
using Tests.Plugins.Hydra.Utf8Json;
using Xunit;

namespace Tests.Plugins.Hydra.nodes
{
  public class iri_node_with_list_of_iri_nodes : IAsyncLifetime
  {
    IResponse response;
    JToken body;
    readonly InMemoryHost server;

    public iri_node_with_list_of_iri_nodes()
    {
      server = new InMemoryHost(() =>
        {
          ResourceSpace.Uses.Hydra(options =>
          {
            options.Vocabulary = "https://schemas.example/schema#";
            options.Serializer = ctx =>
              ctx.Transient(() => new PreCompiledUtf8JsonSerializer()).As<IMetaModelHandler>();
          });

          ResourceSpace.Has.ResourcesOfType<Person>()
            .Vocabulary("https://schemas.example/schema#")
            .AtUri("/people/{id}");

          ResourceSpace.Has.ResourcesOfType<EventWithPerson>()
            .Vocabulary("https://schemas.example/schema#")
            .AtUri("/events/{id}")
            .HandledBy<Handler>();
        },
        startup: new StartupProperties()
        {
          OpenRasta =
          {
            Diagnostics = {TracePipelineExecution = false},
            Errors = {HandleAllExceptions = false, HandleCatastrophicExceptions = false}
          }
        });
    }


    [Fact]
    public void inner_node_has_type()
    {
      body["@id"].ShouldBe("http://localhost/events/2");
      body["@type"].ShouldBe("EventWithPerson");
      body["@context"].ShouldBe("http://localhost/.hydra/context.jsonld");
      body["attendees"][0]["name"].ShouldBe("person name");
      body["attendees"][0]["@type"].ShouldBe("Person");
      body["attendees"][0]["@id"].ShouldBe("http://localhost/people/1");
    }

    public async Task InitializeAsync()
    {
      (response, body) = await server.GetJsonLd("/events/2");
    }

    class Handler
    {
      public EventWithPerson Get(int id)
      {
        return new EventWithPerson()
        {
          Id = id, Name = "event",
          Attendees = new[]{new Person() {Name = "person name", Id = 1}}.ToList()
        };
      }
    }

    public async Task DisposeAsync() => server.Close();
  }
}