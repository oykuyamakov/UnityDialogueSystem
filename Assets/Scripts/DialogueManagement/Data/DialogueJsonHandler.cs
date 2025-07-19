using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace DialogueManagement.Data
{
    public static class DialogueJsonHandler
    {
        public static void SaveDialogue(DialogueContainer dialogueContainer)
        {
            var containerName = dialogueContainer.name;
            var SavePath = Path.Combine(Application.dataPath, "DialogueData", $"{containerName}.json");
            
            string directoryPath = Path.GetDirectoryName(SavePath);
            if (!Directory.Exists(directoryPath))
            {
                Directory.CreateDirectory(directoryPath);
            }

            // JSON verisini serialize et ve kaydet
            string json = JsonConvert.SerializeObject(dialogueContainer, Formatting.Indented);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Dialogue saved at: {SavePath}");
        }

        public static DialogueContainer LoadDialogue(string containerName)
        {
            var SavePath = Path.Combine(Application.dataPath, "DialogueData", $"{containerName}.json");
            
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("Dialogue file not found, returning new DialogueContainer.");
                return ScriptableObject.CreateInstance<DialogueContainer>();
            }

            string json = File.ReadAllText(SavePath);
            return JsonConvert.DeserializeObject<DialogueContainer>(json);
        } 
        
        public static DialogueContainer LoadDialogueFromPath(string dataPath)
        {
            if (!File.Exists(dataPath))
            {
                Debug.LogWarning("Dialogue file not found, returning new DialogueContainer.");
                return ScriptableObject.CreateInstance<DialogueContainer>();
            }

            string json = File.ReadAllText(dataPath);
            return JsonConvert.DeserializeObject<DialogueContainer>(json);
        }

        public static string GetDialogueJson(string containerName)
        {
            var SavePath = Path.Combine(Application.dataPath, "DialogueData", $"{containerName}.json");
            
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("Dialogue file not found.");
                return string.Empty;
            }
            return File.ReadAllText(SavePath);
        }

        public static void SetDialogueFromJson(string json, string containerName)
        {
            var SavePath = Path.Combine(Application.dataPath, "DialogueData", $"{containerName}.json");
            
            File.WriteAllText(SavePath, json);
            Debug.Log("Dialogue JSON updated.");
        }
    }
}
