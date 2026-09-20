// ==========================================================================
//  Notifo.io
// ==========================================================================
//  Copyright (c) Sebastian Stehle
//  All rights reserved. Licensed under the MIT license.
// ==========================================================================

using Microsoft.Data.SqlClient;
using MySqlConnector;
using Npgsql;

namespace Notifo.Infrastructure;

public static class EFExceptionExtensions
{
    public static bool IsUniqueViolation(this Exception exception)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            switch (current)
            {
                case PostgresException postgres when postgres.SqlState is PostgresErrorCodes.UniqueViolation:
                case SqlException sqlServer when sqlServer.Number is 2601 or 2627:
                // Bulk inserts skip duplicate rows with MySQL and report the missing rows afterwards.
                case MySqlException mySql when mySql.ErrorCode is MySqlErrorCode.DuplicateKeyEntry or MySqlErrorCode.BulkCopyFailed:
                    return true;
            }
        }

        return false;
    }
}
