using System.Collections;
using System.Collections.Generic;

#if UNITY_EDITOR
using UnityEditor;
#endif

using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class AssetPreviewGenerator : MonoBehaviour {
    public List<GameObject> l;

	// Use this for initialization
	void Start () {
		foreach (GameObject g in l) {
            Texture2D t = null;
#if UNITY_EDITOR
            t = AssetPreview.GetAssetPreview(g);

            while (AssetPreview.IsLoadingAssetPreview(g.GetInstanceID())) {
                System.Threading.Thread.Sleep(15);
            }
#endif
            FileStream file;
            string destination = Application.dataPath + "/Resources/Sprites/" + g.name + ".png";
            if (!File.Exists(destination)) {
                Debug.Log("Location does not exist: " + destination + ". Creating.");
                file = File.Open(destination, FileMode.Create);
                file.Close();
            }

            file = File.OpenWrite(destination);
            var bytes = ImageConversion.EncodeToPNG(t);
            var binary = new BinaryWriter(file);
            binary.Write(bytes);
            file.Close();
        }
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}
