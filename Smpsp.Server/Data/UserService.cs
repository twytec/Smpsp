using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;
using System.Security.Principal;

namespace Smpsp.Server.Data
{
    public class UserService
    {
        public IEnumerable<User> GetAllUsers() => _usersByEmail.Values;

        private readonly IDbContextFactory<AppDbContext> _dbF;
        private readonly TranslationService _ts;
        private readonly ILogger<UserService> _log;
        private readonly ConcurrentDictionary<string, User> _usersByEmail = [];
        private readonly ConcurrentDictionary<string, User> _usersById = [];

        public UserService(IDbContextFactory<AppDbContext> dbF, TranslationService ts, ILogger<UserService> log)
        {
            _dbF = dbF;
            _ts = ts;
            _log = log;

            using var db = _dbF.CreateDbContext();
            var ru = db.Records.Where(x => x.RecordType == DataRecordType.User);
            if (ru.Any())
            {
                foreach (var r in ru)
                {
                    if (Helpers.Json.TryGetModel<User>(r.Data, out var u))
                    {
                        _usersByEmail[u.EMail] = u;
                        _usersById[u.Id] = u;
                    }
                }
            }
        }

        public User? GetUserByEMail(string email)
        {
            _usersByEmail.TryGetValue(email, out var user);
            return user;
        }

        public User? GetUserById(string id)
        {
            _usersById.TryGetValue(id, out var user);
            return user;
        }

        public User? GetUserBasedIdentity(IIdentity? identity)
        {
            if (identity is IIdentity i && i.Name is string n && GetUserByEMail(n) is User u && u.Active)
            {
                return u;
            }
            return null;
        }

        public async Task AddUserAsync(User user)
        {
            if (_usersByEmail.ContainsKey(user.EMail))
            {
                throw new Exception(_ts.DefaultTranslations.UserAlreadyExists);
            }

            try
            {
                user.Id = Guid.NewGuid().ToString();
                user.Active = true;
                using var db = _dbF.CreateDbContext();

                DataRecord r = new()
                {
                    Data = Helpers.Json.GetJson(user),
                    RecordType = DataRecordType.User,
                    Id = user.Id,
                    UserId = user.Id,
                };
                await db.Records.AddAsync(r);
                await db.SaveChangesAsync();

                _usersByEmail[user.EMail] = user;
                _usersById[user.Id] = user;
                return;
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, nameof(AddUserAsync));
                throw new Exception(_ts.DefaultTranslations.UnknownError);
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            try
            {
                if (_usersById.ContainsKey(user.Id))
                {
                    using var db = _dbF.CreateDbContext();
                    var ava = await db.Records.FirstOrDefaultAsync(x => x.Id == user.Id && x.RecordType == DataRecordType.User);
                    if (ava is not null)
                    {
                        ava.UpdateData(user);
                        db.Records.Update(ava);
                        await db.SaveChangesAsync();

                        _usersByEmail[user.EMail] = user;
                        _usersById[user.Id] = user;
                        return;
                    }
                }

                throw new Exception(_ts.DefaultTranslations.UserDoesNotExist);
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, nameof(UpdateUserAsync));
                throw new Exception(_ts.DefaultTranslations.UnknownError);
            }
        }

        public async Task DeleteUserAsync(string id)
        {
            try
            {
                if (_usersById.TryRemove(id, out var u))
                {
                    using var db = _dbF.CreateDbContext();
                    var ava = await db.Records.FirstOrDefaultAsync(x => x.Id == id && x.RecordType == DataRecordType.User);
                    if (ava is not null)
                    {
                        db.Records.Remove(ava);
                        await db.SaveChangesAsync();

                        _usersByEmail.TryRemove(u.EMail, out _);
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                _log.LogWarning(ex, nameof(DeleteUserAsync));
                throw new Exception(_ts.DefaultTranslations.UnknownError);
            }
        }
    }
}
