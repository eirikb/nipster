using System;
using Microsoft.WindowsAzure.Storage.Table;
using Nipster.Util;

namespace Nipster
{
    public abstract class Entity : TableEntity
    {
        protected Entity(string partitionKey, string id)
        {
            PartitionKey = partitionKey;
            id = id.Trim().ToLower();
            RowKey = id.ToRowKey();
            Name = id;
        }

        protected Entity()
        {
        }

        public string Name { get; set; }
    }

    public class NpmEntity : Entity
    {
        public NpmEntity(string id) : base("npm", id)
        {
        }

        public NpmEntity()
        {
        }

        public string GitHubRepoUrl { get; set; }
        public string Description { get; set; }
        public string Keywords { get; set; }
        public DateTime? Modified { get; set; }
        public string Author { get; set; }
        public string AuthorUrl { get; set; }
    }

    public class GitHubEntity : Entity
    {
        public GitHubEntity(string id) : base("github", id)
        {
        }

        public GitHubEntity()
        {
        }

        public int Forks { get; set; }
        public int Stargazers { get; set; }
        public int Watchers { get; set; }
        public string GitHubETag { get; set; }
        public bool Found { get; set; }
        public string StatusCode { get; set; }
    }
}