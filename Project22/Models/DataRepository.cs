using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public Account GetAccount(string mobileNumber, string pin)
        {
            return new Account { Id = 0, IsActive = true, LastLogin = DateTime.Now, Mobile = mobileNumber, Tokens = 100 };
        }

        public IEnumerable<Session> GetSession(string phoneNumber)
        {
            var account = Accounts.FirstOrDefault(a => a.Mobile == phoneNumber);
            if(account != null)
            {
                return GetSessions(account.Id);
            }
            return Enumerable.Empty<Session>();
        }
            
        public IEnumerable<Session> GetSessions(int accountId) => Sessions.Where(s => s.AccountId == accountId);

        public Session GetSession(int accountId, int sessionId) => Sessions.Include(s=>s.Tokens).FirstOrDefault(s => s.AccountId == accountId && s.Id == sessionId);

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
                    SaveChanges();
                }
                catch (DbUpdateConcurrencyException e)
                {
                    GetCurrrentSessionInfo(session, e);
                }
            }
        }

        public Token GetToken(int accountId, int tokenId) => Tokens.Include(t => t.Session).FirstOrDefault(t => t.Id == tokenId && t.Session.AccountId == accountId);

        public void CreateToken(int accountId, int sessionId, Token token)
        {
            var session = GetSession(accountId, sessionId);
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
