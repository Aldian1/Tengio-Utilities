namespace Tengio
{
    using System;
    using System.Collections;
    using System.IO;
    using System.Runtime.Serialization.Formatters.Binary;
    using System.Threading;
    using UnityEngine;

    public class SaveFileManager : Singleton<SaveFileManager>
    {
        public const long CURRENT_SAVE_WIPE_CODE = 20170425160000000;

        public delegate void SaveFileManagerEvent();
        public static event SaveFileManagerEvent OnSaveFileLoaded;

        private const string BASE_FIL_NAME = "save";
        private const string FILE_EXTENSION = ".sav";
        private const int AUTO_SAVE_DELAY = 60; // In s.

        private SaveFile saveFile;
        private Coroutine loadSaveFileCoroutine;
        private Coroutine autoSaveCoroutine;

        public SaveFile SaveFile
        {
            get
            {
                if (saveFile == null)
                {
                    saveFile = new SaveFile();
                }
                return saveFile;
            }
            set
            {
                saveFile = value;
            }
        }

        public void SaveToFile()
        {
            if (saveFile == null)
            {
                saveFile = new SaveFile();
            }
            saveFile.TimeStamp = GetCurrentTimeStamp();

            string filePath = GetSavePath();

            new Thread(() =>
            {
                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(filePath, FileMode.Create))
                {
                    try
                    {
                        formatter.Serialize(stream, saveFile);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error when saving: \n" + e.Message);
                    }
                }
            }).Start();
        }

        public void LoadSaveFromFile(Action<SaveFile> callback = null)
        {
            if (loadSaveFileCoroutine != null)
            {
                StopCoroutine(loadSaveFileCoroutine);
            }
            loadSaveFileCoroutine = StartCoroutine(LoadFromFileAsync(callback));
        }

        private IEnumerator LoadFromFileAsync(Action<SaveFile> callback = null)
        {
            string filePath = GetSavePath();
            Thread loadFileThread = new Thread(() =>
            {
                if (!File.Exists(filePath))
                {
                    Debug.LogError("Load save failed: There is no file to load.");
                    saveFile = null;
                    return;
                }

                BinaryFormatter formatter = new BinaryFormatter();
                using (FileStream stream = new FileStream(filePath, FileMode.Open))
                {
                    try
                    {
                        SaveFile loaded = formatter.Deserialize(stream) as SaveFile;
                        if (ShouldWipeSaveFile(loaded.StoredWipeCode))
                        {
                            loaded.Overwrite(new SaveFile());
                        }
                        saveFile = loaded;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError("Error when loading: \n" + e.Message);
                        saveFile.Overwrite(new SaveFile());
                    }
                }
            });
            loadFileThread.Start();

            while (loadFileThread.IsAlive) // TODO: Maybe add failsafe if thread never completes.
            {
                yield return null;
            }

            if (callback != null)
            {
                callback(saveFile);
            }
            if (OnSaveFileLoaded != null)
            {
                OnSaveFileLoaded();
            }
        }

        private string GetSavePath()
        {
            return Path.Combine(Application.persistentDataPath, BASE_FIL_NAME + FILE_EXTENSION);
        }

        private long GetCurrentTimeStamp()
        {
            return long.Parse(DateTime.Now.ToString("yyyyMMddHHmmssfff"));
        }

        private bool ShouldWipeSaveFile(long fileWipeCode)
        {
            return fileWipeCode != CURRENT_SAVE_WIPE_CODE;
        }

        public void StartAutoSave()
        {
            StopAutoSave();
            autoSaveCoroutine = StartCoroutine(AutoSaveToFile());
        }

        public void StopAutoSave()
        {
            if (autoSaveCoroutine != null)
            {
                StopCoroutine(autoSaveCoroutine);
            }
        }

        private IEnumerator AutoSaveToFile()
        {
            yield return new WaitForSeconds(AUTO_SAVE_DELAY);
            SaveToFile();
        }
    }
}

