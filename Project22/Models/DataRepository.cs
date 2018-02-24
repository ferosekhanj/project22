using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Project22.Models
{
    public class DataRepository : DbContext
    {
        public DbSet<Account> Accounts { get; set; }

        public DbSet<Session> Sessions { get; set; }

        public DbSet<Token> Tokens { get; set; }

        public DataRepository(DbContextOptions<DataRepository> options):
            base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Account>().HasIndex(c => c.Mobile).IsUnique();
        }

        public void CreateAccount(Account account)
        {
            Add(account);
            SaveChanges();
        }

        public Account GetAccount(string mobileNumber)
        {
            return Accounts.FirstOrDefault(a => a.Mobile == mobileNumber);
        }

        public (Account a,IEnumerable<Session> s) GetSession(string mobileNumber)
        {
            var account = Accounts.FirstOrDefault(a => a.Mobile == mobileNumber);
            if(account != null)
            {
                return (account, GetAvailableSessions(account.Id));
            }
            return (null, Enumerable.Empty<Session>());
        }

        public IEnumerable<Session> GetAvailableSessions(int accountId) => Sessions.Where(s => s.AccountId == accountId && s.TokenCount<s.MaxTokens);

        public IEnumerable<Session> GetSessions(int accountId) => Sessions.Where(s => s.AccountId == accountId);

        public Session GetSession(int sessionId) => Sessions.Include(s=>s.Tokens).FirstOrDefault(s => s.Id == sessionId);

        public void CreateSession(Session session)
        {
            Add(session);
            SaveChanges();
        }
        public void UpdateSession(Session session)
        {
            Update(session);

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    // Make sure maxtokens is not less than issued tokens
                    session.MaxTokens = session.MaxTokens < session.TokenCount ? session.TokenCount : session.MaxTokens;
                    SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    GetCurrrentSessionInfo(session, e);
                }
            }
        }

        public Token GetToken(int tokenId) => Tokens.Include(t => t.Session).FirstOrDefault(t => t.Id == tokenId);

        public void CreateToken(int sessionId, Token token)
        {
            var session = GetSession(sessionId);
            token.Number = session.TokenCount + 1;
            session.TokenCount = session.TokenCount + 1;

            token.Session = null;
            session.Tokens.Add(token);

            for (int i = 0; i < 5; i++)
            {
                try
                {
                    SaveChanges();
                    break;
                }
                catch (DbUpdateConcurrencyException e)
                {
                    GetNewTokenNumber(session, token, e);
                }
            }
        }
        private void GetCurrrentSessionInfo(Session session, DbUpdateConcurrencyException e)
        {
            foreach (var entry in e.Entries)
            {
                if (entry.Entity is Session conflictedSession)
                {
                    var databaseEntity = Sessions.AsNoTracking().Single(p => p.Id == conflictedSession.Id);
                    var databaseEntry = Entry(databaseEntity);
                    var databaseValue = databaseEntry.Property(nameof(Session.TokenCount)).CurrentValue;
                    entry.Property(nameof(Session.TokenCount)).CurrentValue = databaseEntry.Property(nameof(Session.TokenCount)).CurrentValue;
                    entry.Property(nameof(Session.TokenCount)).OriginalValue = databaseEntry.Property(nameof(Session.TokenCount)).CurrentValue;
                    session.TokenCount = (int)databaseEntry.Property(nameof(Session.TokenCount)).CurrentValue;
                }
            }
        }

        private void GetNewTokenNumber(Session session, Token token, DbUpdateConcurrencyException e)
        {
            foreach (var entry in e.Entries)
            {
                if (entry.Entity is Session conflictedSession)
                {
                    var databaseEntity = Sessions.AsNoTracking().Single(p => p.Id == conflictedSession.Id);
                    var databaseEntry = Entry(databaseEntity);
                    var proposedValue = entry.Property(nameof(Session.TokenCount)).CurrentValue;
                    var originalValue = entry.Property(nameof(Session.TokenCount)).OriginalValue;
                    var databaseValue = databaseEntry.Property(nameof(Session.TokenCount)).CurrentValue;
                    Console.WriteLine($"Conflict =>Proposed {proposedValue} Orginal {originalValue} DB {databaseValue}");
                    var newTokenNumber = ((int)entry.Property(nameof(Session.TokenCount)).CurrentValue) + 1;
                    entry.Property(nameof(Session.TokenCount)).CurrentValue = newTokenNumber;
                    entry.Property(nameof(Session.TokenCount)).OriginalValue = newTokenNumber - 1;
                    session.TokenCount = newTokenNumber;
                    token.Number = newTokenNumber;
                }
            }
        }
    }
}
