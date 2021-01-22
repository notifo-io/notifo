// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using MongoDB.Driver;
using Notifo.Infrastructure.MongoDb;

namespace Notifo.Identity.MongoDb
{
    public sealed class MongoDbUserStore :
        MongoDbRepository<MongoDbUser>,
        IUserAuthenticationTokenStore<NotifoUser>,
        IUserAuthenticatorKeyStore<NotifoUser>,
        IUserClaimStore<NotifoUser>,
        IUserEmailStore<NotifoUser>,
        IUserLockoutStore<NotifoUser>,
        IUserLoginStore<NotifoUser>,
        IUserPasswordStore<NotifoUser>,
        IUserPhoneNumberStore<NotifoUser>,
        IUserRoleStore<NotifoUser>,
        IUserSecurityStampStore<NotifoUser>,
        IUserTwoFactorStore<NotifoUser>,
        IUserTwoFactorRecoveryCodeStore<NotifoUser>,
        IUserFactory,
        IQueryableUserStore<NotifoUser>
    {
        private const string InternalLoginProvider = "[AspNetUserStore]";
        private const string AuthenticatorKeyTokenName = "AuthenticatorKey";
        private const string RecoveryCodeTokenName = "RecoveryCodes";

        static MongoDbUserStore()
        {
            BsonClassMap.RegisterClassMap<Claim>(cm =>
            {
                cm.MapConstructor(typeof(Claim).GetConstructors()
                    .First(x =>
                    {
                        var parameters = x.GetParameters();

                        return parameters.Length == 2 &&
                            parameters[0].Name == "type" &&
                            parameters[0].ParameterType == typeof(string) &&
                            parameters[1].Name == "value" &&
                            parameters[1].ParameterType == typeof(string);
                    }))
                    .SetArguments(new[]
                    {
                        nameof(Claim.Type),
                        nameof(Claim.Value)
                    });

                cm.MapMember(x => x.Type);
                cm.MapMember(x => x.Value);
            });

            BsonClassMap.RegisterClassMap<UserLogin>(cm =>
            {
                cm.MapConstructor(typeof(UserLogin).GetConstructors()
                    .First(x =>
                    {
                        var parameters = x.GetParameters();

                        return parameters.Length == 3;
                    }))
                    .SetArguments(new[]
                    {
                        nameof(UserLogin.LoginProvider),
                        nameof(UserLogin.ProviderKey),
                        nameof(UserLogin.ProviderDisplayName)
                    });

                cm.AutoMap();
            });

            BsonClassMap.RegisterClassMap<IdentityUserToken<string>>(cm =>
            {
                cm.AutoMap();

                cm.UnmapMember(x => x.UserId);
            });

            BsonClassMap.RegisterClassMap<IdentityUser<string>>(cm =>
            {
                cm.AutoMap();

                cm.MapMember(x => x.Id)
                    .SetSerializer(new StringSerializer(BsonType.ObjectId));

                cm.MapMember(x => x.AccessFailedCount)
                    .SetIgnoreIfDefault(true);

                cm.MapMember(x => x.EmailConfirmed)
                    .SetIgnoreIfDefault(true);

                cm.MapMember(x => x.LockoutEnd)
                    .SetElementName("LockoutEndDateUtc").SetIgnoreIfNull(true);

                cm.MapMember(x => x.LockoutEnabled)
                    .SetIgnoreIfDefault(true);

                cm.MapMember(x => x.PasswordHash)
                    .SetIgnoreIfNull(true);

                cm.MapMember(x => x.PhoneNumber)
                    .SetIgnoreIfNull(true);

                cm.MapMember(x => x.PhoneNumberConfirmed)
                    .SetIgnoreIfDefault(true);

                cm.MapMember(x => x.SecurityStamp)
                    .SetIgnoreIfNull(true);

                cm.MapMember(x => x.TwoFactorEnabled)
                    .SetIgnoreIfDefault(true);
            });
        }

        public MongoDbUserStore(IMongoDatabase database)
            : base(database)
        {
        }

        protected override string CollectionName()
        {
            return "Identity_Users";
        }

        protected override Task SetupCollectionAsync(IMongoCollection<MongoDbUser> collection, CancellationToken ct)
        {
            return collection.Indexes.CreateManyAsync(new[]
            {
                new CreateIndexModel<MongoDbUser>(
                    IndexKeys
                        .Ascending("Logins.LoginProvider")
                        .Ascending("Logins.ProviderKey")),
                new CreateIndexModel<MongoDbUser>(
                    IndexKeys
                        .Ascending(x => x.NormalizedUserName),
                    new CreateIndexOptions
                    {
                        Unique = true
                    }),
                new CreateIndexModel<MongoDbUser>(
                    IndexKeys
                        .Ascending(x => x.NormalizedEmail),
                    new CreateIndexOptions
                    {
                        Unique = true
                    })
            }, ct);
        }

        protected override MongoCollectionSettings CollectionSettings()
        {
            return new MongoCollectionSettings { WriteConcern = WriteConcern.WMajority };
        }

        public void Dispose()
        {
        }

        public IQueryable<NotifoUser> Users
        {
            get { return Collection.AsQueryable(); }
        }

        public NotifoUser CreateUser(string email)
        {
            return new MongoDbUser { Email = email, UserName = email, Id = null };
        }

        public async Task<NotifoUser> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            return await Collection.Find(x => x.Id == userId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<NotifoUser> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
        {
            var result = await Collection.Find(x => x.NormalizedEmail == normalizedEmail).FirstOrDefaultAsync(cancellationToken);

            return result;
        }

        public async Task<NotifoUser> FindByNameAsync(string normalizedUserName, CancellationToken cancellationToken)
        {
            var result = await Collection.Find(x => x.NormalizedEmail == normalizedUserName).FirstOrDefaultAsync(cancellationToken);

            return result;
        }

        public async Task<NotifoUser> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            var result = await Collection.Find(x => x.Logins.Any(y => y.LoginProvider == loginProvider && y.ProviderKey == providerKey)).FirstOrDefaultAsync(cancellationToken);

            return result;
        }

        public async Task<IList<NotifoUser>> GetUsersForClaimAsync(Claim claim, CancellationToken cancellationToken)
        {
            var result = await Collection.Find(x => x.Claims.Any(y => y.Type == claim.Type && y.Value == claim.Value)).ToListAsync(cancellationToken);

            return result.OfType<NotifoUser>().ToList();
        }

        public async Task<IList<NotifoUser>> GetUsersInRoleAsync(string roleName, CancellationToken cancellationToken)
        {
            var result = await Collection.Find(x => x.Roles.Contains(roleName)).ToListAsync(cancellationToken);

            return result.OfType<NotifoUser>().ToList();
        }

        public async Task<IdentityResult> CreateAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            user.Id = ObjectId.GenerateNewId().ToString();

            await Collection.InsertOneAsync((MongoDbUser)user, null, cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> UpdateAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            await Collection.ReplaceOneAsync(x => x.Id == user.Id, (MongoDbUser)user, cancellationToken: cancellationToken);

            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            await Collection.DeleteOneAsync(x => x.Id == user.Id, null, cancellationToken);

            return IdentityResult.Success;
        }

        public Task<string> GetUserIdAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.Id;

            return Task.FromResult(result);
        }

        public Task<string> GetUserNameAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.UserName;

            return Task.FromResult(result);
        }

        public Task<string> GetNormalizedUserNameAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.NormalizedUserName;

            return Task.FromResult(result);
        }

        public Task<string> GetPasswordHashAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.PasswordHash;

            return Task.FromResult(result);
        }

        public Task<IList<string>> GetRolesAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = ((MongoDbUser)user).Roles.ToList();

            return Task.FromResult<IList<string>>(result);
        }

        public Task<bool> IsInRoleAsync(NotifoUser user, string roleName, CancellationToken cancellationToken)
        {
            var result = ((MongoDbUser)user).Roles.Contains(roleName);

            return Task.FromResult(result);
        }

        public Task<IList<UserLoginInfo>> GetLoginsAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = ((MongoDbUser)user).Logins.Select(x => new UserLoginInfo(x.LoginProvider, x.ProviderKey, x.ProviderDisplayName)).ToList();

            return Task.FromResult<IList<UserLoginInfo>>(result);
        }

        public Task<string> GetSecurityStampAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.SecurityStamp;

            return Task.FromResult(result);
        }

        public Task<string> GetEmailAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.Email;

            return Task.FromResult(result);
        }

        public Task<bool> GetEmailConfirmedAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.EmailConfirmed;

            return Task.FromResult(result);
        }

        public Task<string> GetNormalizedEmailAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.NormalizedEmail;

            return Task.FromResult(result);
        }

        public Task<IList<Claim>> GetClaimsAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = ((MongoDbUser)user).Claims;

            return Task.FromResult<IList<Claim>>(result);
        }

        public Task<string> GetPhoneNumberAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.PhoneNumber;

            return Task.FromResult(result);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.PhoneNumberConfirmed;

            return Task.FromResult(result);
        }

        public Task<bool> GetTwoFactorEnabledAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.TwoFactorEnabled;

            return Task.FromResult(result);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.LockoutEnd;

            return Task.FromResult(result);
        }

        public Task<int> GetAccessFailedCountAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.AccessFailedCount;

            return Task.FromResult(result);
        }

        public Task<bool> GetLockoutEnabledAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = user.LockoutEnabled;

            return Task.FromResult(result);
        }

        public Task<string> GetTokenAsync(NotifoUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            var result = ((MongoDbUser)user).GetToken(loginProvider, name)!;

            return Task.FromResult(result);
        }

        public Task<string> GetAuthenticatorKeyAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = ((MongoDbUser)user).GetToken(InternalLoginProvider, AuthenticatorKeyTokenName)!;

            return Task.FromResult(result);
        }

        public Task<bool> HasPasswordAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = !string.IsNullOrWhiteSpace(user.PasswordHash);

            return Task.FromResult(result);
        }

        public Task<int> CountCodesAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            var result = ((MongoDbUser)user).GetToken(InternalLoginProvider, RecoveryCodeTokenName)?.Split(';').Length ?? 0;

            return Task.FromResult(result);
        }

        public Task SetUserNameAsync(NotifoUser user, string userName, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).UserName = userName;

            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(NotifoUser user, string normalizedName, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).NormalizedUserName = normalizedName;

            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(NotifoUser user, string passwordHash, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).PasswordHash = passwordHash;

            return Task.CompletedTask;
        }

        public Task AddToRoleAsync(NotifoUser user, string roleName, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).AddRole(roleName);

            return Task.CompletedTask;
        }

        public Task RemoveFromRoleAsync(NotifoUser user, string roleName, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).RemoveRole(roleName);

            return Task.CompletedTask;
        }

        public Task AddLoginAsync(NotifoUser user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).AddLogin(login);

            return Task.CompletedTask;
        }

        public Task RemoveLoginAsync(NotifoUser user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).RemoveLogin(loginProvider, providerKey);

            return Task.CompletedTask;
        }

        public Task SetSecurityStampAsync(NotifoUser user, string stamp, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).SecurityStamp = stamp;

            return Task.CompletedTask;
        }

        public Task SetEmailAsync(NotifoUser user, string email, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).Email = email;

            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(NotifoUser user, bool confirmed, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).EmailConfirmed = confirmed;

            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(NotifoUser user, string normalizedEmail, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).NormalizedEmail = normalizedEmail;

            return Task.CompletedTask;
        }

        public Task AddClaimsAsync(NotifoUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).AddClaims(claims);

            return Task.CompletedTask;
        }

        public Task ReplaceClaimAsync(NotifoUser user, Claim claim, Claim newClaim, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).ReplaceClaim(claim, newClaim);

            return Task.CompletedTask;
        }

        public Task RemoveClaimsAsync(NotifoUser user, IEnumerable<Claim> claims, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).RemoveClaims(claims);

            return Task.CompletedTask;
        }

        public Task SetPhoneNumberAsync(NotifoUser user, string phoneNumber, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).PhoneNumber = phoneNumber;

            return Task.CompletedTask;
        }

        public Task SetPhoneNumberConfirmedAsync(NotifoUser user, bool confirmed, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).PhoneNumberConfirmed = confirmed;

            return Task.CompletedTask;
        }

        public Task SetTwoFactorEnabledAsync(NotifoUser user, bool enabled, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).TwoFactorEnabled = enabled;

            return Task.CompletedTask;
        }

        public Task SetLockoutEndDateAsync(NotifoUser user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).LockoutEnd = lockoutEnd?.UtcDateTime;

            return Task.CompletedTask;
        }

        public Task<int> IncrementAccessFailedCountAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).AccessFailedCount++;

            return Task.FromResult(((MongoDbUser)user).AccessFailedCount);
        }

        public Task ResetAccessFailedCountAsync(NotifoUser user, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).AccessFailedCount = 0;

            return Task.CompletedTask;
        }

        public Task SetLockoutEnabledAsync(NotifoUser user, bool enabled, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).LockoutEnabled = enabled;

            return Task.CompletedTask;
        }

        public Task SetTokenAsync(NotifoUser user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).ReplaceToken(loginProvider, name, value);

            return Task.CompletedTask;
        }

        public Task RemoveTokenAsync(NotifoUser user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).RemoveToken(loginProvider, name);

            return Task.CompletedTask;
        }

        public Task SetAuthenticatorKeyAsync(NotifoUser user, string key, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).ReplaceToken(InternalLoginProvider, AuthenticatorKeyTokenName, key);

            return Task.CompletedTask;
        }

        public Task ReplaceCodesAsync(NotifoUser user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            ((MongoDbUser)user).ReplaceToken(InternalLoginProvider, RecoveryCodeTokenName, string.Join(";", recoveryCodes));

            return Task.CompletedTask;
        }

        public Task<bool> RedeemCodeAsync(NotifoUser user, string code, CancellationToken cancellationToken)
        {
            var mergedCodes = ((MongoDbUser)user).GetToken(InternalLoginProvider, RecoveryCodeTokenName) ?? string.Empty;

            var splitCodes = mergedCodes.Split(';');
            if (splitCodes.Contains(code))
            {
                var updatedCodes = new List<string>(splitCodes.Where(s => s != code));

                ((MongoDbUser)user).ReplaceToken(InternalLoginProvider, RecoveryCodeTokenName, string.Join(";", updatedCodes));

                return Task.FromResult(true);
            }

            return Task.FromResult(false);
        }
    }
}