using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShowCard.Services;

public static class FileBackupHelper
{
public static void BackupFile(string filePath, ILogService log)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                log.Warn($"File not found for backup: {filePath}");
                return;
            }
            var backupPath = $"{filePath}.{DateTime.Now.ToString("yyyyMMddHHmmssfff")}.bak";
            
            File.Copy(filePath, backupPath, overwrite: true);
            log.Info($"Backup created: {backupPath}");
        }
        catch (Exception ex)
        {
            log.Error($"Failed to backup file: {filePath}", ex);
        }
    }
}
