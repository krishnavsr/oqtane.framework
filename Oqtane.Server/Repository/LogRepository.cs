using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Oqtane.Models;

namespace Oqtane.Repository
{
    public class LogRepository : ILogRepository
    {
        private readonly IDbContextFactory<TenantDBContext> _dbContextFactory;

        public LogRepository(IDbContextFactory<TenantDBContext> dbContextFactory)
        {
            _dbContextFactory = dbContextFactory;
        }

        public IEnumerable<Log> GetLogs(int siteId, string level, string function, int rows)
        {
            using var db = _dbContextFactory.CreateDbContext();
            if (level == null)
            {
                if (function == null)
                {
                    return db.Log.Where(item => item.SiteId == siteId).
                        OrderByDescending(item => item.LogDate).Take(rows).ToList();
                }

                return db.Log.Where(item => item.SiteId == siteId && item.Function == function).
                    OrderByDescending(item => item.LogDate).Take(rows).ToList();
            }

            if (function == null)
            {
                return db.Log.Where(item => item.SiteId == siteId && item.Level == level)
                    .OrderByDescending(item => item.LogDate).Take(rows).ToList();
            }

            return db.Log.Where(item => item.SiteId == siteId && item.Level == level && item.Function == function)
                .OrderByDescending(item => item.LogDate).Take(rows).ToList();
        }

        public Log GetLog(int logId)
        {
            using var db = _dbContextFactory.CreateDbContext();
            return db.Log.Find(logId);
        }

        public void AddLog(Log log)
        {
            using var db = _dbContextFactory.CreateDbContext();
            db.Log.Add(log);
            db.SaveChanges();
        }

        public int DeleteLogs(int siteId, int age)
        {
            using var db = _dbContextFactory.CreateDbContext();
            // delete logs in batches of 100 records
            var count = 0;
            var purgedate = DateTime.UtcNow.AddDays(-age);
            var logs = db.Log.Where(item => item.SiteId == siteId && item.Level != "Error" && item.LogDate < purgedate)
                .OrderBy(item => item.LogDate).Take(100).ToList();
            while (logs.Count > 0)
            {
                count += logs.Count;
                db.Log.RemoveRange(logs);
                db.SaveChanges();
                logs = db.Log.Where(item => item.SiteId == siteId && item.Level != "Error" && item.LogDate < purgedate)
                    .OrderBy(item => item.LogDate).Take(100).ToList();
            }
            return count;
        }
    }
}
