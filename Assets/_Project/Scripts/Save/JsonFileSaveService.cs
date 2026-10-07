using System;
using System.IO;
using UnityEngine;

namespace Game2048.Save
{
   
    public sealed class JsonFileSaveService<T> : ISaveService<T> where T : class
    {
        private readonly string _path;
        private readonly string _tempPath;

        public JsonFileSaveService(string path)
        {
            _path = path;
            _tempPath = path + ".tmp";
        }

        public string FilePath => _path;

        public bool TryLoad(out T data)
        {
            data = null;
            try
            {
                if (!File.Exists(_path))
                    return false;

                string json = File.ReadAllText(_path);
                if (string.IsNullOrWhiteSpace(json))
                    return false;

                data = JsonUtility.FromJson<T>(json);
                return data != null;
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[Save] Не удалось прочитать {_path}: {e.Message}");
                return false;
            }
        }

        public void Save(T data)
        {
            if (data == null)
                return;

            try
            {
                string directory = Path.GetDirectoryName(_path);
                if (!string.IsNullOrEmpty(directory))
                    Directory.CreateDirectory(directory);

                File.WriteAllText(_tempPath, JsonUtility.ToJson(data));
                if (File.Exists(_path))
                    File.Delete(_path);
                File.Move(_tempPath, _path);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Не удалось записать {_path}: {e.Message}");
            }
        }

        public void Delete()
        {
            try
            {
                if (File.Exists(_path)) File.Delete(_path);
                if (File.Exists(_tempPath)) File.Delete(_tempPath);
            }
            catch (Exception e)
            {
                Debug.LogError($"[Save] Не удалось удалить {_path}: {e.Message}");
            }
        }
    }
}
