using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace Blog
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");

            var loggerFactory = LoggerFactory.Create(builder => { builder.AddConsole(); });

            var context = new MyDbContext(loggerFactory);
            context.Database.EnsureCreated();
            InitializeData(context);


            var firstQueryResult = context.BlogComments
                .GroupBy(comment => comment.UserName)
                .Select(group => new
                {
                    UserName = group.Key,
                    AmountOfComments = group.Count()
                }).ToList();


            var secondQueryResult = context.BlogPosts
                .Where(post => post.Comments.Any())
                .Select(post => new
                {
                    PostTitle = post.Title,
                    LastCommentDate = post.Comments
                        .OrderByDescending(comment => comment.CreatedDate)
                        .Select(comment => comment.CreatedDate)
                        .FirstOrDefault()
                })
                .OrderByDescending(post => post.LastCommentDate)
                .ToList();


            var thirdQueryResult = context.BlogPosts
                .Where(post => post.Comments.Any())
                .Select(post => new
                {
                    Post = post,
                    LastComment = post.Comments.OrderByDescending(comment => comment.CreatedDate).FirstOrDefault()
                })
                .GroupBy(post => post.LastComment.UserName)
                .Select(group => new
                {
                    UserName = group.Key,
                    AmuntOfLastComments = group.Count()
                })
                .ToList();

            Console.WriteLine("\n1) How many comments each user left:");
            foreach (var result in firstQueryResult)
            {
                Console.WriteLine($" Username: {result.UserName}, AmountOfComments: {result.AmountOfComments}");
            }

            Console.WriteLine("\n2) Posts ordered by date of last comment:");
            foreach (var result in secondQueryResult)
            {
                Console.WriteLine($" PostTitle: {result.PostTitle}, LastCommentDate: {result.LastCommentDate}");
            }


            Console.WriteLine("\n3) How many last comments each user left:");
            foreach (var result in thirdQueryResult)
            {
                Console.WriteLine($" UserName: {result.UserName}, AmountOfLastComments: {result.AmuntOfLastComments}");
            }
        }

        private static void InitializeData(MyDbContext context)
        {
            context.BlogPosts.Add(new BlogPost("Post1")
            {
                Comments = new List<BlogComment>()
                {
                    new BlogComment("1", new DateTime(2020, 3, 2), "Petr"),
                    new BlogComment("2", new DateTime(2020, 3, 4), "Elena"),
                    new BlogComment("8", new DateTime(2020, 3, 5), "Ivan"),
                }
            });
            context.BlogPosts.Add(new BlogPost("Post2")
            {
                Comments = new List<BlogComment>()
                {
                    new BlogComment("3", new DateTime(2020, 3, 5), "Elena"),
                    new BlogComment("4", new DateTime(2020, 3, 6), "Ivan"),
                }
            });
            context.BlogPosts.Add(new BlogPost("Post3")
            {
                Comments = new List<BlogComment>()
                {
                    new BlogComment("5", new DateTime(2020, 2, 7), "Ivan"),
                    new BlogComment("6", new DateTime(2020, 2, 9), "Elena"),
                    new BlogComment("7", new DateTime(2020, 2, 10), "Ivan"),
                    new BlogComment("9", new DateTime(2020, 2, 14), "Petr"),
                }
            });
            context.SaveChanges();
        }
    }
}