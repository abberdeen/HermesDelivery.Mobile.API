using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Runtime.Caching;
using System.Web.Http;

namespace HermesDMobAPI.Controllers
{
    /// <summary>
    ///     ValuesController
    ///     Web API Controller for the Demo
    ///     Services CRUD requests for the Demo project
    /// </summary>
    public class PeopleController : ApiController
    {
        // Memory Cache
        private readonly ObjectCache cache = MemoryCache.Default;

        // List of people
        private List<string> people;

        /// <summary>
        ///     ValuesController
        ///     Constructs a temporary collection for the
        ///     demo. The collection is a list of string
        ///     entities representing characters from
        ///     Star Trek TNG
        /// </summary>
        public PeopleController()
        {
            if (!cache.Contains("People"))
            {
                // Simple List of People for CRUD Example
                people = new List<string>();

                // Add some generic values
                people.Add("Patrict Stewart");
                people.Add("Brent Spiner");
                people.Add("Jonathon Frakes");
                people.Add("Marina Sirtus");
                people.Add("Gates McFadden");
                people.Add("Michael Dorn");
                people.Add("LeVar Burton");
                people.Add("Wil Wheaton");
                people.Add("Denise Crosby");
                people.Add("Majel Barrett");
                people.Add("Colm Meaney");
                people.Add("Whoopi Goldberg");
                people.Add("John Di Lancie");
                people.Add("Diana Muldaur");
                people.Add($"Cached: {DateTime.Now.ToLongTimeString()}");

                // Cache Expiration set to 2 minutes in the future
                var policy = new CacheItemPolicy {AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(2)};

                // Add a new Cache!
                cache.Add("People", people, policy);
            } // end of if           
        } // end of constructor

        /// <summary>
        ///     Get (Collection)
        ///     Gets the entire collection of the entities
        ///     GET api/values
        /// </summary>
        /// <returns>a collection (IEnumerable) of entities </returns>
        public IEnumerable<string> Get()
        {
            // Get the List of Entities from the Cache
            return (List<string>) cache.Get("People");
        }

        /// <summary>
        ///     Get (Single)
        ///     Gets the entity by the identifier
        ///     GET api/values/5
        ///     IF index out of range => Returns a HttpStatusCode.BadRequest
        /// </summary>
        /// <param name="id">identifier (int) of the entity</param>
        /// <returns>the entity value (string) at the identifer</returns>
        public string Get(int id)
        {
            // Get the List of Entities from the Cache
            people = (List<string>) cache.Get("People");

            // Don't Process if ID is out of Range 0-entity,count
            if (id >= people.Count || id < 0)
            {
                // Make a bad response and throw it
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                throw new HttpResponseException(message);
            }

            return people[id];
        } // end of method

        /// <summary>
        ///     Post
        ///     Adds a new entity to the collection
        ///     POST api/values
        /// </summary>
        /// <param name="value">the value (string) of the entity</param>
        [HttpPost]
        public void Post(People model)
        {
            // Get the List of Entities from the Cache
            people = (List<string>) cache.Get("People");

            // Add the entity
            people.Add(model.Name);
        } // end of method

        /// <summary>
        ///     Put
        ///     Replaces the entity with an identifier with a new value.
        ///     PUT api/values/5
        ///     IF index out of range => Returns a HttpStatusCode.BadRequest
        /// </summary>
        /// <param name="id">identifier (int) of the entity to replace</param>
        /// <param name="value">value (string) to replace the existing entity value</param>
        public void Put(int id, People model)
        {
            // Get the List of Entities from the Cache
            people = (List<string>) cache.Get("People");

            // Don't Process if ID is out of Range 0-entity,count
            if (id >= people.Count || id < 0)
            {
                // Make a bad response and throw it
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                throw new HttpResponseException(message);
            }

            //Update the Entity
            people[id] = model.Name;
        } // end of method

        /// <summary>
        ///     Delete
        ///     Deletes an entity based on the id
        ///     DELETE api/values/5
        ///     IF index out of range => Returns a HttpStatusCode.BadRequest
        /// </summary>
        /// <param name="id">identifier (id) of the entity</param>
        public void Delete(int id)
        {
            // Get the List of Entities from the Cache
            people = (List<string>) cache.Get("People");

            // Don't Process if ID is out of Range 0-entity,count
            if (id >= people.Count || id < 0)
            {
                // Make a bad response and throw it
                var message = new HttpResponseMessage(HttpStatusCode.BadRequest);
                throw new HttpResponseException(message);
            }

            // Delete the Entity
            people.RemoveAt(id);
        } // end of method
    } // end of class

    public class People
    {
        public string Name { get; set; }
    }
} // end of namespace