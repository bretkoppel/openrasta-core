using System;
using System.Collections.Generic;

namespace OpenRastaDemo
{
  public class LittleHydraHandler
  {
    public LittleHydraResponse Get()
    {
      return new LittleHydraResponse
      {
        _id = "1",
        about = "about",
        address = "10 downing street",
        age = 21,
        balance = "12.45",
        company = "IBM",
        email = "me@home.com",
        friends = new List<Friend>(new[] {new Friend {id = 2, name = "Joey"}}),
        name = "Bob",
        gender = "male",
        greeting = "hi",
        guid = Guid.Empty,
        index = 4,
        latitude = 56.54,
        longitude = Decimal.One,
        phone = "020 7890 4322",
        picture = new Uri("http://www.google.com"),
        registered = DateTime.UtcNow,
        tags = new List<string>(new[] {"awesome"}),
        eyeColor = "green",
        favoriteFruit = "coconut",
        isActive = true
      };
    }
  }
}