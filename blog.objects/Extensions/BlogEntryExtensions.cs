using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace blog.objects.Extensions
{
    public static class BlogEntryExtensions
    {
        public static Blog ToBlog(this BlogEntry source)
        {
            return new Blog
            {
                title = source.title,
                article = source.article
            };
        }
    }
}