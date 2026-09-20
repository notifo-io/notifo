using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Notifo.SqlProviders.SqlServer.App.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AppApiKeys",
                columns: table => new
                {
                    ApiKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppApiKeys", x => x.ApiKey);
                });

            migrationBuilder.CreateTable(
                name: "AppContributors",
                columns: table => new
                {
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContributorId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppContributors", x => new { x.AppId, x.ContributorId });
                });

            migrationBuilder.CreateTable(
                name: "Apps",
                columns: table => new
                {
                    DocId = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AuthDomain = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Counters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsPending = table.Column<bool>(type: "bit", nullable: false),
                    CountersVersion = table.Column<long>(type: "bigint", nullable: false),
                    Etag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Doc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Apps", x => x.DocId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ChannelTemplates",
                columns: table => new
                {
                    DocId = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Primary = table.Column<bool>(type: "bit", nullable: false),
                    Etag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Doc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChannelTemplates", x => x.DocId);
                });

            migrationBuilder.CreateTable(
                name: "Events",
                columns: table => new
                {
                    DocId = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Topic = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    SearchText = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    SendChannels = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Counters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Pending = table.Column<bool>(type: "bit", nullable: false),
                    CountersVersion = table.Column<long>(type: "bigint", nullable: false),
                    Etag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Doc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Events", x => x.DocId);
                });

            migrationBuilder.CreateTable(
                name: "Identity_Configuration",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Expires = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity_Configuration", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Identity_Keys",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    KeyId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    D = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DP = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    DQ = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Exponent = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    InverseQ = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Modulus = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    P = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Q = table.Column<byte[]>(type: "varbinary(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity_Keys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Identity_Xml",
                columns: table => new
                {
                    FriendlyName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Identity_Xml", x => x.FriendlyName);
                });

            migrationBuilder.CreateTable(
                name: "KeyValueStore",
                columns: table => new
                {
                    Key = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KeyValueStore", x => x.Key);
                });

            migrationBuilder.CreateTable(
                name: "Log",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Message = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    System = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FirstWriteId = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    FirstSeen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastSeen = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    EventCode = table.Column<int>(type: "int", nullable: false),
                    Count = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Log", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Media",
                columns: table => new
                {
                    DocId = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    LastUpdate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Etag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Doc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Media", x => x.DocId);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ChannelName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    QueueName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MessageData = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    MessageHeaders = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    TimeToLive = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeHandled = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Version = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictAuthorizations",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Scopes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictAuthorizations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Scheduler",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    QueueName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GroupKey = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Jobs = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Progressing = table.Column<bool>(type: "bit", nullable: false),
                    ProgressingStarted = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    DueTime = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Scheduler", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    DocId = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    TopicPrefix = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Etag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Doc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.DocId);
                });

            migrationBuilder.CreateTable(
                name: "Templates",
                columns: table => new
                {
                    DocId = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Etag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Doc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Templates", x => x.DocId);
                });

            migrationBuilder.CreateTable(
                name: "Topics",
                columns: table => new
                {
                    DocId = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Path = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Counters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    IsExplicit = table.Column<bool>(type: "bit", nullable: false),
                    LastUpdate = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CountersVersion = table.Column<long>(type: "bigint", nullable: false),
                    Etag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Doc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Topics", x => x.DocId);
                });

            migrationBuilder.CreateTable(
                name: "UserNotifications",
                columns: table => new
                {
                    DocId = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CorrelationId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SendChannels = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Updated = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    Etag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Doc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotifications", x => x.DocId);
                });

            migrationBuilder.CreateTable(
                name: "UserProperties",
                columns: table => new
                {
                    UserDocId = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    Key = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserProperties", x => new { x.UserDocId, x.Key });
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    DocId = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: false),
                    AppId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ApiKey = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EmailAddress = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Counters = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    CountersVersion = table.Column<long>(type: "bigint", nullable: false),
                    Etag = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Doc = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.DocId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OpenIddictTokens",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ApplicationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    AuthorizationId = table.Column<string>(type: "nvarchar(450)", nullable: true),
                    ConcurrencyToken = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpirationDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Properties = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RedemptionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReferenceId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true),
                    Type = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OpenIddictTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpenIddictTokens_OpenIddictAuthorizations_AuthorizationId",
                        column: x => x.AuthorizationId,
                        principalTable: "OpenIddictAuthorizations",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppApiKeys_AppId",
                table: "AppApiKeys",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_AppContributors_ContributorId",
                table: "AppContributors",
                column: "ContributorId");

            migrationBuilder.CreateIndex(
                name: "IX_Apps_AuthDomain",
                table: "Apps",
                column: "AuthDomain");

            migrationBuilder.CreateIndex(
                name: "IX_Apps_IsPending",
                table: "Apps",
                column: "IsPending");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_ChannelTemplates_AppId_Type",
                table: "ChannelTemplates",
                columns: new[] { "AppId", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_AppId_Created",
                table: "Events",
                columns: new[] { "AppId", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_Events_Created",
                table: "Events",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_Identity_Configuration_Expires",
                table: "Identity_Configuration",
                column: "Expires");

            migrationBuilder.CreateIndex(
                name: "IX_Log_AppId_UserId_LastSeen",
                table: "Log",
                columns: new[] { "AppId", "UserId", "LastSeen" });

            migrationBuilder.CreateIndex(
                name: "IX_Log_FirstWriteId",
                table: "Log",
                column: "FirstWriteId");

            migrationBuilder.CreateIndex(
                name: "IX_Media_AppId_LastUpdate",
                table: "Media",
                columns: new[] { "AppId", "LastUpdate" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ChannelName_TimeHandled",
                table: "Messages",
                columns: new[] { "ChannelName", "TimeHandled" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictAuthorizations_ApplicationId_Status_Subject_Type",
                table: "OpenIddictAuthorizations",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ApplicationId_Status_Subject_Type",
                table: "OpenIddictTokens",
                columns: new[] { "ApplicationId", "Status", "Subject", "Type" });

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_AuthorizationId",
                table: "OpenIddictTokens",
                column: "AuthorizationId");

            migrationBuilder.CreateIndex(
                name: "IX_OpenIddictTokens_ReferenceId",
                table: "OpenIddictTokens",
                column: "ReferenceId",
                unique: true,
                filter: "[ReferenceId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Scheduler_QueueName_GroupKey_Progressing_DueTime",
                table: "Scheduler",
                columns: new[] { "QueueName", "GroupKey", "Progressing", "DueTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Scheduler_QueueName_Progressing_DueTime",
                table: "Scheduler",
                columns: new[] { "QueueName", "Progressing", "DueTime" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_AppId_TopicPrefix",
                table: "Subscriptions",
                columns: new[] { "AppId", "TopicPrefix" });

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_AppId_UserId",
                table: "Subscriptions",
                columns: new[] { "AppId", "UserId" });

            migrationBuilder.CreateIndex(
                name: "IX_Templates_AppId",
                table: "Templates",
                column: "AppId");

            migrationBuilder.CreateIndex(
                name: "IX_Topics_AppId_LastUpdate",
                table: "Topics",
                columns: new[] { "AppId", "LastUpdate" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_AppId_CorrelationId_Updated_IsDeleted_Created",
                table: "UserNotifications",
                columns: new[] { "AppId", "CorrelationId", "Updated", "IsDeleted", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_AppId_UserId_Updated_IsDeleted_Created",
                table: "UserNotifications",
                columns: new[] { "AppId", "UserId", "Updated", "IsDeleted", "Created" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotifications_Created",
                table: "UserNotifications",
                column: "Created");

            migrationBuilder.CreateIndex(
                name: "IX_UserProperties_AppId_Key_Value",
                table: "UserProperties",
                columns: new[] { "AppId", "Key", "Value" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_ApiKey",
                table: "Users",
                column: "ApiKey",
                unique: true,
                filter: "[ApiKey] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Users_AppId_UserId",
                table: "Users",
                columns: new[] { "AppId", "UserId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AppApiKeys");

            migrationBuilder.DropTable(
                name: "AppContributors");

            migrationBuilder.DropTable(
                name: "Apps");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "ChannelTemplates");

            migrationBuilder.DropTable(
                name: "Events");

            migrationBuilder.DropTable(
                name: "Identity_Configuration");

            migrationBuilder.DropTable(
                name: "Identity_Keys");

            migrationBuilder.DropTable(
                name: "Identity_Xml");

            migrationBuilder.DropTable(
                name: "KeyValueStore");

            migrationBuilder.DropTable(
                name: "Log");

            migrationBuilder.DropTable(
                name: "Media");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "OpenIddictTokens");

            migrationBuilder.DropTable(
                name: "Scheduler");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Templates");

            migrationBuilder.DropTable(
                name: "Topics");

            migrationBuilder.DropTable(
                name: "UserNotifications");

            migrationBuilder.DropTable(
                name: "UserProperties");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "OpenIddictAuthorizations");
        }
    }
}
