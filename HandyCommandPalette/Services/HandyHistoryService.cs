// Copyright (c) Microsoft Corporation
// The Microsoft Corporation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using HandyCommandPalette.Models;
using Microsoft.Data.Sqlite;

namespace HandyCommandPalette.Services;

public sealed class HandyHistoryService
{
    private static string GetConnectionString()
    {
        var dbPath = HandyPathResolver.GetDatabasePath();
        return new SqliteConnectionStringBuilder
        {
            DataSource = dbPath,
            Mode = SqliteOpenMode.ReadOnly,
            DefaultTimeout = 3,
        }.ToString();
    }

    public static async Task<TranscriptEntry?> GetLastCompletedTranscriptAsync()
    {
        var dbPath = HandyPathResolver.GetDatabasePath();
        if (!File.Exists(dbPath))
        {
            return null;
        }

        try
        {
            await using var conn = new SqliteConnection(GetConnectionString());
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT
                    id,
                    file_name,
                    timestamp,
                    saved,
                    title,
                    transcription_text,
                    post_processed_text,
                    post_process_prompt,
                    post_process_requested
                FROM transcription_history
                WHERE transcription_text != ''
                ORDER BY timestamp DESC
                LIMIT 1";

            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return MapEntry(reader);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HandyHistoryService] Error reading last transcript: {ex.Message}");
        }

        return null;
    }

    public static async Task<TranscriptEntry[]> SearchTranscriptsAsync(string query, int limit = 100)
    {
        var dbPath = HandyPathResolver.GetDatabasePath();
        if (!File.Exists(dbPath))
        {
            return Array.Empty<TranscriptEntry>();
        }

        var results = new List<TranscriptEntry>();
        try
        {
            await using var conn = new SqliteConnection(GetConnectionString());
            await conn.OpenAsync();

            var cmd = conn.CreateCommand();
            if (string.IsNullOrWhiteSpace(query))
            {
                cmd.CommandText = @"
                    SELECT
                        id,
                        file_name,
                        timestamp,
                        saved,
                        title,
                        transcription_text,
                        post_processed_text,
                        post_process_prompt,
                        post_process_requested
                    FROM transcription_history
                    ORDER BY timestamp DESC
                    LIMIT @limit";
                cmd.Parameters.AddWithValue("@limit", limit);
            }
            else
            {
                cmd.CommandText = @"
                    SELECT
                        id,
                        file_name,
                        timestamp,
                        saved,
                        title,
                        transcription_text,
                        post_processed_text,
                        post_process_prompt,
                        post_process_requested
                    FROM transcription_history
                    WHERE transcription_text LIKE @q
                       OR title LIKE @q
                       OR (post_processed_text IS NOT NULL AND post_processed_text LIKE @q)
                    ORDER BY timestamp DESC
                    LIMIT @limit";
                cmd.Parameters.AddWithValue("@q", $"%{query.Trim()}%");
                cmd.Parameters.AddWithValue("@limit", limit);
            }

            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                results.Add(MapEntry(reader));
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[HandyHistoryService] Error querying transcripts: {ex.Message}");
        }

        return results.ToArray();
    }

    private static TranscriptEntry MapEntry(SqliteDataReader reader)
    {
        long id = reader.GetInt64(0);
        string fileName = reader.GetString(1);
        long timestamp = reader.GetInt64(2);
        bool saved = reader.GetBoolean(3);
        string title = reader.GetString(4);
        string transcriptionText = reader.GetString(5);

        string? postProcessed = reader.IsDBNull(6) ? null : reader.GetString(6);
        string? postProcessPrompt = reader.IsDBNull(7) ? null : reader.GetString(7);
        bool postProcessRequested = !reader.IsDBNull(8) && reader.GetBoolean(8);

        return new TranscriptEntry(
            id,
            fileName,
            timestamp,
            saved,
            title,
            transcriptionText,
            postProcessed,
            postProcessPrompt,
            postProcessRequested);
    }
}
