using System;
using System.Collections.Concurrent;
using System.IO;
using Newtonsoft.Json;

namespace DevDefined.Bitbucket.MMBot.ChangeScanner
{
    public class FileCommentMetaStore : ICommentMetaStore
    {
        readonly string _fileName;
        readonly object _writeLock = new object();
        ConcurrentDictionary<string, CommentMeta> _meta;

        public FileCommentMetaStore(string fileName)
        {
            _fileName = fileName;
            ReadFile();
        }

        public StorageResult Store(long commentId, string commitHash, DateTime updatedOn)
        {
            string key = commentId.ToString();
            CommentMeta meta;
            bool exists = _meta.TryGetValue(key, out meta);
            if (exists)
            {
                if (meta.UpdatedOn != updatedOn)
                {
                    meta.UpdatedOn = updatedOn;
                    SaveFile();
                    return StorageResult.Updated;
                }

                return StorageResult.Exists;
            }

            _meta[key] = new CommentMeta {CommitHash = commitHash, UpdatedOn = updatedOn};
            SaveFile();
            return StorageResult.New;
        }

        void ReadFile()
        {
            if (File.Exists(_fileName))
            {
                _meta = JsonConvert.DeserializeObject<ConcurrentDictionary<string, CommentMeta>>(File.ReadAllText(_fileName)) ?? new ConcurrentDictionary<string, CommentMeta>();
            }
            else
            {
                _meta = new ConcurrentDictionary<string, CommentMeta>();
            }
        }

        void SaveFile()
        {
            lock (_writeLock)
            {
                File.WriteAllText(_fileName, JsonConvert.SerializeObject(_meta));
            }
        }
    }
}