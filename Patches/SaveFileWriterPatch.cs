using HarmonyLib;
using Saving.Serialization.V15;
using Saving.Services.IO;
using Saving.Services.Model;
using Saving.Settings;
using System;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using UnityEngine;

namespace PerformanceFix.Patches
{
    [HarmonyPatch(typeof(SaveFileWriter), nameof(SaveFileWriter.WriteGameFile))]
    public class SaveFileWriterPatch
    {
        public static bool Prefix(SerializableGameWorld gameWorld, string directoryFullName, string saveName, SaveSettings ____saveSettings, ref GameFileInfo __result)
        {
            string screenshotFileName = Path.Combine(directoryFullName, saveName + ".jpg");
            string jsonFilePath = Path.Combine(directoryFullName, saveName + ".json");
            
            __result = new GameFileInfo(jsonFilePath, screenshotFileName);

            Task.Run(() =>
            {
                try
                {
                    JsonSerializerSettings settings = new JsonSerializerSettings
                    {
                        ContractResolver = new WritableOnlyContractResolver()
                    };

                    string jsonString = JsonConvert.SerializeObject(gameWorld, Formatting.None, settings);

                    using (StreamWriter streamWriter = File.Exists(jsonFilePath) 
                        ? new StreamWriter(jsonFilePath, false, ____saveSettings.encoding) 
                        : File.CreateText(jsonFilePath))
                    {
                        streamWriter.Write(jsonString);
                    }
                }
                catch (Exception exception)
                {
                    Debug.LogException(exception);
                }
            });

            return false; 
        }
    }
}