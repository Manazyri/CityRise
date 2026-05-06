#nullable enable

using UnityEngine;

namespace CityRise.Persistence
{
    /// <summary>
    /// ISaveable for the RTS camera rig. Phase 1 close test: place this on the same GameObject
    /// as the camera rig (whose Transform is the canonical source of position+rotation), register
    /// it in the SaveManifest at Bootstrap, and Save/Load round-trips the camera state.
    /// </summary>
    /// <remarks>
    /// Schema v1: position (Vector3) + euler rotation (Vector3). Phase 2+ may extend with FOV,
    /// orbit pivot, dampening targets, etc. The component reads the GameObject's transform at
    /// Serialize and writes it back at Deserialize, so it stays in sync no matter who else
    /// (RtsCameraController, debug console teleport command) moved the camera.
    /// </remarks>
    public sealed class CameraSaveState : MonoBehaviour, ISaveable
    {
        public string SubsystemId => "Camera";
        public int CurrentSchemaVersion => 1;

        public SaveBlob Serialize()
        {
            var blob = new SaveBlob();
            var t = transform;
            var p = t.position;
            var e = t.rotation.eulerAngles;
            blob.Write("posX", p.x);
            blob.Write("posY", p.y);
            blob.Write("posZ", p.z);
            blob.Write("rotX", e.x);
            blob.Write("rotY", e.y);
            blob.Write("rotZ", e.z);
            return blob;
        }

        public void Deserialize(SaveBlob blob, int fromVersion)
        {
            var t = transform;
            t.position = new Vector3(
                blob.ReadFloat("posX"),
                blob.ReadFloat("posY"),
                blob.ReadFloat("posZ"));
            t.rotation = Quaternion.Euler(
                blob.ReadFloat("rotX"),
                blob.ReadFloat("rotY"),
                blob.ReadFloat("rotZ"));
        }
    }
}
