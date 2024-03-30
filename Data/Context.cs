﻿using LoginAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LoginAPI.DbContext
{
    public class Context : IdentityDbContext<User>
    {
        public Context(DbContextOptions<Context> options) :base(options)
        {
            
        }
    }
}
