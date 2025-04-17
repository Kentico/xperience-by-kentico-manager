using System.ComponentModel.DataAnnotations;

namespace Xperience.Manager.Configuration
{
    /// <summary>
    /// Represents the headless options specified in the appsettings.json. See
    /// <see href="https://docs.xperience.io/xp/developers-and-admins/configuration/headless-channel-management#Headlesschannelmanagement-ConfiguretheheadlessAPI"/>.
    /// </summary>
    public class CmsHeadlessConfiguration
    {
        [Display(Description = "Specifies whether GraphQL API endpoints are enabled.")]
        /// <summary>
        /// Specifies whether GraphQL API endpoints are enabled.
        /// </summary>
        public bool Enable { get; set; } = true;


        [Display(Description = "Specifies whether GraphQL introspection is enabled (__schema queries and GUI tools for exploring the " +
            "schema).")]
        /// <summary>
        /// Specifies whether GraphQL introspection is enabled (__schema queries and GUI tools for exploring the schema).
        /// See <see href="https://graphql.org/learn/introspection/"/>.
        /// </summary>
        public bool AllowIntrospection { get; set; }


        [Display(Description = "The slug used in channel endpoint URLs. You need to include the leading slash ('/').")]
        /// <summary>
        /// The slug used in channel endpoint URLs. You need to include the leading slash ('/').
        /// </summary>
        public string GraphQlEndpointPath { get; set; } = "/graphql";


        [Display(Description = "The domains that are allowed origins for CORS (Cross-Origin Resource Sharing).")]
        /// <summary>
        /// The domains that are allowed origins for CORS (Cross-Origin Resource Sharing). See
        /// <see href="https://docs.xperience.io/xp/developers-and-admins/development/content-retrieval/retrieve-headless-content"/>.
        /// </summary>
        public string? CorsAllowedOrigins { get; set; }


        [Display(Description = "The HTTP headers that are allowed for content retrieval requests. If not set, all headers are allowed by " +
            "default.")]
        /// <summary>
        /// The HTTP headers that are allowed for content retrieval requests. If not set, all headers are allowed by default.
        /// See <see href="https://docs.xperience.io/xp/developers-and-admins/development/content-retrieval/retrieve-headless-content"/>.
        /// </summary>
        public string? CorsAllowedHeaders { get; set; }


        [Display(Description = "The caching behavior for headless content.")]
        /// <summary>
        /// Contains options which allow you to customize caching behavior for headless content. The default values provide optimal
        /// caching performance for most projects.
        /// </summary>
        public CachingOptions Caching { get; set; } = new CachingOptions();


        public class CachingOptions
        {
            [Display(Name = "Caching::Enable", Description = "Specifies whether caching is enabled.")]
            /// <summary>
            /// Specifies whether caching is enabled.
            /// </summary>
            public bool Enable { get; set; } = true;


            [Display(Name = "Caching::UseRequestCacheControlHeaders", Description = "Specifies whether caching respects the Cache-Control " +
                "header of individual requests.")]
            /// <summary>
            /// Specifies whether caching respects the Cache-Control header of individual requests.
            /// </summary>
            public bool UseRequestCacheControlHeaders { get; set; } = true;


            [Display(Name = "Caching::AddResponseCacheControlHeaders", Description = "Specifies whether caching sets the Cache-Control " +
                "response header.")]
            /// <summary>
            /// Specifies whether caching sets the Cache-Control response header.
            /// </summary>
            public bool AddResponseCacheControlHeaders { get; set; } = true;


            [Display(Name = "Caching::UseCacheDependencies", Description = "Specifies whether caching uses cache dependencies to flush the " +
                "cache when data in a cached response changes.")]
            /// <summary>
            /// Specifies whether caching uses cache dependencies to flush the cache when data in a cached response changes.
            /// </summary>
            public bool UseCacheDependencies { get; set; } = true;


            [Display(Name = "Caching::AbsoluteExpiration", Description = "The maximum expiration time for a cache entry in minutes.")]
            /// <summary>
            /// The maximum expiration time for a cache entry in minutes.
            /// </summary>
            public int AbsoluteExpiration { get; set; } = 720;


            [Display(Name = "Caching::SlidingExpiration", Description = "The sliding expiration date for cache entries in minutes. Cannot " +
                "exceed the value of AbsoluteExpiration.")]
            /// <summary>
            /// The sliding expiration date for cache entries in minutes. Cannot exceed the value of <see cref="AbsoluteExpiration"/>.
            /// </summary>
            public int SlidingExpiration { get; set; } = 60;


            [Display(Name = "Caching::SizeLimit", Description = "The maximum size for in-memory cache in bytes.")]
            /// <summary>
            /// The maximum size for in-memory cache in bytes.
            /// </summary>
            public long SizeLimit { get; set; } = 100_000_000;
        }
    }
}
