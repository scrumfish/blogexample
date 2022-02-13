using blog.data.Extensions;
using blog.data.Services;
using blog.objects;
using blog.objects.Extensions;
using blog.objects.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blog.data
{
    public class BlogData : IBlogData
    {
        private IConfiguration configuration;
        private string databaseId;
        private string key;
        private string uri;
        private readonly string collection = "blogs";

        public BlogData(IConfiguration config)
        {
            configuration = config;
            databaseId = configuration.GetSection("Database").GetSection("DatabaseId").Value;
            key = configuration.GetSection("Database").GetSection("Key").Value;
            uri = configuration.GetSection("Database").GetSection("Endpoint").Value;
            using (var client = new CosmosClient(uri, key))
            {
                client.CreateDatabaseIfNotExistsAsync(databaseId);
            }
        }

        public string Add(BlogEntry entry)
        {
            var blog = entry.ToBlog();
            blog.fragment = entry.article.ToFragment();
            blog.publishedAt = DateTime.UtcNow;
            blog.id = ShortId.NewId();
            using (var client = new CosmosClient(uri, key))
            {
                var container = client.GetContainer(databaseId, collection);
                container.Database.CreateContainerIfNotExistsAsync(collection, "/id").Wait();
                container.CreateItemAsync(blog, new PartitionKey(blog.id)).Wait();
            }
            return blog.id;
        }

        public IList<Title> GetTitles()
        {
            using (var client = new CosmosClient(uri, key))
            {
                var container = client.GetContainer(databaseId, collection);
                var response = new List<Title>();

                var query = container.GetItemQueryIterator<Title>();
                while (query.HasMoreResults)
                {
                    var result = query.ReadNextAsync().Result;
                    response.AddRange(result.Select(r => WithFragment(r)).ToList());
                }
                return response;
            }

        }

        private Title WithFragment(Title input)
        {
            if (string.IsNullOrWhiteSpace(input.fragment))
            {
                var blog = Get(input.id);
                if (blog == null)
                {
                    throw new NullReferenceException(nameof(input));
                }
                blog.fragment = blog.article.ToFragment();
                UpdateEntry(blog);
                return blog.ToTitle();
            }
            return input;
        }

        public void UpdateEntry(Blog entry)
        {
            using (var client = new CosmosClient(uri, key))
            {
                var container = client.GetContainer(databaseId, collection);
                container.UpsertItemAsync(entry, new PartitionKey(entry.id)).Wait();
            }
        }

        public Blog? Get(string id)
        {
            using (var client = new CosmosClient(uri, key))
            {
                var container = client.GetContainer(databaseId, collection);
                if (id == "latest")
                {
                    var query = new QueryDefinition("select top 1 * from blogs b order by b.publishedAt desc");
                    using (var feed = container.GetItemQueryIterator<Blog>(query))
                    {
                        while (feed.HasMoreResults)
                        {
                            return feed.ReadNextAsync().Result?.FirstOrDefault();
                        }
                    }
                }
                else
                {
                    var response = container.ReadItemAsync<Blog>(id, new PartitionKey(id)).Result;
                    return response;
                }
            }
            return null;
        }
    }
}